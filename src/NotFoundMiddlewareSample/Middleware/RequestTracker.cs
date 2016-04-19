using System.Collections.Generic;

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
        var request = _repo.GetByPath(path);
        if (request != null)
        {
            request.IncrementCount();
        }
        else
        {
            request = new NotFoundRequest(path);
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

    public void UpdateRequest(NotFoundRequest request)
    {
        _repo.Update(request);
    }
}
}