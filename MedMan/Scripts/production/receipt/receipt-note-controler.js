app.instance.controller("ReceiptNoteController", ReceiptNoteController);
ReceiptNoteController.$inject = ['$scope', '$rootScope', '$injector'];
function ReceiptNoteController($scope, $rootScope, $injector) {
    var self = this;
    this.initAtFirstRunning = this.initAtFirstRunning || function () {
        $scope.currentRow = null;
    }

    this.validateInputParams = this.validateInputParams || function () {
        return true;
    }

    this.getRowFromEvent = function (event) {
        var updateBtn = event.target;
        var row = $(updateBtn).closest("tr");
        //var row = updateBtn.parentElement.parentElement.parentElement;
        //if (updateBtn.className == 'glyphicon glyphicon-calendar') {
        //    row = row.parentElement.parentElement;
        //}
        //else if (updateBtn.className == 'glyphicon glyphicon-edit') {
        //    row = row.parentElement.parentElement.parentElement;
        //}
        //else if (updateBtn.className == 'btn edit-mode') {
        //    row = row.parentElement;
        //}
      
        return row;
    }

    this.getDrug = function(row) {
        var codeTextCtrl = $(row).find(".drug-code")[0];
        var drugCode = (codeTextCtrl ==null?"": codeTextCtrl.value);
        if (!drugCode || drugCode.length == 0) {
            return null;
        }

        var priceTextCtrl = $(row).find(".drug-price")[0];
        var priceText = (priceTextCtrl == null ? "" : priceTextCtrl.value);

        var priceOutTextCtrl = $(row).find(".new-out-price-l")[0];
        var priceOutText = (priceOutTextCtrl == null ? "" : priceOutTextCtrl.value);

        var drugNameCtrl = $(row).find(".drug-name")[0];
        var drugName = (drugNameCtrl == null?"":drugNameCtrl.value);

        var drugUnitCtrl = $(row).find(".drug-unit")[0];
        var drugUnit = (drugUnitCtrl == null ? "" : drugUnitCtrl.value);

        var drugBatchCtrl = $(row).find(".drug-batch")[0];
        var drugBatch = (drugBatchCtrl == null ? "" : drugBatchCtrl.value);

        var drugDateCtrl = $(row).find(".drug-date")[0];
        var drugDate = (drugDateCtrl == null ? "" : drugDateCtrl.value);

        return { drugCode: drugCode, drugName: drugName, drugPrice: priceText, newOutPriceL: priceOutText, drugUnit: drugUnit, drugBatch: drugBatch, drugDate: drugDate };
    }

    $scope.updateInPrice = function (event) {
        if (!self.validateInputParams()) return;

        var row = self.getRowFromEvent(event);
        var drug = self.getDrug(row);
        if (drug == null) {
            event.stopImmediatePropagation();
            app.notice.error("Hãy chọn thuốc muốn cập nhật giá.");
            return;
        }
        $scope.currentRow = row;
        fnInitWhenClosePopup(row);
        self.prepareDataForDialog('update-drug-in-price-dialog', { drug: drug });
    }

    $scope.updateOutPrice = function (event) {
        if (!self.validateInputParams()) return;

        var row = self.getRowFromEvent(event);
        var drug = self.getDrug(row);
        if (drug == null) {
            event.stopImmediatePropagation();
            app.notice.error("Hãy chọn thuốc muốn cập nhật giá.");
            return;
        }
        $scope.currentRow = row;
        fnInitWhenClosePopup(row);
        self.prepareDataForDialog('update-drug-out-price-dialog', { drug: drug });
    }

    $scope.updateBatchExpiryDate = function (event) {
        if (!self.validateInputParams()) return;

        var row = self.getRowFromEvent(event);
        var drug = self.getDrug(row);
        if (drug == null) {
            event.stopImmediatePropagation();
            app.notice.error("Hãy chọn thuốc muốn cập nhật số lô/hạn dùng.");
            return;
        }
        $scope.currentRow = row;
        fnInitWhenClosePopup(row);
        self.prepareDataForDialog('update-drug-batch-dialog', { drug: drug }, self.onBatchInfoUpdated);
    }

    this.onBatchInfoUpdated = function (drug) {
        var row = $scope.currentRow;
        var drugDateCtrl = $(row).find(".drug-date")[0];
        drugDateCtrl.value = drug.drugDate;

        var drugDateCtrl = $(row).find(".drug-batch")[0];
        drugDateCtrl.value = drug.drugBatch;

        var drugItemQuantityCtrl = $(row).find(".drug-item-quantity")[0];
        if (drugItemQuantityCtrl != null) {
            drugItemQuantityCtrl.focus();
        }
    }

    $injector.invoke(BaseController, this, {
        $scope: $scope,
        $rootScope: $rootScope,
        $injector: $injector
    });
};