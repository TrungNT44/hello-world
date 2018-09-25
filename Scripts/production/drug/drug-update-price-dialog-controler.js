app.instance.controller("DrugUpdatePriceDialogController", DrugUpdatePriceDialogController);
DrugUpdatePriceDialogController.$inject = ['$scope', '$rootScope', '$injector'];
function DrugUpdatePriceDialogController($scope, $rootScope, $injector) {
    var self = this;
    this.initAtFirstRunning = this.initAtFirstRunning || function () {
        $scope.drug = null;
    }

    this.validateInputParams = this.validateInputParams || function () {
        if (app.utils.isEmpty($scope.drug.drugPrice)) {
            app.notice.error("Giá nhập không được để trống !");
            return false;
        }
        return true;
    }

    this.loadAndBindDataToView = this.loadAndBindDataToView || function () {
        $scope.drug = $scope.externalParams.drug;
        $scope.drug.oldPrice = 0;
        $scope.drug.unitCode = 0;
        $scope.drug.unitName = "";
        self.getDrugInfo($scope.drug.drugCode);

        self.showDialog();
    }

    $scope.updateDrugPrice = function () {
        if (!self.validateInputParams()) return;

        self.requestRemoteUrl('/DrugManagement/UpdateDrugPrice',
           { drugCode: $scope.drug.drugCode, price: $scope.drug.drugPrice, unitCode: $scope.drug.unitCode }, self.onUpdateDrugPriceSuccess, self.onUpdateDrugPriceFailed);
    }

    this.onUpdateDrugPriceSuccess = function (data) {
        if (data.Status == 0) {
            $("#update-drug-price-dialog").modal("hide");
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
        if (data.Status == 0) {          
            $scope.drug.oldPrice = data.Data.Price;
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