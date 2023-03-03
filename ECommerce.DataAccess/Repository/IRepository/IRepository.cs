using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace ECommerce.DataAccess.Repository.IRepository
{
    public interface IRepository<T> where T:class
    {
        //renvoie un élément unique de type "T" à partir d'une source de données
        //en fonction d'un filtre donné sous forme d'expression lambda.
        T GetFirstOrDefault(Expression<Func<T, bool>> filter, string? includeProperties = null,bool tracked= true);

        //La méthode "GetAll" renvoie tous les éléments de type "T" à partir d'une source de données,
        //en fonction d'un filtre donné sous forme d'expression lambda.
        IEnumerable<T> GetAll(Expression<Func<T, bool>>? filter = null, string? includeProperties = null);
   
        void Add(T entity);
        void Remove(T entity);
        void RemoveRange(IEnumerable<T> entities);
    }
}
