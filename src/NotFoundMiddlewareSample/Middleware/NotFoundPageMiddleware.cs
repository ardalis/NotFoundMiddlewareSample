using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNet.Builder;
using Microsoft.AspNet.Diagnostics.Views;
using Microsoft.AspNet.Http;
using Microsoft.AspNet.Http.Headers;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.WebEncoders;

namespace NotFoundMiddlewareSample.Middleware
{
    public class NotFoundPageMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly RequestTracker _requestTracker;
        private readonly ILogger _logger;

        public NotFoundPageMiddleware(RequestDelegate next,
            ILoggerFactory loggerFactory,
            RequestTracker requestTracker)
        {
            _next = next;
            _requestTracker = requestTracker;
            _logger = loggerFactory.CreateLogger<NotFoundPageMiddleware>();
        }

        public async Task Invoke(HttpContext httpContext)
        {
            if (!httpContext.Request.Path.StartsWithSegments("/fix404s"))
            {
                await _next(httpContext);
                return;
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
    <h1>Fix 404s</h1>
    <form id=""requests"" method=""get"">");
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
                if(!String.IsNullOrEmpty(request.CorrectedPath))
                {
                    WriteLiteral($"<td>{ request.CorrectedPath}</td>");
                }
                else
                {
                    WriteLiteral(@"<td><input type=""text"" /><a href=""?path=path&fixedPath=123"">Save</a></td>");
                }
                WriteLiteral("</tr>");
            }

            WriteLiteral(@"
    </tbody>
    </table>
    </form>
</body>
</html>
");
        }
    }
    public abstract class BaseView
    {
        /// <summary>
        /// The request context
        /// </summary>
        protected HttpContext Context { get; private set; }

        /// <summary>
        /// The request
        /// </summary>
        protected HttpRequest Request { get; private set; }

        /// <summary>
        /// The response
        /// </summary>
        protected HttpResponse Response { get; private set; }

        /// <summary>
        /// The output stream
        /// </summary>
        protected StreamWriter Output { get; private set; }

        /// <summary>
        /// Html encoder used to encode content.
        /// </summary>
        protected HtmlEncoder HtmlEncoder { get; set; } = HtmlEncoder.Default;

        /// <summary>
        /// Url encoder used to encode content.
        /// </summary>
        protected UrlEncoder UrlEncoder { get; set; } = UrlEncoder.Default;

        /// <summary>
        /// Execute an individual request
        /// </summary>
        /// <param name="context"></param>
        public async Task ExecuteAsync(HttpContext context)
        {
            Context = context;
            Request = Context.Request;
            Response = Context.Response;
            Output = new StreamWriter(Response.Body, Encoding.UTF8, 4096, leaveOpen: true);
            await ExecuteAsync();
            Output.Dispose();
        }

        /// <summary>
        /// Execute an individual request
        /// </summary>
        public abstract Task ExecuteAsync();

        /// <summary>
        /// Write the given value directly to the output
        /// </summary>
        /// <param name="value"></param>
        protected void WriteLiteral(string value)
        {
            WriteLiteralTo(Output, value);
        }

        /// <summary>
        /// Write the given value directly to the output
        /// </summary>
        /// <param name="value"></param>
        protected void WriteLiteral(object value)
        {
            WriteLiteralTo(Output, value);
        }

        private List<string> AttributeValues { get; set; }

        protected void WriteAttributeValue(string thingy, int startPostion, object value, int endValue, int dealyo, bool yesno)
        {
            if (AttributeValues == null)
            {
                AttributeValues = new List<string>();
            }

            AttributeValues.Add(value.ToString());
        }

        private string AttributeEnding { get; set; }

        protected void BeginWriteAttribute(string name, string begining, int startPosition, string ending, int endPosition, int thingy)
        {
            Debug.Assert(string.IsNullOrEmpty(AttributeEnding));

            Output.Write(begining);
            AttributeEnding = ending;
        }

        protected void EndWriteAttribute()
        {
            Debug.Assert(!string.IsNullOrEmpty(AttributeEnding));

            var attributes = string.Join(" ", AttributeValues);
            Output.Write(attributes);
            AttributeValues = null;

            Output.Write(AttributeEnding);
            AttributeEnding = null;
        }

        /// <summary>
        /// Writes the given attribute to the given writer
        /// </summary>
        /// <param name="writer">The <see cref="TextWriter"/> instance to write to.</param>
        /// <param name="name">The name of the attribute to write</param>
        /// <param name="leader">The value of the prefix</param>
        /// <param name="trailer">The value of the suffix</param>
        /// <param name="values">The <see cref="AttributeValue"/>s to write.</param>
        protected void WriteAttributeTo(
            TextWriter writer,
            string name,
            string leader,
            string trailer,
            params AttributeValue[] values)
        {
            if (writer == null)
            {
                throw new ArgumentNullException(nameof(writer));
            }

            if (name == null)
            {
                throw new ArgumentNullException(nameof(name));
            }

            if (leader == null)
            {
                throw new ArgumentNullException(nameof(leader));
            }

            if (trailer == null)
            {
                throw new ArgumentNullException(nameof(trailer));
            }


            WriteLiteralTo(writer, leader);
            foreach (var value in values)
            {
                WriteLiteralTo(writer, value.Prefix);

                // The special cases here are that the value we're writing might already be a string, or that the
                // value might be a bool. If the value is the bool 'true' we want to write the attribute name
                // instead of the string 'true'. If the value is the bool 'false' we don't want to write anything.
                // Otherwise the value is another object (perhaps an HtmlString) and we'll ask it to format itself.
                string stringValue;
                if (value.Value is bool)
                {
                    if ((bool)value.Value)
                    {
                        stringValue = name;
                    }
                    else
                    {
                        continue;
                    }
                }
                else
                {
                    stringValue = value.Value as string;
                }

                // Call the WriteTo(string) overload when possible
                if (value.Literal && stringValue != null)
                {
                    WriteLiteralTo(writer, stringValue);
                }
                else if (value.Literal)
                {
                    WriteLiteralTo(writer, value.Value);
                }
                else if (stringValue != null)
                {
                    WriteTo(writer, stringValue);
                }
                else
                {
                    WriteTo(writer, value.Value);
                }
            }
            WriteLiteralTo(writer, trailer);
        }

        /// <summary>
        /// Convert to string and html encode
        /// </summary>
        /// <param name="value"></param>
        protected void Write(object value)
        {
            WriteTo(Output, value);
        }

        /// <summary>
        /// Html encode and write
        /// </summary>
        /// <param name="value"></param>
        protected void Write(string value)
        {
            WriteTo(Output, value);
        }

        /// <summary>
        /// <see cref="HelperResult.WriteTo(TextWriter)"/> is invoked
        /// </summary>
        /// <param name="result">The <see cref="HelperResult"/> to invoke</param>
        protected void Write(HelperResult result)
        {
            WriteTo(Output, result);
        }

        /// <summary>
        /// Writes the specified <paramref name="value"/> to <paramref name="writer"/>.
        /// </summary>
        /// <param name="writer">The <see cref="TextWriter"/> instance to write to.</param>
        /// <param name="value">The <see cref="object"/> to write.</param>
        /// <remarks>
        /// <see cref="HelperResult.WriteTo(TextWriter)"/> is invoked for <see cref="HelperResult"/> types.
        /// For all other types, the encoded result of <see cref="object.ToString"/> is written to the
        /// <paramref name="writer"/>.
        /// </remarks>
        protected void WriteTo(TextWriter writer, object value)
        {
            if (value != null)
            {
                var helperResult = value as HelperResult;
                if (helperResult != null)
                {
                    helperResult.WriteTo(writer);
                }
                else
                {
                    WriteTo(writer, Convert.ToString(value, CultureInfo.InvariantCulture));
                }
            }
        }

        /// <summary>
        /// Writes the specified <paramref name="value"/> with HTML encoding to <paramref name="writer"/>.
        /// </summary>
        /// <param name="writer">The <see cref="TextWriter"/> instance to write to.</param>
        /// <param name="value">The <see cref="string"/> to write.</param>
        protected void WriteTo(TextWriter writer, string value)
        {
            WriteLiteralTo(writer, HtmlEncoder.HtmlEncode(value));
        }

        /// <summary>
        /// Writes the specified <paramref name="value"/> without HTML encoding to the <paramref name="writer"/>.
        /// </summary>
        /// <param name="writer">The <see cref="TextWriter"/> instance to write to.</param>
        /// <param name="value">The <see cref="object"/> to write.</param>
        protected void WriteLiteralTo(TextWriter writer, object value)
        {
            WriteLiteralTo(writer, Convert.ToString(value, CultureInfo.InvariantCulture));
        }

        /// <summary>
        /// Writes the specified <paramref name="value"/> without HTML encoding to <see cref="Output"/>.
        /// </summary>
        /// <param name="writer">The <see cref="TextWriter"/> instance to write to.</param>
        /// <param name="value">The <see cref="string"/> to write.</param>
        protected void WriteLiteralTo(TextWriter writer, string value)
        {
            if (!string.IsNullOrEmpty(value))
            {
                writer.Write(value);
            }
        }

        //protected string HtmlEncodeAndReplaceLineBreaks(string input)
        //{
        //    if (string.IsNullOrEmpty(input))
        //    {
        //        return string.Empty;
        //    }

        //    // Split on line breaks before passing it through the encoder.
        //    return string.Join("<br />" + Environment.NewLine,
        //        input.Split(new[] { "\r\n" }, StringSplitOptions.None)
        //        .SelectMany(s => s.Split(new[] { '\r', '\n' }, StringSplitOptions.None))
        //        .Select(HtmlEncoder.Encode));
        //}
    }
}