// Constants for DOM selectors
const selectors = {
    productSelect: "#product-select-id",
    crt: "#crt",
    pickReasonSelect: "#pickReason-select",
    toLocationSelect: "#toLocationId-select2",
    reasonExplanationContainer: "#reasonExplanation-container",
    orderSelect: "#order-select-id",
    requestTypeSelect: "#RequestType-select",
    orderNumberField: "#orderNumber-field"
};

// Cache for products by order
const productCache = {};

// Initialize Select2 for dropdowns
const initializeSelect2 = () => {
    $(`${selectors.productSelect}, ${selectors.crt}, ${selectors.pickReasonSelect}`).select2();
    $(selectors.toLocationSelect).select2({
        placeholder: { id: '0', text: 'Select a Location' }
    });
};

// Handle Pick Reason change event
const handlePickReasonChange = () => {
    const selectedReason = $(selectors.pickReasonSelect).val();
    $(selectors.reasonExplanationContainer).toggle(selectedReason === "Operator Error");
};

// Initialize Select2 for order selection with AJAX
const initializeOrderSelect2 = () => {
    $(selectors.orderSelect).select2({
        placeholder: 'Search Order Number...',
        minimumInputLength: 2,
        allowClear: true,
        ajax: {
            url: '/InventoryRequestForms/GetOrderNumbers',
            dataType: 'json',
            delay: 300,
            data: params => ({ term: params.term }),
            processResults: data => ({ results: data })
        }
    });
};

// Handle order selection event
const handleOrderSelect = e => {
    const OrderNumber = e.params.data.id;
    console.log("Selected Order ID:", OrderNumber);

    // Clear and reset the product dropdown
    $(selectors.productSelect).empty().trigger('change');

    // Fetch products based on RequestType
    loadProductsByRequestType(OrderNumber);
};

// Fetch all products (for Consumable Products request type)
const fetchAllProducts = () => {
    $.ajax({
        url: '/InventoryRequestForms/GetProducts',
        type: 'GET',
        data: { orderNumber: null },
        success: data => {
            populateProductDropdown(data);
        },
        error: (xhr, status, error) => {
            console.error("Error loading all products:", error);
        }
    });
};

// Load products based on RequestType and OrderNumber
const loadProductsByRequestType = (OrderNumber) => {
    const requestType = $(selectors.requestTypeSelect).val();

    if (requestType === 'Production Products') {
        if (productCache[OrderNumber]) {
            populateProductDropdown(productCache[OrderNumber]);
        } else {
            fetchAndCacheProducts(OrderNumber);
        }
    } else if (requestType === 'Consumable Products') {
        fetchAllProducts();
    }
};

// Fetch products for the selected order and cache them
const fetchAndCacheProducts = OrderNumber => {
    $.ajax({
        url: '/InventoryRequestForms/GetProducts',
        type: 'GET',
        data: { orderNumber: OrderNumber },
        success: data => {
            productCache[OrderNumber] = data; // Cache the data
            populateProductDropdown(data);
        },
        error: (xhr, status, error) => {
            console.error("Error loading products for the order:", error);
        }
    });
};

// Populate the product dropdown with data
const populateProductDropdown = data => {
    $(selectors.productSelect).select2({
        data,
        placeholder: 'Select products...'
    });
};

// Show/Hide Order Number field based on RequestType
const handleRequestTypeChange = () => {
    const requestType = $(selectors.requestTypeSelect).val();


    $(selectors.productSelect).empty().trigger('change');
    $(selectors.orderSelect).val(null).trigger('change');

    if (requestType === 'Production Products') {

        $(selectors.orderNumberField).show();
        const OrderNumber = $(selectors.orderSelect).val();
        if (OrderNumber) {
            loadProductsByRequestType(OrderNumber);
        }
    } else if (requestType === 'Consumable Products') {
        Object.keys(productCache).forEach(key => delete productCache[key]);
        $(selectors.orderNumberField).hide();

        fetchAllProducts();
    }
};

// Attach event listeners
const attachEventListeners = () => {
    $(selectors.pickReasonSelect).change(handlePickReasonChange);
    $(selectors.orderSelect).on('select2:select', handleOrderSelect);
    $(selectors.requestTypeSelect).change(handleRequestTypeChange); 
};

// Initialize the application
const initializeApp = () => {
    initializeSelect2();
    initializeOrderSelect2();
    attachEventListeners();

    if ($(selectors.requestTypeSelect).val()) {
        handleRequestTypeChange();
    }
};

// Ensure DOM is ready before initializing the app
$(document).ready(() => {
    initializeApp();
});