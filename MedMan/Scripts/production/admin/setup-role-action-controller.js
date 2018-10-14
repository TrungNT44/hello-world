app.instance.controller("SetupRoleActionController", SetupRoleActionController);
SetupRoleActionController.$inject = ['$scope', '$rootScope', '$injector'];
function SetupRoleActionController($scope, $rootScope, $injector) {
    var self = this;
    this.initAtFirstRunning = this.initAtFirstRunning || function () {
        $scope.roles = roles;
        $scope.permissions = permissions;
    }

    this.validateInputParams = this.validateInputParams || function () {
        return true;
    }

    this.initCacheDictionary = this.initCacheDictionary || function () {       
    }

    this.onInitializeFinished = this.onInitializeFinished || function () {       
    }
    
    this.onPushRemainResourcesToDBSuccess = function (response) {
        window.location.reload();
    }

    this.onPushRemainResourcesToDBFailed = function (response) {
        app.notice.error('Lỗi trong quá trình xử lý.');
    }

    $scope.pushRemainResourcesToDB = function () {
        self.requestRemoteUrl('/Admin/PushRemainResourcesToDB',
            null, self.onPushRemainResourcesToDBSuccess, self.onPushRemainResourcesToDBFailed);
    }   

    $injector.invoke(BaseController, this, {
        $scope: $scope,
        $rootScope: $rootScope,
        $injector: $injector
    });
};
