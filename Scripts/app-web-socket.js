var socketEventEstablished = false;
function handleSocketEvents() {  
    if (socketEventEstablished) return;
    if (app.utils.isStringEmpty(socketServerUrl)) { 
        setTimeout(handleSocketEvents, 50); //wait 50 ms, then try again
    } else {
        socketEventEstablished = true;
        var wsClient = new WsClientWrapper(socketServerUrl, 'MedWebApp', 1, new WsClientRPCHandler());
    }
}

function WsClientRPCHandler() {
    this.reloadWebApp = function (paramsObject) {
        //app.notice.confirm('WEB NHÀ THUỐC mới cập nhật tính năng mới. Bạn có muốn cập nhật?', function (result) {
        //    if (result) {
        //        //var rootUrl = app.utils.getRootUrl();
        //        window.location = '/';
        //        window.location.reload(true);
        //    }
        //});
        var rootUrl = app.utils.getRootUrl();
        window.location.replace(rootUrl);
        //window.location = '/';
        //window.location.reload(true);
    }
    this.loadNotification = function (paramsObject) {
        fnGetNumberNotification();
    }
}