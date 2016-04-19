using System;

namespace NotFoundMiddlewareSample.Middleware
{
    public enum FixPathBehavior
    {
        Redirect,
        Rewrite
    }

    public class NotFoundMiddlewareOptions
    {
        public string Path { get; set; } = "/fix404s";
        public FixPathBehavior FixPathBehavior { get; set; } = FixPathBehavior.Redirect;
    }
}