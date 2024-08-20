using DummyWebApp.Application.Common.Interfaces;
using DummyWebApp.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;


namespace DummyWebApp.Controllers
{
    [Authorize]
    public class VillaController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IWebHostEnvironment _environment;
        public VillaController(IUnitOfWork unitOfWork, IWebHostEnvironment environment)
        {
            _unitOfWork = unitOfWork;
            _environment = environment; 
        }
        public IActionResult Index()
        {
            IEnumerable<Villa> objVillaList = _unitOfWork.Villa.GetAll();
            return View(objVillaList);
        }
        //GET
        public IActionResult Create()
        {
            return View();
        }
        //POST
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(Villa obj)
        {
            if (ModelState.IsValid)
            {
                if(obj.Image!= null)
                {
                    string fileName = Guid.NewGuid().ToString()+Path.GetExtension(obj.Image.FileName);
                    string ImagePath = Path.Combine(_environment.WebRootPath,@"images/Villa");
                    using var fileStream = new FileStream(Path.Combine(ImagePath, fileName), FileMode.Create);
                    obj.Image.CopyTo(fileStream);
                    obj.ImageUrl = @"\images\Villa\" +fileName;
                }
                else
                {
                    obj.ImageUrl = "https://placehold.co/600x400";
                }
                _unitOfWork.Villa.Add(obj);
                _unitOfWork.Villa.Save();
                TempData["success"] = "Villa added successfully";
                return RedirectToAction("Index");
            }
            TempData["error"] = "Create operation was unsuccessful!";

            return View(obj);

        }

        //GET
        public IActionResult Edit(int? id)
        {
            if (id == null || id == 0)
            {
                return RedirectToAction("Error", "Home");
            }
            var villaFromDb = _unitOfWork.Villa.Get(u => u.Id == id);
            //var villaFromDbFirst
            if (villaFromDb == null) return RedirectToAction("Error", "Home");
            return View(villaFromDb);
        }
        //POST
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(Villa obj)
        {
            if (ModelState.IsValid)
            {
                if (obj.Image != null)
                {
                    string fileName = Guid.NewGuid().ToString() + Path.GetExtension(obj.Image.FileName);
                    string ImagePath = Path.Combine(_environment.WebRootPath, @"images/Villa");
                    if (!string.IsNullOrEmpty(obj.ImageUrl))
                    {
                        var oldImageUrl = Path.Combine(_environment.WebRootPath, obj.ImageUrl.TrimStart('/'));
                        if (System.IO.File.Exists(oldImageUrl))
                        {
                            System.IO.File.Delete(oldImageUrl);
                        }
                    }
                    
                    using var fileStream = new FileStream(Path.Combine(ImagePath, fileName), FileMode.Create);
                    obj.Image.CopyTo(fileStream);
                    obj.ImageUrl = @"\images\Villa\" + fileName;
                }
                _unitOfWork.Villa.Update(obj);
                _unitOfWork.Villa.Save();
                TempData["success"] = "Villa edited successfully";
                return RedirectToAction("Index");
            }
            TempData["error"] = "Edit operation was not successful!";
            return View(obj);
        }
        //GET
        public IActionResult Delete(int? id)
        {
            if (id == null || id == 0)
            {
                return NotFound();
            }
            var villaFromDb = _unitOfWork.Villa.Get(u => u.Id == id);
            //var villaFromDbFirst
            if (villaFromDb == null) return NotFound();
            return View(villaFromDb);
        }
        //POST
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Delete(Villa obj)
        {
            var existingVilla = _unitOfWork.Villa.Get(u => u.Id == obj.Id);
            if (existingVilla != null)
            {
                _unitOfWork.Villa.Remove(existingVilla);
                _unitOfWork.Villa.Save();
                TempData["success"] = "Villa deleted successfully";

                return RedirectToAction("Index");

            }
            TempData["error"] = "Delete operation was not successful!";

            return View();
        }
    }
}
