$(document).ready(function () {
    var productVendorMappingTable = $("#auditlog-datatable").DataTable({
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
            "url": "GetAuditLogs",
            "type": "POST",
            "datatype": "json",
        },
        "columns": [

            { "data": "user", "name": "User", "autoWidth": true },
            { "data": "timestamp", "name": "Timestamp", "autoWidth": true },
            { "data": "businessEntity", "name": "BusinessEntity", "autoWidth": true },
            { "data": "propertyName", "name": "PropertyName", "autoWidth": true },
            { "data": "oldValue", "name": "OldValue", "autoWidth": true },
            { "data": "newValue", "name": "NewValue", "autoWidth": true },

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