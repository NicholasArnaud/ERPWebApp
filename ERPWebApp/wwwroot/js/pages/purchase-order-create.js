/* eslint-disable no-undef */
/* eslint-disable no-unused-vars */
/* eslint-disable camelcase */


$('#product-table-page').on('keypress', function (event) {
  const keyPressed = event.keyCode || event.which
  if (keyPressed === 13) {
    event.preventDefault()
    getTotalCost()
    return false
  }
})

$('#pay-table-page').on('keypress', function (event) {
  const keyPressed = event.keyCode || event.which
  if (keyPressed === 13) {
    event.preventDefault()
    return false
  }
})
const coll = document.getElementsByClassName('collapsible')
let i

for (i = 0; i < coll.length; i++) {
  coll[i].addEventListener('click', function () {
    this.classList.toggle('active')
    const content = this.nextElementSibling
    if (content.style.display === 'block') {
      content.style.display = 'none'
    } else {
      content.style.display = 'block'
    }
  })
}

$(document).ready(function () {

  // hides datepicker after selecting date
  $('#date-picker').datepicker('setDate', new Date())
  $('#date-picker').datepicker()
    .on('changeDate', function (ev) {
      $('#date-picker').datepicker('hide')
    })

  $('#date-pickers').datepicker('setDate', new Date())
  $('#date-pickers').datepicker()
    .on('changeDate', function (ev) {
      $('#date-pickers').datepicker('hide')
    })
  const hamburger = document.querySelector('.hamburger')
  hamburger.addEventListener('click', function () {
    document.querySelector('body').classList.toggle('active')
  })
  const hamburgercl = document.querySelector('.hamburgerclose')
  hamburgercl.addEventListener('click', function () {
    document.querySelector('body').classList.toggle('active')
  })
  let current_fs, next_fs, previous_fs // fieldsets
  let opacity

  // next button click, gets next wizard page
  $('.next').click(function () {
    current_fs = $(this).parent()
    next_fs = $(this).parent().next()
    // Add Class Active
    let nextWizardStep = true
    current_fs.find('.wizard-required').each(function () {
      const thisValue = jQuery(this).val()

      if (thisValue === '') {
        jQuery(this).siblings('.wizard-form-error').slideDown()
        // jQuery(this).siblings("#.wizard-error-message").show();
        nextWizardStep = false
      } else {
        jQuery(this).siblings('.wizard-form-error').slideUp()
        // $jQuery(this).siblings("#.wizard-error-message").hide();
      }
    })
    current_fs.find('.wizardHelpFloat').each(function () {
      const thisValue = jQuery(this).val()

      if (isNaN(parseFloat(thisValue)) || parseFloat(thisValue) < 0) {
        jQuery(this).siblings('.wizard-form-error').slideDown()
        // jQuery(this).siblings("#.wizard-error-message").show();
        nextWizardStep = false
      } else {
        jQuery(this).siblings('.wizard-form-error').slideUp()
        // $jQuery(this).siblings("#.wizard-error-message").hide();
      }
    })
    current_fs.find('.wizardHelpInt').each(function () {
      const thisValue = jQuery(this).val()

      if (isNaN(parseInt(thisValue)) || parseInt(thisValue) < 0) {
        jQuery(this).siblings('.wizard-form-error').slideDown()
        // jQuery(this).siblings("#.wizard-error-message").show();
        nextWizardStep = false
      } else {
        jQuery(this).siblings('.wizard-form-error').slideUp()
        // $jQuery(this).siblings("#.wizard-error-message").hide();
      }
    })
    if (nextWizardStep) {
      $('#progressbar li').eq($('fieldset').index(next_fs)).addClass('active')

      // show the next fieldset
      next_fs.show()
      // hide the current fieldset with style
      current_fs.animate({ opacity: 0 }, {
        step: function (now) {
          // for making fielset appear animation
          opacity = 1 - now
          current_fs.css({
            display: 'none',
            position: 'relative'
          })
          next_fs.css({ opacity })
        },
        duration: 600
      })
    }
  })

  // previous button clicked gets previous wizard page
  $('.previous').click(function () {
    current_fs = $(this).parent()
    previous_fs = $(this).parent().prev()

    // Remove class active
    $('#progressbar li').eq($('fieldset').index(current_fs)).removeClass('active')

    // show the previous fieldset
    previous_fs.show()

    // hide the current fieldset with style
    current_fs.animate({ opacity: 0 }, {
      step: function (now) {
        // for making fielset appear animation
        opacity = 1 - now

        current_fs.css({
          display: 'none',
          position: 'relative'
        })
        previous_fs.css({ opacity })
      },
      duration: 600
    })
  })

  // radio class on click fidns next radio
  $('.radio-group .radio').click(function () {
    $(this).parent().find('.radio').removeClass('selected')
    $(this).addClass('selected')
  })

  // click return function dont need anymore
  $('.submit').click(function () {
    return false
  })
})

// on click check for the table
$(document).on('click', '.item-action-btn', function (e) {
  e.preventDefault()
  const tr = $(this).closest('tr')
  console.log(tr.children('td:first').attr('id'))
  tr.remove()
})

// gets any product vendor mappings on the vendor selected
function GetProductsbyVendorId (_vendorId) {
  const url = '../PurchaseOrders/GetProductsByVendorId/'
  $.getJSON(url, { id: _vendorId }).done(function (response) {
    $('#selector-product').empty()
    let s = ''
    for (let i = 0; i < response.length; i++) {
      s += '<option value="' + response[i].product.productId + '">' + response[i].product.sku + ' | ' + response[i].vendorSku + ' | ' + response[i].product.description + '</option>'
    }
    $('#selector-product').html(s)
  })
}

// on change for the vendor, gets vendor information
$('#selector-vendor').on('change', function (e) {
  GetProductsbyVendorId(+$('#selector-vendor').val())
  const selectedvendor = $(this).children('option:selected').val()
  url = '../PurchaseOrders/GetVendorInformation/'
  $.getJSON(url, { id: selectedvendor }, function (response) {
    $('#vendor-contact').val(response.vendorContact)
    $('#vendor-email').val(response.vendorEmail)
  })
})

// date picker sets the initial day
$('#date-picker').datepicker({
  dateFormat: 'mm/dd/yyyy',
  orientation: 'bottom' // add this for placemenet
})

$('#date-pickers').datepicker({
  dateFormat: 'mm/dd/yyyy',
  orientation: 'bottom' // add this for placemenet
})
// shows vendor information on the summary page
function showVendorInfo () {
  $('#vendorInfo').show()
  $('#poNumb').text($('#po-number').val())
  $('#vendorName').text($('#selector-vendor option:selected').text())
  $('#vendorNumb').text($('#vendor-contact').val())
  $('#vendorEml').text($('#vendor-email').val())
  $('#refNumb').text($('#vendor-ref-number').val())
  $('#vendorNts').text($('#vendor-notes').val())
  $('#idvendor').val($('#selector-vendor').val())
}
// shows the shipping info on the summary page
function showShippingInfo () {
  $('#shippingInfo').show()
  $('#orderDt').text($('#datepick').val())
  $('#shipVia').text($('#selector-ship option:selected').text())
  $('#shipType').text($('#selector-ship-method option:selected').text())
  $('#idshippingmeth').val($('#selector-ship-method').val())
  $('#idshippingpro').val($('#selector-ship').val())
}

// shows product information in the summary page gets the dynamicly created products and adds them


// shows purchase order info to the summary page
function showPurchaseInfo () {
  $('#paymentInfo').show()
  $('#itemTot').text($('#item-total').val())
  $('#discountItm').text($('#discount-item').val())
  $('#shippingCst').text($('#shipping-cost').val())
  $('#shippingTx').text($('#shipping-tax').val())
  $('#otherCst').text($('#other-cost').val())
  $('#grandTot').text($('#grand-total').val())
}
// sets grand total in purchase order page
function setTotal () {
  const tp1 = parseFloat($('#item-total').val())
  const dis1 = parseFloat($('#discount-item').val())
  const shp1 = parseFloat($('#shipping-tax').val())
  const shp2 = parseFloat($('#shipping-cost').val())
  const oth1 = parseFloat($('#other-cost').val())
  if (isNaN(tp1) || isNaN(dis1) || isNaN(shp1) || isNaN(shp2)) {
    $('#grand-total').val(0)
  } else {
    $('#grand-total').val(((tp1 - (tp1 * dis1 / 100)) + (tp1 - (tp1 * dis1 / 100)) * shp1 / 100) + shp2 + oth1)
  }
}
