/* eslint-disable no-undef */
$(document).ready(function () {
	//initialize the drag and drop with Dragula
	InitDragNDrop();

	window.toggleHelpInfo = function (id) {
		var helpInfo = document.getElementById(id);
		if (helpInfo) {
			helpInfo.classList.toggle("hidden");
		}
	};

	//#region Leaderboard Setup
	var timeframe = document.getElementById("timeframe-selector")
	if (timeframe) {
		document.getElementById("timeframe-selector").addEventListener("change", function (event) {
			const selectedTimeframe = event.target.value;

			$(".leaderboard-fetching-container").css("display", "flex");
			$("#leaderboard-fetching").removeClass("hidden");
			document.getElementById("past-table").style.display = "none";
			document.getElementById("present-table").style.display = "none";

			loadData(selectedTimeframe);
		});
	}

	function updateRemainingDays(endDate) {
		const today = new Date();
		const remainingDays = Math.ceil((endDate - today) / (1000 * 60 * 60 * 24));
		document.getElementById("remaining-days").textContent = remainingDays;
	}


	function updateHeaderTitles(timeframe) {
		const previousTitleElement = document.getElementById("previous-title");
		const currentTitleElement = document.getElementById("current-title");
		if (previousTitleElement && currentTitleElement) {
			switch (timeframe) {
				case "monthly":
					previousTitleElement.textContent = "Previous Month";
					currentTitleElement.textContent = "Current Month";
					break;
				case "quarterly":
					previousTitleElement.textContent = "Previous Quarter";
					currentTitleElement.textContent = "Current Quarter";
					break;
				case "yearly":
					previousTitleElement.textContent = "Previous Year";
					currentTitleElement.textContent = "Current Year";
					break;
			}
		}
	}

	const pastTable = $("#past-table").DataTable({
		searching: false,
		lengthChange: false,
		pageLength: 5,
		paging: false,
		info: false,
		ordering: false,
		order: [[1, "des"]]
	});

	const presentTable = $("#present-table").DataTable({
		searching: false,
		lengthChange: false,
		pageLength: 5,
		paging: false,
		info: false,
		ordering: false,
		order: [[1, "des"]]
	});

	function aggregateDataByDepartment(data) {
		//For totaling up department totals for quarterly and yearly timeframes.
		const aggregatedData = {};

		data.forEach((item) => {
			if (aggregatedData[item.departmentName]) {
				aggregatedData[item.departmentName] += item.totalItemsShipped;
			} else {
				aggregatedData[item.departmentName] = item.totalItemsShipped;
			}
		});

		return Object.entries(aggregatedData).map(([departmentName, totalItemsShipped]) => ({
			departmentName,
			totalItemsShipped,
		}));
	}

	async function loadData(timeframe) {
		updateHeaderTitles(timeframe);

		const presentTable = $('#present-table').DataTable();
		const pastTable = $('#past-table').DataTable();

		pastTable.clear().draw();
		presentTable.clear().draw();

		let currentStartDate, previousStartDate, currentEndDate, previousEndDate;

		const currentDate = new Date();
		const currentYear = currentDate.getFullYear();
		const currentMonth = currentDate.getMonth();

		if (timeframe === "monthly") {
			currentStartDate = new Date(currentYear, currentMonth, 1);
			previousStartDate = new Date(currentYear, currentMonth - 1, 1);
			currentEndDate = new Date(currentYear, currentMonth + 1, 1);
			previousEndDate = new Date(currentYear, currentMonth, 1);
		} else if (timeframe === "quarterly") {
			const currentQuarter = Math.floor(currentMonth / 3);
			currentStartDate = new Date(currentYear, currentQuarter * 3, 1);
			previousStartDate = new Date(currentYear, (currentQuarter - 1) * 3, 1);
			currentEndDate = new Date(currentYear, (currentQuarter + 1) * 3, 1);
			previousEndDate = new Date(currentYear, currentQuarter * 3, 1);
		} else if (timeframe === "yearly") {
			currentStartDate = new Date(currentYear, 0, 1);
			previousStartDate = new Date(currentYear - 1, 0, 1);
			currentEndDate = new Date(currentYear + 1, 0, 1);
			previousEndDate = new Date(currentYear, 0, 1);
		}

		const currentData = await fetchTopDepartmentData(currentStartDate, currentEndDate);
		const previousData = await fetchTopDepartmentData(previousStartDate, previousEndDate);

		const aggregatedCurrentData = aggregateDataByDepartment(currentData);
		const aggregatedPreviousData = aggregateDataByDepartment(previousData);

		populateTable("#present-table", aggregatedCurrentData, 5);
		populateTable("#past-table", aggregatedPreviousData, 5);

		$(".leaderboard-fetching-container").css("display", "none");
		$("#leaderboard-fetching").addClass("hidden");
		const isPresentTable = document.getElementById("present-table");
		isPresentTable ? isPresentTable.style.display = "table" : null;
		const isPastTable = document.getElementById("past-table");
		isPastTable ? isPastTable.style.display = "table" : null;
		addTrophyToTopDepartments("present-table");
		addTrophyToTopDepartments("past-table");

		updateRemainingDays(currentEndDate);
	}

	async function fetchTopDepartmentData(startDate, endDate) {
		const startDateFormat = formatDate(startDate);
		const endDateFormat = formatDate(endDate);
		const response = await fetch(`/Home/TopDepartment?startDate=${startDateFormat}&endDate=${endDateFormat}`);
		const data = await response.json();
		return data;
	}

	function formatDate(date) {
		const year = date.getFullYear();
		const month = String(date.getMonth() + 1).padStart(2, "0");
		const day = String(date.getDate()).padStart(2, "0");
		return `${year}-${month}-${day}`;
	}

	function populateTable(tableSelector, data, limit = 5) {
		var table = $(tableSelector).DataTable();
		table.clear().draw();
		//Sorts the data by totalItemsShipped in descending order.
		data.sort((a, b) => b.totalItemsShipped - a.totalItemsShipped);

		//Slices the sorted data array to get only the top 5 items, but we can always adjust this later if desired.
		const limitedData = data.slice(0, limit);

		limitedData.forEach(function (item) {
			table.row.add([
				item.departmentName,
				//Apply any kind of formatting needed right here. It's bolded right now, now sure what more we want.
				`<i><b>${item.totalItemsShipped}</b></i>`
			]).draw(false);
		});
	}

	loadData("monthly");

	function addTrophyToTopDepartments(tableId) {
		const table = document.getElementById(tableId);
		const rows = table.getElementsByTagName("tr");

		let topDepartments = [
			{ row: null, ordersShipped: -1 },
			{ row: null, ordersShipped: -1 },
			{ row: null, ordersShipped: -1 },
		];

		for (let i = 1; i < rows.length; i++) {
			const cells = rows[i].getElementsByTagName("td");
			if (cells.length < 2) continue;
			const ordersShipped = parseInt(cells[1].innerText, 10);

			for (let j = 0; j < topDepartments.length; j++) {
				if (ordersShipped > topDepartments[j].ordersShipped) {
					topDepartments.splice(j, 0, { row: rows[i], ordersShipped: ordersShipped });
					topDepartments.pop();
					break;
				}
			}
		}

		const trophyColors = ["gold", "silver", "darkorange"];
		const trophyIcons = ["🥇", "🥈", "🥉"];

		topDepartments.forEach((department, index) => {
			if (department.row) {
				const departmentCell = department.row.getElementsByTagName("td")[0];
				const trophyIcon = document.createElement("span");
				trophyIcon.innerText = trophyIcons[index];
				trophyIcon.style.color = trophyColors[index];
				trophyIcon.style.marginRight = "5px";
				departmentCell.prepend(trophyIcon);
			}
		});
	}
	// #endregion

	//#region New SpeedOMeter
	async function fetchDailyOrderCompletionCount() {
		const response = await fetch("Home/GetDailyOrderCompletionCount");
		var results = await response;
		const data = results.json();
		return data;
	}

	function generateDepartmentOptions(departments) {
		var departmentsSelect = document.querySelector(".departments-select");
		departmentsSelect.innerHTML = departments.map(function (department) {
			return `<option value="${department}">${department}</option>`;
		}).join("");

		$(departmentsSelect).select2({
			placeholder: "Select Departments",
			allowClear: true
		});

		$(departmentsSelect).on("change", function () {
			var selectedDepartments = $(departmentsSelect).val();
			DailyOrderCompletionCount(selectedDepartments);
		});

		departmentsSelect.parentElement.style.display = "";
	}


	function generateFormElements(departments, originalTargets) {
		var formDiv = document.getElementById("dynamic-form");
		formDiv.innerHTML = ""; // Clear the existing content
		departments.forEach(function (department, index) {
			const originalTarget = originalTargets[index];
			// Check if the department is "UVP" and change it to "UV"
			const displayDepartment = department === "UVP" ? "UV" : department;
			// Create a new div for the form group
			const formGroup = document.createElement("div");
			formGroup.classList.add("form-group");
			// Create the label element
			const label = document.createElement("label");
			label.setAttribute("for", `SpeedOMeterGoal_${displayDepartment}Goal`);
			label.textContent = `${displayDepartment} Goal`;
			// Create the input element
			const input = document.createElement("input");
			input.setAttribute("id", `SpeedOMeterGoal_${displayDepartment}Goal`);
			input.setAttribute("name", `SpeedOMeterGoal.${displayDepartment}Goal`);
			input.classList.add("form-control");
			input.setAttribute("type", "number");
			input.setAttribute("min", "1");
			input.setAttribute("value", originalTarget || 1); // Default to 1 if no value is provided
			input.setAttribute("oninput", "validity.valid||(value='');");
			// Create the span element for error message (if needed)
			const errorSpan = document.createElement("span");
			errorSpan.classList.add("text-danger");
			// Append elements to formGroup
			formGroup.appendChild(label);
			formGroup.appendChild(input);
			formGroup.appendChild(errorSpan);
			// Append the formGroup to the formDiv
			formDiv.appendChild(formGroup);
		});
	}

	let radialChart;
	const departmentColors = [
		"#FF5733",
		"#33FF57",
		"#3357FF",
		"#F533FF",
		"#57FF33",
		"#FF33F5",
		"#33FFF5",
		"#F5FF33",
		"#FF5733",
		"#33FF57"
	];

	function GetDepartmentColours() {
		return departmentColors;
	}

	async function DailyOrderCompletionCount(selectedDepartments) {

		const allData = await fetchDailyOrderCompletionCount();

		// If no department is selected, display data for all departments
		if (selectedDepartments.length === 0) {
			selectedDepartments = allData.map(entry => entry.departmentName);
		}

		const dailyOrderCompletionCountData = allData.filter(entry => selectedDepartments.includes(entry.departmentName));

		const departmentNames = dailyOrderCompletionCountData.map(entry => entry.departmentName);
		const departmentTallies = dailyOrderCompletionCountData.map(entry => entry.tally);
		const departmentTargets = dailyOrderCompletionCountData.map(entry => entry.target);
		const departmentOriginalTargets = dailyOrderCompletionCountData.map(entry => entry.originalTarget);

		// This is for the modify goals form, and will dynamically create a section for each found department.
		generateFormElements(departmentNames, departmentOriginalTargets);

		const departmentPercentages = departmentTallies.map((tally, index) => {
			if (departmentTargets[index] === 1) {
				return 1;
			}
			return parseFloat(((tally / departmentTargets[index]) * 100).toFixed(2));
		});

		var options = {
			series: departmentPercentages,
			chart:
			{
				height: 350,
				type: "radialBar",
			},
			plotOptions: {
				radialBar: {
					max: 100,
					dataLabels: {
						name: {
							show: true,
						},
						value: {
							show: true,
						},
					},
				},
			},
			labels: departmentNames,
			colors: departmentColors,
			responsive: [{
				breakpoint: 480,
				options: {
					legend: {
						show: !1
					}
				}
			}]
		};
		//This is for displaying the "No products shipped" message when no data is being passed in.
		const noDataMessage = document.getElementById("no-data-message");
		if (!dailyOrderCompletionCountData || dailyOrderCompletionCountData.length === 0) {
			noDataMessage.classList.remove("d-none");
			if (radialChart) {
				radialChart.destroy();
				radialChart = null;
			}
			return;
		} else {
			noDataMessage.classList.add("d-none");
		}

		if (radialChart) {
			radialChart.destroy();
		}
		radialChart = new ApexCharts(document.querySelector("#new-speed-o-meter-chart"), options);
		radialChart.render();

		displayChartLabels(departmentNames, departmentTallies, departmentTargets);
		createPieCharts(dailyOrderCompletionCountData);
	}

	var orderAvailable = document.getElementById("drac-ordhistory");
	if (orderAvailable) {
		fetchDailyOrderCompletionCount().then(data => {
			const departmentNames = data.map(entry => entry.departmentName);
			generateDepartmentOptions(departmentNames);
			DailyOrderCompletionCount(departmentNames);
		});
	}

	//Display custom labels
	function displayChartLabels(labels, amounts, targets, departmentColors) {
		var chartLabelsDiv = document.getElementById("chart-labels");
		chartLabelsDiv.className = "d-flex flex-wrap justify-content-center";
		chartLabelsDiv.innerHTML = ""; // Clear existing content
		allDepartmentColors = GetDepartmentColours();
		labels.forEach(function (label, index) {
			// Create the outer div for each chart label
			const chartLabelDiv = document.createElement("div");
			chartLabelDiv.classList.add("chart-label", "d-flex", "flex-column", "align-items-center", "me-3", "mb-2");
			// Create the amount span element
			const amountDiv = document.createElement("div");
			const amountSpan = document.createElement("span");
			amountSpan.classList.add("chart-amount", "h5");
			amountSpan.textContent = `${amounts[index]} / ${targets[index]}`;
			amountDiv.appendChild(amountSpan);

			// Create the department label with color dot
			const departmentDiv = document.createElement("div");
			const dotSpan = document.createElement("span");
			dotSpan.classList.add("chart-dot");
			dotSpan.style.backgroundColor = allDepartmentColors[index];
			dotSpan.style.display = "inline-block";
			dotSpan.style.width = "10px";
			dotSpan.style.height = "10px";
			dotSpan.style.borderRadius = "50%";
			dotSpan.style.marginRight = "5px";
			const departmentSpan = document.createElement("span");
			departmentSpan.classList.add("chart-department", "small");
			departmentSpan.textContent = label;
			departmentDiv.appendChild(dotSpan);
			departmentDiv.appendChild(departmentSpan);
			// Append the inner divs to the chart label div
			chartLabelDiv.appendChild(amountDiv);
			chartLabelDiv.appendChild(departmentDiv);
			// Append the chart label div to the parent container
			chartLabelsDiv.appendChild(chartLabelDiv);
		});
	}

	function createPieCharts(data) {
		var detailsTab = document.getElementById("details");
		detailsTab.innerHTML = "";

		var combinedTop10Goals = data.flatMap(entry => entry.departmentGoals).sort((a, b) => b.quantity - a.quantity).slice(0, 10);

		var amounts = combinedTop10Goals.map(goal => goal.quantity);
		var productSkus = combinedTop10Goals.map(goal => goal.productSku);

		var pieOptions = {
			series: amounts,
			labels: productSkus,
			chart: {
				type: "pie",
				height: 400,
				toolbar: {
					show: false
				}
			},
			noData: {
				text: "No data available.",
				align: 'center',
				verticalAlign: 'middle',
				style: {
					color: '#AAB8C5',
					fontSize: '16px',
				}
			},
			legend: {
				position: "right"
			},
		};

		var pieChart = new ApexCharts(detailsTab, pieOptions);
		pieChart.render();
	}
	//#endregion

	//#region Daily Orders
	let endDate = new Date();
	let startDate = new Date();
	startDate.setDate(startDate.getDate() - 30);

	const urlDailyShipstationOrders = "home/GetDailyShipstationOrdersAll";

	var colors = ["#fa5c7c", "#6c757d"];

	options = {
		chart: {
			stacked: false,
			type: "area",
			zoom: {
				enabled: true,
				autoScaleYaxis: true
			},
			animations: {
				enabled: true
			}
		},
		plotOptions: {
			line: {
				curve: "smooth",

			}
		},
		dataLabels: {
			enabled: false
		},
		colors: colors,
		stroke: {
			width: [3, 3],
			curve: "straight"
		},
		series: [{
			name: "Orders Out",
			data: []
		}, {
			name: "Orders In",
			data: []
		},
		],
		noData: { text: "Loading..." },
		title: {
			text: "Daily Orders",
			align: "left"
		},
		grid: {
			row: {
				colors: ["transparent", "transparent"],
				opacity: 0.2
			},
			borderColor: "#f1f3fa",
		},
		fill: {
			gradient: {
				enabled: true,
				shadeIntensity: 1,
				inverseColors: false,
				opacityFrom: 0.5,
				opacityTo: 0.1,
				stops: [0, 70, 80, 100]
			},
		},
		yaxis: {
			title: "Orders"
		},
		xaxis: {
			labels: {
				datetimeFormatter: {
					year: "yyyy",
					month: "MMM 'yy",
					day: "dd MMM"
				}
			},
			type: "datetime",
			categories: []
		}
	};

	var DailyOrdersChart = new ApexCharts(
		document.querySelector("#daily-orders-chart"),
		options
	);
	DailyOrdersChart.render();

	function addDailyOrdersData(ordersInData, ordersOutData) {
		let ordersInChartData = ordersInData.map(o => ({ x: o.Item1, y: o.Item2 }));
		let ordersOutChartData = ordersOutData.map(o => ({ x: o.Item1, y: o.Item2 }));

		DailyOrdersChart.updateOptions({
			series: [{ name: "Orders In", data: ordersInChartData }, { name: "Orders Out", data: ordersOutChartData }],
		});
	}

	function clearDailyOrders() {
		DailyOrdersChart.updateSeries([
			{
				name: "Orders In",
				data: []
			},
			{
				name: "Orders Out",
				data: []
			}
		]);
	}
	$("#department-select").select2();
	$("#department-select").on("select2:select", function (e) {
		let data = e.params.data;
		let departmentName = data.text;
		let departmentId = data.id;

		if (departmentName === "All") {
			departmentId = "All";
		}
		$(".order-fetching-container").css("display", "flex");
		$("#order-fetching").removeClass("hidden");
		updateChartWithNewDates(startDate, endDate, departmentId)
			.done(function (responseJson) {
				let response = JSON.parse(responseJson);

				let ordersInData = response.OrderDateOrdersIn;
				let ordersOutData = response.ShipDateOrdersOut;

				addDailyOrdersData(ordersInData, ordersOutData, departmentName, departmentId);
				$(".order-fetching-container").css("display", "none");
				$("#order-fetching").addClass("hidden");
			})
			.fail(function (jqXHR, textStatus, errorThrown) {
				console.error("Error fetching data:", textStatus, errorThrown);
				$(".order-fetching-container").css("display", "none");
				$("#order-fetching").addClass("hidden");
			});
	});

	function updateChartWithNewDates(start, end, departmentId) {
		startDate = start;
		endDate = end;

		let requestData = {
			startDate: startDate.toJSON(),
			endDate: endDate.toJSON(),
		};

		if (departmentId !== "All") {
			requestData.departmentId = departmentId;
		}

		return $.ajax({
			url: urlDailyShipstationOrders,
			type: "GET",
			dataType: "text",
			data: requestData,
			success: function (responseJson) {
				var response = JSON.parse(responseJson);

				clearDailyOrders();

				addDailyOrdersData(response.OrderDateOrdersIn, response.ShipDateOrdersOut, "All");
			},
			error: function (jqXHR, textStatus, errorThrown) {
				console.error("Error fetching data:", textStatus, errorThrown);
			}
		});
	}

	updateChartWithNewDates(moment().subtract(29, "days"), moment());

	$("#departmentDateRange").daterangepicker({
		startDate: startDate,
		endDate: endDate,
		ranges: {
			"Last 30 Days": [moment().subtract(29, "days"), moment()],
			"This Month": [moment().startOf("month"), moment().endOf("month")],
			"Last Month": [moment().subtract(1, "month").startOf("month"), moment().subtract(1, "month").endOf("month")],
			"Last 3 Months": [moment().subtract(3, "month").startOf("month"), moment().subtract(1, "month").endOf("month")],
			"Last 6 Months": [moment().subtract(6, "month").startOf("month"), moment().subtract(1, "month").endOf("month")],
			"Last 9 Months": [moment().subtract(9, "month").startOf("month"), moment().subtract(1, "month").endOf("month")]
		}
	}, function (start, end) {
		updateChartWithNewDates(start, end);
	});

	function updateChartWithNewDates(start, end, departmentId) {
		startDate = start;
		endDate = end;

		return $.ajax({
			url: urlDailyShipstationOrders,
			type: "GET",
			dataType: "text",
			data: { startDate: startDate.toJSON(), endDate: endDate.toJSON(), departmentId: departmentId },
			success: function (responseJson) {
				var response = JSON.parse(responseJson);

				let ordersInData = response.OrderDateOrdersIn;
				let ordersOutData = response.ShipDateOrdersOut;

				addDailyOrdersData(ordersInData, ordersOutData);

				if (!DailyOrdersChart.el) {
					DailyOrdersChart.render();
				}
			},
			error: function (jqXHR, textStatus, errorThrown) {
				console.error("Error fetching data:", textStatus, errorThrown);
			}
		});
	}
	//#endregion
});


function InitDragNDrop() {

	//#region dragula drag-n-drop implementation

	//listen to the drag n drop controller checkbox event and change the label text
	$("#layout-editing-check").on('change', function () {
		$('#layout-editing-check-label').html($(this).is(':checked') ? 'Disable Layout Editing' : 'Enable Layout Editing');
	});

	//get main dragula container
	let dracContainer = $('#drac-container');

	//get error message element
	let errorMsg = $('#error-message');

	var ongoingAjax;
	var isAjaxPending = false;


	//if nothing received from the server (previous data is empty), get the current element positions
	if (!elementPositions || !Array.isArray(elementPositions) || elementPositions.length == 0) {
		elementPositions = [];
		dracContainer.children("div").each((i, elem) => {
			elementPositions.push({ position: i, elemId: elem.id });
		});
	}

	dragula([document.getElementById('drac-container')], {
		isContainer: function (el) {
			return false; // only elements in drake.containers will be taken into account
		},
		moves: function (el, source, handle, sibling) {
			return $('#layout-editing-check').is(':checked'); //enable or disable drag n drop according to the user preference
		},
		accepts: function (el, target, source, sibling) {
			return true; // elements can be dropped in any of the `containers` by default
		},
		invalid: function (el, handle) {
			return false; // don't prevent any drags from initiating by default
		},
		direction: 'vertical',             // Y axis is considered when determining where an element would be dropped
		copy: false,                       // elements are moved by default, not copied
		copySortSource: false,             // elements in copy-source containers can be reordered
		revertOnSpill: false,              // spilling will put the element back where it was dragged from, if this is true
		removeOnSpill: false,              // spilling will `.remove` the element, if this is true
		mirrorContainer: document.body,    // set the element that gets mirror elements appended
		ignoreInputTextSelection: true,     // allows users to select input text, see details below
		slideFactorX: 0,               // allows users to select the amount of movement on the X axis before it is considered a drag instead of a click
		slideFactorY: 0,               // allows users to select the amount of movement on the Y axis before it is considered a drag instead of a click
	})
		.on('drop', function (el, target, source, sibling) {
			//when the item dropped succesfully, prepare the new element order to save in the DB
			let newPositionsArr = [];
			dracContainer.children("div").each((i, elem) => {
				newPositionsArr.push({ Position: i, ElemId: elem.id });
			});

			//abort pending ajax calls
			if (isAjaxPending && ongoingAjax) {
				ongoingAjax.abort();
			}

			isAjaxPending = true;//mark as ajax is started

			var data = { Name: dashboardName, Layouts: newPositionsArr };
			ongoingAjax = $.ajax({
				url: "/UserPreference/SaveLayout",
				type: "POST",
				contentType: "application/json; charset=utf-8",
				data: JSON.stringify(data),
				dataType: "json",
				success: function (data) {
					errorMsg.attr('style', 'display:none !important');
					isAjaxPending = false;
				},
				error: function (error) {
					if (error && error.statusText && error.statusText != 'abort') {
						errorMsg.show();
					}

					isAjaxPending = false;

					//if in error, reset the view to the prvious order
					dracContainer.empty();//remove the existing child elements from main dragula container

					//add the elements back with the previously persisted order
					elementPositions.forEach((item, i) => {
						dracContainer.append(item.elem);
					});

					isAjaxBeingAborted = false;
				}
			});
		});

	//#endregion

	//#region dragular drag-n-drop set initial positions

	if (dracContainer.children.length == 0) {
		return;
	}

	elementPositions.sort((a, b) => a.position - b.position);//sort the retrived previously persisted element order array

	//add the dom elements to the retrived array
	dracContainer.children("div").each((i, elem) => {
		elementPositions.forEach((item, j) => {
			if (elem.id == item.elemId) {
				elementPositions[j]["elem"] = elem;
			}
		});
	});

	dracContainer.empty();//remove the existing child elements from main dragula container

	//add the elements back with the previously persisted order
	elementPositions.forEach((item, i) => {
		dracContainer.append(item.elem);
	});

	dracContainer.show();

	//#endregion

}