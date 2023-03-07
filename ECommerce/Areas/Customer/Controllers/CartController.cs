using ECommerce.DataAccess.Migrations;
using ECommerce.DataAccess.Repository.IRepository;
using ECommerce.Models;
using ECommerce.Models.ViewModels;
using ECommerce.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Stripe.Checkout;
using System.Security.Claims;

namespace ECommerce.Areas.Customer.Controllers
{
    [Area("Customer")]
    [Authorize]
    public class CartController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IEmailSender _emailSender;

        [BindProperty]
        public ShoppingCartVM ShoppingCartVM { get; set; }
        public int OrderTotal { get; set; }
        public CartController(IUnitOfWork unitOfWork, IEmailSender emailSender)
        {
            _unitOfWork = unitOfWork;
            _emailSender = emailSender;
        }
        public IActionResult Index()
        {
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);

            ShoppingCartVM = new ShoppingCartVM()
            {
                ListCart = _unitOfWork.ShoppingCart.GetAll
                (u => u.ApplicationUserId == claim.Value, includeProperties: "Product"),
                OrderHeader = new()
            };
            foreach (var cart in ShoppingCartVM.ListCart)
            {
                ShoppingCartVM.OrderHeader.OrderTotal += (cart.Count * cart.Product.Price);
            }

            return View(ShoppingCartVM);
        }

        public IActionResult Summary()
        {
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);


            ShoppingCartVM = new ShoppingCartVM()
            {
                ListCart = _unitOfWork.ShoppingCart.GetAll
                (u => u.ApplicationUserId == claim.Value, includeProperties: "Product"),
                OrderHeader = new()
            };

            ShoppingCartVM.OrderHeader.ApplicationUser = _unitOfWork.ApplicationUser.GetFirstOrDefault
                (u => u.Id == claim.Value);

            ShoppingCartVM.OrderHeader.Name = ShoppingCartVM.OrderHeader.ApplicationUser.Name;
            ShoppingCartVM.OrderHeader.PhoneNumber = ShoppingCartVM.OrderHeader.ApplicationUser.PhoneNumber;
            ShoppingCartVM.OrderHeader.StreetAddress = ShoppingCartVM.OrderHeader.ApplicationUser.StreetAddress;
            ShoppingCartVM.OrderHeader.City = ShoppingCartVM.OrderHeader.ApplicationUser.City;
            ShoppingCartVM.OrderHeader.State = ShoppingCartVM.OrderHeader.ApplicationUser.State;
            ShoppingCartVM.OrderHeader.PostalCode = ShoppingCartVM.OrderHeader.ApplicationUser.PostalCode;

            foreach (var cart in ShoppingCartVM.ListCart)
            {
                ShoppingCartVM.OrderHeader.OrderTotal += (cart.Count * cart.Product.Price);
            }
            return View(ShoppingCartVM);
        }

        public IActionResult Plus(int cartID)
        {

            var cart = _unitOfWork.ShoppingCart.GetFirstOrDefault(u => u.Id == cartID);
            _unitOfWork.ShoppingCart.IncrementCount(cart, 1);
            _unitOfWork.Save();
            return RedirectToAction(nameof(Index));
        }
        public IActionResult Minus(int cartID)
        {
            //var qui contient le cart cherché
            var cart = _unitOfWork.ShoppingCart.GetFirstOrDefault(u => u.Id == cartID);

            //pour eviter que ca fasse -1. si le compte arrive à 0 ca se éfface
            if (cart.Count <= 1)
            {
                _unitOfWork.ShoppingCart.Remove(cart);

                //pour ajuster la quantite du shoppingcart icon
                var count = _unitOfWork.ShoppingCart.GetAll(u => u.ApplicationUserId == cart.ApplicationUserId).ToList().Count() - 1;
                HttpContext.Session.SetInt32(SD.SessionCart, count);
            }
            else
            {
                _unitOfWork.ShoppingCart.DecrementCount(cart, 1);
            }
            _unitOfWork.Save();
            return RedirectToAction(nameof(Index));
        }
        public IActionResult Remove(int cartID)
        {

            var cart = _unitOfWork.ShoppingCart.GetFirstOrDefault(u => u.Id == cartID);
            _unitOfWork.ShoppingCart.Remove(cart);
            _unitOfWork.Save();

            //ajuster icon shoppingcart
            var count = _unitOfWork.ShoppingCart.GetAll(u => u.ApplicationUserId == cart.ApplicationUserId).ToList().Count();
            HttpContext.Session.SetInt32(SD.SessionCart, count);

            return RedirectToAction(nameof(Index));
        }



        [HttpPost]
        [ActionName("Summary")]
        [ValidateAntiForgeryToken]
        public IActionResult SummaryPOST()
        {
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);

            ShoppingCartVM.ListCart = _unitOfWork.ShoppingCart.GetAll
                            (u => u.ApplicationUserId == claim.Value, includeProperties: "Product");

            //on met à jour le status de la commande

            ShoppingCartVM.OrderHeader.OrderDate = System.DateTime.Now;
            ShoppingCartVM.OrderHeader.ApplicationUserId = claim.Value;

            //prix 
            foreach (var cart in ShoppingCartVM.ListCart)
            {
                cart.Price = cart.Product.Price;
                ShoppingCartVM.OrderHeader.OrderTotal += (cart.Count * cart.Price);
            }
            ApplicationUser applicationUser = _unitOfWork.ApplicationUser.GetFirstOrDefault(u => u.Id == claim.Value);


            ShoppingCartVM.OrderHeader.PaymentStatus = SD.PaymentStatusPending;
            ShoppingCartVM.OrderHeader.OrderStatus = SD.StatusPending;



            _unitOfWork.OrderHeader.Add(ShoppingCartVM.OrderHeader);
            _unitOfWork.Save();

            foreach (var cart in ShoppingCartVM.ListCart)
            {
                OrderDetail orderDetail = new()
                {
                    ProductId = cart.ProductId,
                    OrderId = ShoppingCartVM.OrderHeader.Id,
                    Price = cart.Product.Price,
                    Count = cart.Count
                };
                _unitOfWork.OrderDetail.Add(orderDetail);
                _unitOfWork.Save();
            }
                //stripe settings //stripe checkout
                /*
                 configure une session Stripe pour traiter le paiement,
                crée la session et redirige le client vers la page de paiement Stripe.
                 */
                var domain = "https://localhost:44319/";
                var options = new SessionCreateOptions
                {
                    PaymentMethodTypes = new List<string>
                {
                    "card",
                },
                    LineItems = new List<SessionLineItemOptions>(),

                    Mode = "payment",
                    SuccessUrl = domain + $"Customer/Cart/OrderConfirmation?id={ShoppingCartVM.OrderHeader.Id}",
                    CancelUrl = domain + $"Customer/Cart/index",
                };

                foreach (var item in ShoppingCartVM.ListCart)
                {
                    {
                        var sessionLineItem = new SessionLineItemOptions
                        {
                            PriceData = new SessionLineItemPriceDataOptions
                            {
                                UnitAmount = (long)(item.Price * 100), //10.00=1000
                                Currency = "CAD",
                                ProductData = new SessionLineItemPriceDataProductDataOptions
                                {
                                    Name = item.Product.Name
                                },
                            },
                            Quantity = item.Count,
                        };
                        options.LineItems.Add(sessionLineItem);
                    }
                }


                var service = new SessionService();
                Session session = service.Create(options);
                _unitOfWork.OrderHeader.UpdateStripePaymentID(ShoppingCartVM.OrderHeader.Id, session.Id, session.PaymentIntentId);
                _unitOfWork.Save();
                Response.Headers.Add("Location", session.Url);
                return new StatusCodeResult(303);
          

        }


        // La méthode OrderConfirmation a pour objectif de confirmer une commande passée.
        // Cette méthode prend un paramètre d'identifiant 'id' qui représente l'identifiant
        // de la commande à confirmer.

        // D'abord, la méthode récupère l'en-tête de commande correspondante à
        // l'identifiant passé en paramètre en utilisant l'objet UnitOfWork et
        // la méthode GetFirstOrDefault qui récupère la première occurrence de
        // la commande ayant un Id égal à l'identifiant passé en paramètre.

        // Ensuite, la méthode utilise l'API Stripe pour vérifier si le paiement
        // de la commande a été effectué avec succès. Pour cela, elle utilise un
        // objet SessionService pour récupérer la session correspondante à la commande.
        // Si le statut de paiement de la session est "paid", la méthode met à jour le
        // statut de la commande en "StatusApproved" en utilisant l'objet UnitOfWork et
        // la méthode UpdateStatus, puis sauvegarde les changements avec la méthode Save().

        // Enfin, la méthode récupère tous les éléments de panier associés à
        // l'utilisateur qui a passé la commande en utilisant l'objet UnitOfWork
        // et la méthode GetAll, puis supprime ces éléments de panier en utilisant
        // la méthode RemoveRange de l'objet UnitOfWork. Les changements sont sauvegardés
        // avec la méthode Save().

        // Enfin, la méthode renvoie une vue qui prend l'identifiant de la commande comme modèle
        public IActionResult OrderConfirmation(int id)
        {
            OrderHeader orderHeader = _unitOfWork.OrderHeader.GetFirstOrDefault(u => u.Id == id, includeProperties: "ApplicationUser");
            
                var service = new SessionService();
                Session session = service.Get(orderHeader.SessionId);

                //check the stripe status
                if (session.PaymentStatus.ToLower() == "paid")
                {
                    _unitOfWork.OrderHeader.UpdateStripePaymentID(id, orderHeader.SessionId, session.PaymentIntentId);
                    _unitOfWork.OrderHeader.UpdateStatus(id, SD.StatusApproved, SD.PaymentStatusApproved);
                    _unitOfWork.Save();
                }
          
            //envoyer un confirmation via courriel
            _emailSender.SendEmailAsync(orderHeader.ApplicationUser.Email, "New Order - Bulky Book", "<p>New Order Created</p>");

            List<ShoppingCart> shoppingCarts = _unitOfWork.ShoppingCart.GetAll
                (u => u.ApplicationUserId == orderHeader.ApplicationUserId).ToList();

            //clear sesssion pour ajuster le numero item dans sur l'icon de shoppingcart
            HttpContext.Session.Clear();

            _unitOfWork.ShoppingCart.RemoveRange(shoppingCarts);
            _unitOfWork.Save();
            return View(id);
        }
    }
}
