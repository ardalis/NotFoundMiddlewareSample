using System.Collections.Generic;
using System.Linq;

namespace NotFoundMiddlewareSample.Middleware
{
    public class InMemoryNotFoundRequestRepository : INotFoundRequestRepository
    {
        private static Dictionary<string,NotFoundRequest> _requests = new Dictionary<string, NotFoundRequest>();

        public NotFoundRequest GetByPath(string path)
        {
            if (_requests.ContainsKey(path))
            {
                return _requests[path];
            }
            return null;
        }

        public IEnumerable<NotFoundRequest> List()
        {
            return _requests.Values.OrderByDescending(r => r.Count);
        }

        public void Update(NotFoundRequest notFoundRequest)
        {
            if (_requests.ContainsKey(notFoundRequest.Path))
            {
                _requests[notFoundRequest.Path] = notFoundRequest;
            }
        }

        public void Add(NotFoundRequest notFoundRequest)
        {
            if (!_requests.ContainsKey(notFoundRequest.Path))
            {
                _requests.Add(notFoundRequest.Path, notFoundRequest);
            }
        }
    }
}