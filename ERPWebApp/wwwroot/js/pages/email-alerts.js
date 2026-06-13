var tableRows = document.querySelectorAll('[data-email-index]');
var userEmails;

function initEmailAlertsDashboard() {
    // Get the email alert data from the DOM element.
    var emailAlertData = document.getElementById('emailAlertData');

    // Parsing the data attributes so that we can get the user emails, preset triggers data, and alert trigger template mappings.
    userEmails = JSON.parse(emailAlertData.getAttribute('data-user-emails'));
    var presetTriggersData = JSON.parse(emailAlertData.getAttribute('data-preset-triggers-data'));
    var alertTriggerTemplateMappings = JSON.parse(emailAlertData.getAttribute('data-alert-trigger-template-mappings'));

    // Function to toggle the visibility of the alert type fields based on the selected alert type.  
    function toggleAlertTypeFields(alertTypeSelect, frequencyGroup, presetTriggersGroup, scheduledTimeGroup, resetTemplate) {
        if (alertTypeSelect.value == "0") { // TimeBased  
            if (frequencyGroup) frequencyGroup.style.display = 'block';
            if (presetTriggersGroup) presetTriggersGroup.style.display = 'none';
            if (scheduledTimeGroup) scheduledTimeGroup.style.display = 'block';
        } else { // TriggerBased  
            if (frequencyGroup) frequencyGroup.style.display = 'none';
            if (presetTriggersGroup) presetTriggersGroup.style.display = 'block';
            if (scheduledTimeGroup) scheduledTimeGroup.style.display = 'none';
        }
    }  

    // Populate the preset triggers select2 element with the right data.
    function populatePresetTriggersSelect2(selectElement) {
        selectElement.select2({
            data: [{ id: '', text: 'Select a trigger...' }].concat(presetTriggersData),
            placeholder: 'Select a trigger.'
        });
    }  

    //Finds the message contents for a given alert template ID.
    function findMessageByTemplateId(templateId) {
        var numericTemplateId = Number(templateId);

        var alertTriggerTemplateMapping = alertTriggerTemplateMappings.find(function (item) {
            return item.alertTemplateId === numericTemplateId;
        });

        return alertTriggerTemplateMapping ? alertTriggerTemplateMapping.messageContents : '';
    }

    // Set the initial visibility of the fields when the modal is opened  
    function setInitialAlertTypeFieldsVisibility(alertTypeSelect, frequencyGroup, presetTriggersGroup, scheduledTimeGroup) {
        toggleAlertTypeFields(alertTypeSelect, frequencyGroup, presetTriggersGroup, scheduledTimeGroup);
    }

    // CREATE LOGIC
    //---------------------------

    // Initializing select2 for recipients for Creation modal.
    $('#recipientsCreate').select2({
        data: userEmails,
        tags: true,
        tokenSeparators: [',', ' '],
        placeholder: 'Enter email addresses',
        createTag: function (params) {
            var term = $.trim(params.term);

            if (term === '' || userEmails.indexOf(term) === -1) {
                return null;
            }

            return {
                id: term,
                text: term,
                newOption: true
            };
        }
    });  

    // Event listener for the Create Email Alert button.
    $('#createEmailAlert').on('click', function () {
        var createEmailAlertModal = new bootstrap.Modal(document.getElementById('createEmailAlertModal'));
        createEmailAlertModal.show();
    });

    // Event listener for the Create modal's show event  
    document.getElementById('createEmailAlertModal').addEventListener('show.bs.modal', function () {
        var alertTypeSelect = document.getElementById('alertTypeCreate');
        var frequencyGroup = document.getElementById('frequencyGroupCreate');
        var presetTriggersGroup = document.getElementById('presetTriggersGroupCreate');
        var scheduledTimeGroup = document.getElementById('scheduledTimeGroupCreate');

        setInitialAlertTypeFieldsVisibility(alertTypeSelect, frequencyGroup, presetTriggersGroup, scheduledTimeGroup);

        if (alertTypeSelect.value == "1") {
            var selectedTemplateId = $('#createEmailAlertForm #presetTriggersCreate').select2('data')[0].id;
            var message = findMessageByTemplateId(selectedTemplateId);
            $('#createEmailAlertForm #bodyCreate').val(message);
        }  
    });

    // Event listener for the Create modal's AlertType selection change  
    document.getElementById('alertTypeCreate').addEventListener('change', function () {
        var alertTypeSelect = document.getElementById('alertTypeCreate');
        var frequencyGroup = document.getElementById('frequencyGroupCreate');
        var presetTriggersGroup = document.getElementById('presetTriggersGroupCreate');
        var scheduledTimeGroup = document.getElementById('createScheduledTimeGroup');
        toggleAlertTypeFields(alertTypeSelect, frequencyGroup, presetTriggersGroup, scheduledTimeGroup);
    });

    // Event listener for the Create modal's AlertType selection  
    document.getElementById('alertTypeCreate').addEventListener('change', function (event) {
        toggleAlertTypeFields(event.target, document.getElementById('frequencyGroupCreate'), document.getElementById('presetTriggersGroupCreate'), document.getElementById('createScheduledTimeGroup'), true);
    });  

    // Populate the Preset Triggers select2 for the Create modal
    populatePresetTriggersSelect2($('#createEmailAlertForm #presetTriggersCreate'));

    // Event listener for updating the email body when a preset trigger is selected in the Create modal.
    $('#createEmailAlertForm #presetTriggersCreate').on('change', function () {
        var selectedTemplateId = $(this).select2('data')[0].id;
        var message = findMessageByTemplateId(selectedTemplateId);
        $('#createEmailAlertForm #bodyCreate').val(message);
    });

    //---------------------------

    // EDIT LOGIC
    //---------------------------

    // Adding event listeners here for the edit buttons.
    tableRows.forEach(function (row) {
        row.querySelector('.edit-btn').addEventListener('click', async function () {
            var recipients = row.getAttribute('data-email-recipients').split(','); 

            // This is to populate the recipients field with the fetched recipients.
            $('#recipients').empty();
            recipients.forEach(function (recipient) {
                var newOption = new Option(recipient, recipient, true, true);
                $('#recipients').append(newOption).trigger('change');
            });

            // Restore the original options (all user emails) in the select2 control  
            $('#recipients').select2({
                data: userEmails,
                tags: true,
                tokenSeparators: [',', ' '],
                placeholder: 'Enter email addresses',
                createTag: function (params) {
                    var term = $.trim(params.term);

                    if (term === '' || userEmails.indexOf(term) === -1) {
                        return null;
                    }

                    return {
                        id: term,
                        text: term,
                        newOption: true
                    };
                }
            }); 

            document.querySelector('#editEmailAlertModal #recipients').value = recipients;

            // Show the Edit Email Alert Modal
            var editEmailAlertModal = new bootstrap.Modal(document.getElementById('editEmailAlertModal'));
            editEmailAlertModal.show();
        });
    });

    // Event listener for the Edit modal's show event  
    document.getElementById('editEmailAlertModal').addEventListener('shown.bs.modal', function () {
        var alertTypeSelect = document.getElementById('alertType');
        var frequencyGroup = document.getElementById('frequencyGroup');
        var presetTriggersGroup = document.getElementById('presetTriggersGroup');
        var scheduledTimeGroup = document.getElementById('scheduledTimeGroup');

        setInitialAlertTypeFieldsVisibility(alertTypeSelect, frequencyGroup, presetTriggersGroup, scheduledTimeGroup);  

        if (alertTypeSelect.value == "1") {
            var selectedTemplateId = $('#editEmailAlertForm #presetTriggers').select2('data')[0].id;
            var message = findMessageByTemplateId(selectedTemplateId);
            $('#editEmailAlertForm #body').val(message);
        }  
    });  

    // Event listener for the Edit modal's AlertType selection change  
    document.getElementById('alertType').addEventListener('change', function () {
        var alertTypeSelect = document.getElementById('alertType');
        var frequencyGroup = document.getElementById('frequencyGroup');
        var presetTriggersGroup = document.getElementById('presetTriggersGroup');
        var scheduledTimeGroup = document.getElementById('scheduledTimeGroup');

        toggleAlertTypeFields(alertTypeSelect, frequencyGroup, presetTriggersGroup, scheduledTimeGroup);
    });

    // Initialize the previousAlertType variable for the Edit modal  
    var previousAlertType = document.getElementById('alertType') ? document.getElementById('alertType').value : null;  

    // Event listener for the Edit modal's AlertType selection change  
    document.getElementById('alertType').addEventListener('change', function (event) {
        var resetTemplate = previousAlertType === "0" && event.target.value === "1";
        toggleAlertTypeFields(event.target, document.getElementById('frequencyGroup'), document.getElementById('presetTriggersGroup'), document.getElementById('scheduledTimeGroup'), resetTemplate);

        if (resetTemplate) {
            // Reset the selected template to "Select a trigger..."
            var presetTriggersSelect = $(presetTriggersGroup).find('select');
            presetTriggersSelect.val('').trigger('change');
            // Update the message body for the selected trigger  
            var selectedTemplateId = $(presetTriggersSelect).select2('data')[0].id;
            var message = findMessageByTemplateId(selectedTemplateId);
            $('#editEmailAlertForm #body').val(message);
        }

        previousAlertType = event.target.value;
    });  

    // Populate the Preset Triggers select2 for the Edit modal  
    populatePresetTriggersSelect2($('#editEmailAlertForm #presetTriggers'));


    // Event listener for updating the email body when a preset trigger is selected in the Edit modal.
    $('#editEmailAlertForm #presetTriggers').on('change', function () {
        var selectedTemplateId = $(this).select2('data')[0].id;
        var message = findMessageByTemplateId(selectedTemplateId);
        $('#editEmailAlertForm #body').val(message);
    });

    /*
    // Event listener for the Save Changes button in the Edit Email Alert Modal.
    $('#saveChanges').on('click', function () {
        updateEmailAlert();
    });
    */
    //---------------------------

    // This is for initializing the recipients select2 with user emails.
    var userEmails = userEmails;
    $('#recipients').select2({
        data: userEmails,
        tags: true,
        tokenSeparators: [',', ' '],
        placeholder: 'Enter email addresses',
        createTag: function (params) {
            var term = $.trim(params.term);

            if (term === '' || userEmails.indexOf(term) === -1) {
                return null;
            }

            return {
                id: term,
                text: term,
                newOption: true
            };
        }
    }); 

    // Event listeners for delete buttons
    document.querySelectorAll('.delete-btn').forEach(function (btn) {
        btn.addEventListener('click', function (event) {
            // Get the email alert id from the table row
            var row = event.target.closest('tr');
            var emailAlertId = parseInt(row.getAttribute('data-email-alert-id'));

            document.getElementById('emailAlertIdToDelete').value = emailAlertId;  

            // Show the delete confirmation modal
            var deleteConfirmationModal = new bootstrap.Modal(document.getElementById('deleteConfirmationModal'));
            deleteConfirmationModal.show();
        });
    });

    document.querySelectorAll('.status-btn').forEach(function (btn) {
        btn.addEventListener('click', function (event) {
            // Get the email alert id from the table row  
            var row = event.target.closest('tr');
            var emailAlertId = parseInt(row.getAttribute('data-email-alert-id'));

            // Check if the current status is active or not  
            var isActive = event.target.style.backgroundColor === 'green';

            // Update the confirmation modal's title and message  
            document.querySelector('#statusChangeConfirmationModal #statusAction').textContent = isActive ? 'disable' : 'enable';

            // Set the emailAlertId and newStatus values to the hidden input fields  
            document.getElementById('emailAlertIdToChangeStatus').value = emailAlertId;
            document.getElementById('newStatus').value = !isActive;

            // Show the status change confirmation modal  
            var statusChangeConfirmationModal = new bootstrap.Modal(document.getElementById('statusChangeConfirmationModal'));
            statusChangeConfirmationModal.show();
        });
    });  
}
// This will run the initEmailAlertsDashboard() function when the DOM content is fully loaded.
document.addEventListener('DOMContentLoaded', function () {
    initEmailAlertsDashboard();

    // This will query all table rows with the data-email-index attribute.
    var tableRows = document.querySelectorAll('[data-email-index]');

    // Adding event listeners for each table row's edit button.
    tableRows.forEach(function (row) {
        row.querySelector('.edit-btn').addEventListener('click', async function () {
            // Retrieve the email alert details from the table row.
            var emailAlertId = parseInt(row.getAttribute('data-email-alert-id'));  
            var recipients = row.getAttribute('data-email-recipients').split(','); 
            var emailIndex = row.getAttribute('data-email-index');
            var subject = row.querySelector('td:nth-child(1)').textContent;
            var scheduledTime = row.querySelector('td:nth-child(5)').getAttribute('data-utc-time');  
            var body = row.getAttribute('data-email-body');
            var alertTypeText = row.querySelector('td:nth-child(3)').textContent;
            var alertTypeValue = alertTypeText === "Time" ? "0" : "1";

            var alertTemplateId = parseInt(row.getAttribute('data-alert-template-id'));  

            // If the email alert is trigger-based, pre-select the associated trigger.
            if (alertTypeValue === "1") {
                $('#editEmailAlertForm #presetTriggers option[value=' + alertTemplateId + ']').prop('selected', true);
                $('#editEmailAlertForm #presetTriggers').trigger('change');
            }
            // Set the selected recipients  
            $('#recipients').val(recipients).trigger('change');  
          
            // Sets the input fields in the Edit Email Alert modal with the fetched values.
            if (scheduledTime !== "N/A" && scheduledTime) {
                var scheduledDateTime = moment(scheduledTime, 'YYYY-MM-DDTHH:mm');
                $("#editScheduledDateTime").data('daterangepicker').setStartDate(scheduledDateTime);
                $("#editScheduledDateTime").data('daterangepicker').setEndDate(scheduledDateTime);
            }  

            document.querySelector('#editEmailAlertModal').setAttribute('data-email-alert-id', emailAlertId);
            document.querySelector('#editEmailAlertModal #subject').value = subject;

            document.querySelector('#editEmailAlertModal #body').value = body;
            document.querySelector('#editEmailAlertModal #alertType').value = alertTypeValue;
            document.querySelector('#editEmailAlertModal').setAttribute('data-email-index', emailIndex);
            document.getElementById('emailAlertId').value = emailAlertId;
            if (document.getElementById('editModalTimeInput')) {
                document.getElementById('editModalTimeInput').value = inputTimeValue;
            }  

            // Show the Edit Email Alert Modal
            new bootstrap.Modal(document.getElementById('editEmailAlertModal')).show();  

        });
    });

    // Had to change some of this closing logic, because the white backdrop was not going away, and rendering the page unclickable.
    // Add event listener for closing the Edit Email Alert modal.  
    document.querySelector('#editEmailAlertModal #closeModal').addEventListener('click', function () {
        var editEmailAlertModalElement = document.getElementById('editEmailAlertModal');
        var editEmailAlertModal = bootstrap.Modal.getInstance(editEmailAlertModalElement);
        if (!editEmailAlertModal) {
            editEmailAlertModal = new bootstrap.Modal(editEmailAlertModalElement);
        }
        editEmailAlertModal.hide();
    });

    // Add event listener for removing the backdrop when the Edit Email Alert modal is hidden.  
    document.getElementById('editEmailAlertModal').addEventListener('hidden.bs.modal', function () {
        var modalBackdrop = document.querySelector('.modal-backdrop');
        if (modalBackdrop) {
            modalBackdrop.remove();
        }
    });  

    $("#createScheduledDateTime").daterangepicker({
        singleDatePicker: true,
        timePicker: true,
        timePicker24Hour: false,
        startDate: moment(),
        locale: {
            format: 'MM/DD/YYYY hh:mm A',
            cancelLabel: 'Clear'
        },
        // Putting this in to set the parent element for the Date Range Picker.
        parentEl: '#createEmailAlertModal .modal-body'
    });   

    $("#editScheduledDateTime").daterangepicker({
        singleDatePicker: true,
        timePicker: true,
        timePicker24Hour: false,
        startDate: moment(),
        locale: {
            format: 'MM/DD/YYYY hh:mm A',
            cancelLabel: 'Clear'
        },
        parentEl: '#editEmailAlertModal .modal-body'
    });
});