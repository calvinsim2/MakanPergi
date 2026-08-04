var signalRHelper = signalRHelper || {};

(function () {
	signalRHelper.createConnection = function (connectionName) {
		console.log(`Connection Name: ${connectionName}`)
		const connection = new signalR.HubConnectionBuilder()
			.withUrl(`${connectionName}`)
			.withAutomaticReconnect([0, 5000, 5000, 5000, 5000, 50000])
			//.withAutomaticReconnect([0, 2000, 10000, 30000]) yields the default behavior
			.configureLogging(signalR.LogLevel.Information)
			.build();

		connection.onreconnecting((error) => {
			console.log("on_reconnecting-error:" + error);
		});

		connection.onreconnected((connectionId) => {
			console.log("on_reconnected-connectionId:" + connectionId);
		});

		return connection;
	}

	var startSignalRConn = async () => {
		try {
			console.log("signalRHelper.js" + `base url: ${getBaseUrl()}/MainHub`)
			mySignalRConn = signalRHelper.createConnection(getBaseUrl() + "/MainHub");
			await mySignalRConn.start();
		} catch (err) {
			console.log(err);
		}
	};
	startSignalRConn();
}())