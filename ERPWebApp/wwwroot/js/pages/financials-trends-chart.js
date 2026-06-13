function renderColumnChart(chartData, days,department) {
	const departmentNames = Array.from(
		new Set(chartData.flatMap((data) => Object.keys(data.departmentsOrders)))
	);
	const uniqueDates = chartData.map((data) => new Date(data.date).toISOString()).sort();

	// Set dataLabelsEnabled to false if the date range is greater than 30 days  
	const dataLabelsEnabled = days <= 30;  

	const departmentColors = {
		Electroplating: "#c0c0c0", //Silver
		Embroidery: "#ffbc00", //Yellow
		Engraving: "#727cf5", //Medium Light Blue
		Metal: "#0acf97", //Green Cyan
		UVP: "#FA5C7C", //Pink-Red
		Unknown: "#6c757d", //Cyan Blue
		//Didn't find specific colors for the following deps, so setting these manually for now.
		Plants: "#3cb371", //Medium Sea Green
		Sublimation: "#b0c4de", // Light Steel Blue (Off white?)
		Wood: "#ffe4c4", //Bisque Brown
		Fulfillment: "#ee82ee", //Violet
	};

	const series = departmentNames.map((departmentName) => {
		const data = chartData.map(
			(dateData) => dateData.departmentsOrders[departmentName] || 0
		);
		return {
			name: departmentName,
			data: data,
			color: departmentColors[departmentName] || getRandomColor(),
		};
	});
	const totalOrdersByDay = chartData.map((dateData) => {
		return Object.values(dateData.departmentsOrders).reduce(
			(totalOrders, orders) => totalOrders + orders
		);
	});  

	function getRandomColor() {
		const letters = "0123456789ABCDEF";
		let color = "#";
		for (let i = 0; i < 6; i++) {
			color += letters[Math.floor(Math.random() * 16)];
		}
		return color;
	} 
	const seriesColors = series.map((s) => departmentColors[s.name]);  
	const columnOptions = {
		series: series,
		chart: {
			type: "bar",
			toolbar: {
				show: true,
				tools: {
					download: true,
					selection: false,
					zoom: false,
					zoomin: false,
					zoomout: false,
					pan: false,
					reset: false 
				},
				export: {
					svg: {
						filename: "historical_trends_chart",
					},
					png: {
						filename: "historical_trends_chart",
					},
				},
			},
			height: 350,
			stacked: true,
		},
		plotOptions: {
			bar: {
				horizontal: false,
			},
		},
		xaxis: {
			type: "datetime",
			categories: uniqueDates,
		},
		yaxis: {
			title: {
				text: "Number of Orders",
			},
		},
		legend: {
			position: "top",
			horizontalAlign: "left",
		},
		tooltip: {
			y: {
				formatter: function (value, { seriesIndex, dataPointIndex, w }) {
					const departmentName = w.config.series[seriesIndex].name;
					const orders = w.config.series[seriesIndex].data[dataPointIndex];
					const productProfit = chartData[dataPointIndex].departmentsProductProfit[departmentName] || 0;
                    
					return (
						"Orders: " +
                        orders +
                        "<br>" +
                        departmentName + 
                        ":  $" +
                        productProfit.toFixed(2)
					);
				},
				title: {
					formatter: function (seriesName) {
						return "";
					},
				},
			},
			x: {
				show: true,
				formatter: function (value, { dataPointIndex, w }) {
					const totalOrders = totalOrdersByDay[dataPointIndex];
					const date = new Date(value);

					const monthNames = ["Jan", "Feb", "Mar", "Apr", "May", "Jun", "Jul", "Aug", "Sep", "Oct", "Nov", "Dec"];
					const day = date.getDate();
					const month = monthNames[date.getMonth()];
					const formattedDate = `${day} ${month}`;

					return formattedDate + " - Total Orders: " + totalOrders;
				},
			},  


		},  


		dataLabels: {
			enabled: dataLabelsEnabled,
		},
		responsive: [
			{
				breakpoint: 1500,
				options: {
					dataLabels: {
						enabled: false,

					},
				},
			},
		],
	};

	const chart = new ApexCharts(document.querySelector("#column-chart-historical-trends"), columnOptions);
	chart.render().then(() => {
		if (!isTrendsInitialized) {
			chart.updateOptions({});
			isTrendsInitialized = true;
		}
	});
}  
