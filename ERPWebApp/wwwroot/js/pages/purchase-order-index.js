var productFilter = "";
var statusFilter = "";
var startDateInput = "";
var endDateInput = "";
var isEstimateDate = false;
var islastDate = false;

function newexportaction(e, dt, button, config) {
	var self = this;
	var oldStart = dt.settings()[0]._iDisplayStart;
	dt.one("preXhr", function (e, s, data) {


		// Just this once, load all data from the server
		data.start = 0;
		data.length = 2147483647;
		dt.one("preDraw", function (e, settings) {
			// Call the original action function
			if (button[0].className.indexOf("buttons-copy") >= 0) {
				$.fn.dataTable.ext.buttons.copyHtml5.action.call(self, e, dt, button, config);
			} else if (button[0].className.indexOf("buttons-excel") >= 0) {
				$.fn.dataTable.ext.buttons.excelHtml5.available(dt, config) ?
					$.fn.dataTable.ext.buttons.excelHtml5.action.call(self, e, dt, button, config) :
					$.fn.dataTable.ext.buttons.excelFlash.action.call(self, e, dt, button, config);
			} else if (button[0].className.indexOf("buttons-csv") >= 0) {
				$.fn.dataTable.ext.buttons.csvHtml5.available(dt, config) ?
					$.fn.dataTable.ext.buttons.csvHtml5.action.call(self, e, dt, button, config) :
					$.fn.dataTable.ext.buttons.csvFlash.action.call(self, e, dt, button, config);
			} else if (button[0].className.indexOf("buttons-pdf") >= 0) {
				$.fn.dataTable.ext.buttons.pdfHtml5.available(dt, config) ?
					$.fn.dataTable.ext.buttons.pdfHtml5.action.call(self, e, dt, button, config) :
					$.fn.dataTable.ext.buttons.pdfFlash.action.call(self, e, dt, button, config);
			} else if (button[0].className.indexOf("buttons-print") >= 0) {
				$.fn.dataTable.ext.buttons.print.action(e, dt, button, config);
			}
			dt.one("preXhr", function (e, s, data) {
				
				settings._iDisplayStart = oldStart;
				data.start = oldStart;
			});
			
			setTimeout(dt.ajax.reload, 0);
			
			return false;
		});
	});
	
	dt.ajax.reload();
}
var theme = "white";
$(document).ready(function () {
  $(".select2").select2();

  $("#selector-pid").change(function () {
    productFilter = JSON.stringify($(this).val());
    purchaseOrderTable.ajax.reload();
  });
  $("#status-pid").change(function () {
    statusFilter = JSON.stringify($(this).val());
    purchaseOrderTable.ajax.reload();
  });

  $("#estimate-date-picker")
    .daterangepicker({ orientation: "bottom" })
    .on("apply.daterangepicker", function (ev, picker) {
      startDateInput = picker.startDate.format("M/D/YYYY");
      endDateInput = picker.endDate.format("M/D/YYYY");
      const selectedDateRage = `${startDateInput} - ${endDateInput}`;
      $("#estimate-date-picks").val(selectedDateRage);
      isEstimateDate = true;
      purchaseOrderTable.ajax.reload();
    });

  $("#date-picker")
    .daterangepicker({ orientation: "bottom" })
    .on("apply.daterangepicker", function (ev, picker) {
      startDateInput = picker.startDate.format("M/D/YYYY");
      endDateInput = picker.endDate.format("M/D/YYYY");
      const selectedDateRage = `${startDateInput} - ${endDateInput}`;
      $("#date-picks").val(selectedDateRage);
      islastDate = true;
      purchaseOrderTable.ajax.reload();
    });
});
var purchaseOrderTable = $("#purchase-order-datatable").DataTable({
	"processing": true,
	searchDelay: 500,
	"serverSide": true,
	fixedHeader: true,
	lengthMenu: [[10, 25, 100], [10, 25, 100]],
	"filter": true,
	responsive: {
		details: false
	},
	keys: !0, language: {
		"decimal": ".",
		paginate: {
			previous: "<i class='mdi mdi-chevron-left'>", next: "<i class='mdi mdi-chevron-right'>"
		},
	},
	drawCallback: function () {
		$(".dataTables_paginate > .pagination").addClass("pagination-rounded");
	},
	"ajax": {
		"url": "GetPurchaseOrderList",
		"type": "POST",
		"datatype": "json",
		"data": function (d) {
			d.productFilter = productFilter;
			d.statusFilter = statusFilter;
			d.startDateInput = startDateInput;
			d.endDateInput = endDateInput;
			d.isEstimateDate = isEstimateDate;
			d.islastDate = islastDate;
		},
	},
	"dom": "<'row'<'col-sm-12 col-md-7'l><'col-sm-12 col-md-4'f><'col-sm-12 col-md-1'B>>" +
		"<'row'<'col-sm-12'tr>>" +
		"<'row'<'col-sm-12 col-md-5'i><'col-sm-12 col-md-7'p>>",
	columnDefs: [
		{ "visible": false, "targets": 0 }
	],
	"columns": [

		{ "title": "Purchase Order", "data": "purchaseOrderNumber", "name": "PurchaseOrderNumber", "autoWidth": true },
		{
			data: null,
			defaultContent: "<a class=\"mdi mdi-24px mdi-plus-circle\" style=\"color:" + theme + "\"></a>"
		},
		{ "data": "vendor.vendorName", "name": "VendorName", "autoWidth": true },
		{
			"data": "poStatus", "render": function (data) {
				if (data == 0) {
					return " <span class=\"badge rounded-pill bg-success\" style=\"font-size: 1.1em\">Draft</span>";
				} else if (data == 1) {
					return "Open Issued";
				} else if (data == 2) {
					return "<span class=\"badge rounded-pill bg-warning\" style=\"font-size: 1.1em\">In Progress</span>";
				} else if (data == 3) {
					return "<span class=\"badge rounded-pill bg-success\" style=\"font-size: 1.1em\">Closed</span>";
				} else if (data == 4) {
					return "<span class=\"badge rounded-pill bg-danger\" style=\"font-size: 1.1em\">Cancelled</span>";
				}
				else if (data == 5) {
					return "<span class=\"badge rounded-pill purplepill\" style=\"font-size: 1.1em\">Fully Received</span>";
				}
				else {
					return "Draft";
				}

			},
			"name": "POStatus", "autoWidth": true
		},
		{ "data": "referenceNumber", "name": "ReferenceNumber", "autoWidth": true },
		{ "data": "notes", "name": "Notes", "autoWidth": true },
		{
			"data": "orderDate", "name": "OrderDate", "autoWidth": true, "render": function (data) {
				return new Date(data).toLocaleDateString("en-US");
			}
		},
		{
			"data": "estimatedDate", "name": "EstimatedDate", "autoWidth": true, "render": function (data) {
				return new Date(data).toLocaleDateString("en-US");
			}
		},
		{
			"data": "modifyDate", "name": "ModifyDate", "autoWidth": true, "render": function (data) {
				return new Date(data).toLocaleDateString("en-US");
			}
		},
		{ "title": "File(s)", "data": "attachments", "name": "Attachments", "autoWidth": true },
		{
			"data": "isActive",
			"render": function (IsActive) {
				if (IsActive) {
					return "Yes";
				} else {
					return "No";
				}
			},
			"name": "IsActive",
			"autoWidth": true
		},
		{
			"title": "Actions",
			"data": {
				productId: "purchaseOrderId", permission: "permission"
			},
			mRender: function (data, type, row) {
				if (data.permission === "Yes") {
					let linkDetails = "<a  class=\"mdi mdi-24px mdi-book-information-variant\" href=/PurchaseOrders/Details/" + row.purchaseOrderId + "></a>";
					if (row.poStatus !== 3) {
						let linkEdit = "<a  class=\"mdi mdi-24px mdi-pencil\" href=/PurchaseOrders/Edit/" + row.purchaseOrderId + "></a>";
						return linkDetails + " " + linkEdit;
					}
					else {
						return linkDetails;
					}
						
				} else {
					let linkDetails = "<a  class=\"mdi mdi-24px mdi-book-information-variant\" href=/PurchaseOrders/Details/" + row.purchaseOrderId + "></a>";
					return linkDetails;
				}

			}
		},
	],
	"buttons": [
		{
			extend: "collection",
			text: "Export",
			className: "btn btn-dark",
			buttons: [
				{
					"extend": "copy",
					"titleAttr": "Copy",
					"action": newexportaction,
					exportOptions: {
						columns: ":not(.notexport)"
					}
				},
				, {
					"extend": "excel",
					"titleAttr": "Excel",
					"action": newexportaction,
					exportOptions: {
						columns: ":not(.notexport)"
					}
				},
				, {
					"extend": "csv",
					"titleAttr": "CSV",
					"action": newexportaction,
					exportOptions: {
						columns: ":not(.notexport)"
					}
				},
				, {
					"extend": "pdf",
					"titleAttr": "PDF",
					"action": newexportaction,
					exportOptions: {
						columns: ":not(.notexport)"
					}
				},
				, {
					"extend": "print",
					"titleAttr": "Print",
					"action": newexportaction,
					exportOptions: {
						columns: ":not(.notexport)"
					}
				}
			]
		}
	],
	order: [[6, 'desc']],

});

$("#purchase-order-datatable tbody").on("click", "tr", function () {
	purchaseOrderTable.responsive.recalc();
	var tr = $(this).closest("tr");
	var row = purchaseOrderTable.row(tr);
	var rowdata = purchaseOrderTable.row(tr).data();
	if (rowdata != undefined) {
		var rowpurchaseOrderNumber = rowdata.purchaseOrderNumber;

		if (row.child.isShown()) {
			// This row is already open - close it
			destroyChild(row);
			tr.removeClass("shown");
		} else {
			// Open this row
			createChild(row, rowpurchaseOrderNumber);
			tr.addClass("shown");
		}
	}
});

function createChild(row, rowpurchaseOrderNumber) {
	// This is the table we'll convert into a DataTable
	var table = $("<table class=\"display\" width=\"100%\"/>");
	var hiddenColumnsIndex = [];
	purchaseOrderTable.columns().every(function () {
		var column = this;
		var columnElement = column.nodes().to$().eq(0); // Get the first element of the column
		if (columnElement.css("display") === "none") {
			hiddenColumnsIndex.push(column.index());
		}
	});
	var hiddenColumns = "";
	hiddenColumnsIndex.forEach(function (index) {
		var columnTitle = purchaseOrderTable
			.column(index)
			.header()
			.textContent.trim();
		var cellNode = purchaseOrderTable.cell(row, index).node();
		hiddenColumns +=
			"<div style='display: inline-block;'>" +
			"<strong>" +
			columnTitle +
			":</strong> " +
			$(cellNode).html() +
			"<hr style='border-color: white;margin-top: 5%;'><br></div>";
	});

	row.child($("<div/>").append(hiddenColumns, table)).show();
	// Initialise as a DataTable
	table.DataTable({

		"processing": true,
		searchDelay: 500,
		"serverSide": true,
		"searching": false,
		responsive: {
			details: true,
		},
		lengthMenu: [[10, 25, 100], [10, 25, 100]],
		"filter": true,
		keys: !0, language: {
			"decimal": ".",
			paginate: {
				previous: "<i class='mdi mdi-chevron-left'>", next: "<i class='mdi mdi-chevron-right'>"
			},
		},
		drawCallback: function () {
			$(".dataTables_paginate > .pagination").addClass("pagination-rounded");
		},
		"ajax": {
			"url": "GetPOProducts",
			"type": "POST",
			"data": function (d) {
				d.PoNumber = rowpurchaseOrderNumber;
			},
			"datatype": "json"
		},
		columnDefs: [
			{ visible: false, targets: 0 }
		],
		"columns": [
			{ title: "Id", "data": "productId", "name": "productId", "autoWidth": true },
			{ title: "SKU", "data": "sku", "name": "sku", "autoWidth": true },
			{ title: "Cost", "data": "cost", "name": "cost", "autoWidth": true },
			{ title: "Quantity Ordered", "data": "quantity", "name": "quantity", "autoWidth": true },
			{ title: "Quantity Recieved", "data": "totalRecieved", "name": "totalRecieved", "autoWidth": true },
			{ title: "Discount", "data": "discount", "name": "discount", "autoWidth": true },
			{ title: "Total", "data": "total", "name": "total", "autoWidth": true },
		],
	});
}

function destroyChild(row) {
	var table = $("table", row.child());
	table.detach();
	table.DataTable().destroy();

	// And then hide the row
	row.child.hide();
}