app.instance.controller("ReportExampleController", ReportExampleController);
ReportExampleController.$inject = ['$scope', '$rootScope', '$injector'];
function ReportExampleController($scope, $rootScope, $injector) {
    var self = this;
    this.initAtFirstRunning = this.initAtFirstRunning || function () {
    }

    this.validateInputParams = this.validateInputParams || function () {
        return true;
    }

    $scope.searchData = function () {
        if (!self.validateInputParams()) {
            app.notice.error($scope.errorMessage);
            return;
        }
        self.prepareDataForDialog('test-rp-example-dialog', { paramName: 0 });
    }

    $injector.invoke(BaseController, this, {
        $scope: $scope,
        $rootScope: $rootScope,
        $injector: $injector
    });
};