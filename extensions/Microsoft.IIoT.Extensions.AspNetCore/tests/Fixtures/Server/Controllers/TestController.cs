// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.IIoT.AspNetCore.Tests.Controllers {
    using Microsoft.IIoT.AspNetCore.Tests.Models;
    using Microsoft.IIoT.AspNetCore.Tests.Filters;
    using Microsoft.AspNetCore.Mvc;
    using System;
    using System.Threading.Tasks;
    using System.ComponentModel.DataAnnotations;
    using System.Collections.Concurrent;
    using Microsoft.IIoT.Exceptions;

    /// <summary>
    /// Test controller
    /// </summary>
    [ApiVersion("1")]
    [ApiVersion("2")]
    [ApiVersion("3")]
    [ApiVersion("4")]
    [Route("v{version:apiVersion}/path/test")]
    [ExceptionsFilter]
    [ApiController]
    public class TestController : ControllerBase {

        public static ConcurrentDictionary<string, TestResponseModel> State { get; } =
            new ConcurrentDictionary<string, TestResponseModel>();

        [HttpPost("{id}")]
        public Task<TestResponseModel> TestPostAsync(
            string id, [FromBody][Required] TestRequestModel request) {
            if (id is null) {
                throw new ArgumentNullException(nameof(id));
            }
            var s = new TestResponseModel {
                Input = request.Input,
                Method = "Post",
                Id = id,
            };
            State.AddOrUpdate(s.Id, s, (x, y) => s);
            return Task.FromResult(s);
        }

        [HttpPatch("{id}")]
        public Task TestPatchAsync(
            string id, [FromBody][Required] TestRequestModel request) {
            if (id is null) {
                throw new ArgumentNullException(nameof(id));
            }
            var s = new TestResponseModel {
                Input = request.Input,
                Method = "Patch",
                Id = id,
            };
            State.AddOrUpdate(id, s, (x, y) => s);
            return Task.CompletedTask;
        }

        [HttpPut]
        public Task<TestResponseModel> TestPutAsync(
            [FromBody][Required] TestRequestModel request) {
            var s = new TestResponseModel {
                Input = request.Input,
                Method = "Put",
                Id = Guid.NewGuid().ToString(),
            };
            State.AddOrUpdate(s.Id, s, (x, y) => s);
            return Task.FromResult(s);
        }

        [HttpGet("{id}")]
        public Task<TestResponseModel> TestGetAsync(
            string id, [FromQuery] string input) {
            if (id is null) {
                throw new ArgumentNullException(nameof(id));
            }
            if (!State.TryGetValue(id, out var s)) {
                s = new TestResponseModel {
                    Input = input,
                    Method = "Get",
                    Id = id,
                };
            }
            return Task.FromResult(s);
        }

        [HttpDelete("{id}")]
        public Task TestDeleteAsync(string id) {
            if (id is null) {
                throw new ArgumentNullException(nameof(id));
            }
            if (!State.TryRemove(id, out _)) {
                throw new ResourceNotFoundException("Not found");
            }
            return Task.CompletedTask;
        }
    }
}
