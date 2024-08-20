using DummyWebApp.Application.Common.Interfaces;
using DummyWebApp.Application.Utility;
using DummyWebApp.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace DummyWebApp.Controllers
{
    public class DashboardController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        readonly DateTime previousMonthStartDate = new(DateTime.Now.Year, DateTime.Now.Month - 1, 1);
        readonly DateTime currentMonthStartDate = new(DateTime.Now.Year, DateTime.Now.Month, 1);

        public DashboardController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public IActionResult Index()
        {
            return View();
        }
        public IActionResult GetTotalBookingsRadialChartData()
        {
            var totalBookings = _unitOfWork.Booking.GetAll(u => u.Status != SD.StatusPending && u.Status != SD.StatusCancelled);
            var countByCurrentMonth = totalBookings.Count(u => u.BookingDate >= currentMonthStartDate && u.BookingDate <= DateTime.Now);
            var countByPreviousMonth = totalBookings.Count(u => u.BookingDate >= previousMonthStartDate && u.BookingDate <= currentMonthStartDate);
            RadialChartVM radialChartVM = new RadialChartVM();
            radialChartVM.HasRatioIncreased = countByCurrentMonth > countByPreviousMonth;
            radialChartVM.TotalCount = totalBookings.Count();
            radialChartVM.IncreaseDecreaseAmount = countByCurrentMonth - countByPreviousMonth;
            int increaseDecreaseRatio = 100;
            if (countByPreviousMonth > 0)
            {
                increaseDecreaseRatio = (int)(radialChartVM.IncreaseDecreaseAmount * 100 / countByPreviousMonth);
            }
            radialChartVM.Series = [increaseDecreaseRatio];
            return Json(radialChartVM);
        }
        public IActionResult GetTotalUsersRadialChartData()
        {
            var totalRegisters = _unitOfWork.ApplicationUser.GetAll();
            var countByCurrentMonth = totalRegisters.Count(u => u.CreatedAt >= currentMonthStartDate && u.CreatedAt <= DateTime.Now);
            var countByPreviousMonth = totalRegisters.Count(u => u.CreatedAt >= previousMonthStartDate && u.CreatedAt <= currentMonthStartDate);
            RadialChartVM radialChartVM = new RadialChartVM();
            radialChartVM.HasRatioIncreased = countByCurrentMonth > countByPreviousMonth;
            radialChartVM.TotalCount = totalRegisters.Count();
            radialChartVM.IncreaseDecreaseAmount = countByCurrentMonth - countByPreviousMonth;
            int increaseDecreaseRatio = 100;
            if (countByPreviousMonth > 0)
            {
                increaseDecreaseRatio = (int)(radialChartVM.IncreaseDecreaseAmount * 100 / countByPreviousMonth);
            }
            radialChartVM.Series = [increaseDecreaseRatio];
            return Json(radialChartVM);
        }

        public IActionResult GetTotalRevenueRadialChartData()
        {
            var totalBookings = _unitOfWork.Booking.GetAll(u => u.Status != SD.StatusPending && u.Status != SD.StatusCancelled);
            double totalVeneue = 0;
            double cuurentMonthVenue = 0;
            double previousMonthVenue = 0;

            foreach (var booking in totalBookings)
            {
                totalVeneue += booking.TotalCost;
                if (booking.BookingDate >= currentMonthStartDate
                    && booking.BookingDate <= DateTime.Now)
                {
                    cuurentMonthVenue += booking.TotalCost;
                }
                else if (booking.BookingDate >= previousMonthStartDate
                    && booking.BookingDate <= currentMonthStartDate)
                {
                    previousMonthVenue += booking.TotalCost;
                }
            }

            RadialChartVM radialChartVM = new RadialChartVM();
            radialChartVM.HasRatioIncreased = cuurentMonthVenue > previousMonthVenue;
            radialChartVM.TotalCount = (decimal)totalVeneue;
            radialChartVM.IncreaseDecreaseAmount = (decimal)(cuurentMonthVenue - previousMonthVenue);
            int increaseDecreaseRatio = 100;
            if (previousMonthVenue > 0)
            {
                increaseDecreaseRatio = Convert.ToInt32(radialChartVM.IncreaseDecreaseAmount) * 100 /
                    Convert.ToInt32(previousMonthVenue);
            }
            radialChartVM.Series = [increaseDecreaseRatio];
            return Json(radialChartVM);
        }
        public IActionResult GetCustomerBookingsPieChartData()
        {
            var totalBookings = _unitOfWork.Booking.GetAll(u => u.BookingDate >= DateTime.Now.AddDays(-30)
            && u.Status != SD.StatusPending && u.Status != SD.StatusCancelled);
            var newBookingsCount = totalBookings.GroupBy(u => u.UserId).Where(x=> x.Count() == 1).Select(x => x.Key).Count();

            decimal newBookingsPercentage = newBookingsCount* 100 / totalBookings.Count() ;
            decimal returningPercentage = 100 - newBookingsPercentage;
            PieChartVM pieChartVM = new PieChartVM();
            pieChartVM.Labels = ["New Customer", "returningCustomer"];
            pieChartVM.Series = [newBookingsPercentage, returningPercentage];

            return Json(pieChartVM);
        }
        public IActionResult GetMembersAndBookingsLineChartData()
        {
            var totalBookings = _unitOfWork.Booking.GetAll(u => u.BookingDate >= DateTime.Now.AddDays(-30)
            && u.Status != SD.StatusPending && u.Status != SD.StatusCancelled);
            var bookingsData = totalBookings.GroupBy(u => u.BookingDate.Date)
                .Select(u => new
                {
                    DateTime = u.Key,
                    NewBookingCount = u.Count(),
                });
            var totalCustomers = _unitOfWork.ApplicationUser.GetAll(u => u.CreatedAt >= DateTime.Now.AddDays(-30));
            var customersData = totalCustomers.GroupBy(u => u.CreatedAt.Date)
               .Select(u => new
                {
                    DateTime = u.Key,
                    NewCustomerCount = u.Count(),
                });

            var leftJoin = bookingsData.GroupJoin(
                customersData,
                booking => booking.DateTime,
                customer => customer.DateTime,
                (booking, customer) => new {
                booking.DateTime,
                booking.NewBookingCount,
                NewCustomerCount = customer.Select(u => u.NewCustomerCount).FirstOrDefault()
                });
            var rightJoin = customersData.GroupJoin(
                bookingsData,
                customer => customer.DateTime,
                booking => booking.DateTime,
                (customer, booking) => new {
                    customer.DateTime,
                    NewBookingCount = booking.Select(u => u.NewBookingCount).FirstOrDefault(),
                    customer.NewCustomerCount
                });

            var mergedData = leftJoin.Union(rightJoin).OrderBy(u =>u.DateTime).ToList();
            var newBookingsData = mergedData.Select(u=>u.NewBookingCount).ToArray();
            var newCustomersData = mergedData.Select(u => u.NewCustomerCount).ToArray();

            var categories = mergedData.Select(u=> u.DateTime.ToString("MM/dd/yyyy")).ToArray();
            List<ChartData> chartData = new()
            {
                new ChartData
                {
                    Name = "New Bookings",
                    Data = newBookingsData
                },
                new ChartData
                {
                    Name = "New Customers",
                    Data = newCustomersData
                },
            };
            LineChartVM chartVM = new()
            {
                Categories = categories,
                Series = chartData
            };
            return Json(chartVM);
        }
    }
}
