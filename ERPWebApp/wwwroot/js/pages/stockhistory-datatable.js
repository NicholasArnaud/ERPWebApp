var start_date;
var end_date;
var DateFilterFunction = (function (aData) {

    var dateStart = parseDateValue(start_date);
    var dateEnd = parseDateValue(end_date);

    var evalDate = Date.parse(aData[9]);

    if ((isNaN(dateStart) && isNaN(dateEnd)) || (isNaN(dateStart) && evalDate <= dateEnd) || (dateStart <= evalDate && isNaN(dateEnd)) || (dateStart <= evalDate && evalDate <= dateEnd)) {
        return true;
    }
    return false;
});

var SkuFilterFunction = (function (aData) {
    if ($('#product-select' == Sku)) {
        return true;
    }
    return false;
});
var LocationFilterFunction = (function (aData) {
    if ($('#location-select' == Location)) {
        return true;
    }
    return false;
});

function parseDateValue(rawDate) {
    var dateArray = rawDae.split("/");
    var parsedDate = new Date(dateArray[2], parseInt(dateArray[1]) - 1, dateArray[0]);  // -1 because months are from 0 to 11   
    return parsedDate;
}

$(document).ready(function () {
    function newexportaction(e, dt, button, config) {
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
                // Reload the grid with the original page. Otherwise, API functions like table.cell(this) don't work properly.
                setTimeout(dt.ajax.reload, 0);
                // Prevent rendering of the full data to the DOM
                return false;
            });
        });
        // Requery the server with the new one-time export settings
        dt.ajax.reload();
    }

    var testtable = $("#stockhistory-datatable").DataTable({
        "processing": true,
        searchDelay: 500,
        "serverSide": true,
        lengthMenu: [[10, 25, 100], [10, 25, 100]],
        "filter": true,
        "dom": "<'row'<'col-sm-12 col-md-7'l><'col-sm-12 col-md-4'f><'col-sm-12 col-md-1'B>>" + "<'row'<'col-sm-12'tr>>" + "<'row'<'col-sm-12 col-md-5'i><'col-sm-12 col-md-7'p>>",
        order: [[0, 'desc'], [2, 'asc']],
        keys: !0,
        language: {
            paginate: {
                previous: "<i class='mdi mdi-chevron-left'>", next: "<i class='mdi mdi-chevron-right'>"
            },
        },
        drawCallback: function () {
            $(".dataTables_paginate > .pagination").addClass("pagination-rounded")
        },
        "columnDefs": [{
            "targets": [2], "visible": false, "searchable": false
        }, {
            "targets": [3], "visible": false, "searchable": false
        }, {
            "targets": [6], "visible": false, "searchable": false
        },],
        "ajax": {
            "url": "GetStockHistory", "type": "POST", "datatype": "json", //Takes the values of the selected filters
            "data": function (d) {
                d.Sku = $("#product-select").val();
                d.Location = $("#location-select").val();
                d.DateRange = $("#selectedValue").text();
            },
        },
        "columns": [{
            "data": "modifyDate", //Formats from default DateTime to only the Date
            "render": function (data) {
                var date = new Date(data);
                var month = date.getMonth() + 1;
                var day = date.getDate();
                var hour = date.getHours();
                var minute = date.getMinutes();
                var second = date.getSeconds();
                return (month.toString().length > 1 ? month : "0" + month) + "/" + (day.toString().length > 1 ? day : "0" + day) + "/" + date.getFullYear() + " " + (hour.toString().length > 1 ? hour : "0" + hour) + ":" + (minute.toString().length > 1 ? minute : "0" + minute) + ":" + (second.toString().length > 1 ? second : "0" + second);
            }, "name": "Modify Date", "autoWidth": true
        }, {"data": "modifyByUser", "name": "Modify By User", "autoWidth": true}, {
            "data": "stockId",
            "name": "Stock Id",
            "autoWidth": true
        }, {"data": "productId", "name": "Product Id", "autoWidth": true}, {
            "data": "sku",
            "name": "Product Sku",
            "autoWidth": true
        }, {"data": "description", "name": "Product Name", "autoWidth": true}, {
            "data": "locationId",
            "name": "Location Id",
            "autoWidth": true
        }, {"data": "location", "name": "Location", "autoWidth": true}, {
            "data": "totalAvailable",
            "name": "Total Available",
            "autoWidth": true
        },], //export button
        "buttons": [{
            extend: 'collection', text: 'Export', buttons: [{
                "extend": 'copy', "titleAttr": 'Copy', "action": newexportaction, exportOptions: {
                    columns: ':not(.notexport)'
                }
            }, {
                "extend": 'excel', "titleAttr": 'Excel', "action": newexportaction, exportOptions: {
                    columns: ':not(.notexport)'
                }
            }, {
                "extend": 'csv', "titleAttr": 'CSV', "action": newexportaction, exportOptions: {
                    columns: ':not(.notexport)'
                }
            }, {
                "extend": 'pdf', "titleAttr": 'PDF', "action": newexportaction, exportOptions: {
                    columns: ':not(.notexport)'
                }
            }, {
                "extend": 'print', "titleAttr": 'Print', "action": newexportaction, exportOptions: {
                    columns: ':not(.notexport)'
                }
            }]
        }],
    });

    $('#datesearch').daterangepicker({
        autoUpdateInput: false, format: 'MM/DD/YYYY'
    });

    $('#datesearch').on('apply.daterangepicker', function (ev, picker) {

        $('#selectedValue').text(picker.startDate.format('MM/DD/YYYY') + ' - ' + picker.endDate.format('MM/DD/YYYY'));

        start_date = picker.startDate.format('MM/DD/YYYY');
        end_date = picker.endDate.format('MM/DD/YYYY');
        $.fn.dataTableExt.afnFiltering.push(DateFilterFunction);
        testtable.draw();
    });

    $('#datesearch').on('cancel.daterangepicker', function (ev, picker) {
        $(this).val('');
        start_date = '';
        end_date = '';
        $.fn.dataTable.ext.search.splice($.fn.dataTable.ext.search.indexOf(DateFilterFunction, 1));
        testtable.draw();
    });
    $("#product-select").select2().on("select2:select", function (ev) {
        $('#product-select').val();
        $.fn.dataTableExt.afnFiltering.push(SkuFilterFunction);
        testtable.draw();
    });
    $("#location-select").select2().on("select2:select", function (ev) {
        $('#location-select').val();
        $.fn.dataTableExt.afnFiltering.push(LocationFilterFunction);
        testtable.draw();
    });
});
