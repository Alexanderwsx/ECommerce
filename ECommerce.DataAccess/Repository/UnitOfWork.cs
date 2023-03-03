using ECommerce.DataAccess.Data;
using ECommerce.DataAccess.Repository.IRepository;
using ECommerce.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

/*cette classe fournit une implémentation concrète de l'interface 
 * "IUnitOfWork" pour une application de commerce électronique, en 
 * regroupant les dépôts (repositories) pertinents sous une même interface 
 * et en fournissant une méthode pour enregistrer les modifications de manière 
 * cohérente. Cela facilite la gestion des transactions avec la base de données 
 * et la maintenance de l'application.*/
namespace ECommerce.DataAccess.Repository
{
    public class UnitOfWork : IUnitOfWork
    {
        private ApplicationDbContext _db;

        // Constructeur qui prend une référence au contexte de base de données et initialise les dépôts (repositories)
        public UnitOfWork(ApplicationDbContext db)
        {
            _db = db;
            Category = new CategoryRepository(_db);
            Product = new ProductRepository(_db);
            ApplicationUser = new ApplicationUserRepository(_db);
            OrderDetail = new OrderDetailRepository(_db);
            OrderHeader = new OrderHeaderRepository(_db);
        }

        // Propriétés qui exposent les dépôts (repositories) pertinents pour l'application
        public ICategoryRepository Category { get; private set; }
        public IProductRepository Product { get; private set; }
        public IApplicationUserRepository ApplicationUser { get; private set; }
        public IOrderDetailRepository OrderDetail { get; private set; }
        public IOrderHeaderRepository OrderHeader { get; private set; }

        // Méthode pour enregistrer toutes les modifications apportées aux entités dans la base de données
        public void save()
        {
            _db.SaveChanges();
        }
    }
}