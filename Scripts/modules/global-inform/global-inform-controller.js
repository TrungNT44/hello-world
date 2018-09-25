app.instance.controller("GlobalInformController", GlobalInformController);
GlobalInformController.$inject = ['$scope', '$rootScope', '$injector'];
function GlobalInformController($scope, $rootScope, $injector) {
    var self = this;
    this.initAtFirstRunning = this.initAtFirstRunning || function () {
        $scope.numberMessages = 0;
    }

    this.validateInputParams = this.validateInputParams || function () {
        return true;
    }

    this.onGetDataSuccess = function (response) {
        if (!response) {
            self.onGetDataFailed(response);
        }
        $scope.numberMessages = response;
    }

    this.onGetDataFailed = function (response) {
        //app.notice.error('Get data failed.');
    }

    $scope.init = function () {
        $scope.numberMessages = 0;
        self.requestRemoteUrl('/System/GetSystemMessagesCount',
           null, self.onGetDataSuccess, self.onGetDataFailed);
    };

    $scope.showSystemMesssage = function() {
        if ($scope.numberMessages == 0) {
            return;
        }
        self.prepareDataForDialog('system-messages-dialog', null);
    }

    $injector.invoke(BaseController, this, {
        $scope: $scope,
        $rootScope: $rootScope,
        $injector: $injector
    });
};