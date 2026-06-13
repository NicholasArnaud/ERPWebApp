(function ($) {
    const SELECTORS = {
        weightInput: $("#weight-value-input"),
        lengthInput: $("#length-value-input"),
        widthInput: $("#width-value-input"),
        heightInput: $("#height-value-input"),
        weightUnitDropdown: $("#weight-unit-dropdown"),
        dimensionalUnitDropdown: $("#dimensional-unit-dropdown"),
        getEstimatesBtn: $("#get-estimates-btn"),
        createShipmentBtn: $("#create-shipment-btn"),
        addRemoveToggle: $("#addRemoveToggle"),
        weightWarning: $("#weight-warning"),
        volumeWarning: $("#volume-warning"),
        form: $("#modal-form"),
        errorModal: $("#error-modal"),
        errorModalMessage: $("#error-modal-message"),
        shipmentEstimateModal: $('#ShipmentEstimate-modal'),
        carrierId: $('#carrierId'),
        carrierCode: $('#carrierCode'),
        carrierNickname: $('#carrierNickname'),
        serviceCode: $('#serviceCode'),
        estimatedShipmentCost: $('#estimatedShipmentCost'),
        packageCode: $('#packageCode'),
        shipFrom: $('#shipFrom'),
        addOrRemoveInput: $("#addOrRemove"),
        getEstimatesSpinner: $('#get-estimates-spinner'),
        getEstimatesBtnText: $('#get-estimates-btn-text'),
        orderIdHidden: $("#orderId-hidden"),
        orderKeyHidden: $("#orderKey-hidden")
    };

    const CONSTANTS = {
        toleranceInt: 0.01,
        weightConversionFactors: {
            ounce: 1,
            pound: 16,
            gram: 0.035274,
            kilogram: 35.274
        }
    };

    function roundUpWeight() {
        const value = parseFloat(SELECTORS.weightInput.val());
        if (!isNaN(value)) {
            SELECTORS.weightInput.val(Math.ceil(value));
        }
    }

    function handleGetEstimates(action) {
        showLoadingState(true);
        $.ajax({
            type: "POST",
            url: action,
            data: SELECTORS.form.serialize(),
            datatype: "json",
            success: function (responseData) {
                shipmentEstimateModal(responseData);
            },
            complete: function () {
                showLoadingState(false);
            },
            error: function (response) {
                showErrorModal("Error occurred while fetching estimates: " + (response.responseText || "Unknown error"));
            }
        });
    }

    function handleCreateShipment(action) {
        let serializedData = SELECTORS.form.serialize();
        $.ajax({
            type: "POST",
            url: action,
            data: serializedData,
            datatype: "json",
            success: function (responseData) {
                handleLabelResponse(responseData);
            },
            error: function (response) {
                showErrorModal("Error occurred while creating a label: " + (response.responseText || "Unknown error"));
            }
        });
    }

    function handleLabelResponse(responseData) {
        if (responseData.success) {
            const labelDataBytes = atob(responseData.labelData);
            const byteArray = new Uint8Array(labelDataBytes.length);
            for (let i = 0; i < labelDataBytes.length; i++) {
                byteArray[i] = labelDataBytes.charCodeAt(i);
            }
            const blob = new Blob([byteArray], { type: 'application/pdf' });
            const url = window.URL.createObjectURL(blob);
            const a = document.createElement('a');
            a.href = url;
            a.download = responseData.filename;
            document.body.append(a);
            a.click();
            a.remove();
            window.URL.revokeObjectURL(url);

            setTimeout(function () {
                window.location.href = window.OrderAction;
            }, 1000);
        } else {
            showErrorModal(responseData.error || "Unknown error occurred while generating label");
        }
    }

    function updateAddOrRemoveValue() {
        const isChecked = SELECTORS.addRemoveToggle.is(":checked");
        SELECTORS.addOrRemoveInput.val(isChecked);
    }

    function calculateMinimumDimensions(totalVolume) {
        if (!totalVolume) return null;
        const minDimension = Math.ceil(Math.cbrt(totalVolume));
        return { length: minDimension, width: minDimension, height: minDimension };
    }

    function convertToOunces(weight, unit) {
        const factor = CONSTANTS.weightConversionFactors[unit.toLowerCase()];
        return weight * (factor || 1);
    }

    function showLoadingState(isLoading) {
        SELECTORS.getEstimatesSpinner.toggle(isLoading);
        SELECTORS.getEstimatesBtnText.toggle(!isLoading);
        SELECTORS.getEstimatesBtn.prop("disabled", isLoading);
    }

    function showErrorModal(message) {
        SELECTORS.errorModalMessage.text(message);
        SELECTORS.errorModal.modal('show');
        console.error(message);
    }

    function toSingularWeightUnit(unit) {
        if (!unit) return 'ounce';
        switch (unit.toLowerCase()) {
            case "ounces": return "ounce";
            case "pounds": return "pound";
            case "grams": return "gram";
            case "kilograms": return "kilogram";
            default: return unit;
        }
    }

    function shipmentEstimateModal(responseData) {
        if (!responseData) return;

        SELECTORS.carrierId.text(responseData.carrierId || '');
        SELECTORS.carrierCode.text(responseData.carrierCode || '');
        SELECTORS.carrierNickname.text(responseData.carrierNickname || '');
        SELECTORS.serviceCode.text(responseData.serviceCode || '');
        SELECTORS.estimatedShipmentCost.text(responseData.estimatedShipmentCost || '');
        SELECTORS.packageCode.text(responseData.packageCode || '');

        try {
            if (responseData.shipFrom) {
                updateShipFromAddress(JSON.parse(responseData.shipFrom));
            }
        } catch (e) {
            console.error("Error parsing shipFrom data", e);
        }

        SELECTORS.shipmentEstimateModal.modal('show');
    }

    function updateShipFromAddress(shipFromJson) {
        if (!shipFromJson) return;

        SELECTORS.shipFrom.empty();
        const fragment = document.createDocumentFragment();
        const addAddressLine = (text) => {
            if (text) {
                const div = document.createElement('div');
                div.textContent = text;
                fragment.appendChild(div);
            }
        };

        addAddressLine(shipFromJson.street1);
        addAddressLine(shipFromJson.street2);
        addAddressLine(shipFromJson.street3);

        const cityStateZip = [];
        if (shipFromJson.city) cityStateZip.push(shipFromJson.city);
        if (shipFromJson.state) cityStateZip.push(shipFromJson.state);
        if (shipFromJson.postalCode) cityStateZip.push(shipFromJson.postalCode);

        if (cityStateZip.length > 0) {
            addAddressLine(cityStateZip.join(' '));
        }

        addAddressLine(shipFromJson.country);

        SELECTORS.shipFrom.append(fragment);
    }

    function checkWeightWarning(currentWeightInOunces, expectedWeightInOunces, zeroWeightSkus, roundedExpectedWeight) {
        if (currentWeightInOunces + CONSTANTS.toleranceInt < expectedWeightInOunces || zeroWeightSkus.length > 0) {
            let warningMessage = "<br> Expected weight: " + roundedExpectedWeight + " ounces";
            if (zeroWeightSkus.length > 0) {
                warningMessage += "<br>Missing weight for the following SKUs: " + zeroWeightSkus.join(", ");
            }
            SELECTORS.weightWarning.html(warningMessage);
            return true;
        }
        SELECTORS.weightWarning.text("");
        return false;
    }

    function checkVolumeWarning(currentVolume, expectedVolume, selectedDimensionalUnit, dimensionalUnit, zeroVolumeSkus, minimumDimensions) {
        if (isNaN(currentVolume) || currentVolume + CONSTANTS.toleranceInt < expectedVolume ||
            selectedDimensionalUnit !== dimensionalUnit || zeroVolumeSkus.length > 0) {

            let warningMessage = "<br> Dimensions are too low!";
            if (zeroVolumeSkus.length > 0) {
                warningMessage += "<br>Missing volume for the following SKUs: " + zeroVolumeSkus.join(", ");
            }
            SELECTORS.volumeWarning.html(warningMessage);
            return true;
        }
        SELECTORS.volumeWarning.text("");
        return false;
    }

    function toggleCreateShipmentButton(hasWeightWarning, hasVolumeWarning, currentWeight, currentLength, currentWidth, currentHeight) {
        const isValidWeight = currentWeight > 0;
        const isValidDimensions = currentLength > 0 && currentWidth > 0 && currentHeight > 0;
        const noWarnings = !hasWeightWarning && !hasVolumeWarning;

        SELECTORS.createShipmentBtn.prop("disabled", !(isValidWeight && isValidDimensions && noWarnings));

        // Update button styling based on state
        if (SELECTORS.createShipmentBtn.prop("disabled")) {
            SELECTORS.createShipmentBtn.addClass("btn-secondary").removeClass("btn-dark");
        } else {
            SELECTORS.createShipmentBtn.addClass("btn-dark").removeClass("btn-secondary");
        }
    }

    window.initialize_OrderDetails = ({ RateEstimateAction, GenerateLabelAction, Order }) => {
        window.OrderAction = Order;
        roundUpWeight();

        $(".select2").select2();

        updateAddOrRemoveValue();

        SELECTORS.getEstimatesBtn.on("click", function (event) {
            event.preventDefault();
            handleGetEstimates(RateEstimateAction);
        });

        SELECTORS.createShipmentBtn.on("click", function (event) {
            event.preventDefault();
            SELECTORS.createShipmentBtn.prop("disabled", true);

            handleCreateShipment(GenerateLabelAction);
        });

        SELECTORS.addRemoveToggle.on("change", updateAddOrRemoveValue);
    };

    window.updateUI = ({ orderWeight, orderVolume, zeroWeightSkus, zeroVolumeSkus, weightUnit, dimensionalUnit }) => {
        if (orderWeight === null || orderVolume === null) return;

        const currentWeight = parseFloat(SELECTORS.weightInput.val()) || 0;
        const currentLength = parseFloat(SELECTORS.lengthInput.val()) || 0;
        const currentWidth = parseFloat(SELECTORS.widthInput.val()) || 0;
        const currentHeight = parseFloat(SELECTORS.heightInput.val()) || 0;

        const selectedWeightUnit = toSingularWeightUnit(SELECTORS.weightUnitDropdown.find("option:selected").text());
        const selectedDimensionalUnit = SELECTORS.dimensionalUnitDropdown.find("option:selected").text().toLowerCase();

        dimensionalUnit = (selectedDimensionalUnit && dimensionalUnit) ? dimensionalUnit.toLowerCase() : dimensionalUnit;

        // Convert weights to ounces for comparison
        const currentWeightInOunces = convertToOunces(currentWeight, selectedWeightUnit);
        const expectedWeightInOunces = orderWeight;

        // Calculate volumes
        const currentVolume = currentLength * currentWidth * currentHeight;
        const minimumDimensions = calculateMinimumDimensions(orderVolume);

        // Check warnings
        const hasWeightWarning = checkWeightWarning(
            currentWeightInOunces,
            expectedWeightInOunces,
            zeroWeightSkus,
            expectedWeightInOunces.toFixed(2)
        );

        const hasVolumeWarning = checkVolumeWarning(
            currentVolume,
            orderVolume,
            selectedDimensionalUnit,
            dimensionalUnit,
            zeroVolumeSkus,
            minimumDimensions
        );

        // Update button state
        toggleCreateShipmentButton(
            hasWeightWarning,
            hasVolumeWarning,
            currentWeight,
            currentLength,
            currentWidth,
            currentHeight
        );
    };
})(jQuery);