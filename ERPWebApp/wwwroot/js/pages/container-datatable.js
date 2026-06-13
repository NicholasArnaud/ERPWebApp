$(document).ready(function () {
    $(".select2").select2();

    var switchStatus = true;
    var containerTable = $("#product-container-datatable").DataTable({
        "processing": true,
        "serverSide": true,
        "filter": true,
        lengthMenu: [[10, 25, 100], [10, 25, 100]],
        "dom": "<'row'<'col-md-5 col-sm-5 col-xs-12'l><'col-md-5 col-sm-6 col-xs-12 custom-mt-xs'f><'col-md-2 col-sm-3 col-xs-12 custom-mt-xs custom-mt-sm'B>>" +
            "<'row'<'col-sm-12'tr>>" +
            "<'row'<'col-sm-12 col-md-5'i><'col-sm-12 col-md-7'p>>",
        "rowCallback": function (row, data, dataIndex, cells) {
            //Conditional Highlighting Model.Where(x=>x.Height*x.Width*x.Length<x.Products.Length*x.Products.Height*x.Products.Width*x.ContainerQuantity)
            var set = false;
            var pvmap = data["productVendorMappings"];
            var prod = pvmap["product"];
            var volumecon = data["length"];
            var volumeprod = data["length"];
            $('#lastconid').val(data["containerId"]);

            if (data["containerDiminsions"] == "Inches") {

                volumecon = (1 / (12 * 12 * 12));
                console.log(volumecon);
            }
            else if (data["containerDiminsions"] == "Feet") {
                volumecon = 1;
            }
            else if (data["containerDiminsions"] == "Centimeters") {
                volumecon = (1 / (2.54 * 12 * 2.54 * 12 * 2.54 * 12));
            }
            else if (data["containerDiminsions"] == "Meters") {
                volumecon = (100 * 100 * 100) / (2.54 * 12 * 2.54 * 12 * 2.54 * 12);
            }

            var proddims = prod["dimensionalUnit"];
            if (proddims == "Inches") {

                volumeprod = (1 / (12 * 12 * 12));
                console.log(volumecon);
            }
            else if (proddims == "Feet") {
                volumeprod = 1;
            }
            else if (proddims == "Centimeters") {
                volumeprod = (1 / (2.54 * 12 * 2.54 * 12 * 2.54 * 12));
            }
            else if (proddims == "Meters") {
                volumeprod = (100 * 100 * 100) / (2.54 * 12 * 2.54 * 12 * 2.54 * 12);
            }

            var prodlen = prod["length"];
            var prodwid = prod["width"];
            var prodhgt = prod["height"];
            if (data["length"] * data["width"] * data["height"] * volumecon < prodlen * prodwid * prodhgt * data["containerQuantity"] * volumeprod) {
                $(row).children(":nth-child(4)").addClass('bg-danger');
                $(row).children(":nth-child(4)").css('color', 'black');
                $(row).children(":nth-child(5)").addClass('bg-danger');
                $(row).children(":nth-child(5)").css('color', 'black');
                $(row).children(":nth-child(6)").addClass('bg-danger');
                $(row).children(":nth-child(6)").css('color', 'black');
                var displayStatus = document.getElementById("error-warning");
                displayStatus.style.display = 'block';
            }

        },
        keys: !0, language: {
            paginate: {
                previous: "<i class='mdi mdi-chevron-left'>", next: "<i class='mdi mdi-chevron-right'>"
            },
        }, drawCallback: function () {
            $(".dataTables_paginate > .pagination").addClass("pagination-rounded")
        },
        "ajax": {
            "url": "GetContainers",
            "type": "POST",
            "datatype": "json",
            "data": function (d) {
                d.sku = JSON.stringify($('#product-select').val()),
                d.vendor = JSON.stringify($('#vendor-select').val()),
                d.active = switchStatus;
            },
        },
        buttons: [
            {
                extend: 'collection',
                text: 'Export',
                buttons: [
                    'copy',
                    'excel',
                    'csv',
                    'pdf',
                    'print'
                ]
            }
        ],
        "columnDefs": [{
            "targets": [16],
            "visible": false,
            "searchable": false
        }],
        "columns": [
            { "data": "productVendorMappings.product.sku", "name": "ProductVendorMappings.Product.Sku", "autoWidth": true },
            { "data": "productVendorMappings.vendor.vendorName", "name": "ProductVendorMappings.Vendor.VendorName", "autoWidth": true },
            { "data": "productVendorMappings.product.description", "name": "ProductVendorMappings.Product.Description", "autoWidth": true },
            { "data": "containerQuantity", "name": "ContainerQuantity", "autoWidth": true },
            { "data": "length", "name": "Length", "autoWidth": true },
            { "data": "width", "name": "Width", "autoWidth": true },
            { "data": "height", "name": "Height", "autoWidth": true },
            {
                "data": "containerDiminsions", "render": function (ContainerDiminsions, type, full, meta) {
                    if (ContainerDiminsions == 0) {
                        return 'Inches';
                    }
                    else if (ContainerDiminsions == 1) {
                        return 'Feet';
                    }
                    else if (ContainerDiminsions == 2) {
                        return 'Centimeters';
                    }
                    else if (ContainerDiminsions == 3) {
                        return 'Meters';
                    }
                },
                "name": "ContainerDiminsions", "autoWidth": true
            },
            { "data": "productVendorMappings.product.altItemNumber", "name": "ProductVendorMappings.Product.AltItemNumber", "autoWidth": true },
            { "data": "productVendorMappings.product.onOrder", "name": "ProductVendorMappings.Product.OnOrder", "autoWidth": true },
            { "data": "containerCost", "name": "ContainerCost", "autoWidth": true },
            { "data": "productVendorMappings.cost", "name": "ProductVendorMappings.Cost", "autoWidth": true },
            { "data": "productVendorMappings.product.length", "name": "ProductVendorMappings.Product.Length", "autoWidth": true },
            { "data": "productVendorMappings.product.width", "name": "ProductVendorMappings.Product.Width", "autoWidth": true },
            { "data": "productVendorMappings.product.height", "name": "ProductVendorMappings.Product.Height", "autoWidth": true },
            {
                "data": "productVendorMappings.product.dimensionalUnit", "render": function (DimensionalUnit, type, full, meta) {
                    if (DimensionalUnit == 0) {
                        return 'Inches';
                    }
                    else if (DimensionalUnit == 1) {
                        return 'Feet';
                    }
                    else if (DimensionalUnit == 2) {
                        return 'Centimeters';
                    }
                    else if (DimensionalUnit == 3) {
                        return 'Meters';
                    }
                },
                "name": "ProductVendorMappings.Product.DimensionalUnit", "autoWidth": true
            },
            { "data": "containerId", "name": "ContainerId", "autoWidth": true },
            {
                "data": "containerId",
                "render": function (data, type, row, meta) {
                    var linkBarcode = '<a  class="mdi mdi-24px mdi-barcode" href=/ProductContainers/DownloadBarcode/' + row.containerId + '></a>';
                    var linkDetails = '<a  class="mdi mdi-24px mdi-book-information-variant" href=/ProductContainers/Details/' + row.containerId + '></a>';
                    linkDetails = linkDetails.replace("-1", row.productId);
                    var linkEdit = '<a  class="mdi mdi-24px mdi-pencil" href=/ProductContainers/Edit/' + row.containerId + '></a>';
                    linkEdit = linkEdit.replace("-1", row.productId);
                    var linkDelete = '<a  class="mdi mdi-24px mdi-delete" href=/ProductContainers/Delete/' + row.containerId + '></a>';
                    linkDelete = linkDelete.replace("-1", row.productId);
                    return linkBarcode +" "+ linkDetails + " " + linkEdit + " " + linkDelete;

                    return linkDetails;
                }
            }
        ]
    });

    $('#product-select').on('change', function () {
        containerTable.ajax.reload();
    });
    $('#product-select').on('select2:unselect', function () {
        $('#product-select').select2('open');
        $('#product-select').select2().trigger("select2:close");
        containerTable.ajax.reload();

    });
    $("#vendor-select").on('change', function () {
        containerTable.ajax.reload();
    });
    $("#vendor-select").on('select2:unselect', function () {
        $('#vendor-select').select2('open');
        $('#vendor-select').select2().trigger("select2:close");
        containerTable.ajax.reload();

    });

    $("#custom-switch-3").on('change', function () {
        if ($(this).is(':checked')) {
            switchStatus = $(this).is(':checked');
        }
        else {
            switchStatus = $(this).is(':checked');
        }
        containerTable.ajax.reload();
    });
});