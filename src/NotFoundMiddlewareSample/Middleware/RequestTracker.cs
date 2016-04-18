using System.Collections.Generic;
using System.Linq;
using Microsoft.Data.Entity.Design;

namespace NotFoundMiddlewareSample.Middleware
{
    public class RequestTracker
    {
        private readonly INotFoundRequestRepository _repo;

        public RequestTracker(INotFoundRequestRepository repo)
        {
            _repo = repo;
        }

        public void Record(string path)
        {
            var lowerPath = path.ToLowerInvariant();
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

        public IEnumerable<NotFoundRequest> ListRequests()
        {
            return _repo.List();
        }

        public NotFoundRequest GetRequest(string path)
        {
            return _repo.GetByPath(path);
        }

        public string GetCorrectedPath(string path)
        {
            return GetRequest(path)?.CorrectedPath;
        }

        public void UpdateRequest(NotFoundRequest request)
        {
            _repo.Update(request);
        }
    }
}