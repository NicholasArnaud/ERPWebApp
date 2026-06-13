var all = '';
$(document).ready(function () {
    var _productId = $('#add-product-select-id').val();
    //var url = '/PurchaseOrders/GenerateBarcode';
    //$.ajax({
    //    url: url,
    //    type: 'POST',
    //    data: { PurchaseorderId: _productId },
    //    success: function (response) {
    //        console.log("dun");
    //        var s = "<img src='" + response + "'/>";
    //        $('#barcode').html(s);
    //    },
    //    error: function (data) {
    //        console.log(data);
    //    }
    //});
    //$("#Product-datatable").DataTable({
    //    keys: !0, language: {
    //        paginate: {
    //            previous: "<i class='mdi mdi-chevron-left'>", next: "<i class='mdi mdi-chevron-right'>"
    //        },
    //    }, drawCallback: function () {
    //        $(".dataTables_paginate > .pagination").addClass("pagination-rounded")
    //    },
    //});
    //$("#Stock-datatable").DataTable({
    //    keys: !0, language: {
    //        paginate: {
    //            previous: "<i class='mdi mdi-chevron-left'>", next: "<i class='mdi mdi-chevron-right'>"
    //        },
    //    }, drawCallback: function () {
    //        $(".dataTables_paginate > .pagination").addClass("pagination-rounded")
    //    },
    //});
    //$("#File-datatable").DataTable({
    //    keys: !0, language: {
    //        paginate: {
    //            previous: "<i class='mdi mdi-chevron-left'>", next: "<i class='mdi mdi-chevron-right'>"
    //        },
    //    }, drawCallback: function () {
    //        $(".dataTables_paginate > .pagination").addClass("pagination-rounded")
    //    },
    //});
});

function exportAllDatatables() {
    //var table = document.getElementsByTagName("table")[0];// $("#Product-datatable").DataTable();
    var table = $("#Product-datatable").DataTable();
    var table2 = $("#Stock-datatable").DataTable();
    var table3 = $("#File-datatable").DataTable();
    $('#Product-datatable .buttons-print').click();
    $('#Stock-datatable .buttons-print').click();
    var win = window.open("about:blank", "Print View");
    win.document.write('<html>\<head>\<link rel="stylesheet" type="text/css" href="https://cdn.datatables.net/1.11.5/css/jquery.dataTables.min.css">\</head>\<body></body>\</html>');
    win.document.close();
    all = table.table().node().outerHTML;
    $(win.document.body).append($("#title-print"));
    $(win.document.body).append($("#barcode"));
    $(win.document.body).append($("#product-print"));
    $(win.document.body).append('<html><head></head><body>');
    $(win.document.body).append(document.getElementById('targetTextArea').value.replace(/\n/gi, '<br>'));
    $(win.document.body).append('</body></html>');
    $(win.document.body).append($("#second-print"));
    $(win.document.body).append(all);

    all = table2.table().node().outerHTML;
    $(win.document.body).append($('<div style="page-break-after:always"></div><div>&nbsp;</div>'));
    $(win.document.body).append($("#third-print"));
    $(win.document.body).append(all);
    all = table3.table().node().outerHTML;
    $(win.document.body).append($('<div>&nbsp;</div>'));
    $(win.document.body).append(all);
    setTimeout(function () { win.print(); }, 500);
    setTimeout("refreshPage();", 600); // milliseconds
}

function refreshPage() {
    window.location = location.href;
}