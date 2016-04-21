using System.Collections.Generic;
using System.Linq;
using Microsoft.Data.Entity;

namespace NotFoundMiddlewareSample.Middleware
{
    public class EfNotFoundRequestRepository : INotFoundRequestRepository
    {
        private readonly NotFoundMiddlewareDbContext _dbContext;

        public EfNotFoundRequestRepository(NotFoundMiddlewareDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public NotFoundRequest GetByPath(string path)
        {
            return _dbContext.NotFoundRequests.FirstOrDefault(r => r.Path == path);
        }

        public IEnumerable<NotFoundRequest> List()
        {
            return _dbContext.NotFoundRequests.AsEnumerable();
        }

        public void Update(NotFoundRequest notFoundRequest)
        {
            _dbContext.Entry<NotFoundRequest>(notFoundRequest).State = EntityState.Modified;
            _dbContext.SaveChanges();
        }

        public void Add(NotFoundRequest notFoundRequest)
        {
            _dbContext.NotFoundRequests.Add(notFoundRequest);
            _dbContext.SaveChanges();
        }
    }
}