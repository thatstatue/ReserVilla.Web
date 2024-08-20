$(document).ready(function () {
    loadTotalRevenueRadialChart();
});
function loadTotalRevenueRadialChart() {
    $(".chart-spinner").show();

    $.ajax({
        url: "/Dashboard/GetTotalRevenueRadialChartData",
        type: 'GET',
        dataTypr: 'json',
        success: function (data) {
            document.querySelector("#spanTotalRevenueCount").innerHTML = data.totalCount;

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
            document.querySelector("#sectionRevenueCount").append(sectionCurrentCount);
            document.querySelector("#sectionRevenueCount").append("since last month");

            loadRadialBarChart("totalRevenueRadialChart", data);
            $(".chart-spinner").hide();
        }
    })
}

