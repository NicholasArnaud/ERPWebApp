function addEmployees(dropdownData) {
    for (var i = 0; i < dropdownData.length; i++) {
        var newOption = new Option(dropdownData[i].fullName + ' - ' + dropdownData[i].position, dropdownData[i].employeeId, false, false);
        $("#employee-dropdown").append(newOption).trigger('change');
    }
}

function getDesignOrderTotal(designorderdata) {
    var totalDesignCount = 0;
    for (var i = 0; i < designorderdata.length; i++) {
        totalDesignCount += designorderdata[i].quantity;
    }
    return totalDesignCount;
}

function setupDataTable() {
    var table = $('#design-datatable').DataTable();
    if ($.fn.DataTable.isDataTable('#design-datatable')) {
        table.destroy();
    }
    ;
    $("#design-datatable").DataTable({
        "dom": "<'row'<'col-sm-12 col-md-7'l><'col-sm-12 col-md-4'f><'col-sm-12 col-md-1'B>>" +
            "<'row'<'col-sm-12'tr>>" +
            "<'row'<'col-sm-12 col-md-5'i><'col-sm-12 col-md-7'p>>",
        keys: !0, language: {
            paginate: {
                previous: "<i class='mdi mdi-chevron-left'>", next: "<i class='mdi mdi-chevron-right'>"
            },

        }, drawCallback: function () {
            $(".dataTables_paginate > .pagination").addClass("pagination-rounded")
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
        ]
    });
    return table;
}

function getDesignerData() {
    var e = document.getElementById("employee-dropdown");
    var selectedEmployeeId = e.options[e.selectedIndex].value;
    console.log(selectedEmployeeId);
    var url = '/DesignOrderCounter/PullDesignOrdersByDateRange';
    console.log(url);
    console.log(moment($('#daterange').data('daterangepicker').startDate).toDate());
    console.log(moment($('#daterange').data('daterangepicker').endDate).toDate());
    $.get(url, {
        EmployeeId: selectedEmployeeId,
        StartDate: $('#daterange').data('daterangepicker').startDate.format('M-D-YYYY'),
        EndDate: $('#daterange').data('daterangepicker').endDate.format('M-D-YYYY')
    }, function (data) {
        console.log(data);
        $('#design-datatable').load('/DesignOrderCounter/PartialViewIndex', function () {
            console.log("Attempted Refresh");
            $('#total-design-count').text(getDesignOrderTotal(data));
            var table = setupDataTable();
        });
    });
}
