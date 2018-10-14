app.notice;
$(document).ready(function () {

    app.notice = app.notice || {};

    app.notice.message = message;
    app.notice.error = error;

    app.notice.confirm = confirm;
    app.notice.alert = alert;

    app.notice.input = input;

    var noticeScope = angular.element($("#noticeViewContainer")).scope();
    function message() {
        noticeScope.message.apply(noticeScope, arguments);
        applyScope(noticeScope);
    }
    function error() {
        noticeScope.error.apply(noticeScope, arguments);
        applyScope(noticeScope);
    }

    var dialogScope = angular.element($("#noticeDialog")).scope();
    function alert() {
        dialogScope.alert.apply(dialogScope, arguments);
        applyScope(dialogScope);
    }
    function confirm() {
        dialogScope.confirm.apply(dialogScope, arguments);
        applyScope(dialogScope);
    }

    var inputBoxScope = angular.element($("#inputBox")).scope();
    function input() {
        inputBoxScope.show.apply(inputBoxScope, arguments);
        applyScope(inputBoxScope);
    }

    function applyScope($scope) {
        if (!$scope.$$phase) {
            $scope.$digest();
        }
    }
})

app.instance.run(["$rootScope", function ($rootScope) {
    $rootScope.notice = app.notice;
}]);