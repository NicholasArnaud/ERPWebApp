var batchNumber;
var selectedIndex;
var isDeductible;
$(document).ready(function () {
    var replacementSkusJson = null;
    var assignedDepartmentsJson = null;  
    fetchActiveDepartments();  

    // Fetch active SKUs  
    function fetchSkuList() {
        var skus = [];
        $.ajax({
            url: getAllActiveSkusUrl,
            type: 'GET',
            async: false,
            success: function (data) {
                skus = data.map(function (item) {
                    return { id: item.sku, text: `${item.sku}: ${item.description}` };
                });
            }
        });
        return skus;
    }

    // Fetch active departments  
    function fetchActiveDepartments() {
        var departments = [];
        $.ajax({
            url: getAllActiveDepartmentsUrl,
            type: 'GET',
            async: false,
            success: function (data) {
                departments = data.map(function (department) {
                    return { id: department.departmentId, text: department.departmentName };
                });
            }
        });
        return departments;
    }  

    // Functionality for deductible toggles.
    function toggleDeductibleCheckbox() {
        $("#deductibleCheckboxContainer").show();
    }

    $("#CreateBatch-model").on("show.bs.modal", toggleDeductibleCheckbox);
    $("#batchTypeSelect").on("change", toggleDeductibleCheckbox);

    $("#MissingSKUs-model").on('shown.bs.modal', function () {
        $(document).off('focusin.modal');
    });
    var activeSkus = fetchSkuList();
    var activeDepartments = fetchActiveDepartments();

    // Function called when it's time to fill out the Missing Sku modal.
function handleMissingSkus(missingSkus) {  
    var missingSkUsContainer = $("#missingSKUsContainer");  
    missingSkUsContainer.empty();  
    missingSkus.forEach(function (missingSku) {  
        var skuDiv = $('<div>', { class: 'missing-sku', 'data-original-sku': missingSku.sku, style: 'word-wrap: break-word; white-space: pre-wrap;' });   
        skuDiv.append($('<p>', { text: 'Missing SKU: ' + missingSku.sku }));           
        var select2Dropdown = $('<select>', { class: 'form-control select2' });  
        select2Dropdown.append($('<option>', { text: '', value: '', disabled: true, selected: true }).text('Select a replacement SKU...'));  
        activeSkus.forEach(function (sku) {  
            select2Dropdown.append($('<option>', { text: sku.text, value: sku.id }));  
        });  
        skuDiv.append(select2Dropdown);  
        select2Dropdown.select2({  
            placeholder: 'Select a replacement SKU...',  
            dropdownParent: $('#MissingSKUs-model'),  
            allowClear: true  
        });   

        var orderOptionsContainer = $('<div>', { style: 'border: 1px solid #ccc; padding: 10px; margin-top: 10px;' });

        if (missingSku.orderOptions && missingSku.orderOptions.length > 0) {
            missingSku.orderOptions.forEach(function (option) {
                var optionText = 'Option: ' + option.name + ' - ' + option.value;
                orderOptionsContainer.append($('<p>', { text: optionText, style: 'margin: 5px 0; word-wrap: break-word; white-space: pre-wrap;' }));
            });
        }

        skuDiv.append(orderOptionsContainer);  

        var horizontalLine = $('<hr>');  
        skuDiv.append(horizontalLine);  
        missingSkUsContainer.append(skuDiv);  
    });  
    $("#MissingSKUs-model").modal('show');  
}  


    // Function called when it's time to fill out the Unassigned Department modal.
    function handleUnassignedDepartments(unassignedDepartments) {
        var unassignedDepartmentsContainer = $("#unassignedDepartmentsContainer");
        unassignedDepartmentsContainer.empty();
        unassignedDepartments.forEach(function (orderItemId) {
            var departmentDiv = $('<div>', { class: 'unassigned-department' });
            departmentDiv.append($('<p>', { text: 'Unassigned Department: ' + orderItemId }));
            departmentDiv.append($('<input>', { type: 'hidden', value: orderItemId }));
            var select2Dropdown = $('<select>', { class: 'select2 form-control' });
            departmentDiv.append(select2Dropdown);
            select2Dropdown.select2({
                placeholder: 'Select a department...',
                data: activeDepartments,
                dropdownParent: $('#UnassignedDepartments-model')
            });
            var horizontalLine = $('<hr>');
            departmentDiv.append(horizontalLine);
            unassignedDepartmentsContainer.append(departmentDiv);
        });
        $("#UnassignedDepartments-model").modal('show');
    }

    // Reset button states when modals are hidden  
    $("#BatchCreationSuccess-model, #BatchCreationFailure-model, #MissingSKUs-model").on("hidden.bs.modal", function () {
        $("[data-createbatch]").prop("disabled", false).html("Continue");
    });


    // Begins the process of creating a batch.
$("[data-createbatch]").on("click", function () {
    //Setting the context so that we know what functions to do.
    $("#updateDepartments").data("context", "createBatch");  
    $("#replaceMissingSKUs").data("context", "createBatch");  

    $(this).prop("disabled", true);
    $(this).html('<i class="fas fa-circle-notch fa-spin"></i> Processing...');
    // Get the selected ERP order IDs from the hidden input field
    var selectedERPOrderIds = $("#selectedOrderIds-BatchCreation").val();
    var batchNumber = $("#batchNameInput").val();
    isDeductible = $("#isDeductibleCheckbox").is(":checked");
    selectedIndex = isDeductible ? 0 : 1; 
    if (!batchNumber) {
        var currentDate = new Date();
        var formattedDate = (currentDate.getMonth() + 1) + '.' + currentDate.getDate();
        batchNumber = 'OD ' + formattedDate;
    }
    replacementSkusJson = null;
    $.ajax({
        url: batchCreationUrl,
        type: "POST",
        data: {
            ERPOrderIdsJson: selectedERPOrderIds,
            BatchName: batchNumber,
            BatchType: selectedIndex,
            IsDeductible: isDeductible,
        },
        beforeSend: function () {
            // Update the button's text and disable it
            $("#CreateBatch-btn").prop("disabled", true);
            $("#CreateBatch-btn").html("<span class=\"spinner-border spinner-border-sm mr-2\" role=\"status\" aria-hidden=\"true\"></span>Processing...");
        },
        success: function (response) {
            // Re-enable the button and reset its text
            $("#CreateBatch-btn").prop("disabled", false);
            $("#CreateBatch-btn").text("Continue");
            // Close the modal
            $("#CreateBatch-model").modal('hide');
            batchNumber = $("#batchNameInput").val();
            if (response.status === "missing_skus") {
                // Display the missing SKUs in the MissingSKUs-model  
                var missingSKUs = response.missingSkus;
                var missingSKUsContainer = $("#missingSKUsContainer");
                missingSKUsContainer.empty();

                // Show the MissingSKUs-model  
                $("#MissingSKUs-model").modal('show');

                missingSKUs.forEach(function (missingSKU) {
                    // Create a div to display the missing SKU and the select2 dropdown  
                    var skuDiv = $('<div>', { class: 'missing-sku', 'data-original-sku': missingSKU.sku, style: 'word-wrap: break-word; white-space: pre-wrap;' });   
                    skuDiv.append($('<p>', { text: 'Missing SKU: ' + missingSKU.sku }));

                    var select2Dropdown = $('<select>', { class: 'select2 form-control' });
                    select2Dropdown.append($('<option>', { text: '', value: '', disabled: true, selected: true }).text('Select a replacement SKU...'));
                    activeSkus.forEach(function (sku) {
                        select2Dropdown.append($('<option>', { text: sku.text, value: sku.id }));
                    });

                    skuDiv.append(select2Dropdown);
                    select2Dropdown.select2({
                        placeholder: 'Select a replacement SKU...',
                        dropdownParent: $('#MissingSKUs-model'),
                        allowClear: true
                    });

                    var orderOptionsContainer = $('<div>', { style: 'border: 1px solid #ccc; padding: 10px; margin-top: 10px;' });

                    if (missingSKU.orderOptions && missingSKU.orderOptions.length > 0) {
                        missingSKU.orderOptions.forEach(function (option) {
                            var optionText = 'Option: ' + option.name + ' - ' + option.value;
                            orderOptionsContainer.append($('<p>', { text: optionText, style: 'margin: 5px 0; word-wrap: break-word; white-space: pre-wrap;' }));
                        });
                    }

                    skuDiv.append(orderOptionsContainer);  

                    var horizontalLine = $('<hr>');
                    skuDiv.append(horizontalLine);
                    missingSKUsContainer.append(skuDiv);
                });
            }  
            else if (response.status === "unassigned_departments") {
                $("#MissingSKUs-model").modal('hide');
                $("#replaceMissingSKUs").prop("disabled", false).text("Replace Sku");

                // Display the unassigned departments in the UnassignedDepartments-model  
                var unassignedDepartments = response.unassignedDepartments;
                var unassignedDepartmentsContainer = $("#unassignedDepartmentsContainer");
                unassignedDepartmentsContainer.empty();

                // Show the UnassignedDepartments-model  
                $("#UnassignedDepartments-model").modal('show');

                unassignedDepartments.forEach(function (orderItemId) {
                    // Create a div to display the unassigned department and the select2 dropdown  
                    var departmentDiv = $('<div>', { class: 'unassigned-department' });
                    departmentDiv.append($('<p>', { text: 'Unassigned Department: ' + orderItemId }));
                    departmentDiv.append($('<input>', { type: 'hidden', value: orderItemId }));

                    // Create the select2 dropdown for the unassigned department  
                    var select2Dropdown = $('<select>', { class: 'select2 form-control' });
                    departmentDiv.append(select2Dropdown);
                    select2Dropdown.select2({
                        placeholder: 'Select a department...',
                        data: activeDepartments,
                        dropdownParent: $('#UnassignedDepartments-model')
                    });

                    var horizontalLine = $('<hr>');
                    departmentDiv.append(horizontalLine);

                    // Add the div to the unassignedDepartmentsContainer  
                    unassignedDepartmentsContainer.append(departmentDiv);
                });
            }  

            else if (response.status === "error") {
                $("#CreateBatch-btn").prop("disabled", false);
                $("#CreateBatch-btn").text("Continue");

                // Show the failure modal with the custom error message
                $("#BatchCreationFailure-model .modal-body p").text("Batch creation failed: " + response.message);
                $("#BatchCreationFailure-model").modal("show");
            } else {
                $("#BatchCreationSuccess-model").modal("show");
            }
        },
        error: function (xhr, status, error) {

            // Re-enable the button and reset its text
            $("#CreateBatch-btn").prop("disabled", false);
            $("#CreateBatch-btn").text("Continue");

            $("#BatchCreationFailure-model .modal-body p").text("Batch creation failed: " + error);
            $("#BatchCreationFailure-model").modal("show");
        }
    });
});

$(".select2").select2();
var replacementSkusJson = null;

    // Resets the Continue/Processing button in the side modal.
    $("#BatchCreationSuccess-model").on("shown.bs.modal", function () {
        $("[data-createbatch]").prop("disabled", false).html("Continue");
    });
    $("#successModalOkButton").on("click", function () {
        // Redirect the user to the PrintPickList action with the batch number  
        window.open("/Orders/PrintPickList?batchNumber=" + batchNumber, "_blank");
    });

    $("#BatchCreationFailure-model").on("shown.bs.modal", function () {
        $("[data-createbatch]").prop("disabled", false).html("Continue");
    });
    $("#failureModalOkButton").on("click", function () {
        // Redirect the user to the PrintPickList action with the batch number  
        window.location.href = "/Orders/PrintPickList?batchNumber=" + batchNumber;
    });
    $("#MissingSKUs-model").on("hidden.bs.modal", function () {
        $("[data-createbatch]").prop("disabled", false).html("Continue");
    });

    var replacementSkusJson = null;

    $("#addOrderToBatchButton").on("click", function () {
        // Setting the context so that we know which functions we need to do.
        $("#updateDepartments").data("context", "addOrdersToBatch");  
        $("#replaceMissingSKUs").data("context", "addOrdersToBatch");  

        $("#addOrderToBatchButton").prop("disabled", true);
        $("#addOrderToBatchButton").html("<span class=\"spinner-border spinner-border-sm mr-2\" role=\"status\" aria-hidden=\"true\"></span>Processing...");
        submitAddOrdersToBatchForm();
    });

    // Calls the Add Order controller action, and handles the response depending on if a sku or department are missing, or if everything is fine.
    function submitAddOrdersToBatchForm() { 
        var form = $("#AddOrderToBatch-form");
        var formData = form.serializeArray();
        var data = {};
 
        $.each(formData, function (index, field) {
            data[field.name] = field.value;
        });

        if (replacementSkusJson) {
            data.replacementSkusJson = replacementSkusJson;
        }

        if (assignedDepartmentsJson) {
            data.assignedDepartmentsJson = assignedDepartmentsJson;
        }  

        console.log("Form Data:", data);

        $.ajax({
            url: addOrdersToBatchUrl,
            type: "POST",
            data: $.param(data), 
            success: function (response) {
                console.log("AJAX success response:", response);  
                if (response.status === "success") {
                    // Re-enable the button and reset its text
                    $("#updateDepartments").prop("disabled", false);
                    $("#updateDepartments").text("Update Departments");
                    $("#UnassignedDepartments-model").modal("hide");

                    $("#replaceMissingSKUs").prop("disabled", false);
                    $("#replaceMissingSKUs").text("Replace SKUs");
                    $("#MissingSKUs-model").modal("hide");

                    $("#addOrderToBatchButton").prop("disabled", false);
                    $("#addOrderToBatchButton").text("Add");
                    $("#AddOrderToBatch-modal").modal("hide");

                    $("#AddToBatchSuccess-model").modal("show");  
                } else if (response.status === "missing_skus") {
                    $("#AddOrderToBatch-modal").modal("hide");
                    // Handle missing SKUs  
                    handleMissingSkus(response.missingSkus);
                } else if (response.status === "unassigned_departments") {
                    $("#AddOrderToBatch-modal").modal("hide");
                    // Handle unassigned departments  
                    handleUnassignedDepartments(response.unassignedDepartments);
                } else {
                    $("#AddOrderToBatch-modal").modal("hide");
                    // Handle other errors  
                    console.log("Error: " + response.message);
                    $("#BatchCreationFailure-model").modal("show");  
                }
            },
            error: function (xhr, status, error) {
                // Log the error details for debugging  
                console.error("AJAX error:", {
                    responseText: xhr.responseText,
                    status: status,
                    error: error
                });
                // Handle AJAX error  
                alert("AJAX error: " + xhr.responseText);
            }
        });
    }

            // Resumes the process of creating a batch after the user has selected replacement SKUs for missing SKUs.
            $("#replaceMissingSKUs").on("click", function () {  
            console.log("MIssingSKU CLicked");
            var context = $(this).data("context");  
            $(this).prop("disabled", true).text("Processing...");  
  
                var replacementSkus = [];
            $(".missing-sku").each(function () {  
                var originalSku = $(this).data("original-sku");
                var newSku = $(this).find("select").val();  
                replacementSkus.push({ originalSku: originalSku, newSku: newSku });  
            });
            
            replacementSkusJson = JSON.stringify(replacementSkus);  

            if (context === "createBatch") {  
                handleReplaceMissingSkusForCreateBatch();  
            } else if (context === "addOrdersToBatch") {  
                handleReplaceMissingSkusForAddOrdersToBatch();  
            }  
        });  
            // Resumes the process of creating a batch after the user has updated departments for a product with a missing department.
            $("#updateDepartments").on("click", function () {  
            var context = $(this).data("context");  
            $(this).prop("disabled", true).text("Processing...");  
  
            var assignedDepartments = [];  
            $(".unassigned-department").each(function () {  
                var orderItemId = $(this).find("input[type='hidden']").val();  
                var assignedDepartmentId = parseInt($(this).find("select").val());  
                assignedDepartments.push({ OrderItemId: parseInt(orderItemId), AssignedDepartmentId: assignedDepartmentId });  
            });  
  
            assignedDepartmentsJson = JSON.stringify(assignedDepartments);  
  
            if (context === "createBatch") {  
                handleUpdateDepartmentsForCreateBatch();  
            } else if (context === "addOrdersToBatch") {  
                handleUpdateDepartmentsForAddOrdersToBatch();  
            }  
        });  

    // Function to handle missing SKUs  
    function handleMissingSkus(missingSkus) {
        var missingSkUsContainer = $("#missingSKUsContainer");
        missingSkUsContainer.empty();
        missingSkus.forEach(function (missingSku) {
            var skuDiv = $('<div>', { class: 'missing-sku', 'data-original-sku': missingSku.sku, style: 'word-wrap: break-word; white-space: pre-wrap;' });   
            skuDiv.append($('<p>', { text: 'Missing SKU: ' + missingSku.sku }));

            var select2Dropdown = $('<select>', { class: 'form-control select2' });
            select2Dropdown.append($('<option>', { text: '', value: '', disabled: true, selected: true }).text('Select a replacement SKU...'));

            activeSkus.forEach(function (sku) {
                select2Dropdown.append($('<option>', { text: sku.text, value: sku.id }));
            });

            skuDiv.append(select2Dropdown);
            select2Dropdown.select2({
                placeholder: 'Select a replacement SKU...',
                dropdownParent: $('#MissingSKUs-model'),
                allowClear: true
            });
 
            var orderOptionsContainer = $('<div>', { style: 'border: 1px solid #ccc; padding: 10px; margin-top: 10px;' });

            if (missingSku.orderOptions && missingSku.orderOptions.length > 0) {
                missingSku.orderOptions.forEach(function (option) {
                    var optionText = 'Option: ' + option.name + ' - ' + option.value;
                    orderOptionsContainer.append($('<p>', { text: optionText, style: 'margin: 5px 0; word-wrap: break-word; white-space: pre-wrap;' }));
                });
            }

            skuDiv.append(orderOptionsContainer);  

            var horizontalLine = $('<hr>');
            skuDiv.append(horizontalLine);
            missingSkUsContainer.append(skuDiv);
        });
        $("#MissingSKUs-model").modal('show');
    }  

    function handleReplaceMissingSkusForCreateBatch() {  
    var selectedERPOrderIds = $("#selectedOrderIds-BatchCreation").val();  
    var batchId = $("#openBatchesSelect").val();  
    var batchNumber = $("#batchNameInput").val();  
    var isDeductible = $("#isDeductibleCheckbox").is(":checked");  
  
    $.ajax({  
        url: batchCreationUrl,  
        type: "POST",  
        data: {  
            ERPOrderIdsJson: selectedERPOrderIds,  
            BatchName: batchNumber,  
            BatchType: selectedIndex,  
            IsDeductible: isDeductible,  
            replacementSkusJson: replacementSkusJson,  
        },  
        success: function (response) {  
            if (response.status === "unassigned_departments") {  
                $("#MissingSKUs-model").modal('hide');  
                 $("#replaceMissingSKUs").prop("disabled", false).text("Replace Sku");  
                handleUnassignedDepartments(response.unassignedDepartments);  
            } else if (response.status === "success") {  
                $("#MissingSKUs-model").modal('hide');  
                 $("#replaceMissingSKUs").prop("disabled", false).text("Replace Sku");  
                $("#BatchCreationSuccess-model").modal("show");  
            } else {  
                $("#BatchCreationFailure-model .modal-body p").text("Batch creation failed: " + response.message);  
                $("#BatchCreationFailure-model").modal("show");  
                $("#replaceMissingSKUs").prop("disabled", false).text("Replace Sku");  
            }  
        },  
        error: function (xhr, status, error) {  
            $("#replaceMissingSKUs").prop("disabled", false).text("Replace Sku");  
            $("#BatchCreationFailure-model .modal-body p").text("Batch creation failed: " + error);  
            $("#BatchCreationFailure-model").modal("show");  
        }  
    });  
}  
  
function handleReplaceMissingSkusForAddOrdersToBatch() {  
    submitAddOrdersToBatchForm();  
}  

    function handleUpdateDepartmentsForCreateBatch() {  
        var selectedERPOrderIds = $("#selectedOrderIds-BatchCreation").val();  
        var batchId = $("#openBatchesSelect").val();  
        var batchNumber = $("#batchNameInput").val();  
        var isDeductible = $("#isDeductibleCheckbox").is(":checked");  
  
        $.ajax({  
            url: batchCreationUrl,  
            type: "POST",  
            data: {  
                ERPOrderIdsJson: selectedERPOrderIds,  
                BatchName: batchNumber,  
                BatchType: selectedIndex,  
                IsDeductible: isDeductible,  
                assignedDepartmentsJson: assignedDepartmentsJson,  
                replacementSkusJson: replacementSkusJson,  
            },  
            success: function (response) {  
                $("#updateDepartments").prop("disabled", false).text("Update Departments");  
                if (response.status === "success") {  
                    $("#UnassignedDepartments-model").modal('hide');  
                    $("#BatchCreationSuccess-model").modal("show");  
                } else {  
                    alert("Error updating departments and saving batch: " + response.message);  
                }  
            },  
            error: function (xhr, status, error) {  
                $("#updateDepartments").prop("disabled", false).text("Update Departments");  
                alert("Error updating departments and saving batch: " + error);  
            }  
        });  
    }  
  
    function handleUpdateDepartmentsForAddOrdersToBatch() {  
        submitAddOrdersToBatchForm();  
    }  

    // Function to handle unassigned departments  
    function handleUnassignedDepartments(unassignedDepartments) {  
        var unassignedDepartmentsContainer = $("#unassignedDepartmentsContainer");  
        unassignedDepartmentsContainer.empty();  
        unassignedDepartments.forEach(function (orderItemId) {  
            var departmentDiv = $('<div>', { class: 'unassigned-department' });  
            departmentDiv.append($('<p>', { text: 'Unassigned Department: ' + orderItemId }));  
            departmentDiv.append($('<input>', { type: 'hidden', value: orderItemId }));  
            var select2Dropdown = $('<select>', { class: 'select2 form-control' });  
            departmentDiv.append(select2Dropdown);  
            select2Dropdown.select2({  
                placeholder: 'Select a department...',  
                data: activeDepartments,  
                dropdownParent: $('#UnassignedDepartments-model')  
            });  
            var horizontalLine = $('<hr>');  
            departmentDiv.append(horizontalLine);  
            unassignedDepartmentsContainer.append(departmentDiv);  
        });  
        $("#MissingSKUs-model").modal('hide');  
        $("#UnassignedDepartments-model").modal('show');  
    }  

    // Populate the selected orders when the modal is shown  
    $("a.dropdown-item[data-bs-target='#AddOrderToBatch-modal']").on("click", function () {
        var selectedOrderNumbers = [];
        var selectedERPOrderIds = [];
        $(".row-select:checked").each(function () {
            var row = ordersTable.row($(this).closest("tr")).data();
            selectedOrderNumbers.push(row.orderNumber);
            selectedERPOrderIds.push(row.cwaOrderId);
        });

        var orderNumbersList = $("#selectedOrderNumbersList");
        orderNumbersList.empty();
        selectedOrderNumbers.forEach(function (orderNumber) {
            orderNumbersList.append("<li>" + orderNumber + "</li>");
        });

        $("#ERPOrderIdsJson").val(JSON.stringify(selectedERPOrderIds));

            $.ajax({  
        url: '/Orders/CheckForDuplicateBatches', 
        type: "GET",  
        data: { ERPOrderIdsJson: JSON.stringify(selectedERPOrderIds) },  
        success: function (response) {  
            if (response.status === "duplicate_batches") {  
                var duplicateBatchAndOrderNumbers = response.duplicateBatchInfos.map(function (info) {  
                    return info.batchNumber + ': ' + info.orderNumber;  
                });  

                var errorMessage = "Duplicate orders were found in existing/previous batches.\n" + duplicateBatchAndOrderNumbers.join("\n");
                $("#duplicateBatchError").text(errorMessage);

                $("#addOrderToBatchButton").prop("disabled", true).addClass("btn-secondary").removeClass("btn-primary");  
            } else {  
                $("#duplicateBatchError").text("");  

                $("#addOrderToBatchButton").prop("disabled", false).addClass("btn-primary").removeClass("btn-secondary");  
            }  
        },  
        error: function (xhr, status, error) {  
            alert("An error occurred: " + error);  
        }  
    });  
    });
});