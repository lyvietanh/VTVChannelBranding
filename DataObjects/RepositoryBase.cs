using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataObjects
{
    public class RepositoryBase<T> : IRepository<T> where T : class
    {
        public T Add(T entity)
        {
            throw new NotImplementedException();
        }

        public T Get(object key)
        {
            throw new NotImplementedException();
        }

        public IQueryable<T> GetAll()
        {
            throw new NotImplementedException();
        }

        public T Remove(T entity)
        {
            throw new NotImplementedException();
        }

        public void SaveChanges()
        {
            throw new NotImplementedException();
        }

        public T Update(T entity)
        {
            throw new NotImplementedException();
        }
    }
}
