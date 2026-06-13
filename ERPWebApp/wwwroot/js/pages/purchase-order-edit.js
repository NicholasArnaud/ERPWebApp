
var urls = {
	"getStockLocationsAndDefaultLocations": "../GetCurrentStockAndAllOtherLocations",
	"shippingMethod":"../MethodsByShipPro/"
}

var dropDownSelectors = {
	"stockLocationSelection": $("#selector"),
	"shippingMethodSelector": $("#selector-ship-method")
}

function initializeDataTables() {
	$("#purchase-order-datatable").DataTable();
	//datatables initiallization on page
	$("#Product-datatable").DataTable({
		keys: !0, language: {
			paginate: {
				previous: "<i class='mdi mdi-chevron-left'>", next: "<i class='mdi mdi-chevron-right'>"
			},
		}, drawCallback: function () {
			$(".dataTables_paginate > .pagination").addClass("pagination-rounded");
		}
	});
	$("#Stock-datatable").DataTable({
		"pageLength": 5,
		"searching": false,
		lengthMenu: [[5, 10, 25, 100], [5, 10, 25, 100]],
		keys: !0, language: {
			paginate: {
				previous: "<i class='mdi mdi-chevron-left'>", next: "<i class='mdi mdi-chevron-right'>"
			},
		}, drawCallback: function () {
			$(".dataTables_paginate > .pagination").addClass("pagination-rounded");
		}
	});
	$("#file-datatable").DataTable({
		"pageLength": 5,
		"searching": false,
		lengthMenu: [[5, 10, 25, 100], [5, 10, 25, 100]],
		keys: !0, language: {
			paginate: {
				previous: "<i class='mdi mdi-chevron-left'>", next: "<i class='mdi mdi-chevron-right'>"
			},
		}, drawCallback: function () {
			$(".dataTables_paginate > .pagination").addClass("pagination-rounded");
		}
	});
}
function stopMultipleSubmit() {
	if (!isSubmitted) {
		$("#btnRemove").prop("disabled", true);
		$("#btnTransfer").prop("disabled", true);
		$("#btnAdd").prop("disabled", true);
		isSubmitted = true;
		return true;
	}
	else {
		return false;
	}
}

function getTotalCost() {
	$("#total-cost-add-product").val((parseFloat($("#custom-cost-add-product").val()) * parseInt($("#total-ordered-add-product").val())).toFixed(4));
	var els = document.getElementsByClassName("custom-cost");
	var pcostside = document.getElementsByClassName("total-ordered");
	var pcosttotals = document.getElementsByClassName("totalcosteditmain");
	var discountPercentage = document.getElementsByClassName("discount-percentage")
	for (var i = 0; i < els.length; i++) {
		var subTotal = (els[i].value * pcostside[i].value);
		var discount = subTotal * (discountPercentage[i].value / 100);
		pcosttotals[i].value = (subTotal - discount).toFixed(4);

	}
}
//locks the products to not allow them to be changed after
function lockProductChanges() {
	var ttlRecieved = document.getElementsByClassName("total-recieved");
	var ttlOrdered = document.getElementsByClassName("total-ordered");
	var cstCost = document.getElementsByClassName("custom-cost");
	var countlength = ttlRecieved.length;
	var fail = false;
	for (let i = 0; i < countlength; i++) {

		if (isNaN(parseInt(ttlRecieved[i].value)) || isNaN(parseInt(ttlOrdered[i].value)) || isNaN(parseInt(cstCost[i].value))
            || parseInt(ttlRecieved[i].value) < 0 || parseInt(ttlOrdered[i].value) < 0 || parseFloat(cstCost[i].value < 0)
            || parseInt(ttlRecieved[i].value) > parseInt(ttlOrdered[i].value) || parseInt(ttlOrdered[i].value < ttlRecieved[i].value)) {
			fail = true;
		}

	}
	if (!fail) {
		//hides and shows what it needs to and sets inputs to readonly
		$("#lock-changes").hide();
		$(".custom-cost").attr("readonly", "readonly");
		$(".total-ordered").attr("readonly", "readonly");
		$(".total-recieved").attr("readonly", "readonly");
		$("#stock-save").show();
		$("#stock-initial").hide();
		$("#custom-accordion-prod").hide();
		getSmolProd(0);
	}

}
function getSmolProd(counter) {
	var countlength = document.getElementsByClassName("product-sku").length;
	//loop check to go through each product in the PO
	var totrecvd = document.getElementsByClassName("total-recieved");
	var beforecvd = document.getElementsByClassName("total-recbefore");
	var totord = document.getElementsByClassName("total-ordered");

	if (counter < countlength && counter < totrecvd.length && counter < beforecvd.length) {
		$("#product-received").val(0);
		var psku = document.getElementsByClassName("product-sku");
		var totalReceived = parseInt(totrecvd[counter]?.value || "0");
		var totalReceivedBefore = parseInt(beforecvd[counter]?.value || "0");
		if (parseInt(totalReceived - totalReceivedBefore) > 0) {

			$("#next-product-button").hide();
			$("#next-button-decoy").show();
			$("#tooltip-next").show();
			var url = "../getCurrentStock";
			dropDownSelectors.stockLocationSelection.empty();

			var beforervd = document.getElementsByClassName("total-recbefore");
			var totrcvd = document.getElementsByClassName("total-recieved");

			var recievedValue = parseInt(totrcvd[counter].value);
			var editedRecievedValue = parseInt(beforervd[counter].value);
			$("#product-id").val(psku[counter].innerHTML.trimStart());
			$("#product-received").val(recievedValue - editedRecievedValue);

			dropDownSelectors.stockLocationSelection.select2({
				placeholder: "Select a Location",
				ajax: {
					url: urls.getStockLocationsAndDefaultLocations,
					type: 'GET',
					dataType: 'json',
					data: function (params) {
						return { psku: psku[counter].innerHTML };
					},
					success: function (data) {
						dropDownSelectors.stockLocationSelection.empty()
						dropDownSelectors.stockLocationSelection.select2({
							placeholder: "Select a Location",
							data: data
						});

						dropDownSelectors.stockLocationSelection.val(null).trigger('change');
						dropDownSelectors.stockLocationSelection.select2('close');
					},
					error: function (xhr, status, error) {
						console.error("AJAX Request Failed:", status, error);
					}
				}
			});

			dropDownSelectors.stockLocationSelection.select2('open');

		}
		else {
			var url = "../EditStock";
			var customcost = document.getElementsByClassName("custom-cost");
			var productquantity = document.getElementsByClassName("total-ordered");
			var prodpur = document.getElementsByClassName("purchase-order-id");
			var discountPercentage = document.getElementsByClassName("discount-percentage");
			var totalcost = document.getElementsByClassName("totalcosteditmain");

			var discountPercentageValue = parseInt(discountPercentage[counter]?.value || "0");
			var totalcostValue = parseInt(totalcost[counter]?.value || "0");


			$.getJSON(url, {
				Id: psku[counter].innerHTML,
				SiteLocation: $("#selector").val(),
				Recieved: $("#product-received").val(),
				ProductPurchase: parseInt(prodpur[counter]?.innerHTML || "0"),
				CustomCost: parseFloat(customcost[counter]?.value || "0"),
				ProductQuantitiy: parseInt(productquantity[counter]?.value || "0"),
				DiscountPercentage: discountPercentageValue,
				TotalCost: totalcostValue,
				GroupName: "base"
			}).done(function (response) {

			});
			$("#counter-helper").val(parseInt(counter) + 1);
			getSmolProd($("#counter-helper").val());
		}
	}
	else {
		$("#next-product-button").hide();
		$("#stock-save").hide();
		$("#finish-edit").show();
		$("#submit-btn").show();
		$("#stock-modal").hide();
		$("#custom-accordion-prod").hide();
		$("#collapse-Six").collapse("toggle");
	}


}
function add_fields() {
	url = "../GetPVMAverageCost";
	$.getJSON(url, { id: $("#selector-prod option:selected").val() }, function (response) {
		$("#average-cost-add-product").val(response.getaverage);
		$("#custom-cost-add-product").val(response.getcost);
		$("#pvm-id").val(response.pvmid);
	});
}

function GetMethodsByShipPro(_ShipProId) {
	var url = urls.shippingMethod;


	dropDownSelectors.shippingMethodSelector.select2({
		placeholder: "Select a shipping method",
		ajax: {
			url: url,
			type: 'GET',
			dataType: 'json',
			data: function (params) {
				return { shipId: _ShipProId };
			},
			success: function (data) {
				dropDownSelectors.shippingMethodSelector.empty()
				var results = data.map(item => ({
					id: item.shippingMethodId,         // Value for the option
					text: item.shippingMethodName  // Display text for the option
				}));
				dropDownSelectors.shippingMethodSelector.select2({
					placeholder: "Select a shipipng method",
					data: results
				});
				dropDownSelectors.shippingMethodSelector.select2('close');

				$("#selector-ship-method").val($("#selector-ship-method option:first").val());
				$("#id-shipping-meth").val($("#selector-ship-method").val());
				$("#id-shipping-pro").val($("#selector-ship").val());
			},
			error: function (xhr, status, error) {
				console.error("AJAX Request Failed:", status, error);
			}
		}
	});

	dropDownSelectors.shippingMethodSelector.select2('open');
}

function checkRecieved() {
	var input = document.getElementsByClassName();
}
//function to save the stock
function saveStock(savecounter) {
	var url = "../EditStock";
	var customcost = document.getElementsByClassName("custom-cost");
	var productquantity = document.getElementsByClassName("total-ordered");
	var prodpur = document.getElementsByClassName("purchase-order-id");
    var discountPercentage = document.getElementsByClassName("discount-percentage")[savecounter].value;
	var totalcost = document.getElementsByClassName("totalcosteditmain")[savecounter].value;
	$.getJSON(url, {
		Id: $("#product-id").val(),
		SiteLocation: $("#selector").val(),
		Recieved: $("#product-received").val(),
		ProductPurchase: prodpur[savecounter].innerHTML,
		CustomCost: customcost[savecounter].value,
		ProductQuantitiy: productquantity[savecounter].value,
		discountPercentage:discountPercentage,
		TotalCost: totalcost,
		GroupName: document.querySelector("#selector option:checked").parentElement.label
	}).done(function (response) {

	});
	$("#counter-helper").val(parseInt(savecounter) + 1);
	getSmolProd($("#counter-helper").val());
}

//gets a list of stocks and puts them in a datatable
async function getStockList(counter) {
	var getProduct = document.getElementsByClassName("purchase-order-id");
	var Product = getProduct[counter].innerHTML;
	var url = "../GetStockList";
	$.getJSON(url, { psku: Product }).done(function (data) {
		console.log(data);
		//Recalled DataTable in Load to wait for data is allocated in the table before reloading it
		//otherwise DataTable does not work
		$("#stock-load").load("/PurchaseOrders/PartialViewTable", function (response) {
			var awaitingOrdersTable = $("#Stock-datatable").DataTable({
				keys: !0, language: {
					paginate: {
						previous: "<i class='mdi mdi-chevron-left'>", next: "<i class='mdi mdi-chevron-right'>"
					},

				}, drawCallback: function () {
					$(".dataTables_paginate > .pagination").addClass("pagination-rounded");
				}
			});
			console.log("PartialViewTable Reloaded");
		});
	});
}

//checks for file to submit
function filebutton() {
	if ($("#upload").length) {
		var vidFileLength = $("#upload")[0].files.length;
		if (vidFileLength === 0) {
			$("#btnsubmit").hide();
		} else {
			$("#btnsubmit").show();
		}
	}
}