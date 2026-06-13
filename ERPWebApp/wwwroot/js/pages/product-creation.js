function addSkuCategories(dropdownData) {
    for (var i = 0; i < dropdownData.length; i++) {
        var newOption = new Option(dropdownData[i].category, dropdownData[i].attribute, false, false);
        $("#sku-category").append(newOption).trigger('change');
    }
}

function addSkuColors(dropdownData) {
    for (var i = 0; i < dropdownData.length; i++) {
        var newOption = new Option(dropdownData[i].color, dropdownData[i].attribute, false, false);
        $("#sku-color").append(newOption).trigger('change');
    }
}

function addSkuUnitOfMeasure(dropdownData) {
    for (var i = 0; i < dropdownData.length; i++) {
        var newOption = new Option(dropdownData[i].unitOfMeasure, dropdownData[i].attribute, false, false);
        $("#sku-UOM").append(newOption).trigger('change');
    }
}

function buildSkuItem() {
    $("#full-sku").val(
        $("#sku-category").select2().val() +
        $("#sku-UOM").select2().val() +
        $("#sku-color").select2().val()
    );

    if ($("#sku-set-number").val() > 1) {
        $("#full-sku").val($("#full-sku").val() + 'SO' + $("#sku-set-number").val());
    }
    //else {
    //    if ($("#full-sku").val().includes('SO')) {
    //        buildSkuItem();
    //    }
    //}
}
