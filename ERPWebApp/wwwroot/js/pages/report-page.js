window.onload = function () {
    $("#prod-select").hide();
    $("#date-search").hide();
    $("#site-select").hide();
    $("#location-select").hide();
    $("#stockhistory-prod-select").hide();
    $("#subcategory-select").hide();
    $("#department-select").hide();
    $("#date-picker-panel").hide();
    $("#weekly-date-picker").hide();
    $("#rerun-report-sec").hide();
    $("#shipstationstore-select").hide();
}
const initializeProductSelect2 = () => {
    $("#sku-select").select2({
        placeholder: 'Search Products...',
        minimumInputLength: 3,
        allowClear: true,
        ajax: {
            url: '/Report/SearchProducts',
            dataType: 'json',
            delay: 300,
            data: params => ({ searchTerm: params.term }),
            processResults: data => ({ results: data })
        }
    });
};

$(document).ready(function () {

    $("#queries").on('select2:select', function () {
        $("#prod-select").hide();

        if ($('#queries').val() == 4) {
            $("#prod-select").show();
            $("#sku-select").select2('destroy');
            initializeProductSelect2();
        }
    });

    function get1ReportData() {
        //function for Query #1
        var url = '/GetQueries';

        $('#my-loading-modal').modal({
            backdrop: 'static',
            keyboard: false,
        });
        $('#my-loading-modal').modal('show');
        $('#my-spinner-modal').show();
        $('#my-spinner-modal-spinner').show();
        $('#my-modal-btn').hide();
        $('#modal-header-warning').show();
        $('#modal-header-success').hide();
        $('#warning-banner').hide().delay(5000).fadeIn('slow');

        console.log(url);
        console.log($('#queries').val());
        console.log($('#datesearch').text());

        $.get(url, {
            Query: $('#queries').val(),
            StartDate: $('#date-search').data('daterangepicker').startDate.format('MM-DD-YYYY'),
            EndDate: $('#date-search').data('daterangepicker').endDate.format('MM-DD-YYYY')
        },
            function (data) {
                console.log(data);
                $('#report-datatable').load(url, function (response) {
                    console.log("PartialTable Reloaded");

                    $('#my-loading-modal').modal('hide');
                    $('#my-spinner-modal').hide();
                    $('#my-spinner-modal-spinner').hide();
                    $('#my-modal-btn').show();
                    $('#modal-header-warning').hide();
                    $('#modal-header-success').show();

                    destroyDataTable();
                    $("#report-datatable").empty();
                    $("#report-datatable").DataTable({
                        "processing": true,
                        searchDelay: 500,
                        "paging": true,
                        destroy: true,
                        lengthMenu: [[10, 25, 100], [10, 25, 100]],
                        "pageLength": 10,
                        "filter": true,
                        "dom": "<'row'<'col-sm-12 col-md-7'l><'col-sm-12 col-md-4'f><'col-sm-12 col-md-1'B>>" +
                            "<'row'<'col-sm-12'tr>>" +
                            "<'row'<'col-sm-12 col-md-5'i><'col-sm-12 col-md-7'p>>",
                        order: [[0, 'desc'], [2, 'asc']],
                        keys: !0, language: {
                            paginate: {
                                previous: "<i class='mdi mdi-chevron-left'>", next: "<i class='mdi mdi-chevron-right'>"
                            },
                        }, drawCallback: function () {
                            $(".dataTables_paginate > .pagination").addClass("pagination-rounded")
                        },
                        "columnDefs": [{},],
                        "data": data.data,
                        "columns": [
                            {
                                "data": "average",
                                "render": function formatToCurrency(average) {
                                    return "$" + (average).toFixed(2).replace(/\d(?=(\d{3})+\.)/g, '$&,');
                                },
                                "title": "Average Shipment Cost",
                                "autowidth": true,
                                "width": "33%"
                            },
                            { "data": "sku", "title": "Product Sku", "autowidth": true, "width": "33%" },
                            { "data": "description", "title": "Product Description", "autowidth": true, "width": "34%" },
                        ],
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
                        ]
                    });
                });
            });
    }
    function get2ReportData() {
        //function for Query #2
        var url = '/GetQueries';

        $('#my-loading-modal').modal({
            backdrop: 'static',
            keyboard: false,
        });
        $('#my-loading-modal').modal('show');
        $('#my-spinner-modal').show();
        $('#my-spinner-modal-spinner').show();
        $('#my-modal-btn').hide();
        $('#modal-header-warning').show();
        $('#modal-header-success').hide();
        $('#warning-banner').hide().delay(5000).fadeIn('slow');

        console.log(url);
        console.log($('#queries').val());
        console.log($('#date-search').text());

        $.get(url, {
            Query: $('#queries').val(),
            StartDate: $('#date-search').data('daterangepicker').startDate.format('MM-DD-YYYY'),
            EndDate: $('#date-search').data('daterangepicker').endDate.format('MM-DD-YYYY')
        },
            function (data) {
                console.log(data);
                $('#report-datatable').load(url, function (response) {
                    console.log("PartialTable Reloaded");

                    $('#my-loading-modal').modal('hide');
                    $('#my-spinner-modal').hide();
                    $('#my-spinner-modal-spinner').hide();
                    $('#my-modal-btn').show();
                    $('#modal-header-warning').hide();
                    $('#modal-header-success').show();

                    destroyDataTable();
                    $("#report-datatable").empty();
                    $("#report-datatable").DataTable({
                        "processing": true,
                        searchDelay: 500,
                        "paging": true,
                        destroy: true,
                        lengthMenu: [[10, 25, 100], [10, 25, 100]],
                        "pageLength": 10,
                        "filter": true,
                        "dom": "<'row'<'col-sm-12 col-md-7'l><'col-sm-12 col-md-4'f><'col-sm-12 col-md-1'B>>" +
                            "<'row'<'col-sm-12'tr>>" +
                            "<'row'<'col-sm-12 col-md-5'i><'col-sm-12 col-md-7'p>>",
                        order: [[0, 'desc'], [2, 'asc']],
                        keys: !0, language: {
                            paginate: {
                                previous: "<i class='mdi mdi-chevron-left'>", next: "<i class='mdi mdi-chevron-right'>"
                            },
                        }, drawCallback: function () {
                            $(".dataTables_paginate > .pagination").addClass("pagination-rounded")
                        },
                        "columnDefs": [{},],
                        "data": data.data,
                        "columns": [
                            {
                                "data": "average",
                                "render": function formatToCurrency(average) {
                                    return "$" + (average).toFixed(2).replace(/\d(?=(\d{3})+\.)/g, '$&,');
                                },
                                "title": "Average Shipment Cost",
                                "autowidth": true
                            },
                            { "data": "service", "title": "Shipping Service", "autowidth": true },
                            { "data": "carrierCode", "title": "Carrier Code", "autowidth": true },
                        ],
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
                        ]
                    });
                });
            });
    }
    function destroyDataTable() {
        if ($.fn.DataTable.isDataTable('#report-datatable')) {
            $('#report-datatable').DataTable().destroy();
        }
    }
    function get3ReportData() {
        //function for Query #3
        var url = '/GetQueries';

        $('#my-loading-modal').modal({
            backdrop: 'static',
            keyboard: false,
        });
        $('#my-loading-modal').modal('show');
        $('#my-spinner-modal').show();
        $('#my-spinner-modal-spinner').show();
        $('#my-modal-btn').hide();
        $('#modal-header-warning').show();
        $('#modal-header-success').hide();
        $('#warning-banner').hide().delay(5000).fadeIn('slow');

        console.log(url);
        console.log($('#queries').val());
        console.log($('#date-search').text());

        $.get(url, {
            Query: $('#queries').val(),
            StartDate: $('#date-search').data('daterangepicker').startDate.format('MM-DD-YYYY'),
            EndDate: $('#date-search').data('daterangepicker').endDate.format('MM-DD-YYYY'),
        },
            function (data) {
                console.log(data);
                $('#report-datatable').load(url, function (response) {
                    console.log("PartialTable Reloaded");

                    $('#my-loading-modal').modal('hide');
                    $('#my-spinner-modal').hide();
                    $('#my-spinner-modal-spinner').hide();
                    $('#my-modal-btn').show();
                    $('#modal-header-warning').hide();
                    $('#modal-header-success').show();

                    destroyDataTable();
                    $("#report-datatable").empty();
                    $("#report-datatable").DataTable({
                        "processing": true,
                        searchDelay: 500,
                        "paging": true,
                        destroy: true,
                        lengthMenu: [[10, 25, 100], [10, 25, 100]],
                        "pageLength": 10,
                        "filter": true,
                        "dom": "<'row'<'col-sm-12 col-md-7'l><'col-sm-12 col-md-4'f><'col-sm-12 col-md-1'B>>" +
                            "<'row'<'col-sm-12'tr>>" +
                            "<'row'<'col-sm-12 col-md-5'i><'col-sm-12 col-md-7'p>>",
                        order: [[0, 'desc'], [2, 'asc']],
                        keys: !0, language: {
                            paginate: {
                                previous: "<i class='mdi mdi-chevron-left'>", next: "<i class='mdi mdi-chevron-right'>"
                            },
                        }, drawCallback: function () {
                            $(".dataTables_paginate > .pagination").addClass("pagination-rounded")
                        },
                        "columnDefs": [{},],
                        "data": data.data, // Pass the data received from the first AJAX request    
                        "columns": [
                            { "data": "amount", "title": "Amount", "autowidth": true },
                            { "data": "sku", "title": "Product Sku", "autowidth": true },
                            { "data": "description", "title": "Product Description", "autowidth": true },
                            { "data": "department", "title": "Department", "autowidth": true },
                        ],
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
                        ]
                    });
                });
            });
    }
    function get4ReportData() {
        //function for Query #4
        var url = '/GetQueries';

        $('#my-loading-modal').modal({
            backdrop: 'static',
            keyboard: false,
        });
        $('#my-loading-modal').modal('show');
        $('#my-spinner-modal').show();
        $('#my-spinner-modal-spinner').show();
        $('#my-modal-btn').hide();
        $('#modal-header-warning').show();
        $('#modal-header-success').hide();
        $('#warning-banner').hide().delay(5000).fadeIn('slow');

        console.log(url);
        console.log($('#queries').val());
        console.log($('#date-search').text());

        $.get(url, {
            Query: $('#queries').val(),
            ProductId: $('#sku-select').val(),
            StartDate: $('#date-search').data('daterangepicker').startDate.format('MM-DD-YYYY'),
            EndDate: $('#date-search').data('daterangepicker').endDate.format('MM-DD-YYYY')
        },
            function (data) {
                console.log(data);
                $('#report-datatable').ready(function () {
                    console.log("PartialTable Reloaded");

                    $('#my-loading-modal').modal('hide');
                    $('#my-spinner-modal').hide();
                    $('#my-spinner-modal-spinner').hide();
                    $('#my-modal-btn').show();
                    $('#modal-header-warning').hide();
                    $('#modal-header-success').show();

                    destroyDataTable();
                    $("#report-datatable").empty();
                    var table = $("#report-datatable").DataTable({
                        "processing": true,
                        searchDelay: 500,
                        "paging": true,
                        destroy: true,
                        lengthMenu: [[10, 25, 100], [10, 25, 100]],
                        "pageLength": 10,
                        "filter": true,
                        "dom": "<'row'<'col-sm-12 col-md-7'l><'col-sm-12 col-md-4'f><'col-sm-12 col-md-1'B>>" +
                            "<'row'<'col-sm-12'tr>>" +
                            "<'row'<'col-sm-12 col-md-5'i><'col-sm-12 col-md-7'p>>",
                        order: [[0, 'desc'], [2, 'asc']],
                        keys: !0, language: {
                            paginate: {
                                previous: "<i class='mdi mdi-chevron-left'>", next: "<i class='mdi mdi-chevron-right'>"
                            },
                        }, drawCallback: function () {
                            $(".dataTables_paginate > .pagination").addClass("pagination-rounded")
                        },
                        "columnDefs": [{},],
                        "data": data.data, // Pass the data received from the first AJAX request  
                        "columns": [
                            { "data": "amount", "title": "Amount", "autowidth": true },
                            { "data": "sku", "title": "Product Sku", "autowidth": true },
                            { "data": "description", "title": "Product Description", "autowidth": true },
                        ],
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
                        ]
                    });


                });
            });
    }
    function get5ReportData() {
        var baseUrl = '/GetQueries';

        $('#my-loading-modal').modal({
            backdrop: 'static',
            keyboard: false,
        });
        $('#my-loading-modal').modal('show');


        console.log($('#queries').val());
        console.log($('#datesearch').text());

        var queryParams = {
            Query: $('#queries').val(),
            StartDate: $('#date-search').data('daterangepicker').startDate.format('MM-DD-YYYY'),
            EndDate: $('#date-search').data('daterangepicker').endDate.format('MM-DD-YYYY')
        };

        var queryString = $.param(queryParams);

        var url = baseUrl + '?' + queryString;

        window.open(url, '_blank');
        $('#my-loading-modal').modal('hide');
    }
    function getStockHistoryReportData(dateval) {
        //function for Query #1
        var url = '/GetQueries';

        $('#my-loading-modal').modal({
            backdrop: 'static',
            keyboard: false,
        });
        $('#my-loading-modal').modal('show');
        $('#my-spinner-modal').show();
        $('#my-spinner-modal-spinner').show();
        $('#my-modal-btn').hide();
        $('#modal-header-warning').show();
        $('#modal-header-success').hide();
        $('#warning-banner').hide().delay(5000).fadeIn('slow');

        console.log(url);
        console.log($('#queries').val());
        console.log($('#datesearch').text());

        $.get(url, {
            Query: $('#queries').val(),
            SiteId: $('#siteId-select').val(),
            StartDate: dateval,
            EndDate: dateval,
            locationId: $('#locationId-select').val(),
        },
            function (data) {
                $('#my-loading-modal').modal('hide');
                $('#my-spinner-modal').hide();
                $('#my-spinner-modal-spinner').hide();
                $('#my-modal-btn').show();
                $('#modal-header-warning').hide();
                $('#modal-header-success').show();

                destroyDataTable();
                $("#report-datatable").empty();
                $("#report-datatable").DataTable({
                    "processing": true,
                    searchDelay: 500,
                    "paging": true,
                    destroy: true,
                    lengthMenu: [[10, 25, 100], [10, 25, 100]],
                    "pageLength": 10,
                    "filter": true,
                    "dom": "<'row'<'col-sm-12 col-md-7'l><'col-sm-12 col-md-4'f><'col-sm-12 col-md-1'B>>" +
                        "<'row'<'col-sm-12'tr>>" +
                        "<'row'<'col-sm-12 col-md-5'i><'col-sm-12 col-md-7'p>>",
                    order: [[0, 'desc'], [2, 'asc']],
                    keys: !0, language: {
                        paginate: {
                            previous: "<i class='mdi mdi-chevron-left'>", next: "<i class='mdi mdi-chevron-right'>"
                        },
                    }, drawCallback: function () {
                        $(".dataTables_paginate > .pagination").addClass("pagination-rounded")
                    },
                    "columnDefs": [{},],
                    "data": data.data,
                    "columns": [

                        { "data": "date", "title": "Modify Date", "autowidth": true, "width": "15%" },
                        { "data": "user", "title": "Modified By User", "autowidth": true, "width": "15%" },
                        { "data": "sku", "title": "Product Sku", "autowidth": true, "width": "30%" },
                        { "data": "description", "title": "Product Name", "autowidth": true, "width": "40%" },
                        { "data": "location", "title": "Location", "autowidth": true, "width": "34%" },
                        { "data": "totalAvailable", "title": "Total Available", "autowidth": true, "width": "34%" }
                    ],
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
                    ]
                });
            });
    }
    function getOnHandReport() {
        var url = '/GetQueries';

        $('#my-loading-modal').modal({
            backdrop: 'static',
            keyboard: false,
        });
        $('#my-loading-modal').modal('show');
        $('#my-spinner-modal').show();
        $('#my-spinner-modal-spinner').show();
        $('#my-modal-btn').hide();
        $('#modal-header-warning').show();
        $('#modal-header-success').hide();
        $('#warning-banner').hide().delay(5000).fadeIn('slow');

        console.log(url);
        console.log($('#queries').val());
        console.log($('#siteId-select').text());
        $.get(url, {
            Query: $('#queries').val(),
            ProductId: $('#sku-select').val(),
            SiteId: $('#siteId-select').val(),
        },
            function (data) {
                console.log(data);
                $('#report-datatable').load(url, function (response) {
                    console.log("PartialTable Reloaded");

                    $('#my-loading-modal').modal('hide');
                    $('#my-spinner-modal').hide();
                    $('#my-spinner-modal-spinner').hide();
                    $('#my-modal-btn').show();
                    $('#modal-header-warning').hide();
                    $('#modal-header-success').show();

                    destroyDataTable();
                    $("#report-datatable").empty();
                    reportdata = $("#report-datatable").DataTable({
                        "processing": true,
                        searchDelay: 500,
                        "paging": true,
                        destroy: true,
                        lengthMenu: [[10, 25, 100], [10, 25, 100]],
                        "pageLength": 10,
                        "filter": true,
                        "dom": "<'row'<'col-sm-12 col-md-7'l><'col-sm-12 col-md-4'f><'col-sm-12 col-md-1'B>>" +
                            "<'row'<'col-sm-12'tr>>" +
                            "<'row'<'col-sm-12 col-md-5'i><'col-sm-12 col-md-7'p>>",
                        order: [[0, 'desc'], [2, 'asc']],
                        keys: !0, language: {
                            paginate: {
                                previous: "<i class='mdi mdi-chevron-left'>", next: "<i class='mdi mdi-chevron-right'>"
                            },
                        }, drawCallback: function () {
                            $(".dataTables_paginate > .pagination").addClass("pagination-rounded")
                        },
                        "columnDefs": [{},],
                        "data": data.data,
                        "columns": [
                            { "data": "siteName", "title": "Site", "autowidth": true, "width": "33%" },
                            { "data": "locationName", "title": "Location", "autowidth": true, "width": "33%" },
                            { "data": "sku", "title": "Sku", "autowidth": true, "width": "33%" },
                            { "data": "description", "title": "Description", "autowidth": true, "width": "33%" },
                            { "data": "onHand", "title": "OnHand Amount", "autowidth": true, "width": "33%" },
                            {
                                "data": "totalCost",
                                "render": function formatToCurrency(totalCost) {
                                    return "$" + (totalCost).toFixed(2).replace(/\d(?=(\d{3})+\.)/g, '$&,');
                                },
                                "title": "Total Cost", "autowidth": true, "width": "34%"
                            },
                        ],
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
                        ]
                    });

                });


            }
        );

    }
    function getCycleCountReportData() {
        var url = '/GetQueries';

        $('#my-loading-modal').modal({
            backdrop: 'static',
            keyboard: false,
        });
        $('#my-loading-modal').modal('show');
        $('#my-spinner-modal').show();
        $('#my-spinner-modal-spinner').show();
        $('#my-modal-btn').hide();
        $('#modal-header-warning').show();
        $('#modal-header-success').hide();
        $('#warning-banner').hide().delay(5000).fadeIn('slow');

        console.log(url);
        console.log($('#queries').val());
        console.log($('#datesearch').text());

        $.get(url, {
            Query: $('#queries').val(),
            StartDate: $('#date-search').data('daterangepicker').startDate.format('MM-DD-YYYY'),
            EndDate: $('#date-search').data('daterangepicker').endDate.format('MM-DD-YYYY'),
            locationId: $('#locationId-select').val(),

        },
            function (data) {
                console.log(data);
                $('#report-datatable').load(url, function (response) {
                    console.log("PartialTable Reloaded");

                    $('#my-loading-modal').modal('hide');
                    $('#my-spinner-modal').hide();
                    $('#my-spinner-modal-spinner').hide();
                    $('#my-modal-btn').show();
                    $('#modal-header-warning').hide();
                    $('#modal-header-success').show();

                    destroyDataTable();
                    $("#report-datatable").empty();
                    $("#report-datatable").DataTable({
                        "processing": true,
                        searchDelay: 500,
                        "paging": true,
                        destroy: true,
                        lengthMenu: [[10, 25, 100], [10, 25, 100]],
                        "pageLength": 10,
                        "filter": true,
                        "dom": "<'row'<'col-sm-12 col-md-7'l><'col-sm-12 col-md-4'f><'col-sm-12 col-md-1'B>>" +
                            "<'row'<'col-sm-12'tr>>" +
                            "<'row'<'col-sm-12 col-md-5'i><'col-sm-12 col-md-7'p>>",
                        order: [[0, 'desc'], [2, 'asc']],
                        keys: !0, language: {
                            paginate: {
                                previous: "<i class='mdi mdi-chevron-left'>", next: "<i class='mdi mdi-chevron-right'>"
                            },
                        }, drawCallback: function () {
                            $(".dataTables_paginate > .pagination").addClass("pagination-rounded")
                        },
                        "columnDefs": [{},],
                        "data": data.data,
                        "columns": [

                            { "data": "sku", "title": "Product Sku", "autowidth": true, "width": "30%" },
                            { "data": "user", "title": "Modified By User", "autowidth": true, "width": "15%" },
                            { "data": "date", "title": "Last Cycle Count", "autowidth": true, "width": "40%" },
                        ],
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
                        ]
                    });
                });
            });
    }
    function getProductStockReport(dateval) {
        var url = '/GetQueries';
        $('#my-loading-modal').modal({
            backdrop: 'static',
            keyboard: false,
        });
        $('#my-loading-modal').modal('show');
        $('#my-spinner-modal').show();
        $('#my-spinner-modal-spinner').show();
        $('#my-modal-btn').hide();
        $('#modal-header-warning').show();
        $('#modal-header-success').hide();
        $('#warning-banner').hide().delay(5000).fadeIn('slow');
        console.log(url);
        console.log($('#queries').val());
        console.log($('#datesearch').text());
        $.get(url, {
            Query: $('#queries').val(),
            StartDate: dateval,
            EndDate: dateval,
            SiteId: $('#siteId-select').val(),
        },
            function (data) {

                let totalAverage = 0;
                data.data.forEach(item => {
                    totalAverage += item.average;
                });
                const totalAmountDisplay = document.getElementById("totalAmountDisplay");
                totalAmountDisplay.textContent = `Total Cost: $${totalAverage}`;
                $('#report-datatable').load(url, function (response) {
                    console.log("PartialTable Reloaded");
                    $('#my-loading-modal').modal('hide');
                    $('#my-spinner-modal').hide();
                    $('#my-spinner-modal-spinner').hide();
                    $('#my-modal-btn').show();
                    $('#modal-header-warning').hide();
                    $('#modal-header-success').show();
                    destroyDataTable();
                    $("#report-datatable").empty();
                    $("#report-datatable").DataTable({
                        "processing": true,
                        searchDelay: 500,
                        "paging": true,
                        destroy: true,
                        lengthMenu: [[10, 25, 100], [10, 25, 100]],
                        "pageLength": 10,
                        "filter": true,
                        "dom": "<'row'<'col-sm-12 col-md-7'l><'col-sm-12 col-md-4'f><'col-sm-12 col-md-1'B>>" +
                            "<'row'<'col-sm-12'tr>>" +
                            "<'row'<'col-sm-12 col-md-5'i><'col-sm-12 col-md-7'p>>",
                        order: [[0, 'desc'], [2, 'asc']],
                        keys: !0, language: {
                            paginate: {
                                previous: "<i class='mdi mdi-chevron-left'>", next: "<i class='mdi mdi-chevron-right'>"
                            },
                        }, drawCallback: function () {
                            $(".dataTables_paginate > .pagination").addClass("pagination-rounded")
                        },
                        "columnDefs": [{},],
                        "data": data.data,
                        "columns": [
                            { "data": "siteName", "title": "Site Name", "autowidth": true },
                            { "data": "locationName", "title": "Location", "autowidth": true },
                            { "data": "sku", "title": "Product Sku", "autowidth": true},
                            { "data": "description", "title": "Product Description", "autowidth": true},
                            { "data": "maxInventoryAmount", "title": "Max Inventory Amount", "autowidth": true },
                            { "data": "primaryVendorName", "title": "Primary Vendor Name", "autowidth": true },
                            { "data": "totalAvailable", "title": "Total Available", "autowidth": true},
                            {
                                "data": "totalCost",
                                "render": function formatToCurrency(totalCost) {
                                    return "$" + (totalCost).toFixed(2).replace(/\d(?=(\d{3})+\.)/g, '$&,');
                                },
                                "title": "Cost",
                                "autowidth": true
                            },

                        ],
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
                        ]
                    });
                });
            });
    }
    function getInventoryBalanceReport() {
        //function for Query #1
        var url = '/GetQueries';

        $('#my-loading-modal').modal({
            backdrop: 'static',
            keyboard: false,
        });
        $('#my-loading-modal').modal('show');
        $('#my-spinner-modal').show();
        $('#my-spinner-modal-spinner').show();
        $('#my-modal-btn').hide();
        $('#modal-header-warning').show();
        $('#modal-header-success').hide();
        $('#warning-banner').hide().delay(5000).fadeIn('slow');

        console.log(url);
        console.log($('#queries').val());
        console.log($('#datesearch').text());

        $.get(url, {
            Query: $('#queries').val(),
            ProductId: $('#sku-select').val(),
            StartDate: $('#date-search').data('daterangepicker').startDate.format('MM-DD-YYYY'),
            EndDate: $('#date-search').data('daterangepicker').endDate.format('MM-DD-YYYY')
        },
            function (data) {
                console.log(data);
                $('#report-datatable').load(url, function (response) {
                    console.log("PartialTable Reloaded");

                    $('#my-loading-modal').modal('hide');
                    $('#my-spinner-modal').hide();
                    $('#my-spinner-modal-spinner').hide();
                    $('#my-modal-btn').show();
                    $('#modal-header-warning').hide();
                    $('#modal-header-success').show();

                    destroyDataTable();
                    $("#report-datatable").empty();
                    $("#report-datatable").DataTable({
                        "processing": true,
                        searchDelay: 500,
                        "paging": true,
                        destroy: true,
                        lengthMenu: [[10, 25, 100], [10, 25, 100]],
                        "pageLength": 10,
                        "filter": true,
                        "dom": "<'row'<'col-sm-12 col-md-7'l><'col-sm-12 col-md-4'f><'col-sm-12 col-md-1'B>>" +
                            "<'row'<'col-sm-12'tr>>" +
                            "<'row'<'col-sm-12 col-md-5'i><'col-sm-12 col-md-7'p>>",
                        order: [[0, 'desc'], [2, 'asc']],
                        keys: !0, language: {
                            paginate: {
                                previous: "<i class='mdi mdi-chevron-left'>", next: "<i class='mdi mdi-chevron-right'>"
                            },
                        }, drawCallback: function () {
                            $(".dataTables_paginate > .pagination").addClass("pagination-rounded")
                        },
                        "columnDefs": [{},],
                        "data": data.data,
                        "columns": [
                            { "data": "sku", "title": "Product Sku", "autowidth": true, "width": "33%" },
                            { "data": "description", "title": "Product Description", "autowidth": true, "width": "34%" },
                            {
                                "data": "totalAvailable",
                                "render": function formatToCurrency(totalAvailable) {
                                    return "$" + (totalAvailable).toFixed(2).replace(/\d(?=(\d{3})+\.)/g, '$&,');
                                },
                                "title": "Total Available",
                                "autowidth": true,
                                "width": "33%"
                            },
                            {
                                "data": "shipStationOrders",
                                "render": function formatToCurrency(shipStationOrders) {
                                    return "$" + (shipStationOrders).toFixed(2).replace(/\d(?=(\d{3})+\.)/g, '$&,');
                                },
                                "title": "Ship Station Orders",
                                "autowidth": true,
                                "width": "33%"
                            },
                            {
                                "data": "orderDifference",
                                "render": function formatToCurrency(orderDifference) {
                                    return "$" + (orderDifference).toFixed(2).replace(/\d(?=(\d{3})+\.)/g, '$&,');
                                },
                                "title": "Drder Difference",
                                "autowidth": true,
                                "width": "33%"
                            }
                        ],
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
                        ]
                    });
                });
            });
    }

    function getYearlyProductSoldReportData() {

        var url = '/GetYearlyShippedProductReport';

        $('#my-loading-modal').modal({
            backdrop: 'static',
            keyboard: false,
        });
        $('#my-loading-modal').modal('show');
        $('#my-spinner-modal').show();
        $('#my-spinner-modal-spinner').show();
        $('#my-modal-btn').hide();
        $('#modal-header-warning').show();
        $('#modal-header-success').hide();
        $('#warning-banner').hide().delay(5000).fadeIn('slow');

        $.get(url, {
            Query: $('#queries').val(),
        }, function (data) {
            $('#my-loading-modal').modal('hide');
            $('#my-spinner-modal').hide();
            $('#my-spinner-modal-spinner').hide();
            $('#my-modal-btn').show();
            $('#modal-header-warning').hide();
            $('#modal-header-success').show();

            if (!data || data.length === 0) {
                alert('No data available for this report.');
                return;
            }

            let transformedData = {};
            let columnLabels = [];

            data.forEach(item => {
                if (!transformedData[item.sku]) {
                    transformedData[item.sku] = { sku: item.sku };
                }
                transformedData[item.sku][item.label] = item.quantity;
                if (!columnLabels.includes(item.label)) {
                    columnLabels.push(item.label);
                }
            });

            Object.keys(transformedData).forEach(sku => {
                columnLabels.forEach(label => {
                    if (transformedData[sku][label] === undefined) {
                        transformedData[sku][label] = 0;
                    }
                });
            });

            let tableData = Object.values(transformedData);

            let columns = [{ "data": "sku", "title": "SKU", "autowidth": true }];
            columnLabels.forEach(label => {
                columns.push({
                    "data": label,
                    "title": label,
                    "render": function (data, type, row) {
                        return row[label] !== undefined ? row[label] : 0;
                    },
                    "autowidth": true
                });
            });

            destroyDataTable();
            $("#report-datatable").empty();
            $("#report-datatable").DataTable({
                "processing": true,
                searchDelay: 500,
                "paging": true,
                destroy: true,
                lengthMenu: [[10, 25, 100], [10, 25, 100]],
                "pageLength": 10,
                "filter": true,
                "dom": "<'row'<'col-sm-12 col-md-7'l><'col-sm-12 col-md-4'f><'col-sm-12 col-md-1'B>>" +
                    "<'row'<'col-sm-12'tr>>" +
                    "<'row'<'col-sm-12 col-md-5'i><'col-sm-12 col-md-7'p>>",
                "columns": columns,
                "data": tableData,
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
                ]
            });
        }).fail(function (xhr) {
            $('#my-loading-modal').modal('hide');
            $('#my-spinner-modal').hide();
            $('#my-spinner-modal-spinner').hide();
            alert("An error occurred while fetching the report.");
        });
    }


    $("#queries").on('select2:select', function () {
        $("#prod-select").hide();
        $("#date-search").hide();
        $("#site-select").hide();
        $("#location-select").hide();
        $("#stockhistory-prod-select").hide();
        $("#subcategory-select").hide();
        $("#department-select").hide();
        $("#date-picker-panel").hide();
        $("#weekly-date-picker").hide();
        $("#rerun-report-sec").hide();
        if ($('#queries').val() == 4) {
            $("#prod-select").show();
            initDate(true);

        }
        if ($('#queries').val() != 4) {
            $("#prod-select").hide();
        }
        if ($('#queries').val() < 6 && $('#queries').val() > 0) {

            initDate(true);
        }
        if ($('#queries').val() == 4 && $("#selectedValue").text() != "" && $("#sku-select").val() != -1) {
            get4ReportData();
        }
        else if ($('#queries').val() == -1) {
            $("#prod-select").hide();
        }
        else if ($("#selectedValue").text() != "" && $('#queries').val() == 1) {
            $("#prod-select").hide();
            get1ReportData();
        }
        else if ($("#selectedValue").text() != "" && $('#queries').val() == 2) {
            $("#prod-select").hide();
            get2ReportData();
        }
        else if ($("#selectedValue").text() != "" && $('#queries').val() == 3) {
            $("#prod-select").hide();
            get3ReportData();
        }
        else if ($('#queries').val() == 6) {
            $("#site-select").show();
            $("#date-search").hide();
            $("#prod-select").hide();
            $("#sku-select").hide();
        }
        else if ($('#queries').val() == 7) {
            $("#prod-select").show();
            $("#site-select").hide();
            $("#date-search").hide();
            getInventoryBalanceReport();
        }
        else if ($('#queries').val() == 8) {
            $("#prod-select").hide();
            $("#date-search").hide();
            $("#site-select").hide();
            $("#location-select").show();
            initDate(false);
        }
        else if ($('#queries').val() == 9) {
            $("#site-select").show();
            initDate(false);
            $("#prod-select").hide();
        }
        else if ($('#queries').val() == 10) {
            $("#prod-select").hide();
            initDate(true);
            $("#site-select").hide();
            $("#location-select").show();
        }
        else if ($('#queries').val() == 11) {
            initDate(true);
            $("#stockhistory-prod-select").show();
            $("#subcategory-select").show();
            $("#department-select").show();
            $("#shipstationstore-select").show();
        }
        else if ($('#queries').val() == 12) {
            $("#weekly-date-picker").show();
        }
        else if ($('#queries').val() == 13) {
            $("#site-select").hide();
            $("#date-search").hide();
            $("#prod-select").hide();
            $("#sku-select").hide();
            getYearlyProductSoldReportData();
        }
        if ($('#queries').val() > 0) {
            $("#rerun-report-sec").show();
        }

    });

    $("#sku-select").select2().on("select2:select", function (ev) {
        if ($("#selectedValue").text() != "" && $("#queries").val() == 4) {
            get4ReportData();
        }
        else if ($("#queries").val() == 7) {
            getInventoryBalanceReport();
        }
    });


    $("#siteId-select").select2().on("select2:select", function (ev) {
        if ($("#queries").val() == 6) {
            getOnHandReport();
        }
    });

    $('#rerun-report').on('click', function () {
        var queryValue = $('#queries').val();
        queryValue = parseInt(queryValue, 10);
        if (queryValue >= 1 && queryValue <= 13) {
            switch (queryValue) {
                case 1:
                    get1ReportData();
                    break;
                case 2:
                    get2ReportData();
                    break;
                case 3:
                    get3ReportData();
                    break;
                case 4:
                    get4ReportData();
                    break;
                case 5:
                    get5ReportData();
                    break;
                case 6:
                    getOnHandReport();
                    break;
                case 7:
                    getInventoryBalanceReport();
                    break;
                case 8:
                    getStockHistoryReportData($("#date-picker").val());
                    break;
                case 9:
                    getProductStockReport($("#date-picker").val());
                    break;
                case 10:
                    getCycleCountReportData();
                    break;
                case 11:
                    InitDatatableForStockHistoryReport();
                    break;
                case 12:
                    getWeeklyProfitReport();
                    break;
                case 13:
                    getYearlyProductSoldReportData();
                    break;
                default:
                    console.log("Invalid query value");
            }
        }
    });

    //prepare the select2 components for stock history report with addition and substraction
    InitComponentsForStockHistoryReport();

    function initDate(isRange) {
        if (isRange) {
            $('#date-search').daterangepicker({
                autoUpdateInput: false,
                format: 'MM/DD/YYYY'
            });

            $('#date-search').on('apply.daterangepicker', function () {
                $('#selectedValue').text(
                    $('#date-search').data('daterangepicker').startDate.format('MM/DD/YYYY') + ' - ' +
                    $('#date-search').data('daterangepicker').endDate.format('MM/DD/YYYY'));
                if ($("#queries").val() == 1) {
                    get1ReportData();
                }
                else if ($("#queries").val() == 2) {
                    get2ReportData();
                }
                else if ($("#queries").val() == 3) {
                    get3ReportData();
                }
                else if ($("#sku-select").val() != -1 && $("#queries").val() == 4) {
                    get4ReportData();
                }
                else if ($("#queries").val() == 5) {
                    get5ReportData();
                }
                else if ($("#queries").val() == 10) {
                    getCycleCountReportData();
                }
                else if ($("#queries").val() == 11) {
                    //enable other fileters after a date range selected
                    $("#stockhistory-sku-select").removeAttr('disabled');
                    $("#subcategoryId-select").removeAttr('disabled');
                    $("#departmentId-select").removeAttr('disabled');
                    $("#shipstationstoreId-select").removeAttr('disabled');

                    InitDatatableForStockHistoryReport();
                }
                else if ($("#queries").val() == 12) {
                    getWeeklyProfitReport();
                }
                else {
                    return;
                }
            });

            $("#date-search").show();

        } else {

            $("#date-picker").datepicker({ orientation: "bottom", format: 'mm/dd/yyyy' })
                .on("changeDate", function (ev) {
                    $("#date-picker").datepicker("hide");
                    $("#btn-apply").prop('disabled', (!isValidDate($("#date-picker").val())));
                });

            $("#date-picker").on("input", function () {
                $("#btn-apply").prop('disabled', (!isValidDate($("#date-picker").val())));
            });

            $("#btn-apply").click(() => {
                if (isValidDate($("#date-picker").val())) {
                    if ($("#queries").val() == 9) {
                        getProductStockReport($("#date-picker").val());
                    }
                    else if ($("#queries").val() == 8) {
                        getStockHistoryReportData($("#date-picker").val());
                    }
                    else {
                        return;
                    }
                }
            });

            $("#date-picker-panel").show();
            $("#btn-apply").prop('disabled', true);
        }
    }

    function isValidDate(dateString) {
        var parsedDate = new Date(dateString);
        return !isNaN(parsedDate.getTime());
    }


    //prepare the select2 components for stock history report with addition and substraction
    function InitComponentsForStockHistoryReport() {

        //initialize products dropdown
        $('#stockhistory-sku-select').select2({
            placeholder: 'Select value',
            data: [{ text: "All", id: "0" }]
        });

        //apply subcategory dropdown changes
        $("#subcategoryId-select").select2().on("select2:select", function (ev) {
            if ($("#queries").val() == 11) {
                GetProductsBy($('#subcategoryId-select').val(), $('#departmentId-select').val());
            }
        });

        //apply department dropdown changes
        $("#departmentId-select").select2().on("select2:select", function (ev) {
            if ($("#queries").val() == 11) {
                GetProductsBy($('#subcategoryId-select').val(), $('#departmentId-select').val());
            }
        });

        //initialize shipstationStore dropdown
        $("#shipstationstoreId-select").select2({
            placeholder: 'Select store'
        });

    }

    //prepare the datatable for stock history report with addition and substraction
    function InitDatatableForStockHistoryReport() {

        destroyDataTable();
        $("#report-datatable").empty();

        $('#my-loading-modal').modal({
            backdrop: 'static',
            keyboard: false,
        });
        $('#my-loading-modal').modal('show');
        $('#my-spinner-modal').show();
        $('#my-spinner-modal-spinner').show();
        $('#my-modal-btn').hide();
        $('#modal-header-warning').show();
        $('#modal-header-success').hide();
        $('#warning-banner').hide().delay(5000).fadeIn('slow');

        var reportTable = $("#report-datatable").DataTable({
            "processing": true,
            searchDelay: 500,
            "serverSide": true,
            fixedHeader: true,
            "ordering": false,
            "searching": false,
            lengthMenu: [
                [10, 25, 50, 100],
                [10, 25, 50, 100]
            ],
            "pageLength": 100,
            "filter": true,
            "dom": "<'row'<'col-sm-12 col-md-7'l><'col-sm-12 col-md-4'f><'col-sm-12 col-md-1'B>>" + "<'row'<'col-sm-12'tr>>" + "<'row'<'col-sm-12 col-md-5'i><'col-sm-12 col-md-7'p>>",
            "createdRow": function (row, data, dataIndex) {
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
            "columnDefs": [{},],
            "ajax": {
                "url": "GetStockHistoryReport",
                "type": "POST",
                "datatype": "json",
                "data": function (d) {
                    d.Query = $('#queries').val(),
                    d.StartDate = $('#date-search').data('daterangepicker').startDate.format('MM-DD-YYYY'),
                    d.EndDate = $('#date-search').data('daterangepicker').endDate.format('MM-DD-YYYY'),
                    d.ProductId = $('#stockhistory-sku-select').val(),
                    d.SubCategoryId = $('#subcategoryId-select').val(),
                    d.DepartmentId = $('#departmentId-select').val(),
                    d.ShipStationStoreId = $('#shipstationstoreId-select').val(),
                    d.PageNo = (reportTable && reportTable.page && reportTable.page.info()) ? reportTable.page.info().page + 1 : '1'
                },
                "error": function (xhr, textStatus, errorThrown) {
                    $('#my-loading-modal').modal('hide');
                    $('#my-spinner-modal').hide();
                    $('#my-spinner-modal-spinner').hide();
                    $('#my-modal-btn').show();
                    $('#modal-header-warning').hide();
                    $('#modal-header-success').show();

                    if (xhr.status === 401) {
                        alert("401 Authorization Required: You are not authorized to access this resource.")
                        window.location.href = "/Login";
                    } else {
                        alert("An error occurred: " + errorThrown);
                    }
                }
            },
            "initComplete": function (settings, json) {
                $('#my-loading-modal').modal('hide');
                $('#my-spinner-modal').hide();
                $('#my-spinner-modal-spinner').hide();
                $('#my-modal-btn').show();
                $('#modal-header-warning').hide();
                $('#modal-header-success').show();
            },
            "columns": [
                {
                    "data": "productSku",
                    "title": "Product Sku",
                    "autowidth": true
                },
                {
                    "data": "productName",
                    "title": "Product Name",
                    "autowidth": true
                },
                {
                    "data": "siteName",
                    "title": "Site",
                    "autowidth": true
                },
                {
                    "data": "locationName",
                    "title": "Location",
                    "autowidth": true
                },
                {
                    "data": "totalAvailable",
                    "title": "Total Available",
                    "autowidth": true
                },
                {
                    "data": "totalCost",
                    "render": function formatToCurrency(TotalCost) {
                        return "$" + (TotalCost).toFixed(2).replace(/\d(?=(\d{3})+\.)/g, '$&,');
                    },
                    "title": "Total Cost",
                    "autowidth": true
                },
                {
                    "data": "modifiedAt",
                    "render": function formatDate(modifiedAt) {
                        return moment(modifiedAt, "YYYY-MM-DDTHH:mm:ss").format('YYYY-MM-DD hh:mm a');
                    },
                    "title": "Modified At",
                    "autowidth": true
                },
                {
                    "data": "action",
                    "render": function formatToCurrency(Action) {
                        if (Action == 'Opening Balance') {
                            return '<span>' + Action + '</span>';
                        }
                        else if (Action.includes("Added")) {
                            return '<span class="table-success">' + Action + '</span> <span class="mdi mdi-arrow-up-bold"></span>';
                        }
                        else if (Action.includes("Subtracted")) {
                            return '<span class="table-danger">' + Action + '</span> <span class="mdi mdi-arrow-down-bold"></span>';
                        }
                    },
                    "title": "Action",
                    "autowidth": true
                }
            ],
            "buttons": [{
                extend: 'collection', text: 'Export', buttons: [{
                    "extend": 'copy', "titleAttr": 'Copy', "action": newexportaction, exportOptions: {
                        columns: ':not(.notexport)'
                    }
                }, {
                    "extend": 'excel', "titleAttr": 'Excel', "action": newexportaction, exportOptions: {
                        columns: ':not(.notexport)',
                        orthogonal: 'export'
                    },
                }, {
                    "extend": 'csv', "titleAttr": 'CSV', "action": newexportaction, exportOptions: {
                        columns: ':not(.notexport)',
                        orthogonal: 'export'
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

        $('#report-datatable').on('page.dt', function () {
            var info = reportTable.page.info();
            console.log('Showing page: ' + info.page + ' of ' + info.pages);
        });

    }

    //get products by sub category and department
    async function GetProductsBy(subCategoryId, departmentId) {
        try {

            $('#stockhistory-sku-select').empty();

            //if subcategory and deaprtment both empty, don't load anything
            if ((!subCategoryId && !departmentId) || (subCategoryId == '0' && departmentId == '0'))
            {
                $('#stockhistory-sku-select').select2('destroy');
                $('#stockhistory-sku-select').select2({
                    placeholder: 'Select value',
                    data: [{ text: "All", id: "0" }]
                });
                $('#stockhistory-sku-select').trigger('change');

                return;
            }

            const response = await fetch('/Report/GetProductsBy?SubCategoryId=' + subCategoryId + '&DepartmentId=' + departmentId);
            if (!response) return;

            const data = await response.json();
            if (!data || !Array.isArray(data)) return;

            data.unshift({ text: "All", id: "0" });

            $('#stockhistory-sku-select').select2('destroy');
            $('#stockhistory-sku-select').select2({
                placeholder: 'Select value',
                data: data
            });
            $('#stockhistory-sku-select').trigger('change');

        }
        catch (e) {
        }
    }


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

    var startDate,
        endDate;

    $('#weekpicker').datepicker({
        autoclose: true,
        format: 'mm/dd/yyyy',
        forceParse: false
    }).on("changeDate", function (e) {
        var date = e.date;
        startDate = new Date(date.getFullYear(), date.getMonth(), date.getDate() - date.getDay());
        endDate = new Date(date.getFullYear(), date.getMonth(), date.getDate() - date.getDay() + 6);
        $('#weekpicker').datepicker('update', startDate);
        $('#weekpicker').val((startDate.getMonth() + 1) + '/' + startDate.getDate() + '/' + startDate.getFullYear() + ' - ' + (endDate.getMonth() + 1) + '/' + endDate.getDate() + '/' + endDate.getFullYear());

        getWeeklyProfitReport(startDate, endDate);
    });


    function getWeeklyProfitReport() {

        var url = '/GetQueries';

        var weekRange = $('#weekpicker').val().trim();

        if (!weekRange) {
            alert('Please select a week range.');
            return;
        }

        // Split the value to get start and end dates
        var dateParts = weekRange.split(' - ');

        if (dateParts.length !== 2) {
            alert('Invalid date range format. Please select a valid week.');
            return;
        }

        var startDateStr = dateParts[0];  
        var endDateStr = dateParts[1];    
      
        var startDate = new Date(startDateStr);
        var endDate = new Date(endDateStr);

        startDate.setHours(0, 0, 0, 0);

        endDate.setHours(23, 59, 59, 999);

        var formattedStartDate =
            (startDate.getMonth() + 1) + '/' +
            startDate.getDate() + '/' +
            startDate.getFullYear() + ' ' +
            startDate.getHours().toString().padStart(2, '0') + ':' +
            startDate.getMinutes().toString().padStart(2, '0') + ':' +
            startDate.getSeconds().toString().padStart(2, '0');

        var formattedEndDate =
            (endDate.getMonth() + 1) + '/' +
            endDate.getDate() + '/' +
            endDate.getFullYear() + ' ' +
            endDate.getHours().toString().padStart(2, '0') + ':' +
            endDate.getMinutes().toString().padStart(2, '0') + ':' +
            endDate.getSeconds().toString().padStart(2, '0');

        $('#my-loading-modal').modal({
            backdrop: 'static',
            keyboard: false,
        });
        $('#my-loading-modal').modal('show');
        $('#my-spinner-modal').show();
        $('#my-spinner-modal-spinner').show();
        $('#my-modal-btn').hide();
        $('#modal-header-warning').show();
        $('#modal-header-success').hide();
        $('#warning-banner').hide().delay(5000).fadeIn('slow');

        $.get(url, {
            Query: $('#queries').val(),
            StartDate: formattedStartDate,
            EndDate: formattedEndDate,
        },
            function (data) {
                $('#report-datatable').load(url, function (response) {
                    $('#my-loading-modal').modal('hide');
                    $('#my-spinner-modal').hide();
                    $('#my-spinner-modal-spinner').hide();
                    $('#my-modal-btn').show();
                    $('#modal-header-warning').hide();
                    $('#modal-header-success').show();

                    destroyDataTable();
                    $("#report-datatable").empty();
                    $("#report-datatable").DataTable({
                        "processing": true,
                        searchDelay: 500,
                        "paging": true,
                        destroy: true,
                        lengthMenu: [[10, 25, 100], [10, 25, 100]],
                        "pageLength": 10,
                        "filter": true,
                        "dom": "<'row'<'col-sm-12 col-md-7'l><'col-sm-12 col-md-4'f><'col-sm-12 col-md-1'B>>" +
                            "<'row'<'col-sm-12'tr>>" +
                            "<'row'<'col-sm-12 col-md-5'i><'col-sm-12 col-md-7'p>>",
                        order: [[0, 'desc'], [2, 'asc']],
                        keys: !0, language: {
                            paginate: {
                                previous: "<i class='mdi mdi-chevron-left'>", next: "<i class='mdi mdi-chevron-right'>"
                            },
                        }, drawCallback: function () {
                            $(".dataTables_paginate > .pagination").addClass("pagination-rounded")
                        },
                        "columnDefs": [{},],
                        "data": data, // Pass the data received from the first AJAX request    
                        "columns": [
                            { "data": "productId", "title": "Product Id", "autowidth": true },
                            { "data": "productName", "title": "Product Sku", "autowidth": true },
                            { "data": "itemsSold", "title": "Items Sold", "autowidth": true },
                            { "data": "profits", "title": "Profits", "autowidth": true },
                        ],
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
                        ]
                    });
                });
            });
    }
});