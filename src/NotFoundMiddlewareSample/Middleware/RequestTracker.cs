using System.Collections.Generic;

namespace NotFoundMiddlewareSample.Middleware
{
    public class RequestTracker
    {
        private readonly INotFoundRequestRepository _repo;
        private static object _lock = new object();

        public RequestTracker(INotFoundRequestRepository repo)
        {
            _repo = repo;
        }

        public void Record(string path)
        {
            string lowerPath = path.ToLowerInvariant();
            lock (_lock)
            {
                var request = _repo.GetByPath(lowerPath);
                if (request != null)
                {
                    request.IncrementCount();
                }
                else
                {
                    request = new NotFoundRequest(lowerPath);
                    request.IncrementCount();
                    _repo.Add(request);
                }
            }
        }

        public IEnumerable<NotFoundRequest> ListRequests()
        {
            return _repo.List();
        }

        public NotFoundRequest GetRequest(string path)
        {
            return _repo.GetByPath(path);
        }

        public void UpdateRequest(NotFoundRequest request)
        {
            _repo.Update(request);
        }
    }
}