using ECommerce.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ECommerce.DataAccess.Repository.IRepository
{
    public interface IOrderHeaderRepository :IRepository<OrderHeader>
    {
        // Méthode pour mettre à jour un objet OrderHeader dans la source de données
        void Update(OrderHeader obj);

        // Méthode pour mettre à jour l'état de la commande et/ou l'état de paiement d'une commande
        void UpdateStatus(int id, string orderStatus, string? paymentStatus = null);

        // Méthode pour mettre à jour l'ID de paiement Stripe pour une commande
        void UpdateStripePaymentID(int id, string sessionId, string paymentItentId);
    }
}
