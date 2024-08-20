using DummyWebApp.Application.Common.Interfaces;
using DummyWebApp.Application.Utility;
using DummyWebApp.Domain.Entities;
using DummyWebApp.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;

namespace DummyWebApp.Controllers
{
    [Authorize(Roles = SD.Role_Admin)]
    public class AmenityController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        public AmenityController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public IActionResult Index()
        {
            IEnumerable<Amenity> amenitiesList = _unitOfWork.Amenity.GetAll(includeProperties: "Villa").ToList();
            return View(amenitiesList);
        }
        private int generateId()
        {
            int id = 0;
            while(_unitOfWork.Amenity.GetAll().Any(u => u.Id == id)){
                id++;
            }
            return id;
        }

        //GET
        public IActionResult Create()
        {
            AmenityVM vm = new()
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
        public IActionResult Create(AmenityVM amenityVM)
        {
            if (ModelState.IsValid)
            {
                amenityVM.Amenity.Id = generateId();
                _unitOfWork.Amenity.Add(amenityVM.Amenity);
                _unitOfWork.Amenity.Save();

                TempData["success"] = "Amenity added successfully";
                return RedirectToAction("Index");
            }
            TempData["error"] = "Create operation was unsuccessful!";
            return View(amenityVM);
        }

        //GET
        public IActionResult Edit(int? id)
        {
            AmenityVM vm = new()
            {
                VillaList = _unitOfWork.Villa.GetAll().Select(u => new SelectListItem
                {
                    Text = u.Name,
                    Value = u.Id.ToString(),
                }),
                Amenity = _unitOfWork.Amenity.Get(u => u.Id == id)
            };

            if (vm.Amenity == null) return RedirectToAction("Error", "Home");
            return View(vm);
        }
        //POST

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(AmenityVM amenityVM)
        {
            if (ModelState.IsValid)
            {
                _unitOfWork.Amenity.Update(amenityVM.Amenity);
                _unitOfWork.Amenity.Save();

                TempData["success"] = "Amenity updated successfully";
                return RedirectToAction("Index");
            }
            TempData["error"] = "Update operation was unsuccessful!";
            return View(amenityVM);
        }
        //GET
        public IActionResult Delete(int? id)
        {
            if (id == null || id == 0)
            {
                return RedirectToAction("Error", "Home");
            }
            AmenityVM vm = new()
            {
                VillaList = _unitOfWork.Villa.GetAll().Select(u => new SelectListItem
                {
                    Text = u.Name,
                    Value = u.Id.ToString(),
                }),
                Amenity = _unitOfWork.Amenity.Get(u => u.VillaId == id)
            };

            if (vm.Amenity == null) return RedirectToAction("Error", "Home");
            return View(vm);
        }
        //POST
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Delete(AmenityVM vm)
        {
            if (vm != null)
            {
                _unitOfWork.Amenity.Remove(vm.Amenity);
                _unitOfWork.Amenity.Save();
                TempData["success"] = "Amenity deleted successfully";
                return RedirectToAction("Index");
            }
            TempData["error"] = "Delete operation was unsuccessful!";
            return View(vm);
        }
    }
}
