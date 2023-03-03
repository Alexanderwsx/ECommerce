using ECommerce.DataAccess.Data;
using ECommerce.DataAccess.Repository.IRepository;
using ECommerce.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ECommerce.DataAccess.Repository
{
    public class OrderHeaderRepository : Repository<OrderHeader>, IOrderHeaderRepository
    {
        private ApplicationDbContext _db;

        // Constructeur qui prend une référence au contexte de base de données et appelle le constructeur de la classe de base
        public OrderHeaderRepository(ApplicationDbContext db) : base(db)
        {
            _db = db;
        }

        // Méthode pour mettre à jour un objet OrderHeader dans la source de données
        public void Update(OrderHeader obj)
        {
            _db.OrderHeaders.Update(obj);
        }

        // Méthode pour mettre à jour l'état de la commande et/ou l'état de paiement d'une commande
        public void UpdateStatus(int id, string orderStatus, string? paymentStatus = null)
        {
            // Récupère l'objet OrderHeader correspondant à l'ID spécifié
            var orderFromDBb = _db.OrderHeaders.FirstOrDefault(x => x.Id == id);

            // Met à jour l'état de la commande et/ou l'état de paiement s'ils sont spécifiés
            if (orderFromDBb != null)
            {
                orderFromDBb.OrderStatus = orderStatus;
                if (paymentStatus != null)
                {
                    orderFromDBb.PaymentStatus = paymentStatus;
                }
            }
        }

        // Méthode pour mettre à jour l'ID de paiement Stripe pour une commande
        public void UpdateStripePaymentID(int id, string sessionId, string paymentItentId)
        {
            // Récupère l'objet OrderHeader correspondant à l'ID spécifié
            var orderFromDb = _db.OrderHeaders.FirstOrDefault(u => u.Id == id);

            // Met à jour les informations de paiement de la commande
            orderFromDb.PaymentDate = DateTime.Now;
            orderFromDb.SessionId = sessionId;
            orderFromDb.PaymentIntentId = paymentItentId;
        }
    }
}