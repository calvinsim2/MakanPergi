// Please see documentation at https://learn.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.

function getWebUrl(value) {
    return jQuery("#webUrl").val() + value;
}

function getBaseUrl() {
    return window.location.origin + $("#webUrl").val();
}

const startSignalRConn = async () => {
	try {
		var mySignalRConn = signalRHelper.createConnection(getBaseUrl() + "/MainHub");
		await mySignalRConn.start();
	} catch (err) {
		console.log(err);
	}
};
