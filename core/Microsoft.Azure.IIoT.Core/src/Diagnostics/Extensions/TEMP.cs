
namespace Microsoft.Extensions.Logging {
    using System;

    /// <inheritdoc/>
    public static class LoggerExtensions2 {

        /// <inheritdoc/>
        public static void Debug(this ILogger logger, Exception exception, string message, params object[] args) {
            logger.LogDebug(exception, message, args);
        }

        /// <inheritdoc/>
        public static void Debug(this ILogger logger, string message, params object[] args) {
            logger.LogDebug(message, args);
        }

        /// <inheritdoc/>
        public static void Verbose(this ILogger logger, Exception exception, string message, params object[] args) {
            logger.LogTrace(exception, message, args);
        }

        /// <inheritdoc/>
        public static void Verbose(this ILogger logger, string message, params object[] args) {
            logger.LogTrace(message, args);
        }

        /// <inheritdoc/>
        public static void Information(this ILogger logger, Exception exception, string message, params object[] args) {
            logger.LogInformation(exception, message, args);
        }

        /// <inheritdoc/>
        public static void Information(this ILogger logger, string message, params object[] args) {
            logger.LogInformation(message, args);
        }

        /// <inheritdoc/>
        public static void Warning(this ILogger logger, Exception exception, string message, params object[] args) {
            logger.LogWarning(exception, message, args);
        }

        /// <inheritdoc/>
        public static void Warning(this ILogger logger, string message, params object[] args) {
            logger.LogWarning(message, args);
        }

        /// <inheritdoc/>
        public static void Error(this ILogger logger, Exception exception, string message, params object[] args) {
            logger.LogError(exception, message, args);
        }

        /// <inheritdoc/>
        public static void Error(this ILogger logger, string message, params object[] args) {
            logger.LogError(message, args);
        }

        /// <inheritdoc/>
        public static void Fatal(this ILogger logger, Exception exception, string message, params object[] args) {
            logger.LogCritical(exception, message, args);
        }

        /// <inheritdoc/>
        public static void Fatal(this ILogger logger, string message, params object[] args) {
            logger.LogCritical(message, args);
        }
    }
}
