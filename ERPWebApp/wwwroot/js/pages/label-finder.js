$(document).ready(function () {
    $('#search-tracking-number').on("click", function () {
        console.log("Searching Tracking Number");
        $('#error-text').text('');
        resetTrackingStepper();
        //Need to call this in this format since I am not returning a json format
        $.ajax({
            url: '/ShipEngine/GetShipmentLabel',
            type: 'GET',
            data: {TrackingNumber: $('#text-tracking-number').val()},
            success: function (data) {
                $('#error-banner').removeClass('show');
                $('#error-banner').addClass('hide');
                console.log("Grabbed Tracking Number");
                $('#download-pdf').attr('href', data[0]);
                $('#show-image').attr('src', data[1]);
                $('#label-id').val(data[2].toString());
                if (data[3].toString() == "AC") {
                    $('#Tracking-Status-Accepted').addClass("current");
                    $('#Tracking-Status-ProgressBar').width("33%");
                    console.log(data);
                } else if (data[3].toString() == "IT") {
                    $('#Tracking-Status-InTransit').addClass("current");
                    $('#Tracking-Status-ProgressBar').width("66%");
                    console.log(data);
                } else if (data[3].toString() == "DE") {
                    $('#Tracking-Status-Delivered').addClass("current");
                    $('#Tracking-Status-ProgressBar').width("100%");
                    console.log(data);
                } else if (data[3].toString() == "AT") {
                    console.log(data);
                }
            },
            error: function (data) {
                console.log(data);
                $('#error-banner').removeClass('hide');
                $('#error-banner').addClass('show');
                $('#error-text').append("This Tracking Number does not exist or something else went wrong.");
            }
        });
    });

    function resetTrackingStepper() {
        $('#Tracking-Status-ProgressBar').width("0%");
        $('#Tracking-Status-Accepted').removeClass("current");
        $('#Tracking-Status-Delivered').removeClass("current");
        $('#Tracking-Status-InTransit').removeClass("current");
    }

    $('#confirm-void-label').on("click", function () {
        console.log("Void Label");
        //Need to call this in this format since I am not returning a json format
        $.ajax({
            type: 'GET',
            url: '/ShipEngine/VoidShipmentLabel',
            data: {LabelId: $('#label-id').val()},
            success: function (data) {
                console.log("Voided Label");
                $('#void-label-modal').modal('toggle');
                Swal.fire({
                    title: 'Label Voided',
                    text: data,
                    icon: 'success',
                    timer: 1000,
                    timerProgressBar: true,
                    didOpen: () => {
                        Swal.showLoading()
                        timerInterval = setInterval(() => {
                            const content = Swal.getContent()
                            if (content) {
                                const b = content.querySelector('b')
                                if (b) {
                                    b.textContent = Swal.getTimerLeft()
                                }
                            }
                        }, 500)
                    },
                    willClose: () => {
                        clearInterval(timerInterval)
                    }
                });
            },
            error: function (data) {
                console.log(data);
                $('#void-label-modal').modal('toggle');
                Swal.fire({
                    title: 'Failed',
                    text: data.responseText,
                    icon: 'error',
                    timer: 3000,
                    timerProgressBar: true,
                    didOpen: () => {
                        Swal.showLoading()
                        timerInterval = setInterval(() => {
                            const content = Swal.getContent()
                            if (content) {
                                const b = content.querySelector('b')
                                if (b) {
                                    b.textContent = Swal.getTimerLeft()
                                }
                            }
                        }, 500)
                    },
                    willClose: () => {
                        clearInterval(timerInterval)
                    }
                });
            }
        });
    });
});
