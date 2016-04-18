using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNet.Builder;
using Microsoft.AspNet.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace NotFoundMiddlewareSample.Middleware
{
    // You may need to install the Microsoft.AspNet.Http.Abstractions package into your project
    public class NotFoundMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly RequestTracker _requestTracker;
        private readonly ILogger _logger;

        public NotFoundMiddleware(RequestDelegate next,
            ILoggerFactory loggerFactory,
            RequestTracker requestTracker)
        {
            _next = next;
            _requestTracker = requestTracker;
            _logger = loggerFactory.CreateLogger<NotFoundMiddleware>();
        }

        public async Task Invoke(HttpContext httpContext)
        {
            string path = httpContext.Request.Path;
            _logger.LogVerbose("Path: {path}");
            string correctedPath = _requestTracker.GetCorrectedPath(path);
            if(correctedPath != null)
            { 
                httpContext.Request.Path = correctedPath; // rewrite the path
                // OR
                // Send 301 response with correctedPath
            }
            await _next(httpContext);
            if (httpContext.Response.StatusCode == 404)
            {
                _logger.LogVerbose("Sending 404");
                _requestTracker.Record(httpContext.Request.Path); // NOTE: might not be same as original path at this point

                _logger.LogVerbose("NotFound Requests;Count");
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

        public static IServiceCollection AddNotFoundMiddleware(this IServiceCollection services)
        {
            services.AddSingleton<INotFoundRequestRepository, InMemoryNotFoundRequestRepository>();
            return services.AddSingleton<RequestTracker>();
        }
    }
}
