// simple namespacing technique in plain JavaScript.

// var uiCommon = uiCommon || {} checks “does a global uiCommon object already exist?”
// If yes, reuse it.
// If no, create a new empty object { }.

var uiCommon = uiCommon || {};
uiCommon.ui = uiCommon.ui || {};
uiCommon.notify = uiCommon.notify || {};
uiCommon.configs = uiCommon.configs || {}; 
uiCommon.utils = uiCommon.utils || {}; 
uiCommon.message = uiCommon.message || {}; 

(function () {

    if (!toastr) {
        return;
    }

    if (!sweetAlert || !$) {
        return;
    }

    // $.extend(target, source) is a built-in jQuery utility that merges properties from the source object into the target object.
    // ** source value has higher priority, should there be a case where both target and source have the same properties.
    $.extend($.blockUI.defaults, {
        css: {
            border: 'none',
            padding: '15px',
            backgroundColor: '#000',
            '-webkit-border-radius': '10px',
            '-moz-border-radius': '10px',
            opacity: .5,
            color: '#fff'
        },
        overlayCSS: {
            backgroundColor: '#cdcdcd',
            'z-index': 9990,
            opacity: 0.3,
            top: 0,
            left: 0,
            width: '100%',
            height: '100%',
            opacity: 0.3,
            cursor: 'wait'
        }
    });

    // blockUI() and block() are methods provided by the jQuery BlockUI plugin
    uiCommon.ui.block = function (elm) {
        if (!elm) {
            // Operates on the entire page
            $.blockUI();
        } else {
            // Operates on the the element that was passed in as argument
            $(elm).block();
        }
    };

    uiCommon.ui.unblock = function (elm) {
        if (!elm) {
            // Operates on the entire page
            $.unblockUI();
        } else {
            // Operates on the the element that was passed in as argument
            $(elm).unblock();
        }
    };

    toastr.options = {
        "closeButton": false,
        "debug": false,
        "newestOnTop": false,
        "progressBar": true,
        "positionClass": "toast-top-right",
        "preventDuplicates": true,
        "onclick": null,
        "showDuration": "300",
        "hideDuration": "1000",
        "timeOut": "5000",
        "extendedTimeOut": "1000",
        "showEasing": "swing",
        "hideEasing": "linear",
        "showMethod": "fadeIn",
        "hideMethod": "fadeOut"
    }

    /* NOTIFICATION ***************/

    var showNotification = function (type, message, title, options) {
        // type is a string: one of "success", "error", "info", or "warning".
        // toastr[type] is equivalent to calling toastr.success(...), toastr.error(...), etc.
        toastr[type](message, title, options);
    };

    uiCommon.notify.success = function (message, title, options) {
        showNotification('success', message, title, options);
    };

    uiCommon.notify.info = function (message, title, options) {
        showNotification('info', message, title, options);
    };

    uiCommon.notify.warn = function (message, title, options) {
        showNotification('warning', message, title, options);
    };

    uiCommon.notify.error = function (message, title, options) {
        showNotification('error', message, title, options);
    };

    /* POPUP MESSAGE ******************/

    uiCommon.configs.sweetAlert = {
        config: {
            'default': {

            },
            info: {
                icon: 'info'
            },
            success: {
                icon: 'success'
            },
            warn: {
                icon: 'warning'
            },
            error: {
                icon: 'error'
            },
            confirm: {
                icon: 'warning',
                title: 'Are you sure?',
                showCancelButton: true,
                confirmButtonText: 'Yes',
                confirmButtonColor: 'green',
                cancelButtonText: 'No',
                cancelButtonColor: 'red'
            },
            confirmWithCustomButtons: {
                icon: 'warning',
                title: 'Are you sure?'
            }
        }
    };

    var showMessage = function (type, message, title, callback, options) {
        options = options || {};
        var messageContent = {};
        if (title) {
            messageContent.title = title;
        }

        // Decide whether to render HTML or plain text
        if (options.isHtml) {
            var el = document.createElement('div');
            el.style = 'position: relative;';
            el.innerHTML = message;

            messageContent.content = el;
        } else {
            messageContent.text = message;
        }

        // Build the final Sweet alert options by merging:
        //    - global defaults
        //    - the “type”-specific defaults (e.g. icon: 'error')
        //    - the title/text/html which was just set
        //    - any ad-hoc overrides passed in `options`
        var opts = $.extend(
            {},
            uiCommon.configs.sweetAlert.config['default'],
            uiCommon.configs.sweetAlert.config[type],
            messageContent,
            options
        );

        // Show the dialog and return a jQuery Deferred
        return $.Deferred(function ($dfd) {
            sweetAlert.fire(opts).then((result) => {
                if (result.isConfirmed) {
                    callback && callback(result.isConfirmed);
                    $dfd.resolve(result.isConfirmed);
                }
            });
        });
    };

    uiCommon.message.info = function (message, title, options) {
        return showMessage('info', message, title, null, options);
    };

    uiCommon.message.success = function (message, title, options) {
        return showMessage('success', message, title, null, options);
    };

    uiCommon.message.successWithCallback = function (message, title, callback, options) {
        return showMessage('success', message, title, callback, options);
    };

    uiCommon.message.warn = function (message, title, options) {
        return showMessage('warn', message, title, null, options);
    };

    uiCommon.message.error = function (message, title, options) {
        return showMessage('error', message, title, null, options);
    };

    uiCommon.message.confirm = function (message, title, callback, options) {
        return showMessage('confirm', message, title, callback, options);
    };

    uiCommon.message.confirmWithCustomButtons = function (message, title, callback, options) {
        return showMessage('confirm', message, title, callback, options);
    };

    console.log(uiCommon);

})();