$(document).ready(function () {
    loadTotalBookingsRadialChart();
});
function loadTotalBookingsRadialChart() {
    $(".chart-spinner").show();

    $.ajax({
        url: "/Dashboard/GetTotalBookingsRadialChartData",
        type: 'GET',
        dataTypr: 'json',
        success: function (data) {
            document.querySelector("#spanTotalBookingsCount").innerHTML = data.totalCount;

            var sectionCurrentCount = document.createElement("span");
            if (data.hasRatioIncreased) {
                sectionCurrentCount.className = "text-success me-1";
                sectionCurrentCount.innerHTML = '<i class="bi bi-arrow-up-right-circle-fill"></i> <span>' + data.increaseDecreaseAmount +
                    '</span>';
            } else {
                sectionCurrentCount.className = "text-danger me-1";
                sectionCurrentCount.innerHTML = '<i class="bi bi-arrow-down-right-circle-fill"></i> <span>' + data.increaseDecreaseAmount +
                    '</span>';
            }
            document.querySelector("#sectionBookingCount").append(sectionCurrentCount);
            document.querySelector("#sectionBookingCount").append("since last month");

            loadRadialBarChart("totalBookingsRadialChart", data);
            $(".chart-spinner").hide();
        }
    })
}

