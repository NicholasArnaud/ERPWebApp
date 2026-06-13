$(document).ready(function () {
    //register for file upload item click event
    $("#file-upload-option").click(function (event) {
        event.preventDefault()
        $("#userUpload").trigger('click');
    });

    var productTable = initializeProductTable();

    $("#department-pid, #subcat-pid, #producttag-pid")
        .select2({ multiple: true })
        .on("select2:select select2:unselect", function (e) {
            if (e.type === "select2:unselect") {
                $(this).select2("open").trigger("select2:close");
            }
            productTable.ajax.reload();
        });

    //register for file selection input change and upload the file directly
    $("#userUpload").change(function () {
        if ($("#userUpload").length) {
            var vidFileLength = $("#userUpload")[0].files.length;
            if (vidFileLength > 0) {
                $("#btnsubmit").trigger('click');
            }
        }
    });

    $('#btnsubmit').click(function () {
        showModal("File is being uploaded, please wait", "warning");
        $('#mySpinnerExcel, #mySpinnerExcelCon').show();
        $('#myModalBtn').hide();
    });


    handleSwitchChange("#custom-switch-2", function (switchStatus) {
        const url = 'Products/DeptList';

        $.getJSON(url, { id: switchStatus }).done(function (response) {
            const departmentDropdown = $('#department-pid');
            departmentDropdown.empty();
            if (response.length) {
                const options = response.map(item => `<option value="${item.value}">${item.text}</option>`);
                departmentDropdown.html(options.join(''));
                if (switchStatus) {
                    const selectedValues = response.map(item => item.value);
                    departmentDropdown.val(selectedValues).trigger('change');
                }
            }
            productTable.ajax.reload();
        });
    });

    handleSwitchChange("#custom-switch-3", function () {
        productTable.ajax.reload();
    });
});


function newexportaction(e, dt, button, config) {
    document.body.style.cursor = 'wait';
    var self = this;
    var oldStart = dt.settings()[0]._iDisplayStart;
    dt.one('preXhr', function (e, s, data) {
        // Just this once, load all data from the server
        data.start = 0;
        data.length = 2147483647;
        dt.one('preDraw', function (e, settings) {
            // Call the original action function
            if (button[0].className.indexOf('buttons-copy') >= 0) {
                $.fn.dataTable.ext.buttons.copyHtml5.action.call(self, e, dt, button, config);
            } else if (button[0].className.indexOf('buttons-excel') >= 0) {
                $.fn.dataTable.ext.buttons.excelHtml5.available(dt, config) ? $.fn.dataTable.ext.buttons.excelHtml5.action.call(self, e, dt, button, config) : $.fn.dataTable.ext.buttons.excelFlash.action.call(self, e, dt, button, config);
            } else if (button[0].className.indexOf('buttons-csv') >= 0) {
                $.fn.dataTable.ext.buttons.csvHtml5.available(dt, config) ? $.fn.dataTable.ext.buttons.csvHtml5.action.call(self, e, dt, button, config) : $.fn.dataTable.ext.buttons.csvFlash.action.call(self, e, dt, button, config);
            } else if (button[0].className.indexOf('buttons-pdf') >= 0) {
                $.fn.dataTable.ext.buttons.pdfHtml5.available(dt, config) ? $.fn.dataTable.ext.buttons.pdfHtml5.action.call(self, e, dt, button, config) : $.fn.dataTable.ext.buttons.pdfFlash.action.call(self, e, dt, button, config);
            } else if (button[0].className.indexOf('buttons-print') >= 0) {
                $.fn.dataTable.ext.buttons.print.action(e, dt, button, config);
            }
            dt.one('preXhr', function (e, s, data) {
                // DataTables thinks the first item displayed is index 0, but we're not drawing that.
                // Set the property to what it was before exporting.
                settings._iDisplayStart = oldStart;
                data.start = oldStart;
            });
            

            setTimeout(function() {
                // Reload the grid with the original page. Otherwise, API functions like table.cell(this) don't work properly.
                dt.ajax.reload
                document.body.style.cursor = '';
            }, 500);

            // Prevent rendering of the full data to the DOM
            return false;
        });
    });
    // Requery the server with the new one-time export settings
    dt.ajax.reload();
}


const initializeProductTable = () => {

    var tableDom = `<'row'<'col-md-5 col-sm-5 col-xs-12'l>
                            <'col-md-5 col-sm-6 col-xs-12 custom-mt-xs'f>
                            <'col-md-2 col-sm-3 col-xs-12 custom-mt-xs custom-mt-sm'B>> 
                    <'row'<'col-sm-12'tr>> 
                    <'row'<'col-sm-12 col-md-5'i>
                            <'col-sm-12 col-md-7'p>>`;

    return $("#product-datatable").DataTable({
        responsive: true,
        "processing": true,
        searchDelay: 500,
        "serverSide": true,
        fixedHeader: true,
        pageLength: 100,
        lengthMenu: [
            [50, 100, 200, 300, 400, 500],
            [50, 100, 200, 300, 400, 500]
        ],
        "filter": true,
        "dom": tableDom,
        "createdRow": function (row, data, dataIndex) {
            //Conditional Highlighting
            if (data[7] == "<a>Yes</a>") {
                $(row).children(":nth-child(8)").addClass('bg-primary');
                $(row).children(":nth-child(8)").css('color', 'black');
            }
            if (data[9] == "<a>Yes</a>") {
                $(row).children(":nth-child(9)").addClass('bg-primary');
                $(row).children(":nth-child(9)").css('color', 'black');
            }
            if (data[10] == "<a>Yes</a>") {
                $(row).children(":nth-child(10)").addClass('bg-primary');
                $(row).children(":nth-child(10)").css('color', 'black');
            }
            if (data[11] == "<a>Yes</a>") {
                $(row).children(":nth-child(11)").addClass('bg-primary');
                $(row).children(":nth-child(11)").css('color', 'black');
            }
        },
        keys: !0,
        language: {
            paginate: {
                previous: "<i class='mdi mdi-chevron-left'>",
                next: "<i class='mdi mdi-chevron-right'>"
            },
        },
        drawCallback: function () {
            $(".dataTables_paginate > .pagination").addClass("pagination-rounded")
        },
        "columnDefs": [
            {
                "targets": [1], "visible": false, "searchable": false
            },
            {
                "targets": [5], "visible": true, "searchable": true, className: "costColumn"
            },
            {
                "targets": [12], "visible": false, "searchable": false
            },
            {
                "targets": [13], "visible": false, "searchable": false
            },
            {
                "targets": [14], "visible": false, "searchable": false
            },
            {
                "targets": [15], "visible": false, "searchable": false
            }
        ],
        "ajax": {
            "url": "GetMyProducts",
            "type": "POST",
            "datatype": "json",
            "data": function (d) {
                d.department = JSON.stringify($('#department-pid').val());
                d.subcat = JSON.stringify($('#subcat-pid').val());
                d.producttag = JSON.stringify($('#producttag-pid').val());
                d.active = $("#custom-switch-3").is(':checked');
                d.isProduction = $("#custom-switch-2").is(':checked');
            },
            "error": function (xhr, textStatus, errorThrown) {
                if (xhr.status === 401) {
                    alert("401 Authorization Required: You are not authorized to access this resource.")
                    window.location.href = "/Login";
                } else {
                    alert("An error occurred: " + errorThrown);
                }
            }
        },
        "columns": [
            {
                "data": "sku", "name": "Sku", "autoWidth": true
            },
            {
                "data": "productId",
                "name": "ProductId",
                "autoWidth": true
            },
            {
                "data": "description", "name": "Description", "autoWidth": true
            },
            {
                "data": "subCategory.description",
                "defaultContent": "",
                "name": "SubCategory.Description",
                "autoWidth": true
            },
            {
                "data": "fulfillmentCost",
                "name": "FulfillmentCost",
                "autoWidth": true
            },
            {
                "data": { cost: "cost", costpermission: "costpermission" },
                "render": function (data, type, full, meta) {
                    if (data.costpermission === "yes") {
                        $('#costcolumn').show();
                        $('.costColumn').show();
                        return data.cost;
                    } else {
                        $('#costcolumn').hide();
                        $('.costColumn').hide();
                        return '';
                    }
                }
            },
            {
                "data": "overseasCost", "name": "AltItemNumber", "OverseasCost": true
            },
            {
                "data": "laborCost",
                "name": "LaborCost",
                "autoWidth": true
            },
            {
                "data": "altItemNumber", "name": "AltItemNumber", "autoWidth": true
            },
            {
                "data": (row) => {
                    return row && row.alternateProduct ? row.alternateProduct.sku : '';
                },
                "render": (data, type, row) => {
                    if (!row || !row.alternateProduct) { return null; }

                    return '<a href=/Products/Details/' + row.alternateProduct.productId + '>' + row.alternateProduct.sku + '</a>';
                },
                "name": "AlternateProduct",
                "autoWidth": true
            },
            {
                "data": "onOrder",
                "name": "OnOrder",
                "autoWidth": true
            },
            {
                "data": "leadTime", "name": "LeadTime", "autoWidth": true
            },
            {
                "data": "isEmbroidery",
                "name": "IsEmbroidery",
                "autoWidth": true
            },
            {
                "data": "isEngraving", "name": "IsEngraving", "autoWidth": true
            },
            {
                "data": "isMetal",
                "name": "IsMetal",
                "autoWidth": true
            },
            {
                "data": "isUv", "name": "IsUv", "autoWidth": true
            },
            {
                "data": "stockTotalAvailable",
                "name": "StockTotalAvailable",
                "autoWidth": true
            },
            {
                "data": (row) => { return row.productTags.map(x => x.description).join(','); },
                "render": (data, type, row) => {
                    if (type == 'export') return data;

                    if (row.productTags && row.productTags.length > 0) {
                        var div_elm = $('<div></div>');
                        row.productTags.forEach(tag => {
                            var elm = `<span class="badge tag-pill" title="${tag.description}">
                                    <span class="badge badge-lg badge-pill" style="background-color: ${tag.color};">${tag.description}</span>
                                    <span style="background-color: ${tag.color};"></span>
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
            {
                "data": "isActive", "render": function (IsActive, type, full, meta) {
                    if (IsActive) {
                        return 'Yes';
                    } else {
                        return 'No';
                    }
                },
                "name": "IsActive", "autoWidth": true
            },
            {
                "data": "weightAmount",
                "render": $.fn.dataTable.render.number(',', '.', 2),
                "name": "WeightAmount",
                "autoWidth": true
            },
            {
                "data": "weightUnit", "render": function (WeightUnit, type, full, meta) {
                    if (WeightUnit == 0) {
                        return 'Ounce(s)';
                    } else if (WeightUnit == 1) {
                        return 'Pound(s)';
                    }
                },
                "name": "WeightUnit",
                "autoWidth": true
            },
            {
                "data": "length", render: $.fn.dataTable.render.number(',', '.', 2), "name": "Length", "autoWidth": true
            },
            {
                "data": "width", render: $.fn.dataTable.render.number(',', '.', 2), "name": "Width", "autoWidth": true
            },
            {
                "data": "height", render: $.fn.dataTable.render.number(',', '.', 2), "name": "Height", "autoWidth": true
            },
            {
                "data": "dimensionalUnit", "render": function (DimensionalUnit, type, full, meta) {
                    if (DimensionalUnit == 0) {
                        return 'Inches';
                    } else if (DimensionalUnit == 1) {
                        return 'Feet';
                    } else if (DimensionalUnit == 2) {
                        return 'Centimeters';
                    } else if (DimensionalUnit == 3) {
                        return 'Meters';
                    }
                },
                "name": "DimensionalUnit",
                "autoWidth": true
            },
            {
                "data": "isExternalProduct",
                "render": function (IsExternal, type, full, meta) {
                    if (IsExternal) {
                        return 'Yes';
                    } else {
                        return 'No';
                    }
                },
                "name": "External Site Product?",
                "autoWidth": true
            },
            {
                "data": { departments: "departments" },
                "render": (data, type, full) => {
                    if (data.departments != null && data.departments.length > 0) {
                        return data.departments.map(x => x.departmentName).join(', ');
                    }

                    return "";
                },
                "name": "Departments",
                "autoWidth": true
            },
            {
                "data": "minInventory", "name": "MinInventory", "autoWidth": true
            },
            {
                "data": "maxInventory", "name": "MaxInventory", "autoWidth": true
            },
            {
                title: 'Image',
                "data": { imageSrc: "imageSrc", imageSrcDtl: "imageSrcDtl" },
                "render": function (data, type, full) {
                    if (type == 'export') return data.imageSrcDtl;

                    if (data.imageSrc != "/File?id=" && data.imageSrc != "") {
                        return '<a target="_blank" href="' + data.imageSrcDtl + '"><img src="' + data.imageSrc + '"alt="image"  style="width:60%;"></></a>';
                    } else {
                        return '<a style="color:#fff;" class="mdi mdi-24px mdi-package-variant-closed"></a>';
                    }
                },
                "width": "4%"
            },
            {
                title: 'Actions',
                "data": {
                    productId: "productId", permission: "permission"
                },
                "render": function (data, type, row, meta) {
                    //var linkDetails = ' <a id="test" onclick="getProductDetailFields(-1)" class="mdi mdi-24px mdi-book-information-variant" data-bs-toggle="modal" data-bs-target="#my-detail-modal" style="color:white"></a>';
                    var linkBarcode = '<a class="mdi mdi-24px mdi-barcode" href=/Products/DownloadBarcode/' + row.productId + '></a>';
                    var linkDetails = '<a class="mdi mdi-24px mdi-book-information-variant" href=/Products/Details/' + row.productId + '></a>';
                    linkDetails = linkDetails.replace("-1", row.productId);

                    if (data.permission == "yes") {
                        var linkEdit = '<a  class="mdi mdi-24px mdi-pencil" href=/Products/Edit/' + row.productId + '></a>';
                        linkEdit = linkEdit.replace("-1", row.productId);
                        var linkDelete = '<a  class="mdi mdi-24px mdi-delete" href=/Products/Delete/' + row.productId + '></a>';
                        linkDelete = linkDelete.replace("-1", row.productId);

                        return linkBarcode + " " + linkDetails + " " + linkEdit + " " + linkDelete;
                    }

                    return linkBarcode + " " + linkDetails;
                },
                "width": "8%"
            }
        ],
        "buttons": [
            {
                extend: 'collection', text: 'Export', buttons: [
                    {
                        "extend": 'copy', "titleAttr": 'Copy', "action": newexportaction, exportOptions: {
                            columns: ':not(.notexport)'
                        }
                    },
                    {
                        "extend": 'excel', "titleAttr": 'Excel', "action": newexportaction, exportOptions: {
                            columns: ':not(.notexport)',
                            orthogonal: 'export'
                        }
                    },
                    {
                        "extend": 'csv', "titleAttr": 'CSV', "action": newexportaction, exportOptions: {
                            columns: ':not(.notexport)',
                            orthogonal: 'export'
                        }
                    },
                    {
                        "extend": 'pdf', "titleAttr": 'PDF', "action": newexportaction, exportOptions: {
                            columns: ':not(.notexport)'
                        }
                    },
                    {
                        "extend": 'print', "titleAttr": 'Print', "action": newexportaction, exportOptions: {
                            columns: ':not(.notexport)'
                        }
                    }
                ]
            }
        ],
    });
}

const handleSwitchChange = (switchId, callback) => {
    $(switchId).on('change', function () {
        const switchStatus = $(this).is(':checked');
        callback(switchStatus);
    });
}

const showModal = (message, type) => {
    if (message) {
        $('#mySpinnerExcel, #mySpinnerExcelCon').hide();
        $('#myModalBtn').show();

        const modalBody = $("#myModalErrorAlertBody");
        modalBody.text(message || "");

        $('#modalHeadeError').toggle(type === "error");
        $('#modalHeadeSuccess').toggle(type === "success");
        $('#modalHeadeWarning').toggle(type === "warning");

        $('#myModalError').modal({
            backdrop: 'static',
            keyboard: false,
        }).modal("show");
    }
}