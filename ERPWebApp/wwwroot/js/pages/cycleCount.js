$(document).ready(function () {
    // Initialize select2 elements
    $(".select2").select2();
    $('#site-modify-id').select2({
        dropdownParent: $('#exampleModal')
    });

    // Event handler for site filter change
    $("#site-filter-id").on('change', function () {
        cycleCountTable.ajax.reload();
    });

    $('#status-filter').on('change', function () {
        cycleCountTable.ajax.reload();
    });

    // calls the method with the ID to populate the data
    $('#site-modify-id').on('change', function () {
        var SiteModify = $('#site-modify-id').val();
        var url = '/CycleCount/PopulateFrequency';
        $.getJSON(url, { SiteId: SiteModify }, function (data) {
            $('#baseDaysInput').val(data.basefreq);
            $('#over1000Input').val(data.thousandfreq);
            $('#cost10Input').val(data.cost10freq);
        });
    });

    $('#start-selected').on('click', function () {
        var selectedRows = cycleCountTable.rows({ selected: true }).data().toArray();

        if (!selectedRows || selectedRows.length === 0) {
            return; // No rows selected
        }

        var selectedIds = selectedRows.filter(row => !row.beingCounted).map(row => row.stockId);

        if (!selectedIds || selectedIds.length === 0) {
            return; // No uncounted rows selected
        }

        console.log('Selected Stock IDs:', selectedIds);
        showSpinner();

        $.ajax({
            url: '/CycleCount/StartBulk',
            type: 'POST',
            contentType: 'application/json',
            data: JSON.stringify(selectedIds),
            success: function () {
                console.log("Count started successfully");
                $('#cycle-datatable').DataTable().ajax.reload();
            },
            error: function (xhr, status, error) {
                console.error('Error starting count:', error);
            },
            complete: function () {
                hideSpinner();
            }
        });
    });

    $('#start-site').on('click', function () {
        var siteId = $('#site-filter-id').val() || 0;
        if (siteId === 0) return;

        showSpinner();

        $.ajax({
            url: '/CycleCount/StartCountBySite',
            type: 'POST',
            data: { siteId: siteId },
            success: function () {
                console.log("Count started successfully");
                var confirmModal = bootstrap.Modal.getInstance(document.getElementById('confirm-start'));
                confirmModal.hide();
                $('#cycle-datatable').DataTable().ajax.reload();
            },
            error: function (xhr, status, error) {
                console.error('Error starting count:', error);
            },
            complete: function () {
                hideSpinner();
            }
        });
    });

    // Initialize the main data table with AJAX source
    var cycleCountTable = $('#cycle-datatable').DataTable({
        "processing": true,
        searchDelay: 500,
        "serverSide": true,
        fixedHeader: true,
        lengthMenu: [[10, 25, 100], [10, 25, 100]],
        "filter": true,
        ajax: {
            url: 'GetCycleCountList',
            type: 'GET',
            "datatype": "json",
            data: function (d) {
                d.siteId = $('#site-filter-id').val() || 0;

                let selectedStatus = $('#status-filter').val();
                if (selectedStatus === "1") {
                    d.isStarted = true;  // Send true for "Started"
                } else if (selectedStatus === "2") {
                    d.isStarted = false; // Send false for "Not Started"
                } else {
                    d.isStarted = null;  // Send null for "All"
                }
            }
        },
        columns: [
            { data: 'stockId', visible: false },
            { data: 'locationId', visible: false },
            { data: 'location.locationName' },
            { data: 'products.sku' },
            { data: 'products.description' },
            { data: 'totalAvailable' },
            { data: 'lastCounted' },
            {
                data: 'stockId',
                render: function (data, type, row) {
                    if (!row.beingCounted) {
                        return '<button id="' + data + '" onclick="startCountFunction(this.id)" class="btn btn-dark">Start</button>';
                    } else {
                        return '<button id="' + data + '" onclick="getEditFields(this.id)" class="btn btn-success" data-bs-toggle="modal" data-bs-target="#my-edit-modal">Finish</button>';
                    }
                }
            }
        ],
        dom: "<'row'<'col-3 col-sm-4 col-md-4 col-lg-7'l><'col-6 col-sm-4 col-md-4'f><'d-none d-md-block d-lg-block col-sm-4 col-md-4 col-lg-1'B>>" +
            "<'row'<'col-sm-12'tr>>" +
            "<'row'<'col-sm-12 col-md-5'i><'col-sm-12 col-md-7'p>>",
        keys: !0,
        language: {
            paginate: {
                previous: "<i class='mdi mdi-chevron-left'>",
                next: "<i class='mdi mdi-chevron-right'>"
            }
        },
        drawCallback: function () {
            $(".dataTables_paginate > .pagination").addClass("pagination-rounded");
        },
        buttons: [{
            extend: 'print',
            autoPrint: false,
            text: 'Print',
            title: 'Location Stock Count',
            messageTop: 'Sign Name Here _______________________',
            messageBottom: '<h4 style="color: black;">Any extras please document below!</h4> <hr><hr><hr><hr><hr><hr>',
            exportOptions: {
                columns: [2, 3, 4],
                rows: function (idx, data, node) {
                    var dt = new $.fn.dataTable.Api('#cycle-datatable');
                    var selected = dt.rows({ selected: true }).indexes().toArray();
                    return selected.length === 0 || $.inArray(idx, selected) !== -1;
                }
            }
        }],
        select: {
            style: 'multi'
        },
        "oLanguage": {
            "sLengthMenu": '<select class="btn btn-light">' +
                '<option value="10">10</option>' +
                '<option value="20">20</option>' +
                '<option value="30">30</option>' +
                '<option value="40">40</option>' +
                '<option value="50">50</option>' +
                '<option value="-1">All</option>' +
                '</select>'
        },
        order: [[3, 'asc']],
    });

    // Event handler for submit modify button
    $('#submitModify').on('click', function () {
        var BaseDaysResult = $('#baseDaysInput').val();
        var Over1000Result = $('#over1000Input').val();
        var Cost10Result = $('#cost10Input').val();
        var SiteModifyID = $('#site-modify-id').val();
        var url = '/CycleCount/ModifyCycle';

        $.post(url, { BaseDays: BaseDaysResult, Over100: Over1000Result, Cost10: Cost10Result, SiteId: SiteModifyID }, function () {
            table.ajax.reload();
        });
    });
});

// Function to show the spinner
function showSpinner() {
    $("#overlay").show();
    $("#spinner").show();
    $("body").addClass("overlay-active");
}

// Function to hide the spinner
function hideSpinner() {
    $("#overlay").hide();
    $("#spinner").hide();
    $("#content").removeClass("blur");
    $("body").removeClass("overlay-active");
}

function showStartCycleCountConfirmModal() {
    var confirmModal = new bootstrap.Modal(document.getElementById('confirm-start'), {});
    confirmModal.show();
}

// Function to start count
function startCountFunction(id) {
    showSpinner();
    $.ajax({
        url: '/CycleCount/StartCount',
        type: 'POST',
        data: { id: id },
        success: function () {
            console.log("Started ID:" + id);
            $('#cycle-datatable').DataTable().ajax.reload();
        },
        error: function (data) {
            console.log(data);
        },
        complete: function () {
            hideSpinner();
        }
    });
}

// Function to get edit fields
function getEditFields(id) {
    $("#edit-modal-view").load('/CycleCount/Edit?id='+id, function (response, status, xhr) {
        if (status === "success") {
            console.log("Grabbed Detail Info");
        } else {
            console.log("Error: " + xhr.status + " " + xhr.statusText);
        }
    });
}

//Function to finish the CycleCount
function EditStock() {
    showSpinner();
    var formData = $('#editForm').serialize();
    $.ajax({
        url: '/CycleCount/Edit',
        type: 'POST',
        data: formData,
        success: function (response) {
            $('#cycle-datatable').DataTable().ajax.reload();
            $('#confirm-count').modal('hide');
            $('#my-edit-modal').modal('hide');
        },
        error: function (xhr, status, error) {
            console.log("Form submission failed", error);
            $('#error-message-edit').text('An error occurred while submitting the form.');
        },
        complete: function () {
            hideSpinner();
        }
    });
}