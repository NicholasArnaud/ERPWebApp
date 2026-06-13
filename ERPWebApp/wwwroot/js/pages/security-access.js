window.onload = function () {
    var url = '/SecurityAccess/GetAccessPlanIds';
    $.getJSON(url).done(function (response) {
        console.log(response);
        for (var i = 0; i < response.length; i++) {

            dragula([document.getElementById('door-list-left-' + response[i]), document.getElementById('door-list-right-' + response[i])], {
                isContainer: function (el) {
                    return false; // only elements in drake.containers will be taken into account
                },
                moves: function (el, source, handle, sibling) {
                    return true; // elements are always draggable by default
                },
                accepts: function (el, target, source, sibling) {
                    return true; // elements can be dropped in any of the `containers` by default
                },
                invalid: function (el, handle) {
                    return false; // don't prevent any drags from initiating by default
                },
                direction: 'vertical',             // Y axis is considered when determining where an element would be dropped
                copy: false,                       // elements are moved by default, not copied
                copySortSource: false,             // elements in copy-source containers can be reordered
                revertOnSpill: false,              // spilling will put the element back where it was dragged from, if this is true
                removeOnSpill: false,              // spilling will `.remove` the element, if this is true
                mirrorContainer: document.body,    // set the element that gets mirror elements appended
                ignoreInputTextSelection: true,     // allows users to select input text, see details below
                slideFactorX: 0,               // allows users to select the amount of movement on the X axis before it is considered a drag instead of a click
                slideFactorY: 0,               // allows users to select the amount of movement on the Y axis before it is considered a drag instead of a click
            })
                .on('drop', function (el, target, source, sibling) {
                    console.log(el);
                    var accessPlanId = el.getElementsByClassName("access-plan-id-secret")[0].innerText;
                    if (target.id == 'door-list-right-' + accessPlanId) {
                        url = '/SecurityAccess/SaveAccessPlanDoorMapping';
                        var AccessPointId = el.children[1].firstElementChild.children[0].children[1].innerText;
                        $.getJSON(url, {
                            AccessPlanId: accessPlanId,
                            AccessPointId: AccessPointId
                        }).done(function (response) {
                            console.log(response);
                        });
                    } else if (target.id == 'door-list-left-' + accessPlanId) {
                        url = '/SecurityAccess/DeleteAccessPlanDoorMapping';
                        var AccessPointId = el.children[1].firstElementChild.children[0].children[1].innerText;
                        $.getJSON(url, {
                            AccessPlanId: accessPlanId,
                            AccessPointId: AccessPointId
                        }).done(function (response) {
                            console.log(response);
                        });
                    }

                });

            dragula([document.getElementById('card-list-left-' + response[i]), document.getElementById('card-list-right-' + response[i])], {
                isContainer: function (el) {
                    return false; // only elements in drake.containers will be taken into account
                },
                moves: function (el, source, handle, sibling) {
                    return true; // elements are always draggable by default
                },
                accepts: function (el, target, source, sibling) {
                    return true; // elements can be dropped in any of the `containers` by default
                },
                invalid: function (el, handle) {
                    return false; // don't prevent any drags from initiating by default
                },
                direction: 'vertical',             // Y axis is considered when determining where an element would be dropped
                copy: false,                       // elements are moved by default, not copied
                copySortSource: false,             // elements in copy-source containers can be reordered
                revertOnSpill: false,              // spilling will put the element back where it was dragged from, if this is true
                removeOnSpill: false,              // spilling will `.remove` the element, if this is true
                mirrorContainer: document.body,    // set the element that gets mirror elements appended
                ignoreInputTextSelection: true,     // allows users to select input text, see details below
                slideFactorX: 0,               // allows users to select the amount of movement on the X axis before it is considered a drag instead of a click
                slideFactorY: 0,               // allows users to select the amount of movement on the Y axis before it is considered a drag instead of a click
            })
                .on('drop', function (el, target, source, sibling) {
                    console.log(el);
                    var accessPlanId = el.getElementsByClassName("access-plan-id-secret")[0].innerText;
                    if (target == source)
                        return;
                    if (target.id == 'card-list-right-' + accessPlanId) {
                        url = '/SecurityAccess/SaveAccessPlanUserMapping';
                        var AccessCardId = el.children[1].firstElementChild.children[0].children[1].innerText;
                        $.getJSON(url, {
                            AccessPlanId: accessPlanId,
                            AccessCardId: AccessCardId
                        }).done(function (response) {
                            console.log(response);
                        });
                    } else if (target.id == 'card-list-left-' + accessPlanId) {
                        url = '/SecurityAccess/DeleteAccessPlanUserMapping';
                        var AccessCardId = el.children[1].firstElementChild.children[0].children[1].innerText;
                        $.getJSON(url, {
                            AccessPlanId: accessPlanId,
                            AccessCardId: AccessCardId
                        }).done(function (response) {
                            console.log(response);
                        });
                    }
                });
        }

    });
};
