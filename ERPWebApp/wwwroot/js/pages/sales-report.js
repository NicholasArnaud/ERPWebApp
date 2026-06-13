function addStores(dropdownData) {
	for (var i = 0; i < dropdownData.length; i++) {
		var newOption = new Option(dropdownData[i].storeName, dropdownData[i].storeId, false, false);
		$("#store-dropdown").append(newOption).trigger("change");
	}
}

function getSubTotal(invoicedata) {
	var subtotalAmount = 0.0;
	for (var i = 0; i < invoicedata.length; i++) {
		subtotalAmount += invoicedata[i].totalCost;
	}
	return subtotalAmount;
}

function getShippingTotal(invoicedata) {
	var shippingTotal = 0.0;
	for (var i = 0; i < invoicedata.length; i++) {
		shippingTotal += invoicedata[i].shippingCost;
	}
	return shippingTotal;
}

function getInvoiceData() {
	var e = document.getElementById("store-dropdown");
	var selectedStoreId = e.options[e.selectedIndex].value;
	console.log(selectedStoreId);
	var url = "/SalesReports/PullStoreSalesReports";
	console.log(url);
	console.log(moment($("#daterange").data("daterangepicker").startDate).toDate());
	console.log(moment($("#daterange").data("daterangepicker").endDate).toDate());
	$.get(url, {
		StoreId: selectedStoreId,
		StartDate: $("#daterange").data("daterangepicker").startDate.format("M-D-YYYY"),
		EndDate: $("#daterange").data("daterangepicker").endDate.format("M-D-YYYY")
	},
	function (data) {
		var subTotalCost = getSubTotal(data);
		var shippingCost = getShippingTotal(data);
		var totalCost = shippingCost + subTotalCost;
		$("#sub-total").text("$ " + formatMoney(subTotalCost));
		$("#shipping-total").text("$ " + formatMoney(shippingCost));
		$("#total-cost").text("$ " + formatMoney(totalCost));
		console.log(data);

		$("#partial-table").load("/SalesReports/PartialViewInvoice", function () {
			console.log("Attempted Refresh");
		});
	});
}


function formatMoney(number, decPlaces, decSep, thouSep) {
	decPlaces = isNaN(decPlaces = Math.abs(decPlaces)) ? 2 : decPlaces,
	decSep = typeof decSep === "undefined" ? "." : decSep;
	thouSep = typeof thouSep === "undefined" ? "," : thouSep;
	var sign = number < 0 ? "-" : "";
	var i = String(parseInt(number = Math.abs(Number(number) || 0).toFixed(decPlaces)));
	var j = (j = i.length) > 3 ? j % 3 : 0;

	return sign +
        (j ? i.substr(0, j) + thouSep : "") +
        i.substr(j).replace(/(\decSep{3})(?=\decSep)/g, "$1" + thouSep) +
        (decPlaces ? decSep + Math.abs(number - i).toFixed(decPlaces).slice(2) : "");
}
