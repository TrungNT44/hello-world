app.instance.controller("DrugUpdateBatchDialogController", DrugUpdateBatchDialogController);
DrugUpdateBatchDialogController.$inject = ['$scope', '$rootScope', '$injector', '$filter'];
function DrugUpdateBatchDialogController($scope, $rootScope, $injector, $filter) {
    var self = this;
    this.initAtFirstRunning = this.initAtFirstRunning || function () {
        $scope.drug = {drugDate: null};
    }

    this.validateInputParams = this.validateInputParams || function () {
        if ($scope.drug.drugDate != "") {
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

    $scope.updateDrugBatch = function () {
        if (!self.validateInputParams()) return;
        $("#update-drug-batch-dialog").modal("hide");       
        if ($scope.dialogCallbackFunc != null) {
            $scope.dialogCallbackFunc($scope.drug);
        }       
    }

    //this.getDrugInfo = function (drugCode) {       

    //    self.requestRemoteUrl('/DrugManagement/GetDrugInfo',
    //       { drugCode: $scope.drug.drugCode, drugUnit: $scope.drug.drugUnit }, self.onGetDrugInfoSuccess, self.onGetDrugInfoFailed);
    //}

    //this.onGetDrugInfoSuccess = function (data) {
    //    if (data.Status == HTTP_STATUS_CODE_OK) {          
    //        $scope.drug.oldPrice = data.Data.Price;
    //        $scope.drug.unitCode = data.Data.UnitCode;
    //        $scope.drug.unitName = data.Data.UnitName;
    //        $scope.safeApply();
    //    } else {
    //        self.onUpdateDrugPriceFailed(data);
    //    }
    //}

    //this.onGetDrugInfoFailed = function (data) {
        
    //}

    $injector.invoke(BaseController, this, {
        $scope: $scope,
        $rootScope: $rootScope,
        $injector: $injector
    });
};