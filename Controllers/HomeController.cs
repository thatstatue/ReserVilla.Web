using DummyWebApp.Application.Common.Interfaces;
using DummyWebApp.Application.Utility;
using DummyWebApp.Models;
using DummyWebApp.ViewModels;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace DummyWebApp.Controllers
{
    public class HomeController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;

        public HomeController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public IActionResult Index()
        {
            HomeVM homeVm = new()
            {
                VillaList = _unitOfWork.Villa.GetAll(includeProperties: "VillaAmenity"),
                CheckInDate = DateOnly.FromDateTime(DateTime.Now),
                Nights = 1,

            };
            return View(homeVm);
       
        }

        [HttpPost]
        public IActionResult GetVillasBySearch(DateOnly checkInDate, int nights)
        {
            var villaList = _unitOfWork.Villa.GetAll(includeProperties: "VillaAmenity").ToList();
            var availables = SD.RoomsAvailable(null, _unitOfWork.Booking.GetAll(), _unitOfWork.VillaNumber.GetAll(),
                checkInDate, nights);
            foreach (var item in villaList)
            {
                if (!availables.Any(u => u.VillaId == item.Id))
                {
                    item.IsAvailable = false;
                }
            }
            HomeVM homeVM = new()
            {
                CheckInDate = checkInDate,
                VillaList = villaList,
                Nights = nights
            };
            return PartialView("/Views/Shared/_VillaList.cshtml", homeVM);
        } 

        public IActionResult Privacy()
        {
            return View();
        }

       // [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(/*new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier }*/);
        }
    }
}
