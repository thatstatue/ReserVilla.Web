using DummyWebApp.Application.Common.Interfaces;
using DummyWebApp.Application.Utility;
using DummyWebApp.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Stripe;
using Stripe.Checkout;
using System.Collections.Generic;
using System.Security.Claims;

namespace DummyWebApp.Controllers
{
    [Authorize]
    public class BookingController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        public BookingController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }
        public IActionResult Index(string status)
        {
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var userId = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier).Value;
            var user = _unitOfWork.ApplicationUser.Get(u => u.Id == userId);

            IEnumerable < Booking > bookings;

            if (User.IsInRole(SD.Role_Admin)){
                bookings = _unitOfWork.Booking.GetAll();
            }else bookings = _unitOfWork.Booking.GetAll(u=> u.UserId == userId);

            if (!string.IsNullOrEmpty(status))
            {
                bookings = bookings.Where(u => u.Status.ToLower() == status.ToLower()); 
            }
            return View(bookings);
        }
        public IActionResult FinalizeBooking(int villaId, DateOnly checkInDate, int nights)
        {
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var userId = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier).Value;
            var user = _unitOfWork.ApplicationUser.Get(u => u.Id == userId);
            Booking booking = new()
            {
                UserId = userId,
                User = user,
                Name = user.Name,
                Email = user.Email,
                Phone = user.PhoneNumber,

                VillaId = villaId,
                Villa = _unitOfWork.Villa.Get(u => u.Id == villaId, includeProperties: "VillaAmenity"),
                CheckInDate = checkInDate,
                Nights = nights,
                CheckOutDate = checkInDate.AddDays(nights)

            };
            var availables = SD.RoomsAvailable(villaId, _unitOfWork.Booking.GetAll(), 
                _unitOfWork.VillaNumber.GetAll(), checkInDate, nights);
            if (availables== null || availables.Count()==0)
            {
                return RedirectToAction("Error", "Home");
            }
            booking.Villa_Number = availables.First().Villa_Number;
            booking.TotalCost = booking.Nights * booking.Villa.Price;
            return View(booking);
        }
        [HttpPost]

        public IActionResult FinalizeBooking(Booking booking)
        {
            Villa villa = _unitOfWork.Villa.Get(u => u.Id == booking.VillaId);
            booking.BookingDate = DateTime.Now;
            booking.CheckOutDate = booking.CheckInDate.AddDays(booking.Nights);
            booking.TotalCost = villa.Price * booking.Nights;
            //booking.Villa = villa;
            booking.Status = SD.StatusPending;
            _unitOfWork.Booking.Add(booking);
            _unitOfWork.Booking.Save();

            return CreateSessionService(booking, villa);
            //return RedirectToAction(nameof(BookingConfirmation), new { bookingId = booking.Id});
        }
        public IActionResult BookingConfirmation(int bookingId)
        {
            var booking = _unitOfWork.Booking.Get(u => u.Id == bookingId);
            if (string.IsNullOrEmpty(booking.Status) || booking.Status == SD.StatusPending)
            {
                var service = new SessionService();
                Session session = service.Get(booking.StripeSessionId);
                if (session.PaymentStatus == "paid")
                {
                    _unitOfWork.Booking.UpdateStatus(bookingId, SD.StatusApproved);
                    _unitOfWork.Booking.UpdatePaymentStatus(bookingId, session.Id, session.PaymentIntentId);
                    _unitOfWork.Booking.Save();
                }
            }
            return View(bookingId);
        }
        public IActionResult BookingDetails(int bookingId)
        {
            Booking bookingFromDb = _unitOfWork.Booking.Get(u => u.Id == bookingId,
            includeProperties: "User,Villa");
            var villa = _unitOfWork.Villa.Get(u => u.Id == bookingFromDb.VillaId, includeProperties: "VillaAmenity");
            bookingFromDb.Villa = villa;

            return View(bookingFromDb);
        }

        [HttpPost]
        [Authorize(Roles = SD.Role_Admin)]
        public IActionResult CheckIn(int bookingId)
        {
            _unitOfWork.Booking.UpdateStatus(bookingId, SD.StatusCheckedIn);
            _unitOfWork.Booking.Save();
            TempData["success"] = "Booking state is modified to Checked-in";
            return RedirectToAction(nameof(BookingDetails), new { bookingId });
        }
        [HttpPost]
        [Authorize(Roles = SD.Role_Admin)]
        public IActionResult CheckOut(int bookingId)
        {
            _unitOfWork.Booking.UpdateStatus(bookingId, SD.StatusCompleted);
            _unitOfWork.Booking.Save();
            TempData["success"] = "Booking state is modified to Completed";
            return RedirectToAction("Index", "Booking");
        }

        [HttpPost]
        [Authorize(Roles = SD.Role_Admin)]
        public IActionResult Cancel(int bookingId)
        {
            _unitOfWork.Booking.UpdateStatus(bookingId, SD.StatusCancelled);
            _unitOfWork.Booking.Save();
            TempData["success"] = "Booking state is modified to Cancelled";
            return RedirectToAction("Index", "Booking");
        }
        private IStatusCodeActionResult CreateSessionService(Booking booking, Villa villa)
        {
            try
            {
                var domain = Request.Scheme + "://" + Request.Host.Value + "/";
                var options = new SessionCreateOptions
                {
                    LineItems = new List<SessionLineItemOptions>(),
                    Mode = "payment",
                    SuccessUrl = domain + $"booking/BookingConfirmation?bookingId={booking.Id}",
                    CancelUrl = domain + $"booking/FinalizeBooking?villaId={booking.VillaId}&checkInDate={booking.CheckInDate}&nights={booking.Nights}"
                };

                options.LineItems.Add(new SessionLineItemOptions
                {
                    PriceData = new SessionLineItemPriceDataOptions
                    {
                        UnitAmount = (long)(booking.TotalCost * 100),
                        Currency = "aed",
                        ProductData = new SessionLineItemPriceDataProductDataOptions
                        {
                            Name = villa.Name
                        }
                    },
                    Quantity = 1,
                });

                var service = new SessionService();

                Session session = service.Create(options); // <<<<the line of exception

                _unitOfWork.Booking.UpdatePaymentStatus(booking.Id, session.Id, session.PaymentIntentId);
                _unitOfWork.Booking.Save();

                Response.Headers.Append("Location", session.Url);
                return new StatusCodeResult(303);
            }
            catch (StripeException ex)
            {
                // Log Stripe-specific exceptions
                Console.WriteLine(ex.StackTrace, "Stripe error occurred while creating session.");
                return StatusCode(500, "An error occurred while processing the payment.");
            }
            catch (HttpRequestException ex)
            {
                // Log network-related exceptions
                Console.WriteLine(ex.StackTrace, "Network error occurred while creating session.");
                return StatusCode(500, "A network error occurred. Please try again later.");
            }
            catch (Exception ex)
            {
                // Log other exceptions
                Console.WriteLine(ex.StackTrace, "An unexpected error occurred while creating session.");
                return StatusCode(500, "An unexpected error occurred. Please try again later.");
            }
        }

    }
    }
