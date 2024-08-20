using DummyWebApp.Application.Common.Interfaces;
using DummyWebApp.DataAccess.Data;
using DummyWebApp.Domain.Entities;
using DummyWebApp.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace DummyWebApp.Controllers
{
    public class VillaNumberController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        public VillaNumberController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }
        public IActionResult Index()
        {
            IEnumerable<VillaNumber> objVillaNumberList = _unitOfWork.VillaNumber.GetAll(includeProperties :"Villa").ToList();
            return View(objVillaNumberList);
        }
        //GET
        public IActionResult Create()
        {
            VillaNumberVM vm = new()
            {
                VillaList = _unitOfWork.Villa.GetAll().Select(u => new SelectListItem
                {
                    Text = u.Name,
                    Value = u.Id.ToString(),
                })
            };

            return View(vm);
        }
        //POST
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(VillaNumberVM obj)
        {

            bool roomNumberExists = _unitOfWork.VillaNumber.Any(u => u.Villa_Number == obj.VillaNumber.Villa_Number);
      
            if (ModelState.IsValid && !roomNumberExists)
            {
                _unitOfWork.VillaNumber.Add(obj.VillaNumber);
                _unitOfWork.VillaNumber.Save();
                TempData["success"] = "Villa Number added successfully";
                return RedirectToAction("Index");
            }
            if(roomNumberExists) TempData["error"] = "Villa Number already exists!";
            else TempData["error"] = "Create operation was unsuccessful!";
          
            return View(obj);

        }

        //GET
        public IActionResult Edit(int? id)
        {
            if (id == null || id == 0)
            {
                return RedirectToAction("Error", "Home");
            }
            VillaNumberVM vm = new()
            {
                VillaList = _unitOfWork.Villa.GetAll().Select(u => new SelectListItem
                {
                    Text = u.Name,
                    Value = u.Id.ToString(),
                }),
                VillaNumber = _unitOfWork.VillaNumber.Get(u => u.Villa_Number == id)
            };

            if (vm.VillaNumber == null) return RedirectToAction("Error", "Home");
            return View(vm);
        }
        //POST
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(VillaNumberVM obj)
        {
           
            if (ModelState.IsValid)
            {
                _unitOfWork.VillaNumber.Update(obj.VillaNumber);
                _unitOfWork.VillaNumber.Save();
                TempData["success"] = "Villa Number edited successfully";
                return RedirectToAction("Index");
            }
            else TempData["error"] = "Edit operation was unsuccessful!";
            return View(obj);
        }
        //GET
        public IActionResult Delete(int? id)
        {
            if (id == null || id == 0)
            {
                return RedirectToAction("Error", "Home");
            }
            VillaNumberVM vm = new()
            {
                VillaList = _unitOfWork.Villa.GetAll().Select(u => new SelectListItem
                {
                    Text = u.Name,
                    Value = u.Id.ToString(),
                }),
                VillaNumber = _unitOfWork.VillaNumber.Get(u => u.Villa_Number == id)
            };

            if (vm.VillaNumber == null) return RedirectToAction("Error", "Home");
            return View(vm);
        }
        //POST
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Delete(VillaNumberVM obj)
        {
            if(obj.VillaNumber!= null) {
                _unitOfWork.VillaNumber.Remove(obj.VillaNumber);
                _unitOfWork.VillaNumber.Save();
                TempData["success"] = "Villa Number deleted successfully";
                return RedirectToAction("Index");
            }
            else TempData["error"] = "Delete operation was unsuccessful!";
            return View(obj);
        }
    }
}
