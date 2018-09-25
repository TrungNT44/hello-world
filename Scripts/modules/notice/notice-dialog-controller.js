app.instance.controller("noticeDialogController", noticeDialogController);
noticeDialogController.$inject = ["$scope"];
function noticeDialogController($scope) {
    //public
    $scope.alert = alert;
    $scope.confirm = confirm;

    //private
    $scope.model = {};
    $scope.onClose = onClose;
    $scope.onOK = onOK;
    $scope.onConfirm = onConfirm;


    function alert(content, callback) {
        show('Alert', content, 'alert', callback);
    }

    function confirm(content, callback) {
        show('Confirm', content, 'confirm', callback);
    }

    function show(title, content, type, callback) {
        $scope.model.title = title;
        $scope.model.content = content;
        $scope.model.type = type;
        $scope.callback = callback;

        $("#noticeDialog").modal('show');
    }

    function hide() {
        $("#noticeDialog").modal('hide');
    }

    function onClose() {
        callback(false);
        hide();
    }

    function onOK() {
        callback();
        hide();
    }

    function onConfirm() {
        callback(true);
        hide();
    }

    function callback() {
        if ($scope.callback) {
            $scope.callback.apply($scope, arguments);
        }
    }
}
