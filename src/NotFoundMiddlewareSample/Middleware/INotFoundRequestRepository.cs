using System.Collections.Generic;

namespace NotFoundMiddlewareSample.Middleware
{
    public interface INotFoundRequestRepository
    {
        NotFoundRequest GetByPath(string path);
        IEnumerable<NotFoundRequest> List();
        void Update(NotFoundRequest notFoundRequest);
        void Add(NotFoundRequest notFoundRequest);
    }
}