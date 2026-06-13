const selectors = {
    isPicked: $("#is-picked"),
    extraLocationsCheckBox: $("#extra-locations-check-box"),
    pickEmployeeDropdown: $("#pick-employee-dropdown"),
    stockDropdown: $("#stock-dropdown"),
    submitButton: $("#submit-button"),
    stock: $("#stock"),
    extraLocations: $("#extra-locations"),
    pickEmployee: $("#pick-employee"),
    or: $("#or"),
    reasonExplanationContainer: $("#reasonExplanation-container"),
    pickReasonSelect: $("#pickReason-select")
};

const toggleVisibility = (elements, isVisible) => {
    const elementArray = Array.isArray(elements) ? elements : [elements];
    $(elementArray).each(function () {
        if (isVisible) {
            $(this).show();
        } else {
            $(this).hide();
        }
    });
};

const toggleDisable = (elements, isDisabled) => {
    $(elements).prop("disabled", isDisabled);
};

const isFormValid = () => {
    const isEmployeeSelected = selectors.pickEmployeeDropdown.val();
    const isExtraLocationChecked = selectors.extraLocationsCheckBox.prop("checked");
    const isStockSelected = selectors.stockDropdown.val();
    return isEmployeeSelected && (isExtraLocationChecked || isStockSelected);
};

const handleIsPickedChange = () => {
    const isPicked = selectors.isPicked.prop("checked");

    // Toggle visibility based on the "Is Picked" state
    toggleVisibility([selectors.stock, selectors.extraLocations, selectors.pickEmployee, selectors.or], isPicked);

    // Disable/Enable elements based on the state
    toggleDisable([selectors.submitButton, selectors.pickEmployeeDropdown], !isPicked);
    toggleDisable(selectors.extraLocationsCheckBox, !isPicked);

    // Handle stock dropdown based on extra locations checkbox
    const extraLocationsChecked = selectors.extraLocationsCheckBox.prop("checked");
    toggleDisable(selectors.stockDropdown, !isPicked || extraLocationsChecked);

    // Enable submit button based on form validity
    toggleDisable(selectors.submitButton, !isFormValid());
};

// Handle change event for the "Extra Locations" checkbox
const handleExtraLocationsChange = () => {
    toggleDisable(selectors.stockDropdown, selectors.extraLocationsCheckBox.prop("checked"));
    toggleDisable(selectors.submitButton, !isFormValid());
};

// Handle change event for the "Pick Employee" dropdown
const handlePickEmployeeChange = () => {
    toggleDisable(selectors.submitButton, !isFormValid());
};

// Handle change event for the "Pick Reason" dropdown (for toggling reason explanation)
const handlePickReasonChange = () => {
    const selectedReason = selectors.pickReasonSelect.val();
    toggleVisibility(selectors.reasonExplanationContainer, selectedReason === "Operator Error");
};

// Initialize Select2 for dropdowns
const initializeSelect2 = () => {
    $(".select2").select2();
};

// Bind event listeners for all necessary elements
const bindEventListeners = () => {
    selectors.isPicked.on('change', handleIsPickedChange);
    selectors.extraLocationsCheckBox.on('change', handleExtraLocationsChange);
    selectors.pickEmployeeDropdown.on('change', handlePickEmployeeChange);
    selectors.pickReasonSelect.on('change', handlePickReasonChange);
};

// Initialize the application (select2 and event listeners)
const initializeApp = () => {
    initializeSelect2();
    handleIsPickedChange();
    handleExtraLocationsChange();
    handlePickEmployeeChange();
    handlePickReasonChange();
    bindEventListeners();
};

// Run initialization when the document is ready
$(document).ready(() => {
    initializeApp();
});