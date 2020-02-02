using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace SimpleNotes.Api.Middleware {
    public class ExceptionHandlerMiddleware {

        private readonly RequestDelegate next;

        public ExceptionHandlerMiddleware(RequestDelegate next) {
            this.next = next;
        }

        public async Task Invoke(HttpContext context) {
            try {
                await next(context);
            } catch (Exception ex) {
                context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                context.Response.ContentType = "application/json";
                string json = JsonSerializer.Serialize(new { error = ex.Message });
                await context.Response.WriteAsync(json);
            }
        }

    }
}
