using System.Collections.Generic;
using System.Linq;

namespace NotFoundMiddlewareSample.Middleware
{
    public class InMemoryNotFoundRequestRepository : INotFoundRequestRepository
    {
        private static Dictionary<string,NotFoundRequest> _requests = new Dictionary<string, NotFoundRequest>();

        public NotFoundRequest GetByPath(string path)
        {
            var lowerPath = path.ToLowerInvariant();
            if (_requests.ContainsKey(lowerPath))
            {
                return _requests[lowerPath];
            }
            return null;
        }

        public IEnumerable<NotFoundRequest> List()
        {
            return _requests.Values.OrderByDescending(r => r.Count);
        }

        public void Update(NotFoundRequest notFoundRequest)
        {
            var lowerPath = notFoundRequest.Path;

            if (_requests.ContainsKey(lowerPath))
            {
                _requests[lowerPath] = notFoundRequest;
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