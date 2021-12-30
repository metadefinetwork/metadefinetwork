var HomeController = function () {
    this.initialize = function () {
        registerControls();
        registerEvents();
    }



    function registerControls() {
        chartMAR();
        chartMVR();
    }

    function registerEvents() {
    }

    function chartMAR() {
        am4core.useTheme(am4themes_animated);
        am4core.addLicense('ch-custom-attribution');
        am4core.options.autoSetClassName = true;

        var chart = am4core.create("chartdiv", am4charts.PieChart3D);
        chart.responsive.useDefault = false
        chart.responsive.enabled = true;
        chart.hiddenState.properties.opacity = 0; // this creates initial fade-in

        chart.data = [{
            txt: "AIRDROP",
            val: 1250000000,
            "color": am4core.color("#72757C")
        }, {
            txt: "CLAIM",
            val: 3000000000,
            "color": am4core.color("#D765CA")
        }, {
            txt: "PLAY GAME",
            val: 10750000000,
            "color": am4core.color("#F7E525")
        }, {
            txt: "TEAM",
            val: 5000000000,
            "color": am4core.color("#a367dc")
        }, {
            txt: "RESERVE BUDGET",
            val: 2500000000,
            "color": am4core.color("#2FC0EE")
        }, {
            txt: "STRATEGIC ADVISOR",
            val: 2500000000,
            "color": am4core.color("#D6323C")
        }
            //    , {
            //    txt: "LIQUIDITY",
            //    val: 7000000,
            //    "color": am4core.color("#72757C")
            //}, {
            //    txt: "GAME REWARD POOL",
            //    val: 41000000,
            //    "color": am4core.color("#FEA100")
            //    }
        ];

        chart.innerRadius = am4core.percent(40);
        chart.depth = 100;

        let custom_color_arr = [
            "#72757C",
            "#D765CA",
            "#F7E525",
            "#a367dc",
            "#2FC0EE",
            "#D6323C",
            //"#72757C",
            //"#FEA100"
        ] //custom color arr for chart legends

        var pieSeries = chart.series.push(new am4charts.PieSeries3D());
        pieSeries.dataFields.value = "val";
        pieSeries.dataFields.depthValue = "val";
        pieSeries.dataFields.category = "txt";
        pieSeries.slices.template.cornerRadius = 5;
        pieSeries.ticks.template.disabled = true;
        pieSeries.labels.template.fill = am4core.color("white");
        pieSeries.alignLabels = false;
        pieSeries.labels.template.text = "{value.percent.formatNumber('#.')}%";

        pieSeries.slices.template.propertyFields.fill = "color";

        // Create custom legend
        chart.events.on("ready", function (event) {
            // populate our custom legend when chart renders
            chart.customLegend = document.getElementById('legend');
            pieSeries.dataItems.each(function (row, i) {
                var color = custom_color_arr[i]
                var percent = Math.round(row.values.value.percent * 100) / 100;
                var value = numberWithCommas(row.value) + " MAR";
                legend.innerHTML += '<div class="legend-item" id="legend-item-' + i + ';" onmouseover="hoverSlice(' + i + ');" onmouseout="blurSlice(' + i + ');" style="color: ' + color + ';"><div class="legend-marker" style="background: ' + color + '"></div>' + row.category + '<div class="legend-value">' + value + ' | ' + percent + '%</div></div>';
            });
        });

        function numberWithCommas(num) {
            return num.toString().replace(/\B(?=(\d{3})+(?!\d))/g, ',');
        }

        function toggleSlice(item) {
            var slice = pieSeries.dataItems.getIndex(item);
            if (slice.visible) {
                slice.hide();
            } else {
                slice.show();
            }
        }

        function hoverSlice(item) {
            var slice = pieSeries.slices.getIndex(item);
            slice.isHover = true;
        }

        function blurSlice(item) {
            var slice = pieSeries.slices.getIndex(item);
            slice.isHover = false;

        }
    }

    function chartMVR() {
        am4core.useTheme(am4themes_animated);
        am4core.addLicense('ch-custom-attribution');
        am4core.options.autoSetClassName = true;

        var chart2 = am4core.create("chartdiv2", am4charts.PieChart3D);
        chart2.responsive.useDefault = false
        chart2.responsive.enabled = true;
        chart2.hiddenState.properties.opacity = 0; // this creates initial fade-in

        chart2.data = [{
            txt: "MARKETING",
            val: 18000000,
            "color": am4core.color("#D765CA")
        }, {
            txt: "PRIVATE SALE",
            val: 9000000,
            "color": am4core.color("#7dc627")
        }, {
            txt: "PUBLISH SALE",
            val: 18000000,
            "color": am4core.color("#FEA100")
        }, {
            txt: "GAME",
            val: 36000000,
            "color": am4core.color("#D6323C")
        }, {
            txt: "ECOSYSTEM GROWTH",
            val: 36000000,
            "color": am4core.color("#72757C")
        }, {
            txt: "STRATEGIC ADVISOR",
            val: 18000000,
            "color": am4core.color("#F7E525")
        }, {
            txt: "TEAM",
            val: 18000000,
            "color": am4core.color("#a367dc")
        }, {
            txt: "RESERVE BUDGET",
            val: 27000000,
            "color": am4core.color("#2FC0EE")
        }];

        chart2.innerRadius = am4core.percent(40);
        chart2.depth = 100;

        let custom_color_arr = [
            "#D765CA",
            "#7dc627",
            "#FEA100",
            "#D6323C",
            "#72757C",
            "#F7E525",
            "#a367dc",
            "#2FC0EE"
        ] //custom color arr for chart legends

        var pieSeries2 = chart2.series.push(new am4charts.PieSeries3D());
        pieSeries2.dataFields.value = "val";
        pieSeries2.dataFields.depthValue = "val";
        pieSeries2.dataFields.category = "txt";
        pieSeries2.slices.template.cornerRadius = 5;
        pieSeries2.ticks.template.disabled = true;
        pieSeries2.labels.template.fill = am4core.color("white");
        pieSeries2.alignLabels = false;
        pieSeries2.labels.template.text = "{value.percent.formatNumber('#.')}%";

        pieSeries2.slices.template.propertyFields.fill = "color";

        // Create custom legend
        chart2.events.on("ready", function (event) {
            // populate our custom legend when chart renders
            chart2.customLegend = document.getElementById('legend2');
            pieSeries2.dataItems.each(function (row, i) {
                var color = custom_color_arr[i]
                var percent = Math.round(row.values.value.percent * 100) / 100;
                var value = numberWithCommas(row.value) + " MAR";
                //legend2.innerHTML += '<div class="legend-item" id="legend-item-' + i + ';" onmouseover="hoverSlice2(' + i + ');" onmouseout="blurSlice2(' + i + ');" style="color: ' + color + ';"><div class="legend-marker" style="background: ' + color + '"></div>' + row.category + '<div class="legend-value">' + value + ' | ' + percent + '%</div></div>';
            });
        });

        function numberWithCommas(num) {
            return num.toString().replace(/\B(?=(\d{3})+(?!\d))/g, ',');
        }

        function toggleSlice(item) {
            var slice = pieSeries2.dataItems.getIndex(item);
            if (slice.visible) {
                slice.hide();
            } else {
                slice.show();
            }
        }

        function hoverSlice2(item) {
            var slice = pieSeries2.slices.getIndex(item);
            slice.isHover = true;
        }

        function blurSlice2(item) {
            var slice = pieSeries2.slices.getIndex(item);
            slice.isHover = false;

        }
    }


}