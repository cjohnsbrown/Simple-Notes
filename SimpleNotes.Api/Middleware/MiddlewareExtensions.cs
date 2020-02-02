using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;

namespace SimpleNotes.Api.Middleware {
    public static class MiddlewareExtensions {
       
        /// <summary>
        /// Catch any uncaught exceptions thrown from controllers
        /// </summary>
        public static IApplicationBuilder UseGlobalExceptionHandler(this IApplicationBuilder builder) {
            return builder.UseMiddleware<ExceptionHandlerMiddleware>();
        }
    }
}
