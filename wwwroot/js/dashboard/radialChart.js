function loadRadialBarChart(id, data) {
    var chartColors = getChartColorsArray(id);

    var options = {
        series: data.series,
        chart: {
            height: 180,
            width: 150,
            type: 'radialBar',
            sparkLine: {
                enabled: true
            },
            offset: -10,
        },
        plotOptions: {
            radialBar: {
                hollow: {
                    size: '60%',
                }
            },
        },
        labels: ['Progress'],
        colors: chartColors,
    };

    var chart = new ApexCharts(document.querySelector("#" + id), options);
    chart.render();
}
function getChartColorsArray(id) {
    if (document.getElementById(id) !== null) {
        var colors = document.getElementById(id).getAttribute("data-colors");
        if (colors) {
            colors = JSON.parse(colors);
            return colors.map(function (value) {
                var newValue = value.replace(" ", "");
                if (newValue.indexOf(",") === -1) {
                    var color = getComputedStyle(document.documentElement).getPropertyValue(newValue);
                    if (color) return color;
                    else return newValue;
                }
            });
        }

    }
}