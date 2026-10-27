var sharedFunctions = sharedFunctions || {};
sharedFunctions.func = sharedFunctions.func || {};
sharedFunctions.ui = sharedFunctions.ui || {};
sharedFunctions.input = sharedFunctions.input || {};
sharedFunctions.image = sharedFunctions.image || {};
sharedFunctions.data = sharedFunctions.data || {};
sharedFunctions.text = sharedFunctions.text || {};

(function () {

    console.log("Loaded sharedFunctions");
    sharedFunctions.func.commonRejectFunction = (event) => {
        switch (event.status) {
            case 409:
                sharedFunctions.ui.promptRefresh({ text: event.responseText });
                break;

            case 404:
                uiCommon.message.error(event.responseText ? event.responseText : "Entity not found.");
                break;

            case 400:
                uiCommon.message.error(event.responseText ? event.responseText : "Bad Request Error");
                break;

            default:
                uiCommon.notify.error(event.responseText ? event.responseText : "Internal Server Error");
                break;
        }
    }

    console.log(sharedFunctions);
})();