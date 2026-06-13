function showOrderSkus(dropdownData) {
    for (var i = 0; i < dropdownData.length; i++) {
        var newOption = new Option(dropdownData[i].itemSku, dropdownData[i].itemSku, false, false);
        $("#SkuNumber").append(newOption).trigger('change');
    }
}

function getFormattedDate(date) {
    var year = date.getFullYear();
    var month = (1 + date.getMonth()).toString();
    month = month.length > 1 ? month : '0' + month;
    var day = date.getDate().toString();
    day = day.length > 1 ? day : '0' + day;
    return year + '-' + month + '-' + day;
}
