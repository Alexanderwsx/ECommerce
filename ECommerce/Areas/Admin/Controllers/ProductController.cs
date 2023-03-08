using ECommerce.DataAccess.Repository.IRepository;
using ECommerce.Models.ViewModels;
using ECommerce.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace ECommerce.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = SD.Role_Admin)]

    public class ProductController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IWebHostEnvironment _hostEnvironment;

        public ProductController(IUnitOfWork unitOfWork, IWebHostEnvironment hostEnvironment)
        {
            _unitOfWork = unitOfWork;
            _hostEnvironment = hostEnvironment;
        }

        // Affiche la liste de toutes les Products
        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Upsert(int? id)
        {
            ProductVM productVM = new()
            {
                Product = new(),
                CategoryList = _unitOfWork.Category.GetAll().Select
                (u => new SelectListItem
                {
                    Text = u.Name,
                    Value = u.Id.ToString()
                })
            };

            if (id == null || id == 0)
            {
                return View(productVM);
            }
            else
            {
                productVM.Product = _unitOfWork.Product.GetFirstOrDefault(u => u.Id == id);
                return View(productVM);
            }
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        //action de contrôleur qui est appelée lors de la soumission du formulaire
        //pour créer ou mettre à jour un produit. 
        public IActionResult Upsert(ProductVM obj, IFormFile? file)
        {
            if (ModelState.IsValid) // Vérifie que le modèle est valide
            {
                string wwwRootPatch = _hostEnvironment.WebRootPath; // Récupère le chemin du dossier web racine
                if (file != null) // Vérifie qu'un fichier est bien envoyé
                {
                    string fileName = Guid.NewGuid().ToString(); // Génère un nouveau nom de fichier unique
                    var uploads = Path.Combine(wwwRootPatch, @"images\products"); // Chemin du dossier où stocker les images
                    var extension = Path.GetExtension(file.FileName); // Récupère l'extension du fichier

                    if (obj.Product.ImageUrl != null) // Vérifie si l'objet contient une URL d'image déjà existante
                    {
                        var oldImagePath = Path.Combine(wwwRootPatch, obj.Product.ImageUrl.TrimStart('\\')); // Récupère le chemin de l'ancienne image
                        if (System.IO.File.Exists(oldImagePath)) // Vérifie que le fichier existe
                        {
                            System.IO.File.Delete(oldImagePath); // Supprime l'ancienne image
                        }
                    }

                    using (var fileStreams = new FileStream(Path.Combine(uploads, fileName + extension), FileMode.Create)) // Crée un nouveau fichier
                    {
                        file.CopyTo(fileStreams); // Copie le contenu du fichier envoyé dans le nouveau fichier
                    }
                    obj.Product.ImageUrl = @"\images\products\" + fileName + extension; // Met à jour l'URL de l'image dans l'objet ProductVM
                }

                if (obj.Product.Id == 0) // Vérifie si l'objet contient un nouvel enregistrement ou une mise à jour
                {
                    _unitOfWork.Product.Add(obj.Product); // Ajoute le nouvel enregistrement dans la base de données
                    TempData["success"] = "Produit ajouté"; //make a notification when success

                }
                else
                {
                    _unitOfWork.Product.Update(obj.Product); // Met à jour l'enregistrement dans la base de données
                    TempData["success"] = "Produit modifié"; //make a notification when success

                }

                _unitOfWork.Save(); // Enregistre les modifications dans la base de données
                return RedirectToAction("Index"); // Redirige vers la page d'accueil des produits
            }
            return View(obj); // Retourne la vue Upsert avec l'objet ProductVM en cas de modèle invalide
        }


        #region API CALLS
        [HttpGet]
        public IActionResult GetAll()
        {
            var productList = _unitOfWork.Product.GetAll(includeProperties: "Category");
            return Json(new { data = productList });
        }

        [HttpDelete]
        public IActionResult Delete(int id)
        {
            var obj = _unitOfWork.Product.GetFirstOrDefault(x => x.Id == id);
            if (obj == null)
            {
                return Json(new { success = false, message = "Erreur lors de la suppression" });
            }

            var oldImagePath = Path.Combine(_hostEnvironment.WebRootPath, obj.ImageUrl.TrimStart('\\'));
            if (System.IO.File.Exists(oldImagePath))
            {
                System.IO.File.Delete(oldImagePath);
            }

            _unitOfWork.Product.Remove(obj);
            _unitOfWork.Save();
            return Json(new { success = true, message = "Supression Reussi" });
        }
        #endregion
    }

}