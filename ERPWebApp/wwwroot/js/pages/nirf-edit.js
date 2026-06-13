//dynamically adds another variation option
var urls = {
    locationBySiteId: "/NirfForms/LocationBySiteId"
}

var dropDownSelectors = {
    membraneSiteSelection: $("#membrane-site-select-id"),
    mainSiteSelection: $("#main-site-select-id"),
    membraneLocationSelection: $("#membrane-location-select-id"),
    altMembraneLocationSelection: $("#alt-membrane-location-id"),
    mainLocationSelection: $("#main-location-select-id"),
    altMainLocationSelection: $("#alt-main-location-id")
}

function GetLocationBySiteId(_siteId) {
    let url = urls.locationBySiteId;
    var location = dropDownSelectors.membraneSiteSelection.attr('data-locations');
    location = location ? JSON.parse(location) : null;




    dropDownSelectors.membraneLocationSelection.select2({
        placeholder: "Select a Category",
        ajax: {
            url: url,
            type: 'GET',
            dataType: 'json',
            data: function (params) {
                return {
                    SiteId: _siteId
                };
            },
            success: function (data) {
                dropDownSelectors.membraneLocationSelection.empty()
                var results = data.map(item => ({
                    id: item.siteId,         // Value for the option
                    text: item.locationName  // Display text for the option
                }));

                dropDownSelectors.membraneLocationSelection.select2({
                    placeholder: "Select a Category",
                    data: results
                });
                dropDownSelectors.membraneLocationSelection.select2('close');
            },
            error: function (xhr, status, error) {
                console.error("AJAX Request Failed:", status, error);
            }
        }
    });


    dropDownSelectors.altMembraneLocationSelection.select2({
        placeholder: "Select a Category",
        ajax: {
            url: url,
            type: 'GET',
            dataType: 'json',
            data: function (params) {
                return {
                    SiteId: _siteId
                };
            },
            success: function (data) {
                dropDownSelectors.altMembraneLocationSelection.empty()

                var results = data.map(item => ({
                    id: item.siteId,
                    text: item.locationName
                }));

                dropDownSelectors.altMembraneLocationSelection.select2({
                    placeholder: "Select a Category",
                    data: results
                });
                dropDownSelectors.altMembraneLocationSelection.select2('close');

            },
            error: function (xhr, status, error) {
                console.error("AJAX Request Failed:", status, error);
            }
        }
    });

    dropDownSelectors.membraneLocationSelection.select2('open');
    dropDownSelectors.altMembraneLocationSelection.select2('open');


    if (location && location.MembraneLocationId) {
        dropDownSelectors.membraneLocationSelection.val(location.MembraneLocationId).trigger('change');
        dropDownSelectors.altMembraneLocationSelection.val(location.MembraneLocationId).trigger('change');
    }
}

function GetMainLocationBySiteId(_siteId) {
    let url = urls.locationBySiteId;
    var location = dropDownSelectors.mainSiteSelection.attr('data-locations');
    debugger
    location = location ? JSON.parse(location) : null;


    dropDownSelectors.altMainLocationSelection.select2({
        placeholder: "Select a Category",
        ajax: {
            url: url,
            type: 'GET',
            dataType: 'json',
            data: function (params) {
                // Pass the SiteId as query parameter
                return {
                    SiteId: _siteId
                };
            },
            success: function (data) {
                dropDownSelectors.altMainLocationSelection.empty()
                // Format the data for Select2 options
                var results = data.map(item => ({
                    id: item.siteId,         // Value for the option
                    text: item.locationName  // Display text for the option
                }));

                // Initialize Select2 with pre-populated data
                dropDownSelectors.altMainLocationSelection.select2({
                    placeholder: "Select a Category",
                    data: results  // Pass the pre-populated data to Select2
                });
                dropDownSelectors.altMainLocationSelection.select2('close');
                // $('#membrane-location-select-id').val(location.MembraneLocationId).trigger('change');
            },
            error: function (xhr, status, error) {
                console.error("AJAX Request Failed:", status, error);
            }
        }
    });


    dropDownSelectors.mainLocationSelection.select2({
        placeholder: "Select a Category",
        ajax: {
            url: url,
            type: 'GET',
            dataType: 'json',
            data: function (params) {
                return {
                    SiteId: _siteId
                };
            },
            success: function (data) {
                dropDownSelectors.mainLocationSelection.empty()
                var results = data.map(item => ({
                    id: item.siteId,
                    text: item.locationName
                }));

                dropDownSelectors.mainLocationSelection.select2({
                    placeholder: "Select a Category",
                    data: results
                });
                dropDownSelectors.mainLocationSelection.select2('close');

            },
            error: function (xhr, status, error) {
                console.error("AJAX Request Failed:", status, error);
            }
        }
    });

    dropDownSelectors.altMainLocationSelection.select2('open');
    dropDownSelectors.mainLocationSelection.select2('open');


    if (location && location.MembraneLocationId) {
        dropDownSelectors.altMainLocationSelection.val(location.MembraneLocationId).trigger('change');
        dropDownSelectors.mainLocationSelection.val(location.MembraneLocationId).trigger('change');
    }
}



var url = '/NirfForms/AddNirfProductMapping';
function add_fields() {
    var i = $('#sku-variations > div').length;

    // Create the new variation template with proper naming conventions
    var variationHtml = `
        <div id="variant-div-${i}">
            <input type="hidden" name="NirfProducts[${i}].ProductId" value="0" />
            <label class="control-label">Sku Variant ${i + 1}</label>
            <input id="sku-variation" name="NirfProducts[${i}].Sku" minlength="4" class="form-control" value="${$('#full-sku').val() }" type="text" required />
            <label class="control-label">Description Variant ${i + 1}</label>
            <input id="desc-variation" name="NirfProducts[${i}].Description" class="form-control" type="text" required />
            <i class="mdi mdi-24px mdi-delete item-action-btn" id="remove-button" onclick="removeValues(this)"></i><br/>
        </div>
    `;

    // Append the new HTML to the DOM
    $('#sku-variations').append(variationHtml);
}

//removes variation
removeValues = function (e) {
    $(e).parent().remove();
};
function addHash(elem) {
    var val = elem.value;
    if (!val.match(/^#/)) {
        elem.value = "#" + val;
    }
}

function infoInventory() {
    var popup = document.getElementById("info-popup");
    popup.classList.toggle("show");
}
function infoVariation() {
    var popup = document.getElementById("info-variant");
    popup.classList.toggle("show");
}
function infoSizing() {
    var popup = document.getElementById("info-sizing");
    popup.classList.toggle("show");
}
function infoPackaging() {
    var popup = document.getElementById("info-packaging");
    popup.classList.toggle("show");
}
function infoForecasting() {
    var popup = document.getElementById("info-forecasting");
    popup.classList.toggle("show");
}
function infoShipping() {
    var popup = document.getElementById("info-shipping");
    popup.classList.toggle("show");
}
function infoVendor() {
    var popup = document.getElementById("info-vendor");
    popup.classList.toggle("show");
}


$('#main-site-select-id').on("change", function (e) {
    GetMainLocationBySiteId($('#main-site-select-id').val());
    $('#main-location').show();
    $('#alt-main-location').show();
});

var mainSiteId = $('#main-site-select-id').val();
if (mainSiteId) {
    GetMainLocationBySiteId($('#main-site-select-id').val());
    $('#main-location').show();
    $('#alt-main-location').show();
}

$('#membrane-site-select-id').on("change", function (e) {
    GetLocationBySiteId($('#membrane-site-select-id').val());
    $('#membrane-location').show();
    $('#alt-membrane-location').show();
});

var membraneSiteId = $('#membrane-site-select-id').val();
if (membraneSiteId) {
    GetLocationBySiteId($('#membrane-site-select-id').val());
    $('#membrane-location').show();
    $('#alt-membrane-location').show();
}

$(document).ready(function () {
    $('#department-error').hide();
    $('.btnClose').click(function () {
        $(this).parents('.dropdown').find('button.dropdown-toggle').dropdown('toggle')
    });
    (function () {

        // Fetch all the forms we want to apply custom Bootstrap validation styles to
        var forms = document.querySelectorAll('.needs-validation');

        // Loop over them and prevent submission
        Array.prototype.slice.call(forms)
            .forEach(function (form) {
                form.addEventListener('submit', function (event) {
                    if (!form.checkValidity()) {
                        event.preventDefault();
                        event.stopPropagation();
                    }
                    form.classList.add('was-validated');
                }, false);
            });
    });
});
$("#thread-type").select2({
})
    .on("select2:select", function (e) {
        var providerId = $('#thread-type').val();
        if (providerId == 0) {
            var e = document.getElementById("thread-hex");
            e.style.display = "block";
            var f = document.getElementById("thread-code");
            f.style.display = "none";
        }
        else {
            var e = document.getElementById("thread-hex");
            e.style.display = "none";
            var f = document.getElementById("thread-code");
            f.style.display = "block";  
        }
    });
$('#add-provider').click(function () {
    var providerId = $('#shipper-list').val();
    $('#' + providerId).show();
    var s = document.getElementById(providerId + "-e")
    s.value = "false";
    $('#shipping-button').prop('disabled', false);
});

$(".dismiss").click(function () {
    $(this).parent().hide();
    var providerId = $(this).parent().attr('id');
    var s = document.getElementById(providerId + "-e");
    s.value = "true";

});

$("#DepartmentList").select2({
})
    .on("select2:select", function (e) {
        $('#department-error').hide();
    }).on("select2:unselect", function (e) {
        let selected_element = $(e.currentTarget);
        let select_val = selected_element.select2('data');
        if (select_val.length > 0) {
        }
        else {
            $('#department-error').show();
        }
    }).on("select2:", function (e) {
        var selected_element = $(e.currentTarget);
        var select_val = selected_element.select2('data');
        if (!select_val.length > 0) {
            $('#department-error').hide();
        }
    });


