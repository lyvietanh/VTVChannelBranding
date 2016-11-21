using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataObjects
{
    public interface IRepository<T>
    {
        T Add(T entity);
        T Remove(T entity);
        T Update(T entity);
        IQueryable<T> GetAll();
        T Get(object key);
        void SaveChanges();
    }
}
