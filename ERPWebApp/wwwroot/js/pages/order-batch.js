
$("#successModalOkButton").on("click", function () {
    $("#successModal").modal("hide");
});
// Close the failure modal when the OK button is clicked
$("#failureModalOkButton").on("click", function () {
    $("#failureModal").modal("hide");
});

//$("#removeOrderModalYesButton").on("click", function () {
//    var cwaOrderId = $("#removeOrderModal").data("cwa-order-id");
//    var orderBatchId = $("#removeOrderModal").data("order-batch-id");
//    removeOrders(cwaOrderId, orderBatchId);
//});
//$("#removeOrderModal .btn-secondary").on("click", function () {
//    $("#removeOrderModal").modal("hide");
//});

$("#completed-batch-dropdown").on("change", function () {

    var table = $('#completed-batches-table-container');

   //  Empty the tbody and show the loading spinner
    table.find("tbody").empty();
    document.getElementById('completed-batches-loading-spinner').style.display = 'block';

    var orderBatchId = $(this).val();

    // Use the load method to fetch and load the partial view HTML
    $('#completed-batches-table-container').load('/OrderBatch/GetCompletedBatchItems?orderBatchId=' + orderBatchId, function (response, status, xhr) {
        if (status === "error") {
            console.error("Error fetching Completed Batch PartialView:", xhr.statusText);
        } else {
            // Hide the loading spinner once the content is loaded
            document.getElementById('completed-batches-loading-spinner').style.display = 'none';
        }
    });

});

$("#design-batch-dropdown").on("change", function () {
    var table = $('#design-batches-container');

    // Empty the tbody and show the loading spinner
    table.find("tbody").empty();
    document.getElementById('design-batches-loading-spinner').style.display = 'block';

    const orderBatchId = $(this).val();

    // Use the load method to fetch and load the partial view HTML
    $("#design-batches-container").load(
        "/OrderBatch/GetDesignBatchItems?orderBatchId=" + orderBatchId,
        function (response, status, xhr) {
            document.getElementById('design-batches-loading-spinner').style.display = 'none';

            if (status === "error") {
                console.error("Error fetching design batch product mappings:", xhr.statusText);
            } else {
                // Conditionally show or hide elements based on the loaded content
                if ($("#design-batches-table tbody tr").length > 0) {
                    $(".department-selection").show();
                    $("#design-batch-buttons").show();
                } else {
                    $(".department-selection").hide();
                    $("#design-batch-buttons").hide();
                }
            }
        }
    );
});



$("#inventory-batch-dropdown").on("change", function () {
    console.log("Dropdown changed");
    var table = $('#inventory-batches-container');

    // Empty the tbody and show the loading spinner
    table.find("tbody").empty();
    document.getElementById('inventory-batches-loading-spinner').style.display = 'block';

    const orderBatchId = $(this).val();
    console.log("Selected orderBatchId: ", orderBatchId);

    // Use the load method to fetch and load the partial view HTML
    $("#inventory-batches-container").load(
        "/OrderBatch/GetTransferableBatchItems?orderBatchId=" + orderBatchId,
        function (response, status, xhr) {
            document.getElementById('inventory-batches-loading-spinner').style.display = 'none';

            if (status === "error") {
                console.error("Error fetching inventory batch products:", xhr.statusText);
            } else {
                console.log("SUCCESS");

                // Conditionally show or hide elements based on the loaded content
                if ($("#inventory-batches-table tbody tr").length > 0) {
                    $("#inventory-batch-buttons").show();
                } else {
                    $("#inventory-batch-buttons").hide();
                }
            }
        }
    );
});



$("#info-btn").on("click", function (e) {
    toggleHelpInfo(e, "info-container");
});
$(document).on('click', '#removeBatchButton', function () {
    console.log("Remove batch button clicked");
    $("#removeBatchModal").modal("show");
});

$(document).on('click', '#removeBatchConfirmButton', async function () {
    console.log("Remove batch confirm button clicked");
    var orderBatchId = $("#design-batch-dropdown").val();
    await removeBatch(orderBatchId);
    $("#removeBatchModal").modal("hide");
});  
$("#setStatusModalConfirmButton").on("click", async function () {
    const row = $(this).data("row");
    const newStatusId = $("#status-select").val();

    // Update the row with the new status (assuming the status is in the 5th column)  
    row.find("td:nth-child(5)").text(newStatusId);

    // Call your UpdateOrderBatchItemAsync function to save the changes to the database  
    const orderBatchItemId = row.data("orderbatchitemid");
    await updateOrderBatchItemStatus(orderBatchItemId, newStatusId);

    // Close the modal  
    $("#setStatusModal").modal("hide");
});  

async function updateOrderBatchItemStatus(orderBatchItemId, newStatusId) {
    const orderBatchItem = {
        OrderBatchItemId: orderBatchItemId,
        BatchItemStatusId: newStatusId,
    };
    await $.ajax({
        url: '/OrderBatch/UpdateOrderBatchItemAsync',
        type: 'POST',
        data: orderBatchItem,
    });
}

async function removeBatch(orderBatchId) {
    try {
        const result = await $.ajax({
            url: '/OrderBatch/RemoveBatch',
            type: 'POST',
            data: { orderBatchId: orderBatchId }
        });

        if (result) {
            location.reload();
        } else {
            alert("An error occurred while removing the batch. Please try again.");
        }
    } catch (error) {
        console.error("Error removing batch:", error);
    }
}

function checkUnknown(button, mainTableRow) {
    const cwaOrderId = $("#cwaOrderId").val();
    if (cwaOrderId === "Unknown") {
        $("#isUnknown").val("true");

        // Find the row containing the clicked remove button  
        const targetRow = mainTableRow.next().find("table.nested-table tbody tr").filter(function () {
            return $(this).find("td:eq(1)").text() === "Unknown";
        });

        // Get the SKU and quantity for the target row  
        const sku = targetRow.find("td:eq(0)").text(); // Assuming SKU is in the first column  
        const quantity = parseInt(targetRow.find("td:eq(2)").text()); // Assuming quantity is in the third column  

        // Create an array containing the target row's SKU and quantity  
        const unknownProducts = [{ sku: sku, quantity: quantity }];

        console.log(unknownProducts);
        // Serialize the unknown products list to a JSON string  
        const unknownProductsJson = JSON.stringify(unknownProducts);

        // Set the unknownProducts hidden input field value  
        $("#unknownProducts").val(unknownProductsJson);
    }
    else {
        $("#isUnknown").val("false");
    }
}

function getUnknownProductsInfoFromTable(currentEntry) {
    var productsInfo = [];

    // Find the nearest tr for the current entry (remove button)  
    var row = $(currentEntry).closest("tr");

    var cwaOrderId = row.find("td:nth-child(2)").text();
    if (cwaOrderId === "Unknown") {
        var sku = row.find("td:nth-child(1)").text();
        var requiredAmount = row.find("td:nth-child(3)").text();
        productsInfo.push({ sku: sku, requiredAmount: requiredAmount });
    }

    return productsInfo;
}

function toggleHelpInfo(e, containerId) {
    var container = $('#' + containerId);
    if (container.is(':visible')) {
        container.hide();
    } else {
        var alertMessage = `
                            <div class="alert alert-info alert-dismissible">
                            If a product is not found in stock, or there's not enough of a product in stock, you can't perform any transfers.
                            </div>
                        `;
        container.html(alertMessage).show();
    }
}

function getProductIdBySku(productSku) {
    return $.ajax({
        url: '/OrderBatch/GetProductIdBySku',
        type: 'GET',
        data: {
            sku: productSku
        }
    });
}
async function showSetStatusModal(row) { 
    const statuses = await fetchStatuses();
    const statusDropdown = $("#status-select");
    statusDropdown.empty();
    statuses.forEach(status => {
        statusDropdown.append(`<option value="${status.id}">${status.name}</option>`);
    });
  
    $("#setStatusModalConfirmButton").data("row", row);

    $("#setStatusModal").modal("show");
}  
