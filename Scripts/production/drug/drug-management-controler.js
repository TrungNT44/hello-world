app.instance.controller("DrugManagementController", DrugManagementController);
DrugManagementController.$inject = ['$scope', '$rootScope', '$injector'];
function DrugManagementController($scope, $rootScope, $injector) {
    var self = this;
    this.initAtFirstRunning = this.initAtFirstRunning || function () {
    }

    this.validateInputParams = this.validateInputParams || function () {
        return true;
    }

    $injector.invoke(BaseController, this, {
        $scope: $scope,
        $rootScope: $rootScope,
        $injector: $injector
    });
};