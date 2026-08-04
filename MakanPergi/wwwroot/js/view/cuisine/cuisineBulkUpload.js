(function ($) {
    var _$SelectFileInput = $('#SelectFileInput'),
        _$UploadExcelFileInput = $('#UploadExcelFileInput'),
        _$Upload = $('#Upload'),
        _$FileName = $("#FileName"),
        _$Card = $('#Card'),
        file = null;

    _$SelectFileInput.click(() => {
        $('#divUploadResult').hide();
        _$Upload.click();
    });

    _$Upload.on("change", (e) => {
        console.log("clicked!");
        file = e.target.files[0];
        if (file) {
            console.log(file);
            _$FileName.text(`File Selected: ${e.target.files[0].name}`);
        }
        else {
            console.log("no file found");
            _$FileName.text("");
        }
    });

    $(document).ready(() => {
        console.log("Cuisine Bulk Upload Document ready");

        setTimeout(function () { startUploadSignalRConn(); }, 1000);
    });

    _$UploadExcelFileInput.click(async () => {
        const selectedFile = $('#Upload').prop('files')[0];
        console.log(selectedFile);

        if (!selectedFile) {
            displayMessage("No file selected", "File Not Found!", "alert-warning");
        } else {
            const formFile = new FormData();
            formFile.append("file", selectedFile);

            try {
                uiCommon.ui.block(_$Card);

                let cuisineUploadUrl = getBaseUrl() + '/Cuisine/ExcelUpload';
                var response = await fetch(cuisineUploadUrl, {
                    method: "POST",
                    body: formFile,
                });

                console.log(`response: ${response}`)
                displayMessage(response, "Success", "alert-success");
            } catch (err) {
                console.error(err);
                displayMessage(err, "Error", "alert-danger");
            } finally {
                uiCommon.ui.unblock(_$Card);
            }
        }
    });

    function displayMessage(messages, title, alertClass) {
        console.log('display message function called!');
        console.log(messages);
        const errorMessage = Array.isArray(messages) ? messaages.join("<br />") : messages;
        $('#resultText').html(errorMessage);
        $('#resultTitle').text(title);
        $('#alertMsg').attr("class", '');
        $('#alertMsg').addClass(`alert alert-dismissible ${alertClass}`);
        $('#divUploadResult').show();
    }

    var startUploadSignalRConn = async () => {
        try {
            console.log("Current signalRConn at cuisineBulkUpload.js");
            console.log(mySignalRConn);
            if (mySignalRConn === undefined) {
                return;
            }
            mySignalRConn.on("_UploadCuisine_Progress", (message) => {
                console.log(message);
                var progressObj = JSON.parse(message);
                if (progressObj.IsComplete === false) {
                    $('.blockUI.blockMsg.blockElement > h1').text('Please Wait ' + progressObj.Current + '/' + progressObj.Total);
                } else {
                    const data = JSON.parse(progressObj.Result);
                    if (isUploadImage === false) {
                        handleUploadResult({ totalCount: progressObj.Total, resultList: data });
                    } else {
                        handleImageUploadResult(progressObj);
                    }
                }
            });
        } catch (err) {
            console.log(err);
        }
    };

    function handleUploadResult(data) {
        const totalCount = data.totalCount;
        const addedResult = data.resultList.filter((x) => x.isValid === true);
        const failedResult = data.resultList.filter((x) => x.isValid === false);

        const successCount = addedResult.length;
        const errorCount = failedResult.length;

        const resultMsg = `IMPORT RESULT: <span style = "margin-left: 10px;padding: 4px;border-radius: 3px;">${successCount}/${totalCount} records have been uploaded successfully.`;

        const errorMessages = failedResult.map((r) => r.message);

        // Separate success and error messages
        const successMessage = resultMsg;
        const errorMessage = resultMsg + "<br/>" + errorMessages.join("<br />");

        // Display success and error messages in separate alert boxes
        if (successCount > 0) {
            displayMessage(successMessage, "Success", "alert-success");
        }

        if (errorCount > 0) {
            displayMessage(errorMessage, "Error");
        }
    }

    function handleImageUploadResult(res) {
        //_resultTxt.text(res.Message + ":");
        var msg = res.Message + ":";
        var allSuccess = true;
        const imageResult = JSON.parse(res.Result);
        imageResult.forEach(r => {
            if (r.Message != null) {
                msg = msg + "<br />" + r.Message;
                if (!r.Message.toLowerCase().includes("successfully")) {
                    allSuccess = false;
                }
                //_resultTxt.append("<br />" + r.message);
                //$('#divUploadResult').show();
            }
        });
        if (allSuccess) {
            displayMessage(msg, "Success", "alert-success");
        } else {
            displayMessage(msg, "Error", "alert-danger");
        }
        certis.ui.unblock(_$Card);
    }

})(jQuery);