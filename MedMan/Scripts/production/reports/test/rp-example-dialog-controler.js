app.instance.controller("RpExampleDialogController", RpExampleDialogController);
RpExampleDialogController.$inject = ['$scope', '$rootScope', '$injector'];
function RpExampleDialogController($scope, $rootScope, $injector) {
    var self = this;
    this.initAtFirstRunning = this.initAtFirstRunning || function () {

    }

    this.validateInputParams = this.validateInputParams || function () {
        return true;
    }

    this.loadAndBindDataToView = this.loadAndBindDataToView || function () {
        //$scope.model.paramName = $scope.externalParams.paramName;
        self.showDialog();
    }

    this.onUpdateFuncSuccess = function (data) {
        $("#test-rp-example-dialog").modal("hide");
        app.notice.message('Update successfully.');
        if ($scope.dialogCallbackFunc != null) {
            $scope.dialogCallbackFunc(data);
        }
    }

    this.onUpdateFuncFailed = function (data) {
        $("#test-rp-example-dialog").modal("hide");
        app.notice.error('Update failed.');
    }

    $scope.updateFunc = function () {
        if (!self.validateInputParams()) {
            app.notice.error($scope.errorMessage);
            return;
        }

        self.requestRemoteUrl('/Report/UpdateFuncTest',
            {
                id: 0,

            }, self.onUpdateFuncSuccess, self.onUpdateFuncFailed);
    }

    $injector.invoke(BaseController, this, {
        $scope: $scope,
        $rootScope: $rootScope,
        $injector: $injector
    });
};