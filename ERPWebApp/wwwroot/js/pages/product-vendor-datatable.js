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

$(document).ready(function () {
    var productVendorMappingTable = $("#product-vendor-mapping-datatable").DataTable({
        "processing": true,
        searchDelay: 500,
        "serverSide": true,
        fixedHeader: true,
        lengthMenu: [
            [10, 25, 100],
            [10, 25, 100]
        ],
        "filter": true,
        "ajax": {
            "url": "GetProductList",
            "type": "POST",
            "datatype": "json",
        },
        "dom": "<'row'<'col-xs-12 col-sm-5 col-md-5'l><'col-xs-12 col-sm-6 col-md-5 custom-mt-xs'f><'col-xs-12 col-sm-3 col-md-2 custom-mt-xs custom-mt-sm'B>>" +
            "<'row'<'col-sm-12'tr>>" +
            "<'row'<'col-sm-12 col-md-5'i><'col-sm-12 col-md-7'p>>",
        "columns": [

            {
                "data": "vendor.vendorName", "name": "VendorName", "autoWidth": true,
                "render": (data, type, row) => {
                    return '<a href="/Vendors/Details/' + row.vendorId + '">' + row.vendor.vendorName + '</a>';
                }
            },
            { "data": "vendorSku", "name": "VendorSku", "autoWidth": true },
            {
                "data": "product.sku", "name": "Sku", "autoWidth": true,
                "render": (data, type, row) => {
                    return '<a href="/Products/Details/' + row.productId + '">' + row.product.sku + '</a>';
                }

            },
            { "data": "product.description", "name": "Description", "autoWidth": true },
            {
                "data": (row) => { return row.product.productTags.map(x => x.description).join(','); },
                "render": (data, type, row) => {
                    if (type == 'export') return data;
                    debugger
                    if (row.product.productTags && row.product.productTags.length > 0) {
                        var div_elm = $('<div></div>');
                        row.product.productTags.forEach(x => {
                            var elm = `<span class="badge tag-pill" title="${x.tag.description}">
                                                    <span class="badge badge-lg badge-pill" style="background-color: ${x.tag.color};">${x.tag.description}</span>
                                                    <span style="background-color: ${x.tag.color};"></span>
                                                </span>`;
                            div_elm.append(elm);
                        });
                        return div_elm.prop('outerHTML');
                    }

                    return "N/A";
                },
                "name": "Tags",
                "autoWidth": true
            },
            { "data": "cost", "name": "Cost", "autoWidth": true },
            { "data": "leadTime", "name": "LeadTime", "autoWidth": true },
            {

                "data": "isPrimaryVendor",
                "render": function (data, type, row, meta) {
                    if (data) {
                        return '<a>Yes</a>';
                    } else {
                        return '<a>No</a>';
                    }
                }
            },
            { "data": "unitofMeasure", "name": "UnitofMeasure", "autoWidth": true },
            { "data": "term", "name": "Term", "autoWidth": true },
            {

                "data": "isRawMaterial",
                "render": function (data, type, row, meta) {
                    if (data) {
                        return '<a>Yes</a>';
                    } else {
                        return '<a>No</a>';
                    }
                }
            },
            {

                "data": {
                    productVendorMappingId: "productVendorMappingId", permission: "permission"
                },
                "render": function (data, type, row, meta) {
                    var linkDetails = '<a class="mdi mdi-24px mdi-book-information-variant" href=/ProductVendorMapping/Details/' + row.productVendorMappingId + '></a>';
                    linkDetails = linkDetails.replace("-1", row.productVendorMappingId);

                    if (data.permission == "Yes") {
                        var linkEdit = '<a  class="mdi mdi-24px mdi-pencil" href=/ProductVendorMapping/Edit/' + row.productVendorMappingId + '></a>';
                        linkEdit = linkEdit.replace("-1", row.productVendorMappingId);
                        var linkDelete = '<a  class="mdi mdi-24px mdi-delete" href=/ProductVendorMapping/Delete/' + row.productVendorMappingId + '></a>';
                        linkDelete = linkDelete.replace("-1", row.productVendorMappingId);

                        return linkDetails + " " + linkEdit + " " + linkDelete;
                    }

                    return linkDetails;
                },
                "autoWidth": true
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

        //preset visuals for the table
        keys: !0,
        language: {
            paginate: {
                previous: "<i class='mdi mdi-chevron-left'>",
                next: "<i class='mdi mdi-chevron-right'>"
            },

        },
        drawCallback: function () {
            $(".dataTables_paginate > .pagination").addClass("pagination-rounded");
        },
    });

    var vendorProductMappingTable = $("#vendor-product-mapping-datatable").DataTable({
        "processing": true,
        searchDelay: 500,
        "serverSide": true,
        fixedHeader: true,
        lengthMenu: [
            [10, 25, 100],
            [10, 25, 100]
        ],
        "filter": true,
        "ajax": {
            "url": "GetVendorList",
            "type": "POST",
            "datatype": "json",
        },
        "dom": "<'row'<'col-xs-12 col-sm-5 col-md-5'l><'col-xs-12 col-sm-6 col-md-5 custom-mt-xs'f><'col-xs-12 col-sm-3 col-md-2 custom-mt-xs custom-mt-sm'B>>" +
            "<'row'<'col-sm-12'tr>>" +
            "<'row'<'col-sm-12 col-md-5'i><'col-sm-12 col-md-7'p>>",
        "columns": [

            {
                "data": "product.sku", "name": "sku", "autoWidth": true,
                "render": (data, type, row) => {
                    return '<a href="/Products/Details/' + row.productId + '">' + row.product.sku + '</a>';
                }
            },
            {
                "data": "vendor.vendorName", "name": "vendorName", "autoWidth": true,
                "render": (data, type, row) => {
                    return '<a href="/Vendors/Details/' + row.vendorId + '">' + row.vendor.vendorName + '</a>';
                }
            },
            { "data": "vendorSku", "name": "VendorSku", "autoWidth": true },
            { "data": "product.description", "name": "Description", "autoWidth": true },
            { "data": "cost", "name": "Cost", "autoWidth": true },
            { "data": "leadTime", "name": "LeadTime", "autoWidth": true },
            {

                "data": "isPrimaryVendor",
                "render": function (data, type, row, meta) {
                    if (data) {
                        return '<a>Yes</a>';
                    } else {
                        return '<a>No</a>';
                    }
                }
            },
            { "data": "unitofMeasure", "name": "UnitofMeasure", "autoWidth": true },
            { "data": "term", "name": "Term", "autoWidth": true },
            {

                "data": "isRawMaterial",
                "render": function (data, type, row, meta) {
                    if (data) {
                        return '<a>Yes</a>';
                    } else {
                        return '<a>No</a>';
                    }
                }
            },
            {

                "data": {
                    productVendorMappingId: "productVendorMappingId", permission: "permission"
                },
                "render": function (data, type, row, meta) {
                    var linkDetails = '<a class="mdi mdi-24px mdi-book-information-variant" href=/ProductVendorMapping/Details/' + row.productVendorMappingId + '></a>';
                    linkDetails = linkDetails.replace("-1", row.productVendorMappingId);

                    if (data.permission == "Yes") {
                        var linkEdit = '<a  class="mdi mdi-24px mdi-pencil" href=/ProductVendorMapping/Edit/' + row.productVendorMappingId + '></a>';
                        linkEdit = linkEdit.replace("-1", row.productVendorMappingId);
                        var linkDelete = '<a  class="mdi mdi-24px mdi-delete" href=/ProductVendorMapping/Delete/' + row.productVendorMappingId + '></a>';
                        linkDelete = linkDelete.replace("-1", row.productVendorMappingId);

                        return linkDetails + " " + linkEdit + " " + linkDelete;
                    }

                    return linkDetails;
                },
                "autoWidth": true
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

        //preset visuals for the table
        keys: !0,
        language: {
            paginate: {
                previous: "<i class='mdi mdi-chevron-left'>",
                next: "<i class='mdi mdi-chevron-right'>"
            },

        },
        drawCallback: function () {
            $(".dataTables_paginate > .pagination").addClass("pagination-rounded");
        },
    });
});
