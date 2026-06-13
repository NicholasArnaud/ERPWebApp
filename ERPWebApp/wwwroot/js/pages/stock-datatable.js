var departmentSelect = $("#department-select-id");
var siteSelect = $("#site-select-id");
var subCategorySelect = $("#subCategory-select-id");
var productTagsSelect = $("#producttag-select-id");
var vendorSelect = $("#vendor-select-id");
var stockTable = undefined;



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
            }
            else if (button[0].className.indexOf("buttons-excel") >= 0) {
                $.fn.dataTable.ext.buttons.excelHtml5.available(dt, config) ?
                    $.fn.dataTable.ext.buttons.excelHtml5.action.call(self, e, dt, button, config) :
                    $.fn.dataTable.ext.buttons.excelFlash.action.call(self, e, dt, button, config);
            }
            else if (button[0].className.indexOf("buttons-csv") >= 0) {
                $.fn.dataTable.ext.buttons.csvHtml5.available(dt, config) ?
                    $.fn.dataTable.ext.buttons.csvHtml5.action.call(self, e, dt, button, config) :
                    $.fn.dataTable.ext.buttons.csvFlash.action.call(self, e, dt, button, config);
            }
            else if (button[0].className.indexOf("buttons-pdf") >= 0) {
                $.fn.dataTable.ext.buttons.pdfHtml5.available(dt, config) ?
                    $.fn.dataTable.ext.buttons.pdfHtml5.action.call(self, e, dt, button, config) :
                    $.fn.dataTable.ext.buttons.pdfFlash.action.call(self, e, dt, button, config);
            }
            else if (button[0].className.indexOf("buttons-print") >= 0) {
                $.fn.dataTable.ext.buttons.print.action(e, dt, button, config);
            }
            dt.one("preXhr", function (e, s, data) {
                // DataTables thinks the first item displayed is index 0, but we're not drawing that.
                // Set the property to what it was before exporting.
                settings._iDisplayStart = oldStart;
                data.start = oldStart;
            });
            // Reload the grid with the original page. Otherwise, API functions like table.cell(this) don't work properly.
            setTimeout(dt.ajax.reload, 0);
            // Prevent rendering of the full data to the DOM
            //test me
            return false;
        });
    });
    // Requery the server with the new one-time export settings
    dt.ajax.reload();
}

$(document).ready(function () {

    var theme = "white";
    stockTable = $("#stock-datatable").DataTable(
        {
            "processing": true,
            searchDelay: 500,
            "serverSide": true,
            pageLength: 100,
            lengthMenu: [[50, 100, 200, 300, 400, 500], [50, 100, 200, 300, 400, 500]],
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
                "url": "GetProducts",
                "type": "Get",
                "datatype": "json",
                "data": function (d) {
                    d.subCategoryId = subCategorySelect.val();
                    d.siteId = siteSelect.val();
                    d.departmentId = departmentSelect.val();
                    d.productTagId = productTagsSelect.val();
                    d.zeroQtyStock = $("#zero-quantity-checkbox").is(":checked");
                    d.vendorId = vendorSelect.val();
                },
                "error": function (xhr, textStatus, errorThrown) {
                    if (xhr.status === 401) {
                        alert("401 Authorization Required: You are not authorized to access this resource.");
                        window.location.href = "/Login";
                    } else {
                        alert("An error occurred: " + errorThrown);
                    }
                }
            },
            "dom": "<'row'<'col-md-5 col-sm-5 col-xs-12'l><'col-md-5 col-sm-6 col-xs-12 custom-mt-xs'f><'col-md-2 col-sm-3 col-xs-12 custom-mt-xs custom-mt-sm'B>>" + "<'row'<'col-sm-12'tr>>" + "<'row'<'col-sm-12 col-md-5'i><'col-sm-12 col-md-7'p>>",
            columnDefs: [
                { "visible": false, "targets": 0 }
            ],
            "columns": [
                { "data": "productId", "name": "ProductId", "autoWidth": true },
                {
                    className: 'dt-control',
                    data: null,
                    defaultContent: ""
                },
                { "data": "sku", "name": "Sku", "autoWidth": true },
                { "data": "description", "name": "Description", "autoWidth": true },
                {
                    "data": (row) => { return row.productTags.map(x => x.tag.description).join(","); }, "name": "ProductTag", "autoWidth": true,
                    "render": (data, type, row) => {
                        if (row.productTags && row.productTags.length > 0) {
                            var div_elm = $("<div></div>");
                            row.productTags.forEach(ptag => {
                                var tag = ptag.tag;
                                var elm = `<span class="badge tag-pill" title="${tag.description}">
									<span class="badge badge-lg badge-pill" style="background-color: ${tag.color};">${tag.description}</span>
									<span style="background-color: ${tag.color};"></span>
								</span>`;
                                div_elm.append(elm);
                            });
                            return div_elm.prop("outerHTML");
                        }
                        return "N/A";
                    }
                },
                { "data": "stockTotalAvailable", "name": "StockTotalAvailable", "autoWidth": true },
                { "data": "stockTotalAvailableFilter", "name": "StockTotalAvailableFilter", "autoWidth": true },
                { "data": "onOrder", "name": "OnOrder", "autoWidth": true }
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
        });

    $("#selectPageLength").change(function (e) {
        document.getElementById("myPageSize").value = $("#selectPageLength :selected").val();
    });

    $("#stock-datatable tbody").on("click", "tr", function () {
        var tr = $(this).closest("tr");
        var row = stockTable.row(tr);
        var rowdata = stockTable.row(tr).data();
        if (rowdata != undefined) {
            var rowsku = rowdata.sku;

            if (row.child.isShown()) {
                // This row is already open - close it
                destroyChild(row);
                tr.removeClass("shown");
            } else {
                // Open this row
                createChild(row, rowsku);
                tr.addClass("shown");
            }
        }
    });
    $('#inactive-checkbox').change(function () { stockTable.ajax.reload(); });
    $('#zero-quantity-checkbox').change(function () { stockTable.ajax.reload(); });

});

function createChild(row, rowsku) {
    // This is the table we'll convert into a DataTable
    var table = $("<table class=\"display\" width=\"100%\"/>");
    // Display it the child row
    row.child(table).show();
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
            "url": "GetProductsStock",
            "type": "POST",
            "data": function (d) {
                d.sku = rowsku;
            },
            "datatype": "json"
        },
        columnDefs: [
            { visible: false, targets: 0 }
        ],
        "columns": [
            { title: "Id", "data": "stockId", "name": "StockId", "autoWidth": true },
            { title: "Location", "data": "location.locationName", "name": "Location.LocationName", "autoWidth": true },
            {
                title: "Site Name",
                "data": "location.sites.siteName",
                "name": "Location.Sites.SiteName",
                "autoWidth": true
            },
            { title: "Total Available", "data": "totalAvailable", "name": "TotalAvailable", "autoWidth": true },
            {
                title: "Departments",
                "data": "products.departments[, ].departmentName",
                "name": "Products.Departments",
                "autoWidth": true
            },
            { title: "Recently Readded", "data": "recentlyReadded", "name": "RecentlyReadded", "autoWidth": true },
            { title: "Is Primary", "data": "isPrimary", "name": "IsPrimary", "autoWidth": true },
            {
                title: "Actions",
                "data": { stockId: "stockId", permission: "products.permission", siteId: "location.sites.siteId" },
                mRender: function (data, type, row, meta) {
                    var linkBarcode = '<a style=\"color:white\" class=\"mdi mdi-24px mdi-barcode\" href=/Stocks/DownloadBarcode/' + row.stockId + '></a>';
                    var linkDetails = "<a style=\"color:white\" class=\"mdi mdi-24px mdi-book-information-variant\" href=/Stocks/Details/" + row.stockId + "></a>";
                    linkDetails = linkDetails.replace("-1", row.stockId);
                    if (data.products.permission == "Yes") {
                        var linkEdit = "<a style=\"color:white\" class=\"mdi mdi-24px mdi-pencil\" href=/Stocks/Edit/" + row.stockId + "></a>";
                        linkEdit = linkEdit.replace("-1", row.stockId);
                        var linkDelete = "<a style=\"color:white\" class=\"mdi mdi-24px mdi-delete\" href=/Stocks/Delete/" + row.stockId + "></a>";
                        linkDelete = linkDelete.replace("-1", row.stockId);
                        return linkBarcode + " " + linkDetails + " " + linkEdit + " " + linkDelete;
                    } else if (data.products.permission == "InvShip" && (data.location.siteId == 2 || data.location.siteId == 49 || data.location.siteId == 48 || data.location.siteId == 1)) {
                        var linkEdit = "<a style=\"color:white\" class=\"mdi mdi-24px mdi-pencil\" href=/Stocks/Edit/" + row.stockId + "></a>";
                        linkEdit = linkEdit.replace("-1", row.stockId);
                        var linkDelete = "<a style=\"color:white\" class=\"mdi mdi-24px mdi-delete\" href=/Stocks/Delete/" + row.stockId + "></a>";
                        linkDelete = linkDelete.replace("-1", row.stockId);
                        return linkBarcode + " " + linkDetails + " " + linkEdit + " " + linkDelete;
                    } else if (data.products.permission == "Inv" && (data.location.siteId == 2)) {
                        var linkEdit = "<a style=\"color:white\" class=\"mdi mdi-24px mdi-pencil\" href=/Stocks/Edit/" + row.stockId + "></a>";
                        linkEdit = linkEdit.replace("-1", row.stockId);
                        var linkDelete = "<a style=\"color:white\" class=\"mdi mdi-24px mdi-delete\" href=/Stocks/Delete/" + row.stockId + "></a>";
                        linkDelete = linkDelete.replace("-1", row.stockId);
                        return linkBarcode + " " + linkDetails + " " + linkEdit + " " + linkDelete;
                    } else if (data.products.permission == "Ship" && ((data.location.siteId == 49) || (data.location.siteId == 48) || (data.location.siteId == 1))) {
                        var linkEdit = "<a style=\"color:white\" class=\"mdi mdi-24px mdi-pencil\" href=/Stocks/Edit/" + row.stockId + "></a>";
                        linkEdit = linkEdit.replace("-1", row.stockId);
                        var linkDelete = "<a style=\"color:white\" class=\"mdi mdi-24px mdi-delete\" href=/Stocks/Delete/" + row.stockId + "></a>";
                        linkDelete = linkDelete.replace("-1", row.stockId);
                        return linkBarcode + " " + linkDetails + " " + linkEdit + " " + linkDelete;
                    }
                    return linkBarcode + " " + linkDetails;
                }
            }],
    });
}

function destroyChild(row) {
    var table = $("table", row.child());
    table.detach();
    table.DataTable().destroy();

    // And then hide the row
    row.child.hide();
}

function applyFilters() {
    stockTable.ajax.reload();
    updateHeader();
}
function debounce(func, wait, immediate) {
    var timeout;
    return function () {
        var context = this, args = arguments;
        var later = function () {
            timeout = null;
            if (!immediate) func.apply(context, args);
        };
        var callNow = immediate && !timeout;
        clearTimeout(timeout);
        timeout = setTimeout(later, wait);
        if (callNow) func.apply(context, args);
    };
}

// Apply the debounce function to the applyFilters function  
var debouncedApplyFilters = debounce(applyFilters, 300);

// Attach the event listeners to the select elements  
departmentSelect.on("change", debouncedApplyFilters);
siteSelect.on("change", debouncedApplyFilters);
subCategorySelect.on("change", debouncedApplyFilters);
productTagsSelect.on("change", debouncedApplyFilters);
vendorSelect.on("change", debouncedApplyFilters);


function updateHeader() {
    document.getElementById("my-filtered-select").innerHTML =
        "Site: " + $("#site-select-id option:selected").text() + "<br/>" +
        "Department: " + $("#department-select-id option:selected").text() + "<br/>" +
        "Sub Category: " + $("#subCategory-select-id option:selected").text() + "<br/>" +
        "Product Tag: " + $("#producttag-select-id option:selected").text();
}
