$(document).ready(function () {
    $(".select2").select2();

    //set inital store selection
    $("#StoreName").val(storeName);

    //set selected store name
    $('#StoreId').on('change', function (e) {
        var selectedText = $(this).find('option:selected').text();

        $("#StoreName").val(selectedText);
    });

    //load products to order item drop down
    InitOrderItemSelect2();

    //add existing items to the order item table
    AddExistingItems();
});

//add existing order items to the table
function AddExistingItems() {
    if (orderItems && orderItems != null && Array.isArray(orderItems) && orderItems.length > 0) {

        orderItems.forEach(function (val, index) {

            const orderItem = new OrderItem(
                val.cwaOrderItemId,
                val.quantity,
                val.sku,
                val.name,
                val.imageUrl,
                val.unitPrice,
                val.productId,
                val.product
            );

            AddRow(orderItem, index);
        });

    }
}

//add new product item to the table
function AddNewProduct() {
    let productId = $("#new-order-item").val();
    let table = document.getElementById("product-items-table").getElementsByTagName('tbody')[0];

    let addBtn = $("#add-new-product-btn");
    let addBtnProgress = $("#add-new-product-spinner");
    let errorPanel = $("#add-new-item-error");
    
    errorPanel.hide();//hide error panel
    addBtn.prop("disabled", true);//disable button
    addBtnProgress.show();//show progress

    if (!productId || productId == undefined || productId == null || productId == "") {
        errorPanel.html("Select a Product for the dropdown before continue.");
        errorPanel.show();

        addBtn.prop("disabled", false);//enable button
        addBtnProgress.hide();//hide progress
        return;
    }

    //check if the product already added. if added ignore
    var allTableData = GetTableData();
    if (allTableData && allTableData != null && Array.isArray(allTableData) && allTableData.length > 0) {
        var existing = allTableData.filter(r => r.ProductId == productId);

        if (existing && existing != null && Array.isArray(existing) && existing.length > 0) {
            errorPanel.html("This product is already included in the table.");
            errorPanel.show();

            addBtn.prop("disabled", false);//enable button
            addBtnProgress.hide();//hide progress
            return;
        }
    }

    $.ajax({
        url: '/Orders/GetProductBy',
        type: 'GET',
        data: { productId: productId },
        success: function (orderItem) {
            addBtn.prop("disabled", false);//enable button
            addBtnProgress.hide();//hide progress

            if (!orderItem || orderItem == undefined || orderItem == null) {
                errorPanel.html("Product selection failed. Please try again.");
                errorPanel.show();
                return;
            }
            
            AddRow(orderItem, table.rows.length)
        },
        error: function (data) {
            addBtn.prop("disabled", false);//enable button
            addBtnProgress.hide();//hide progress

            errorPanel.html("Product selection failed. Please try again.");
            errorPanel.show();
            console.log(data);
        }
    });
}

//add new product to the order item table
function AddRow(item, rowIndex) {
    let table = document.getElementById("product-items-table").getElementsByTagName('tbody')[0];
    let newRow = table.insertRow();
    newRow.setAttribute("data-bs-placement", "left");
    newRow.setAttribute("data-bs-toggle", "popover");
    newRow.setAttribute("data-bs-trigger", "hover");
    newRow.setAttribute("data-bs-content", '&lt;img alt="' + item.name + '" src="' + item.imageUrl + '" width="80" height="80" /&gt;');
    newRow.setAttribute("data-bs-html", "true");
    newRow.setAttribute("id", "product-items-row-" + rowIndex);

    for (let i = 0; i < 7; i++) {

        let cell = newRow.insertCell(i);

        //add product image
        if (i === 0 && item.imageUrl && item.imageUrl != undefined && item.imageUrl != null && item.imageUrl != "") {
            cell.setAttribute("style", "text-align: center;");

            let img = document.createElement("img");
            img.src = item.imageUrl;
            img.className = "img-fluid";
            img.setAttribute("style", "max-height: 80px; width: auto;");
            cell.appendChild(img);
        }
        //add product SKU
        if (i === 1) {
            cell.setAttribute("style", "text-align: center;");
            cell.textContent = item.sku;
        }
        //add design name
        if (i === 2 && item.sku.includes("UVP")) {
            cell.setAttribute("style", "text-align: center;");

            let startIndex = item.sku.indexOf("UVP");
            cell.textContent = startIndex;
        }
        //add item name
        if (i === 3) {
            cell.setAttribute("style", "text-align: center;");

            let nameTextArea = document.createElement("textarea");
            nameTextArea.id = "Name";
            nameTextArea.className = "form-control";
            nameTextArea.rows = 3;
            nameTextArea.value = item.name;
            cell.appendChild(nameTextArea);

            //add error messages
            let nameErr = document.createElement("span");
            nameErr.id = "name-error-" + rowIndex;
            nameErr.className = "text-danger";
            nameErr.setAttribute("style", "display: none;");
            cell.appendChild(nameErr);
        }
        //add unit price
        if (i === 4) {
            cell.setAttribute("style", "text-align: right; padding-right: 8px;");

            cell.textContent = DecimalToCurrencyString(item.unitPrice, 2);
        }
        //add quantity and other necessary elements
        if (i === 5) {
            cell.setAttribute("style", "text-align: center;");

            //row index
            let rowInput = document.createElement("input");
            rowInput.type = "hidden";
            rowInput.id = "RowIndex";
            rowInput.value = rowIndex;
            cell.appendChild(rowInput);

            //product id
            let pIdInput = document.createElement("input");
            pIdInput.type = "hidden";
            pIdInput.id = "ProductId";
            pIdInput.value = (item.product && item.product != null) ? item.product.productId : null;
            cell.appendChild(pIdInput);

            //order item id
            let itemIdInput = document.createElement("input");
            itemIdInput.type = "hidden";
            itemIdInput.id = "ERPOrderItemId";
            itemIdInput.value = item.cwaOrderItemId;
            cell.appendChild(itemIdInput);

            //product SKU
            let skuInput = document.createElement("input");
            skuInput.type = "hidden";
            skuInput.id = "Sku";
            skuInput.value = item.sku;
            cell.appendChild(skuInput);

            //product quantity
            let itemQuInput = document.createElement("input");
            itemQuInput.type = (item.sku && item.sku != "") ? "number" : "hidden";
            itemQuInput.id = "Quantity";
            itemQuInput.value = item.quantity;
            itemQuInput.step = 1;
            itemQuInput.min = 1;
            itemQuInput.className = "form-control";
            cell.appendChild(itemQuInput);

            //add error messages
            let qtyErr = document.createElement("span");
            qtyErr.id = "qty-error-" + rowIndex;
            qtyErr.className = "text-danger";
            qtyErr.setAttribute("style", "display: none;");
            cell.appendChild(qtyErr);

            if (!item.sku || item.sku == undefined || item.sku == null || item.sku === "") {
                let qtySpan = document.createElement("span");
                qtySpan.textContent = item.quantity; 
                cell.appendChild(qtySpan);
            }
        }
        //add remove item button
        if (i === 6) {
            cell.setAttribute("style", "text-align: center;");

            let removeBtn = document.createElement("a");
            removeBtn.setAttribute("class", "mdi mdi-24px mdi-delete");
            removeBtn.href = "#";
            removeBtn.onclick = function () {
                removeItem("product-items-row-" + rowIndex);
            }
            cell.appendChild(removeBtn);
        }
    }
}

//remove an item from the table
function removeItem(rowId) {
    var tableErr = $('#product-items-table-error');
    var rowCount = $('#product-items-table >tbody >tr').length;

    tableErr.hide();
    if (rowCount && rowCount > 1) {
        $('#' + rowId).remove();
    }
    else {
        tableErr.html("You can't remove all order items.");
        tableErr.show();
    }
}

//get all data in the table as a JSON
function GetTableData() {
    let table = document.getElementById("product-items-table").getElementsByTagName('tbody')[0];
    let rows = table.getElementsByTagName('tr');
    let data = [];

    for (let i = 0; i < rows.length; i++) {
        let inputs = rows[i].getElementsByTagName('input');
        let textAreas = rows[i].getElementsByTagName('textarea');
        let rowData = {};

        for (let j = 0; j < inputs.length; j++) {
            //convert numbers
            if (inputs[j].id === "RowIndex" || inputs[j].id === "ProductId"
                || inputs[j].id === "ERPOrderItemId" || inputs[j].id === "Quantity") {

                if (inputs[j].value && inputs[j].value != null && inputs[j].value != "" && !isNaN(inputs[j].value)) {
                    rowData[inputs[j].id] = parseInt(inputs[j].value);
                }
                else {
                    rowData[inputs[j].id] = 0;
                }

            }
            else {
                rowData[inputs[j].id] = inputs[j].value;
            }
        }
        for (let h = 0; h < textAreas.length; h++) {
            rowData[textAreas[h].id] = textAreas[h].value;
        }

        data.push(rowData);
    }

    return data;
}

function SubmitData() {
    var jsonData = GetTableData();
    var tableErr = $('#product-items-table-error');

    if (!jsonData || jsonData == undefined || jsonData == null || !Array.isArray(jsonData) || jsonData.length == 0) {
        tableErr.html("Cannot retrieve the product item details. Please try again.");
        tableErr.show();
        return;
    }

    var isInvalid = false;
    //validate table data
    jsonData.forEach(function (rowData, rowIndex) {
        if (!rowData.Name || rowData.Name == "") {
            $("#name-error-" + rowData.RowIndex).html("Product name cannot be empty.");
            $("#name-error-" + rowData.RowIndex).show();

            isInvalid = true;
        }
        if (!rowData.Quantity || rowData.Quantity == "" || rowData.Quantity == 0) {
            $("#qty-error-" + rowData.RowIndex).html("Quantity is required and must be greater than 0.");
            $("#qty-error-" + rowData.RowIndex).show();

            isInvalid = true;
        }
    });

    if (isInvalid) { return; }

    console.log(jsonData);
    //set item data to input
    $("#order-items-json-input").val(JSON.stringify(jsonData));

    //submit the form
    $("#duplicate-order-form").submit();

    $("#duplicate-order-btn").attr("disabled", true);//disable submit button
    $("#duplicate-order-btn-spinner").show();//show progress
}

//show/hide validation summery
document.addEventListener('DOMContentLoaded', function () {
    var validationSummary = document.getElementById('validation-summary');

    // Check if the validation summary contains any content
    if (validationSummary.innerText.trim() === "") {
        // If no errors, hide the validation summary
        validationSummary.style.display = 'none';
    } else {
        // If errors are present, show the validation summary
        validationSummary.style.display = 'block';
    }
});


//manage the order item related operations
function InitOrderItemSelect2() {

    $('#new-order-item').select2({
        ajax: {
            url: '/Products/GetAllProducts',
            dataType: 'json',
            type: "GET",
            data: function (params) {
                return { queryString: !params ? null : params.term };
            },
            processResults: function (data) {
                let select2Data = { results: [{ id: '', text: 'Select a Product' }] };

                if (data && Array.isArray(data)) {

                    data.forEach((value, index) => {
                        select2Data.results.push({ id: value.productId, text: value.sku + " | " + value.description });
                    });
                }

                return select2Data;
            }
        }
    });

}

//convert decimal value to readable currency string
function DecimalToCurrencyString(decimalValue, decimalPlaces = 2) {
    // Use toLocaleString to add commas as a thousands separator
    const formattedDecimal = Number(decimalValue).toFixed(decimalPlaces).replace(/\B(?=(\d{3})+(?!\d))/g, ',');

    return formattedDecimal;
}


class OrderItem {
    constructor(cwaOrderItemId, quantity, sku, name, imageUrl, unitPrice, productId, product) {
        this.cwaOrderItemId = cwaOrderItemId;
        this.quantity = quantity;
        this.sku = sku;
        this.name = name;
        this.imageUrl = imageUrl;
        this.unitPrice = unitPrice;
        this.productId = productId;
        this.product = product;
    }
}