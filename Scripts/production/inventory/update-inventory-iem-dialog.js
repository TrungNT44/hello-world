﻿app.instance.controller("UpdateInventoryItemDialogController", UpdateInventoryItemDialogController);
UpdateInventoryItemDialogController.$inject = ['$scope', '$rootScope', '$injector', '$filter'];
function UpdateInventoryItemDialogController($scope, $rootScope, $injector, $filter) {
    var self = this;
    this.initAtFirstRunning = this.initAtFirstRunning || function () {
        
        //$scope.drug = { HanDung: null };
    }

    this.validateInputParams = this.validateInputParams || function () {

        if ($scope.drug.HanDung != "") {
            var isDateValid = moment($scope.drug.HanDung, "DD/MM/YYYY").isValid();
            if (!isDateValid) {
                //app.notice.error("Giá nhập không được để trống !");
                app.notice.error("Hạn dùng chưa chính xác!");
                return false;
            }
        }


        return true;
    }

    this.loadAndBindDataToView = this.loadAndBindDataToView || function () {
        console.log($scope.drug);
        //if ($scope.externalParams.drug.HanDung != null) {
        //    $scope.externalParams.drug.HanDung = $scope.externalParams.drug.HanDung.replace("T00:00:00", "");
        //}
        
        $scope.drug = $scope.externalParams.drug;

        self.showDialog();
    }

    this.onUpdateDrugSuccess = function (data) {
            $("#update-inventory-item-dialog").modal("hide");
            app.notice.message('Cập nhật thành công.');
            if ($scope.dialogCallbackFunc != null) {
                $scope.dialogCallbackFunc($scope.drug);
            }
        
    }

    this.onUpdateDrugFailed = function (data) {
        app.notice.error('Không cập nhật được.');
    }

    $scope.updateDrugBatch = function () {
        console.log($scope.drug);

        //if (!self.validateInputParams()) return;
        //var HanDung = moment($scope.drug.HanDung).format('YYYY-MM-DD');
        if ($scope.drug.InventoryId == -1) {
            self.onUpdateDrugSuccess();
        }
        else {
            self.requestRemoteUrl('/Inventory/UpdateDrugSerialNoAndExpDate',
                { inventoryEditModel: $scope.drug }
                , self.onUpdateDrugSuccess, self.onUpdateDrugFailed);
        }
        
        
    }
    this.SaveDrugBatch = function (drug) {

    }
    $injector.invoke(BaseController, this, {
        $scope: $scope,
        $rootScope: $rootScope,
        $injector: $injector
    });
};