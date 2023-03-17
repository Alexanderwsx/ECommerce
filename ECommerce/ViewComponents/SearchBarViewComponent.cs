using ECommerce.DataAccess.Repository.IRepository;
using Microsoft.AspNetCore.Mvc;

namespace ECommerce.ViewComponents
{

    public class SearchBarViewComponent : ViewComponent
    {
        private readonly IUnitOfWork _unitOfWork;

        public SearchBarViewComponent(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            var categories = _unitOfWork.Category.GetAll();
            return View(categories);
        }
    }
}