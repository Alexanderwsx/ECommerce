using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

/*cette interface fournit une abstraction pour l'accès à la base de données dans une
 * application de commerce électronique, en regroupant les dépôts (repositories) 
 * pertinents sous une même interface et en fournissant une méthode pour enregistrer
 * les modifications de manière cohérente. Cela facilite la gestion des transactions 
 * avec la base de données et la maintenance de l'application.
 */
namespace ECommerce.DataAccess.Repository.IRepository
{
    public interface IUnitOfWork
    {
        ICategoryRepository Category { get; }
        IProductRepository Product { get; }
        IApplicationUserRepository ApplicationUser { get; }
        IOrderDetailRepository OrderDetail { get; }
        IOrderHeaderRepository OrderHeader { get; }

        void save();
    }
}
