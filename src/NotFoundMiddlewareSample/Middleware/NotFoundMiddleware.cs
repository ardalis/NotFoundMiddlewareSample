using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNet.Builder;
using Microsoft.AspNet.Http;
using Microsoft.Extensions.Logging;

namespace NotFoundMiddlewareSample.Middleware
{
    // You may need to install the Microsoft.AspNet.Http.Abstractions package into your project
    public class NotFoundMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger _logger;

        public NotFoundMiddleware(RequestDelegate next,
            ILoggerFactory loggerFactory)
        {
            _next = next;
            _logger = loggerFactory.CreateLogger<NotFoundMiddleware>();
        }

        public async Task Invoke(HttpContext httpContext)
        {
            string path = httpContext.Request.Path;
            _logger.LogVerbose("Path: {path}");
            if (path == "/nothere")
            {
                httpContext.Request.Path = "/home/index"; // rewrite the path
            }
            await _next(httpContext);
            if (httpContext.Response.StatusCode == 404)
            {
                _logger.LogVerbose("Sending 404");
            }
        }
    }

    // Extension method used to add the middleware to the HTTP request pipeline.
    public static class NotFoundMiddlewareExtensions
    {
        public static IApplicationBuilder UseNotFoundMiddleware(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<NotFoundMiddleware>();
        }
    }
}
