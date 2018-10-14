app.instance.controller("DrugUpdateInPriceDialogController", DrugUpdateInPriceDialogController);
DrugUpdateInPriceDialogController.$inject = ['$scope', '$rootScope', '$injector'];
function DrugUpdateInPriceDialogController($scope, $rootScope, $injector) {
    var self = this;
    this.initAtFirstRunning = this.initAtFirstRunning || function () {
        $scope.drug = null;
    }

    this.validateInputParams = this.validateInputParams || function () {
        if (app.utils.isEmpty($scope.drug.drugPrice)) {
            app.notice.error("Giá nhập không được để trống !");
            return false;
        }
        //if (app.utils.isEmpty($scope.drug.)) {
        //    app.notice.error("Giá nhập không được để trống !");
        //    return false;
        //}
        return true;
    }

    this.loadAndBindDataToView = this.loadAndBindDataToView || function () {
        $scope.drug = $scope.externalParams.drug;
        $scope.drug.oldPrice = 0;
        $scope.drug.unitCode = 0;
        $scope.drug.oldOutPriceL = 0;
        //$scope.drug.oldOutPriceB = 0;
        //$scope.drug.newOutPriceL = 0;
        //$scope.drug.newOutPriceB = 0;
        $scope.drug.unitName = "";
        self.getDrugInfo($scope.drug.drugCode);
       
        self.showDialog();
    }

    $scope.updateDrugPrice = function () {
        if (!self.validateInputParams()) return;

        self.requestRemoteUrl('/DrugManagement/UpdateDrugPrice',
           { drugCode: $scope.drug.drugCode, inPrice: $scope.drug.drugPrice, OutPriceL: $scope.drug.newOutPriceL, OutPriceB: $scope.drug.newOutPriceB, unitCode: $scope.drug.unitCode }, self.onUpdateDrugPriceSuccess, self.onUpdateDrugPriceFailed);
    }

    this.onUpdateDrugPriceSuccess = function (data) {
        if (data.Status == HTTP_STATUS_CODE_OK) {
            $("#update-drug-in-price-dialog").modal("hide");
            app.notice.message('Cập nhật giá thuốc thành công.');
            if ($scope.dialogCallbackFunc != null) {
                $scope.dialogCallbackFunc(data);
            }
        } else {
            self.onUpdateDrugPriceFailed(data);
        }
    }

    this.onUpdateDrugPriceFailed = function (data) {
        app.notice.error('Không cập nhật được giá thuốc.');
    }

    this.getDrugInfo = function (drugCode) {       

        self.requestRemoteUrl('/DrugManagement/GetDrugInfo',
           { drugCode: $scope.drug.drugCode, drugUnit: $scope.drug.drugUnit }, self.onGetDrugInfoSuccess, self.onGetDrugInfoFailed);
    }

    this.onGetDrugInfoSuccess = function (data) {
        if (data.Status == HTTP_STATUS_CODE_OK) {
            $scope.drug.oldPrice = data.Data.InPrice;
            $scope.drug.oldOutPriceL = data.Data.OutPrice;
            //$scope.drug.newOutPriceL = data.Data.OutPrice;
            $scope.drug.oldOutPriceB = data.Data.OutPriceB;
            $scope.drug.newOutPriceB = data.Data.OutPriceB;
            $scope.drug.unitCode = data.Data.UnitCode;
            $scope.drug.unitName = data.Data.UnitName;
            $scope.safeApply();
        } else {
            self.onUpdateDrugPriceFailed(data);
        }
    }

    this.onGetDrugInfoFailed = function (data) {
        
    }

    $injector.invoke(BaseController, this, {
        $scope: $scope,
        $rootScope: $rootScope,
        $injector: $injector
    });
};