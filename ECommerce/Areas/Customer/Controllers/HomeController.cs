using ECommerce.DataAccess.Repository.IRepository;
using ECommerce.Models;
using ECommerce.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.Diagnostics;
using System.Security.Claims;
using X.PagedList;

namespace ECommerce.Controllers;
[Area("Customer")]


public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;
    private readonly IUnitOfWork _unitOfWork;

    public HomeController(ILogger<HomeController> logger, IUnitOfWork unitOfWork)
    {
        _logger = logger;
        _unitOfWork = unitOfWork;
    }

    public IActionResult Index(int? page)
    {
        int pageSize = 8;
        int pageNumber = page ?? 1;
        IPagedList<Product> productList = _unitOfWork.Product.GetAll(includeProperties: "Category").ToPagedList(pageNumber, pageSize);
        return View(productList);
    }

    [HttpGet]
    public IActionResult Search(string searchString, int? categoryId, int? page)
    {
        int pageSize = 8;
        int pageNumber = page ?? 1;
        IPagedList<Product> productList;

        if (categoryId.HasValue)
        {
            productList = _unitOfWork.Product.GetAll(
                filter: p => p.CategoryId == categoryId && (searchString.IsNullOrEmpty() || p.Name.Contains(searchString)),
                includeProperties: "Category"
            ).ToPagedList(pageNumber, pageSize);
        }
        else
        {
            productList = _unitOfWork.Product.GetAll(
                filter: p => searchString.IsNullOrEmpty() || p.Name.Contains(searchString),
                includeProperties: "Category"
            ).ToPagedList(pageNumber, pageSize);
        }

        return View("Index", productList);
    }




    public IActionResult Details(int productId)
    {
        ShoppingCart cartObj = new()
        {
            Count = 1,
            ProductId = productId,
            Product = _unitOfWork.Product.GetFirstOrDefault(u => u.Id == productId, includeProperties: "Category")
        };
        return View(cartObj);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize] //only authorize user peut utilisater la methode
    public IActionResult Details(ShoppingCart shoppingCart)
    {
        var claimsIdentity = (ClaimsIdentity)User.Identity;
        var claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);
        shoppingCart.ApplicationUserId = claim.Value;
        ShoppingCart cartFromDb = _unitOfWork.ShoppingCart.GetFirstOrDefault
            (u => u.ApplicationUserId == claim.Value && u.ProductId == shoppingCart.ProductId);

        if (cartFromDb == null)
        {
            _unitOfWork.ShoppingCart.Add(shoppingCart);
            _unitOfWork.Save();
            HttpContext.Session.SetInt32(SD.SessionCart,
                _unitOfWork.ShoppingCart.GetAll(u => u.ApplicationUserId == claim.Value).ToList().Count);
        }
        else
        {
            _unitOfWork.ShoppingCart.IncrementCount(cartFromDb, shoppingCart.Count);
            _unitOfWork.Save();
        }
        return RedirectToAction(nameof(Index));
    }

    public IActionResult Privacy()
    {
        return View();
    }

    public IActionResult About()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
