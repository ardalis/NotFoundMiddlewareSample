using System.Collections.Generic;
using System.Linq;

namespace NotFoundMiddlewareSample.Middleware
{
    public class RequestTracker
    {
        // TODO: Pull out a repository for this
        private static List<NotFoundRequest> _requests = new List<NotFoundRequest>();
        public void Record(string path)
        {
            var lowerPath = path.ToLowerInvariant();
            NotFoundRequest request = _requests.FirstOrDefault(r => r.Path == lowerPath);
            if (request != null)
            {
                request.IncrementCount();
            }
            else
            {
                request = new NotFoundRequest(lowerPath);
                request.IncrementCount();
                _requests.Add(request);
            }
        }

        public IEnumerable<NotFoundRequest> ListRequests()
        {
            return _requests.ToArray();
        }

        public string GetCorrectedPath(string path)
        {
            var lowerPath = path.ToLowerInvariant();
            return _requests.FirstOrDefault(r => r.Path == lowerPath)?.CorrectedPath;
        }
    }

    
}