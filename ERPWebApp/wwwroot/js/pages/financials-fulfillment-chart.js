/* eslint-disable no-mixed-spaces-and-tabs */
/* eslint-disable no-unused-vars */
function renderPieChart(departmentData, department) {
	departmentData = departmentData.filter((d) => d.DepartmentName !== null);  
	if (departmentData.length === 0) {
		document.getElementById("no-data-message").style.display = "block";
		return;
	}

	let selectedDepartment = null;
	let lastClickedDataPointIndex = null;

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
	function getRandomColor() {
		const letters = "0123456789ABCDEF";
		let color = "#";
		for (let i = 0; i < 6; i++) {
			color += letters[Math.floor(Math.random() * 16)];
		}
		return color;
	}

	function updateTooltipPosition(event) {
		const tooltip = document.getElementById("custom-tooltip");
		if (tooltip.style.visibility === "visible") {
			tooltip.style.left = `${event.clientX}px`;
			tooltip.style.top = `${event.clientY}px`;
		}
	}

	const departmentNames = departmentData.map((d) => d.DepartmentName);
	//Also implemented a fall-back color in case one isn't setup.
	const chartColors = departmentData.map((d) => departmentColors[d.DepartmentName] || getRandomColor());

	const pieOptions = {
		series: departmentData.map((d) => d.ProductProfit),
		chart: {
			id: "fulfillment",
			type: "pie",
			toolbar: {
				show: true,
				tools: {
					download: true
				},
				export: {
					svg: {
						filename: "fulfillment_totals_chart",
					},
					png: {
						filename: "fulfillment_totals_chart",
					},
				},
			},
			events: {
				dataPointSelection: function (event, chartContext, config) {
					const chartContainer = chartContext.el.parentNode;
					const tooltip = document.getElementById("custom-tooltip");
					const departmentCost = departmentData[config.dataPointIndex].ProductProfit;

					chartContainer.addEventListener("mousemove", (e) => {
						if (tooltip.style.visibility === "visible") {
							const containerRect = chartContainer.getBoundingClientRect();
							tooltip.style.left = (e.clientX - containerRect.left + 10) + "px";
							tooltip.style.top = (e.clientY - containerRect.top + 10) + "px";
						}
					});

					if (config.selectedDataPoints[0].length) {
						const selectedIndex = config.selectedDataPoints[0][0];

						selectedDepartment = departmentData[selectedIndex];
						lastClickedDataPointIndex = selectedIndex;
						const { clientX, clientY } = event;
						const containerRect = chartContainer.getBoundingClientRect();
						tooltip.style.left = (clientX - containerRect.left + 10) + "px";
						tooltip.style.top = (clientY - containerRect.top + 10) + "px";

						const departmentName = departmentNames[selectedIndex];
						const storeBreakdowns = selectedDepartment.StoreFulfillmentCost
							? Object.entries(selectedDepartment.StoreFulfillmentCost)
								.map(function (storeCostPair) {
									var store = storeCostPair[0];
									var cost = storeCostPair[1];
									return store + ": $" + cost.toFixed(2);
								})
								.join("<br>")
							: "No store/cost information available";

						tooltip.innerHTML = "<strong>" + departmentName + "</strong><br>" + storeBreakdowns;
						tooltip.style.visibility = "visible";
					} else {
						selectedDepartment = null;
						lastClickedDataPointIndex = null;
						const departmentName = departmentNames[config.dataPointIndex];
						//The second $ is to add a dollar sign to the tooltip, the first is to pass it as a string literal so that it can be inserted directly into the string.
						tooltip.innerHTML = `${departmentName} - $${departmentCost.toFixed(2)}`;
					}
				},

				dataPointMouseEnter: function (event, chartContext, config) {
					const chartContainer = chartContext.el.parentNode;
					const tooltip = document.getElementById("custom-tooltip");
					const departmentName = departmentNames[config.dataPointIndex];
					const departmentCost = departmentData[config.dataPointIndex].ProductProfit;

					chartContainer.addEventListener("mousemove", (e) => {
						if (tooltip.style.visibility === "visible" && !selectedDepartment) {
							const containerRect = chartContainer.getBoundingClientRect();
							tooltip.style.left = `${e.clientX - containerRect.left + 10}px`;
							tooltip.style.top = `${e.clientY - containerRect.top + 10}px`;
						}
					});

					const containerRect = chartContainer.getBoundingClientRect();
					tooltip.style.left = `${event.clientX - containerRect.left + 10}px`;
					tooltip.style.top = `${event.clientY - containerRect.top + 10}px`;

					if (config.dataPointIndex === lastClickedDataPointIndex) {
						tooltip.innerHTML = selectedDepartment.StoreFulfillmentCost
							? `<strong>${departmentName}</strong><br>` +
                            Object.entries(selectedDepartment.StoreFulfillmentCost)
                            	.map(function (storeCostPair) {
                            		var store = storeCostPair[0];
                            		var cost = storeCostPair[1];
                            		return store + ": $" + cost.toFixed(2);
                            	})
                            	.join("<br>")
							: `<strong>${departmentName}</strong><br>No store/cost information available`;
					} else {
						//The second $ is to add a dollar sign to the tooltip, the first is to pass it as a string literal so that it can be inserted directly into the string.
						tooltip.innerHTML = `${departmentName} - $${departmentCost.toFixed(2)}`;
					}

					tooltip.style.visibility = "visible";
				},

				dataPointMouseLeave: function (event, chartContext, config) {
					const tooltip = document.getElementById("custom-tooltip");
					{
						tooltip.style.visibility = "hidden";
					}
				},

			},
		},
		labels: departmentNames,
		colors: chartColors,
		tooltip: {
			enabled: false
		},
		responsive: [
			{
				breakpoint: 350,
				options: {
					chart: {
						width: 200,
					},
					legend: {
						position: "bottom",
					},
				},
			},
		],
	};
	const grandTotal = departmentData.reduce((total, d) => total + d.ProductProfit, 0);
	//The second $ is to add a dollar sign to the tooltip, the first is to pass it as a string literal so that it can be inserted directly into the string.
	document.querySelector("#grand-total").innerHTML = `<b>Grand Total: $${grandTotal.toFixed(2)}</b>`;


	const chart = new ApexCharts(document.querySelector("#pie-chart-total-fulfillment-sales"), pieOptions);
	chart.render().then(() => {
		const chartContainer = document.querySelector("#chart-container");
		chartContainer.addEventListener("mousemove", updateTooltipPosition);
	});
}  
