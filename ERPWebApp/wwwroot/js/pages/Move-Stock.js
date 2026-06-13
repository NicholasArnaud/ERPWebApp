urls = {
	moveStockLocationsBySiteId: "/MoveStock/LocationsBySiteId"
}
dropDownSelectors = {
	stockToSiteSelector: $("#to-site-select-id"),
	stockFromSiteSelector: $("#site-select-id"),
	moveStockToLocationSelector: $("#to-location-select-id"),
	removeStockToLocationSelector: $("#removesite-select-id"),
	addToLocationSelector :$("#add-to-location-select-id")
}

function generateBarcode() {
	let _productId = $("#add-product-select-id").val();

	let url = "/MoveStock/GenerateBarcode";
	$.ajax({
		url: url,
		type: "POST",
		data: { ProductId: _productId },
		success: function (response) {

			// Find the image element and set its source
			let barcodeImage = $("#barcode")[0];
			barcodeImage.src = response;

			// Trigger printing after 1 second
			setTimeout(function () {
				PrintElement();
			}, 1000);
		},
		error: function (data) {
			console.log(data);
		}
	});
}

function PrintElement() {
	let mywindow = window.open($("#barcode").html());
	mywindow.document.write("<center>" + $("#barcode").html() + "</center>");

	mywindow.document.close(); // necessary for IE >= 10
	mywindow.focus(); // necessary for IE >= 10*/

	mywindow.print();
	mywindow.close();

	return true;
}


function GetMoveLocationBySiteId(_siteId) {
	let url = "/MoveStock/LocationsBySiteId";
	dropDownSelectors.moveStockToLocationSelector.select2({
		placeholder: "Select a Location",
		ajax: {
			url: url,
			type: 'GET',
			dataType: 'json',
			data: function (params) {
				return { SiteId: _siteId };
			},
			success: function (data) {
				dropDownSelectors.moveStockToLocationSelector.empty();

				var results = data.map(item => ({
					id: item.locationId,         // Value for the option
					text: item.locationName  // Display text for the option
				}));
				dropDownSelectors.moveStockToLocationSelector.select2({
					placeholder: "Select a Location",
					data: results
				});
				dropDownSelectors.moveStockToLocationSelector.val(null).trigger('change');
				dropDownSelectors.moveStockToLocationSelector.select2('close');
			},
			error: function (xhr, status, error) {
				console.error("AJAX Request Failed:", status, error);
			}
		}
		});
	dropDownSelectors.moveStockToLocationSelector.select2('open');

}

dropDownSelectors.stockToSiteSelector.on("change", function (e) {
	GetMoveLocationBySiteId(+$("#to-site-select-id").val());
});


function GetAddLocationBySiteId(_siteId) {
	let url = "/MoveStock/LocationsBySiteId";

	dropDownSelectors.addToLocationSelector.select2({
		placeholder: "Select a Location",
		ajax: {
			url: url,
			type: 'GET',
			dataType: 'json',
			data: function (params) {
				return { SiteId: _siteId };
			},
			success: function (data) {
				dropDownSelectors.addToLocationSelector.empty();

				var results = data.map(item => ({
					id: item.locationId,         // Value for the option
					text: item.locationName  // Display text for the option
				}));
				dropDownSelectors.addToLocationSelector.select2({
					placeholder: "Select a Location",
					data: results
				});
				dropDownSelectors.addToLocationSelector.val(null).trigger('change');
				dropDownSelectors.addToLocationSelector.select2('close');
			},
			error: function (xhr, status, error) {
				console.error("AJAX Request Failed:", status, error);
			}
		}
	});
	dropDownSelectors.addToLocationSelector.select2('open');
}

$("#add-to-site-select-id").on("change", function (e) {
	GetAddLocationBySiteId(+$("#add-to-site-select-id").val());
});


$("#site-select-id").on("change", function (e) {
	var x = document.getElementById("site-select-id");
	if (x.options.length > 0 && x.selectedIndex > -1) {
		var arrayText = x.options[x.selectedIndex].text;
		var splitArray = arrayText.toString().split(" : ");
		$("#quantity-id").attr(
			{
				"max": splitArray[2]
			}
		);
	}
});

function productsInSite(_productId) {
	let url = "/MoveStock/SitesByProduct";

	dropDownSelectors.stockFromSiteSelector.select2({
		placeholder: "Select a Location",
		ajax: {
			url: url,
			type: 'GET',
			dataType: 'json',
			data: function (params) {
				return { ProductId: _productId };
			},
			success: function (data) {
				dropDownSelectors.stockFromSiteSelector.empty();

				var results = data.map(item => ({
					id: item.locationId,         // Value for the option
					text: item.siteLocation  // Display text for the option
				}));
				dropDownSelectors.stockFromSiteSelector.select2({
					placeholder: "Select a Location",
					data: results
				});
				dropDownSelectors.stockFromSiteSelector.val(null).trigger('change');
				dropDownSelectors.stockFromSiteSelector.select2('close');
			},
			error: function (xhr, status, error) {
				console.error("AJAX Request Failed:", status, error);
			}
		}
	});
	dropDownSelectors.stockFromSiteSelector.select2('open');
}

$("#product-select-id").on("change", function (e) {
	productsInSite(+$("#product-select-id").val());
});


function removeProductsInSite(_productId) {
	let url = "/MoveStock/SitesByProduct";


	dropDownSelectors.removeStockToLocationSelector.select2({
		placeholder: "Select a Location",
		ajax: {
			url: url,
			type: 'GET',
			dataType: 'json',
			data: function (params) {
				return { ProductId: _productId };
			},
			success: function (data) {
				dropDownSelectors.removeStockToLocationSelector.empty();

				var results = data.map(item => ({
					id: item.locationId,         // Value for the option
					text: item.siteLocation  // Display text for the option
				}));
				dropDownSelectors.removeStockToLocationSelector.select2({
					placeholder: "Select a Location",
					data: results
				});
				dropDownSelectors.removeStockToLocationSelector.val(null).trigger('change');
				dropDownSelectors.removeStockToLocationSelector.select2('close');
			},
			error: function (xhr, status, error) {
				console.error("AJAX Request Failed:", status, error);
			}
		}
	});
	dropDownSelectors.removeStockToLocationSelector.select2('open');
}

$("#removeproduct-select-id").on("change", function (e) {
	removeProductsInSite(+$("#removeproduct-select-id").val());
});
