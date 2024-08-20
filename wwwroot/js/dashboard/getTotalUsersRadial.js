﻿$(document).ready(function () {
    loadTotalUsersRadialChart();
});
function loadTotalUsersRadialChart() {
    $(".chart-spinner").show();

    $.ajax({
        url: "/Dashboard/GetTotalUsersRadialChartData",
        type: 'GET',
        dataTypr: 'json',
        success: function (data) {
            document.querySelector("#spanTotalUserCount").innerHTML = data.totalCount;

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
            document.querySelector("#sectionUserCount").append(sectionCurrentCount);
            document.querySelector("#sectionUserCount").append("since last month");

            loadRadialBarChart("totalUsersRadialChart", data);
            $(".chart-spinner").hide();
        }
    })
}

