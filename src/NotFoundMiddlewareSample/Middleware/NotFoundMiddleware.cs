using System.Threading.Tasks;
using Microsoft.AspNet.Builder;
using Microsoft.AspNet.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.OptionsModel;
using NotFoundMiddlewareSample.Models;
using Microsoft.Data.Entity;

namespace NotFoundMiddlewareSample.Middleware
{
    // You may need to install the Microsoft.AspNet.Http.Abstractions package into your project
public class NotFoundMiddleware
{
    private readonly RequestDelegate _next;
    private readonly RequestTracker _requestTracker;
    private readonly ILogger _logger;
    private NotFoundMiddlewareOptions _options;

    public NotFoundMiddleware(RequestDelegate next,
        ILoggerFactory loggerFactory,
        RequestTracker requestTracker,
        IOptions<NotFoundMiddlewareOptions> options)
    {
        _next = next;
        _requestTracker = requestTracker;
        _logger = loggerFactory.CreateLogger<NotFoundMiddleware>();
        _options = options.Value;
    }

        public async Task Invoke(HttpContext httpContext)
        {
            string path = httpContext.Request.Path;
            _logger.LogVerbose("Path: {path}");
            string correctedPath = _requestTracker.GetRequest(path)?.CorrectedPath;
            if(correctedPath != null)
            {
                if (_options.FixPathBehavior == FixPathBehavior.Redirect)
                {
                    httpContext.Response.Redirect(correctedPath, permanent: true);
                    return;
                }
                if(_options.FixPathBehavior == FixPathBehavior.Rewrite)
                {
                    httpContext.Request.Path = correctedPath; // rewrite the path
                }
                // throw invalid operation exception
            }
            await _next(httpContext);
            if (httpContext.Response.StatusCode == 404)
            {
                _requestTracker.Record(httpContext.Request.Path); // NOTE: might not be same as original path at this point
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

        public static IServiceCollection AddNotFoundMiddlewareInMemory(this IServiceCollection services)
        {
            services.AddSingleton<INotFoundRequestRepository, InMemoryNotFoundRequestRepository>();
            return services.AddSingleton<RequestTracker>();
        }
        public static IServiceCollection AddNotFoundMiddlewareEntityFramework(this IServiceCollection services, string connectionString)
        {
                services.AddEntityFramework()
                    .AddSqlServer()
                    .AddDbContext<NotFoundMiddlewareDbContext>(options =>
                        options.UseSqlServer(connectionString));

            services.AddSingleton<INotFoundRequestRepository, EfNotFoundRequestRepository>();
            return services.AddSingleton<RequestTracker>();
        }
    }
}
