using ECommerce.DataAccess.Data;
using ECommerce.DataAccess.Repository.IRepository;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics.Internal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
//Cette classe permet à l'application d'interagir avec la source
//de données à l'aide de méthodes génériques 
namespace ECommerce.DataAccess.Repository
{
    public class Repository<T> : IRepository<T> where T : class
    {
        private readonly ApplicationDbContext _db;
        internal DbSet<T> dbSet { get; set; }

        public Repository(ApplicationDbContext db)
        {
            _db = db;
            this.dbSet = _db.Set<T>();
        }

        public void Add(T entity)
        {
            dbSet.Add(entity);
        }

        // Cette méthode récupère tous les éléments de type "T" dans la source de données en
        // fonction d'un filtre et des propriétés de navigation à inclure
        public IEnumerable<T> GetAll(Expression<Func<T, bool>>? filter = null, string? includeProperties = null)
        {
            // Obtient un objet IQueryable<T> qui représente tous les éléments de type "T" dans la source de données
            IQueryable<T> query = dbSet;

            // Applique le filtre spécifié s'il est défini
            if (filter != null)
            {
                query = query.Where(filter);
            }

            // Inclut les propriétés de navigation spécifiées s'il y en a
            if (includeProperties != null)
            {
                foreach (var property in includeProperties.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
                {
                    query = query.Include(property);
                }
            }

            // Renvoie la liste des éléments correspondant aux critères spécifiés
            return query.ToList();
        }

        // Cette méthode renvoie le premier élément de type "T" qui correspond aux critères de filtrage spécifiés
        public T GetFirstOrDefault(Expression<Func<T, bool>> filter, string? includeProperties = null, bool tracked = true)
        {
            IQueryable<T> query;

            // Crée une requête de suivi ou de non-suivi en fonction de la valeur du paramètre "tracked"
            if (tracked)
            {
                query = dbSet;
            }
            else
            {
                query = dbSet.AsNoTracking();
            }

            // Applique le filtre spécifié à la requête
            query = query.Where(filter);

            // Inclut les propriétés de navigation spécifiées s'il y en a
            if (includeProperties != null)
            {
                foreach (var includeProp in includeProperties.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
                {
                    query = query.Include(includeProp);
                }
            }

            // Renvoie le premier élément correspondant aux critères de filtrage spécifiés
            return query.FirstOrDefault();
        }

        public void Remove(T entity)
        {
            dbSet.Remove(entity);
        }

        public void RemoveRange(IEnumerable<T> entities)
        {
            dbSet.RemoveRange(entities);
        }
    }
}
