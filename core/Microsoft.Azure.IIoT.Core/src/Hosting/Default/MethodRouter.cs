// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.Azure.IIoT.Hosting.Services {
    using Microsoft.Azure.IIoT.Rpc.Default;
    using Microsoft.Azure.IIoT.Rpc;
    using Microsoft.Azure.IIoT.Hosting;
    using Microsoft.Azure.IIoT.Exceptions;
    using Microsoft.Azure.IIoT.Serializers;
    using Serilog;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using System.Threading.Tasks;

    /// <summary>
    /// Provides request routing to module controllers
    /// </summary>
    public sealed class MethodRouter : IMethodRouter {

        /// <summary>
        /// Property Di to prevent circular dependency between host and controller
        /// </summary>
        public IEnumerable<IMethodController> Controllers {
            get {
                return Enumerable.Empty<IMethodController>();
            }
            set {
                if (value is null) {
                    return;
                }
                foreach (var controller in value) {
                    AddToCallTable(controller);
                }
            }
        }

        /// <summary>
        /// Property Di to prevent circular dependency between host and invoker
        /// </summary>
        public IEnumerable<IMethodInvoker> ExternalInvokers {
            get {
                return _calltable.Values;
            }
            set {
                if (value is null) {
                    return;
                }
                foreach (var invoker in value) {
                    _calltable.AddOrUpdate(invoker.MethodName, invoker);
                }
            }
        }

        /// <summary>
        /// Create router
        /// </summary>
        /// <param name="serializer"></param>
        /// <param name="logger"></param>
        public MethodRouter(IJsonSerializer serializer, ILogger logger) {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _serializer = serializer ?? throw new ArgumentNullException(nameof(serializer));

            // Create chunk server always
            var server = new ChunkMethodServer(_serializer, logger);
            _calltable = new Dictionary<string, IMethodInvoker> {
                { server.MethodName.ToUpperInvariant(), server }
            };
        }

        /// <inheritdoc/>
        public async Task<byte[]> InvokeAsync(string target, string method, byte[] payload,
            string contentType) {
            if (string.IsNullOrEmpty(method)) {
                throw new ArgumentNullException(nameof(method));
            }
            if (!_calltable.TryGetValue(method.ToUpperInvariant(), out var invoker)) {
                throw new NotSupportedException(
                    $"Unknown controller method {method} called.");
            }
            return await invoker.InvokeAsync(target, payload, contentType,
                this).ConfigureAwait(false);
        }

        /// <summary>
        /// Add target to calltable
        /// </summary>
        /// <param name="target"></param>
        private void AddToCallTable(object target) {
            var versions = target.GetType().GetCustomAttributes<VersionAttribute>(true)
                .Select(v => "_v" + v.Value)
                .ToList();
            if (versions.Count == 0) {
                versions.Add(string.Empty);
            }
            foreach (var methodInfo in target.GetType().GetMethods()) {
                if (methodInfo.GetCustomAttribute<IgnoreAttribute>() != null) {
                    // Should be ignored
                    continue;
                }
                if (!typeof(Task).IsAssignableFrom(methodInfo.ReturnType)) {
                    // must be assignable from task
                    continue;
                }
                var tArgs = methodInfo.ReturnParameter.ParameterType
                    .GetGenericArguments();
                if (tArgs.Length > 1) {
                    // must have exactly 0 or one (serializable) type to return
                    continue;
                }
                var name = methodInfo.Name;
                if (name.EndsWith("Async", StringComparison.Ordinal)) {
                    name = name[0..^5];
                }
                name = name.ToUpperInvariant();

                // Register for all defined versions
                foreach (var version in versions) {
                    var versionedName = name + version;
                    versionedName = versionedName.ToUpperInvariant();
                    if (!_calltable.TryGetValue(versionedName, out var invoker)) {
                        invoker = new DynamicInvoker(_logger);
                        _calltable.Add(versionedName, invoker);
                    }
                    if (invoker is DynamicInvoker dynamicInvoker) {
                        dynamicInvoker.Add(target, methodInfo, _serializer);
                    }
                    else {
                        // Should never happen...
                        throw new InvalidOperationException(
                            $"Cannot add {versionedName} since invoker is private.");
                    }
                }
            }
        }

        /// <summary>
        /// Encapsulates invoking a matching service on the controller
        /// </summary>
        private class DynamicInvoker : IMethodInvoker {

            /// <inheritdoc/>
            public string MethodName { get; private set; }

            /// <summary>
            /// Create dynamic invoker
            /// </summary>
            public DynamicInvoker(ILogger logger) {
                _logger = logger;
                _invokers = new List<JsonMethodInvoker>();
            }

            /// <summary>
            /// Add invoker
            /// </summary>
            /// <param name="controller"></param>
            /// <param name="controllerMethod"></param>
            /// <param name="serializer"></param>
            public void Add(object controller, MethodInfo controllerMethod,
                IJsonSerializer serializer) {
                _logger.Verbose("Adding {controller}.{method} method to invoker...",
                    controller.GetType().Name, controllerMethod.Name);
                _invokers.Add(new JsonMethodInvoker(controller, controllerMethod, serializer, _logger));
                MethodName = controllerMethod.Name;
            }

            /// <inheritdoc/>
            public async Task<byte[]> InvokeAsync(string target, byte[] payload,
                string contentType, IMethodHandler handler) {
                Exception e = null;
                foreach (var invoker in _invokers) {
                    try {
                        return await invoker.InvokeAsync(target, payload, contentType,
                            handler).ConfigureAwait(false);
                    }
                    catch (Exception ex) {
                        // Save last, and continue
                        e = ex;
                    }
                }
                _logger.Verbose(e, "Exception during method invocation.");
                throw e;
            }

            /// <inheritdoc/>
            public void Dispose() {
                foreach (var invoker in _invokers) {
                    invoker.Dispose();
                }
            }

            private readonly ILogger _logger;
            private readonly List<JsonMethodInvoker> _invokers;
        }

        /// <summary>
        /// Invokes a method with json payload
        /// </summary>
        private class JsonMethodInvoker : IMethodInvoker {

            /// <inheritdoc/>
            public string MethodName => _controllerMethod.Name;

            /// <summary>
            /// Default filter implementation if none is specified
            /// </summary>
            private class DefaultFilter : ExceptionFilterAttribute {
                public override Exception Filter(Exception exception, out int status) {
                    status = 400;
                    return exception;
                }
            }

            /// <summary>
            /// Create invoker
            /// </summary>
            /// <param name="controller"></param>
            /// <param name="controllerMethod"></param>
            /// <param name="serializer"></param>
            /// <param name="logger"></param>
            public JsonMethodInvoker(object controller, MethodInfo controllerMethod,
                IJsonSerializer serializer, ILogger logger) {
                _logger = logger;
                _serializer = serializer;
                _controller = controller;
                _controllerMethod = controllerMethod;
                _methodParams = _controllerMethod.GetParameters();
                _ef = _controllerMethod.GetCustomAttribute<ExceptionFilterAttribute>(true) ??
                    controller.GetType().GetCustomAttribute<ExceptionFilterAttribute>(true) ??
                    new DefaultFilter();
                var returnArgs = _controllerMethod.ReturnParameter.ParameterType.GetGenericArguments();
                if (returnArgs.Length > 0) {
                    _methodTaskContinuation = kMethodResponseAsContinuation.MakeGenericMethod(
                        returnArgs[0]);
                }
            }

            /// <inheritdoc/>
            public Task<byte[]> InvokeAsync(string target, byte[] payload,
                string contentType, IMethodHandler handler) {
                object task;
                try {
                    object[] inputs;
                    if (_methodParams.Length == 0) {
                        inputs = Array.Empty<object>();
                    }
                    else if (_methodParams.Length == 1) {
                        var data = _serializer.Deserialize(payload, _methodParams[0].ParameterType);
                        inputs = new[] { data };
                    }
                    else {
                        var data = _serializer.Parse(payload);
                        inputs = _methodParams.Select(param => {
                            if (data.TryGetProperty(param.Name,
                                out var value, StringComparison.InvariantCultureIgnoreCase)) {
                                return value.ConvertTo(param.ParameterType);
                            }
                            return param.HasDefaultValue ? param.DefaultValue : null;
                        }).ToArray();
                    }
                    task = _controllerMethod.Invoke(_controller, inputs);
                }
                catch (Exception e) {
                    task = Task.FromException(e);
                }
                if (_methodTaskContinuation == null) {
                    return VoidContinuation((Task)task);
                }
                return (Task<byte[]>)_methodTaskContinuation.Invoke(this, new[] {
                    task
                });
            }

            /// <inheritdoc/>
            public void Dispose() {
            }

            /// <summary>
            /// Helper to convert a typed response to buffer or throw appropriate
            /// exception as continuation.
            /// </summary>
            /// <typeparam name="T"></typeparam>
            /// <param name="task"></param>
            /// <returns></returns>
            public Task<byte[]> MethodResultConverterContinuation<T>(Task<T> task) {
                return task.ContinueWith(tr => {
                    if (tr.IsFaulted || tr.IsCanceled) {
                        var ex = tr.Exception?.Flatten().InnerExceptions.FirstOrDefault();
                        if (ex == null) {
                            ex = new TaskCanceledException(tr);
                        }
                        _logger.Verbose(ex, "Method call error");
                        ex = _ef.Filter(ex, out var status);
                        throw new MethodCallStatusException(ex != null ?
                           _serializer.SerializeToString(ex) : null, status);
                    }
                    return _serializer.SerializeToBytes(tr.Result).ToArray();
                }, TaskScheduler.Current);
            }

            /// <summary>
            /// Helper to convert a void response to buffer or throw appropriate
            /// exception as continuation.
            /// </summary>
            /// <param name="task"></param>
            /// <returns></returns>
            public Task<byte[]> VoidContinuation(Task task) {
                return task.ContinueWith(tr => {
                    if (tr.IsFaulted || tr.IsCanceled) {
                        var ex = tr.Exception?.Flatten().InnerExceptions.FirstOrDefault();
                        if (ex == null) {
                            ex = new TaskCanceledException(tr);
                        }
                        _logger.Verbose(ex, "Method call error");
                        ex = _ef.Filter(ex, out var status);
                        throw new MethodCallStatusException(ex != null ?
                            _serializer.SerializeToString(ex) : null, status);
                    }
                    return Array.Empty<byte>();
                }, TaskScheduler.Current);
            }

            private static readonly MethodInfo kMethodResponseAsContinuation =
                typeof(JsonMethodInvoker).GetMethod(nameof(MethodResultConverterContinuation),
                    BindingFlags.Public | BindingFlags.Instance);
            private readonly IJsonSerializer _serializer;
            private readonly ILogger _logger;
            private readonly object _controller;
            private readonly ParameterInfo[] _methodParams;
            private readonly ExceptionFilterAttribute _ef;
            private readonly MethodInfo _controllerMethod;
            private readonly MethodInfo _methodTaskContinuation;
        }

        private readonly ILogger _logger;
        private readonly IJsonSerializer _serializer;
        private readonly Dictionary<string, IMethodInvoker> _calltable;
    }
}
