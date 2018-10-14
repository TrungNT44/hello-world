app.instance.controller("DetailInventoryController", DetailInventoryController);
DetailInventoryController.$inject = ['$scope', '$rootScope', '$injector', '$filter', '$window'];
function DetailInventoryController($scope, $rootScope, $injector, $filter, $window) {
    var self = this;
    this.initAtFirstRunning = this.initAtFirstRunning || function () {
        // File angular controller của các View: Inventory/Details.cshtml, Inventory/Delete.cshtml

        $scope.InventoryModel = viewModel;
        $scope.currentEditingItem = null;

    }

    // gọi webservice /Inventory/DeleteInventory để xóa phiếu kiểm kê
    $scope.onDeleteInventory = function () {
        self.requestRemoteUrl('/Inventory/DeleteInventory',
            { inventoryId: $scope.InventoryModel.Id }, self.onDeleteInventorySuccess, null);
    }

    // nếu xóa OK, chuyển về màn hình Inventory/Index
    this.onDeleteInventorySuccess = function (response) {
        app.notice.message("Phiếu đã được xóa.");
        var rootUrl = app.utils.getRootUrl();
        var detailUrl = String.format('{0}/Inventory/Index', rootUrl);

        window.location.href = detailUrl;
    }

    $injector.invoke(BaseController, this, {
        $scope: $scope,
        $rootScope: $rootScope,
        $injector: $injector
    });

    // mở dialog để cập nhật giá/lô/hạn dùng của thuốc
    $scope.updateDrug = function (item) {
        var drug = {
            InventoryId: $scope.InventoryModel.Id,
            DaCanKho: $scope.InventoryModel.DaCanKho,
            MaThuoc: item.MaThuoc,
            TenThuoc: item.TenThuoc,
            Donvi: item.TenDonViTinh,
            Gia: item.Gia,
            SoLo: item.SoLo,
            HanDung: item.HanDung != null ? moment(item.HanDung).toDate() : null,
            onlyUpdateDrugAtClient: false
        };
        $scope.currentEditingItem = item;
        self.prepareDataForDialog('update-inventory-item-dialog', { drug: drug }, self.afterUpdated);
    }

    this.afterUpdated = function (data) {
        if ($scope.currentEditingItem == null) return;
        $scope.currentEditingItem.Gia = data.Gia;
        $scope.currentEditingItem.SoLo = data.SoLo;
        $scope.currentEditingItem.HanDung = data.HanDung;
    }

    this.onSaveInventorySuccess = function (response) {
        app.notice.message("Đã lưu các thay đổi vừa thực hiện");
        var rootUrl = app.utils.getRootUrl();
        var detailUrl = String.format('{0}/Inventory/Details/{1}',
            rootUrl, response.Data);

        window.location.href = detailUrl;
    }

    // lưu các thay đổi đã thực hiện vào DB
    $scope.saveInventory = function () {
        var r = confirm("Lưu các thay đổi vừa thực hiện?");
        if (r == false) {
            return;
        }
        var oData = {};
        oData = $scope.InventoryModel;

        self.requestRemoteUrl('/Inventory/SaveInventory',
            { model: oData }, self.onSaveInventorySuccess, null);
    }


};
