app.instance.controller("UpdateDrugExpriedDateDialogController", UpdateDrugExpriedDateDialogController);
UpdateDrugExpriedDateDialogController.$inject = ['$scope', '$rootScope', '$injector', '$filter'];
function UpdateDrugExpriedDateDialogController($scope, $rootScope, $injector, $filter) {
    var self = this;
    this.initAtFirstRunning = this.initAtFirstRunning || function () {
        $scope.drug = {drugDate: null};
    }

    this.validateInputParams = this.validateInputParams || function () {
        
        if ($scope.drug.drugDate)
        {
            var isDateValid = moment($scope.drug.drugDate, "DD/MM/YYYY").isValid();
            if (!isDateValid) {
                //app.notice.error("Giá nhập không được để trống !");
                app.notice.error("Hạn dùng chưa chính xác!");
                return false;
            }
        }
        
       
        return true;
    }

    this.loadAndBindDataToView = this.loadAndBindDataToView || function () {
        //$scope.externalParams.drug.drugDate = $scope.externalParams.drug.drugDate.replace(" 00:00:00", "");
        $scope.drug = $scope.externalParams.drug;
     
        self.showDialog();
    }

    this.onUpdateDrugSuccess = function (data) {
        if (data.Status == HTTP_STATUS_CODE_OK) {
            $("#update-drug-expried-date-dialog").modal("hide");
            app.notice.message('Cập nhật thành công.');
            if ($scope.dialogCallbackFunc != null) {
                $scope.dialogCallbackFunc($scope.drug);
            }
        } else {
            self.onUpdateDrugPriceFailed(data);
        }
    }

    this.onUpdateDrugFailed = function (data) {
        app.notice.error('Không cập nhật được.');
    }
    $scope.updateDrugBatch = function () {
        if (!self.validateInputParams()) return;
        var drugDate = moment($scope.drug.drugDate).format('YYYY-MM-DD');
        self.requestRemoteUrl('/DrugManagement/UpdateExpiredDateDrug',
            { noteItemId: $scope.drug.noteItemId, batchNumber: $scope.drug.drugBatch, expiredDate: drugDate }
            , self.onUpdateDrugSuccess, self.onUpdateDrugFailed);
    }
    this.SaveDrugBatch = function (drug) {

    }
    $injector.invoke(BaseController, this, {
        $scope: $scope,
        $rootScope: $rootScope,
        $injector: $injector
    });
};