app.instance.controller("noticeViewController", noticeViewController);
noticeViewController.$inject = ["$scope", "$timeout"];
function noticeViewController($scope, $timeout) {
    //public
    $scope.message = message;
    $scope.error = error;

    //private
    $scope.model = {};
    $scope.model.messages = [];
    $scope.removeNotice = removeNotice;


    function message(content, timeoutInMilisecond) {
        addNotice('Success', content, 'success', timeoutInMilisecond || 3000);
    }

    function error(content, timeoutInMilisecond) {
        addNotice('Error', content, 'danger', timeoutInMilisecond || 6000);
    }

    function addNotice(title, content, type, timeoutInMilisecond) {
        var notice = {
            type: type,
            title: title,
            content: content,
        };
        $scope.model.messages.push(notice);

        if(timeoutInMilisecond > 0){
            $timeout(removeNotice, timeoutInMilisecond, true, notice);
        }
    }

    function removeNotice(notice) {
        var index = $scope.model.messages.indexOf(notice);
        if (index >= 0) {
            $scope.model.messages.splice(index, 1);
        }
    }
}
