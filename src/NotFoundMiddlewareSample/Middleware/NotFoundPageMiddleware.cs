using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.ApplicationInsights.AspNet.Extensions;
using Microsoft.AspNet.Builder;
using Microsoft.AspNet.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.OptionsModel;

namespace NotFoundMiddlewareSample.Middleware
{
    public class NotFoundPageMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly RequestTracker _requestTracker;
        private readonly ILogger _logger;
        private readonly NotFoundMiddlewareOptions _options;

        public NotFoundPageMiddleware(RequestDelegate next,
            ILoggerFactory loggerFactory,
            RequestTracker requestTracker,
            IOptions<NotFoundMiddlewareOptions> options)
        {
            _next = next;
            _requestTracker = requestTracker;
            _logger = loggerFactory.CreateLogger<NotFoundPageMiddleware>();
            _options = options.Value;
        }

        public async Task Invoke(HttpContext httpContext)
        {
            if (!httpContext.Request.Path.StartsWithSegments(_options.Path))
            {
                await _next(httpContext);
                return;
            }
            if (httpContext.Request.Query.Keys.Contains("path") &&
                httpContext.Request.Query.Keys.Contains("fixedpath"))
            {
                var request = _requestTracker.GetRequest(httpContext.Request.Query["path"]);
                request.SetCorrectedPath(httpContext.Request.Query["fixedpath"]);
                _requestTracker.UpdateRequest(request);
            }
            Render404List(httpContext);
        }

        private async void Render404List(HttpContext httpContext)
        {
            var model = _requestTracker.ListRequests().OrderByDescending(r => r.Count);
            var requestPage = new RequestPage(model);
            await requestPage.ExecuteAsync(httpContext);
        }
    }

    // Extension method used to add the middleware to the HTTP request pipeline.
    public static class NotFoundPageMiddlewareExtensions
    {
        public static IApplicationBuilder UseNotFoundPageMiddleware(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<NotFoundPageMiddleware>();
        }
    }

    public class RequestPage : BaseView
    {
        private readonly IEnumerable<NotFoundRequest> _requests;

        public RequestPage(IEnumerable<NotFoundRequest> requests)
        {
            _requests = requests;
        }

        public override async Task ExecuteAsync()
        {
            WriteLiteral("\r\n");
            Response.ContentType = "text/html";
            WriteLiteral(@"
<!DOCTYPE html>
<html>
<head>
    <meta charset=""utf-8"" />
    <title>Fix 404s</title>
    <script src=""//ajax.aspnetcdn.com/ajax/jquery/jquery-2.1.1.min.js""></script>
    <style>
        body {
font-size: .813em;
white-space: nowrap;
margin: 20px;
}
col:nth-child(2n) {
background-color: #FAFAFA;
}
form { 
display: inline-block; 
}
h1 {
margin-left: 25px;
}
table {
margin: 0px auto;
border-collapse: collapse;
border-spacing: 0px;
table-layout: fixed;
width: 100%;
}
td, th {
padding: 4px;
}
thead {
font-size: 1em;
font-family: Arial;
}
tr {
height: 23px;}

#requestHeader {
border-bottom: solid 1px gray;
border-top: solid 1px gray;
margin-bottom: 2px;
font-size: 1em;
line-height: 2em;
}

.collapse {
color: black;
float: right;
font-weight: normal;
width: 1em;
}

.date, .time {
width: 70px; 
}

.logHeader {
border-bottom: 1px solid lightgray;
color: gray;
text-align: left;
}

.logState {
text-overflow: ellipsis;
overflow: hidden;
}

.logTd {
border-left: 1px solid gray;
padding: 0px;
}

.logs {
width: 80%;
}

.logRow:hover {
background-color: #D6F5FF;
}

.requestRow>td {
border-bottom: solid 1px gray;
}

.severity {
width: 80px;
}

.summary {
color: black;
line-height: 1.8em;
}

.summary>th {
font-weight: normal;
}

.tab {
margin-left: 30px;
}

#viewOptions {
margin: 20px;
}

#viewOptions > * {
margin: 5px;
}
        body {
font-family: 'Segoe UI', Tahoma, Arial, Helvtica, sans-serif;
line-height: 1.4em;
}

h1 {
font-family: 'Segoe UI', Helvetica, sans-serif;
font-size: 2.5em;
}

td {
text-overflow: ellipsis;
overflow: hidden;
}

tr:nth-child(2n) {
background-color: #F6F6F6;
}

.critical {
background-color: red;
color: white;
}

.error {
color: red;
}

.information {
color: blue;
}

.debug {
color: black;
}

.warning {
color: orange;
}
    </style>
</head>
<body>
    <h1>Fix 404s</h1>");
            // render requests
            WriteLiteral(@"
    <table>
    <thead id=""requestHeader"">
        <th class=""path"">Path</th>
        <th>404 Count</th>
        <th>Corrected Path</th>
    </thead>
    <tbody>
");
            foreach (var request in _requests)
            {
                WriteLiteral(@"<tr class=""requestRow"">");
                WriteLiteral($"<td>{request.Path}</td><td>{request.Count}</td>");
                if (!String.IsNullOrEmpty(request.CorrectedPath))
                {
                    WriteLiteral($"<td>{ request.CorrectedPath}</td>");
                }
                else
                { // TODO: UrlEncode request.Path
                    WriteLiteral(@"<td><input type=""text"" />&nbsp;<a href=""?path=" + request.Path + @"&fixedPath="" class=""fixLink"">Save</a></td>");
                }
                WriteLiteral("</tr>");
            }

            WriteLiteral(@"
    </tbody>
    </table>
    <script type=""text/javascript"">
        $(function() {
            $("".fixLink"").click(function(event) {
                event.preventDefault();
                url = $(this).attr(""href"");
                url += $(this).prev().val();
                document.location = url;
            });
        });
    </script>
</body>
</html>
");
        }
    }
}