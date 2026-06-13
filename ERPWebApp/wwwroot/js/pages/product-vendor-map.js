function ValidateEditForm() {
    if ($('#edit-cost').val() == "" || $('#edit-cost').val() < 0) {
        document.getElementById("error-message-edit").innerHTML = "Cost Not Valid";
    } else if ($('#edit-lead-time').val() == "" || $('#edit-lead-time').val() <= 0 || $('#edit-lead-time').val() != Math.floor($('#edit-lead-time').val())) {
        document.getElementById("error-message-edit").innerHTML = "Lead Time Not Valid";
    } else {
        document.getElementById("error-message-edit").innerHTML = "";
        postEditFieldValues();
    }
}


function ValidateForm() {
    if ($('#selector-pid').val() == "") {
        document.getElementById("error-message").innerHTML = "Product not Selected";

    } else if ($('#selector-vid').val() == "") {
        document.getElementById("error-message").innerHTML = "Vendor not Selected";
    } else if ($('#create-cost').val() == "" || $('#create-cost').val() < 0) {
        document.getElementById("error-message").innerHTML = "Cost Not Valid";
    } else if ($('#create-lead-time').val() == "" || $('#create-lead-time').val() <= 0) {
        document.getElementById("error-message").innerHTML = "Lead Time Not Valid";
    } else {
        document.getElementById("error-message").innerHTML = "";
        CreateThePVM();
    }
}

function createPVM() {
    $('#selector-pid').val();
}

function getVendorDetailFields(id) {

    $('#details-modal-view').load('/ProductVendorMapping/GetDetails?id=' + id, function (response, status, xhr) {
        if (status == "success") {
            console.log("Grabbed Detail Info");
        } else {
            console.log("Error: " + xhr.status);
        }
    });
}
function getVendorDeleteFields(id) {
    $('#delete-modal-view').load('/ProductVendorMapping/GetDelete?id='+id, function (response, status, xhr) {
        if (status == "success") {
            console.log("Grabbed Delete Info");
        } else {
            console.log("Error: " + xhr.status);
        }
    });
}

function getVendorEditFieldss(id) {
    $('#edit-modal-view').load('/ProductVendorMapping/GetEdit?id='+id, function (response, status, xhr) {
        if (status == "success") {
            console.log("Recieved Edit Info");
        } else {
            console.log("Error: " + xhr.status);
        }
    });
}


$(document).on('click', '#location-datatable tr', function () {
    var hashrateId = $(this).find("td").eq(0).text().trim();

});

function postEditFieldValues() {
    // get the edit modal inputs
    $("#filter-button").html('<span class="spinner-border spinner-border-sm mr-2" role="status" aria-hidden="true"></span>Loading...').addClass("disabled");
    $("#filter-button-vendor").html('<span class="spinner-border spinner-border-sm mr-2" role="status" aria-hidden="true"></span>Loading...').addClass("disabled");
    var modal = document.getElementById("my-edit-modal");
    //let mapping = mappings.find(m => m.ProductVendorMappingId == inputs[4].value);
    $.ajax({
        url: '/ProductVendorMapping/PostEdit',
        type: 'POST',
        data: {
            id: document.getElementById("editid").value,
            cost: document.getElementById("edit-cost").value,
            leadtime: document.getElementById("edit-lead-time").value,
            primaryvendor: document.getElementById("myCheck").checked,
            vendorsku: document.getElementById("edit-vendor-sku").value,
            isActive: $('#checkIsActive').prop('checked')
        },
        success: function (partialView) {
            $("#my-edit-modal").modal('hide');
            refreshproductlist();
            console.log("Grabbed PVM Info");
        },
        error: function (data) {
            console.log(data);
        }
    }).then(function (partialView) {
        editvendor()
    });

}

function editvendor() {
    var modal = document.getElementById("my-edit-modal");
    $.ajax({
        url: '/ProductVendorMapping/PostEditVend',
        type: 'POST',
        data: {
            id: document.getElementById("editid").value,
            cost: document.getElementById("edit-cost").value,
            leadtime: document.getElementById("edit-lead-time").value,
            primaryvendor: document.getElementById("myCheck").checked,
            vendorsku: document.getElementById("edit-vendor-sku").value,
            isActive: $('#checkIsActive').prop('checked')
        },
        success: function (partialView) {
            console.log("Grabbed PVM Info");
            refreshvendorlist();
        },
        error: function (data) {
            console.log(data);
        }
    });
}

$('#selector-pid').select2({
    dropdownParent: $('#my-create-modal')
});
$('#selector-vid').select2({
    dropdownParent: $('#my-create-modal')
});
