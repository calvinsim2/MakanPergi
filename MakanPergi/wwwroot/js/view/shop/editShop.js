(function ($) {

    // Use delegated event binding so it works for dynamically injected modal content
    $(document).on('click', 'form[data-mode="edit"] .save-button', function (e) {
        e.preventDefault();

        let _$form = $(this).closest('form[data-mode="edit"]');
        let _$modal = $('#ShopModalPlaceholder');
        let _$shopTable = $('#shopTable');

        if (_$form.length === 0) {
            console.error("Form not found.");
            return;
        }

        _$form.validate({
            rules: {
                Name: {
                    required: true,
                    maxlength: 15
                }
            },
            messages: {
                Name: {
                    required: "Please enter Shop Name."
                }
            },

            // Specifies the HTML tag to use for error messages
            errorElement: 'span',
            errorClass: 'invalid-feedback',
            // Allows custom placement of the error message in the DOM.
            errorPlacement: function (error, element) {
                // error is the jQuery object for the error <span>
                // element is the input element that failed validation

                // error.addClass("invalid-feedback"); // we can manually do this, if we dont want to use errorClass
                error.insertAfter(element);
            },
            // Adds a class (usually to show a red border) when an input is invalid.
            highlight: function (element, errorClass, validClass) {
                $(element).addClass('is-invalid');
            },
            // Removes that class when the field becomes valid again.
            unhighlight: function (element, errorClass, validClass) {
                $(element).removeClass('is-invalid');
            }
        })

        if (!_$form.valid()) {
            return;
        }

        let actionUrl = _$form.attr('action');
        let shop = _$form.serialize();

        uiCommon.ui.block(_$modal);
        $.ajax({
            url: actionUrl,
            type: 'POST',
            data: shop,
            contentType: 'application/x-www-form-urlencoded; charset=UTF-8',
            success: function (data) {

                _$modal.modal('hide');
                _$form[0].reset();
                _$shopTable.DataTable().ajax.reload();
                uiCommon.notify.success("Shop updated successfully!");
            },
            error: function (xhr) {
                uiCommon.notify.error("Edit Shop Failed");
                console.log(xhr);
                // Any time a server writes data in the body of an HTTP response—whether 
                // it’s a 200 or a 400 / 500, jQuery will stash that raw string in xhr.responseText
                sharedFunctions.func.commonRejectFunction(xhr);
            },
            complete: function () {
                uiCommon.ui.unblock(_$modal);
            }
        });
    });

    // Prevent fallback form submission via Enter key or default browser behavior
    $(document).on('submit', 'form[name="editShopForm"]', function (e) {
        e.preventDefault();
    });
})(jQuery);