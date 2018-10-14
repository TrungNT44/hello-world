app.instance.controller("ExpiryWarningController", ExpiryWarningController);
ExpiryWarningController.$inject = ['$scope', '$rootScope', '$injector'];
function ExpiryWarningController($scope, $rootScope, $injector) {
    var self = this;
    this.initAtFirstRunning = this.initAtFirstRunning || function () {
        $scope.ignoreLoadingItemsInFirstLoad = false;
        $scope.usingTrNgGrid = true;
        $scope.model.pageSize = 1000000; // Not paging
        $scope.model.paging = false;
        //$scope.cachePageSizeKey = 'expiry-warning-page-size-cache-key';
        $scope.sType = "1";
        $scope.sNhomThuocId = "";
        $scope.ma_thuoc = "";
        $scope.currentItem = null;
        self.readCachedValues();
    }
    $injector.invoke(BaseController, this, {
        $scope: $scope,
        $rootScope: $rootScope,
        $injector: $injector
    });

    // Init grid
    this.initGrid(false, false, false, false, false,
        self.loadData, {
            remoteUrl: '/Utilities/GetCanhBaoHangHetHan',
            extraParams: {
                sType: $scope.sType,
                sNhomThuocId: $scope.sNhomThuocId,
                sMaThuoc: $scope.ma_thuoc
            },
            bindingFunc: self.bindDataToGrid
        });
    this.standardizeReciveData = this.standardizeReciveData || function (response) {
        if (response && response.Data) {
            var pagingModel = response.Data.PagingResultModel;
            $scope.model.totalItems = pagingModel.TotalSize;
            $scope.model.items = pagingModel.Results;
            $scope.model.Total = response.Data.Total;
        } else {
            app.notice.error('Lỗi khi truy cập dữ liệu máy chủ.');
        }
    }

    this.afterUpdated = function (data) {
        if ($scope.currentItem == null) return;
        $scope.currentItem.Han = data.drugDate;
        $scope.currentItem.Solo = data.drugBatch;
    }

    $scope.fnDpdateDrugPrice = function (drug_info) {
        var drug = {
            SoPhieuNhap: drug_info.SoPhieuNhap,
            NgayPhieuNhap: drug_info.NgayPhieuNhap,
            drugCode: drug_info.MaThuoc,
            drugName: drug_info.TenThuoc,
            drugBatch: drug_info.Solo != null? drug_info.Solo: '',
            drugDate: drug_info.Han != null ? drug_info.Han : '',
            noteItemId: drug_info.IdPhieuNhap
        };
        $scope.currentItem = drug_info;
        self.prepareDataForDialog('update-drug-expried-date-dialog', { drug: drug }, self.afterUpdated);
    }
    $scope.resetFilter = function () {
        $scope.sNhomThuocId = "";
        $scope.ma_thuoc = "";
    }
    $scope.fnFilter = function () {
        if ($scope.sType == "2") {
            if ($scope.sNhomThuocId == null || $scope.sNhomThuocId == "") {
                app.notice.error('Chọn nhóm thuốc.');
                return;
            }
        }
        if ($scope.sType == "3") {
            if ($scope.ma_thuoc == null || $scope.ma_thuoc == "") {
                app.notice.error('Nhập tên thuốc.');
                return;
            }
        }
        $scope.gridCallbackFuncParams.extraParams = {
            sType: $scope.sType,
            sNhomThuocId: $scope.sNhomThuocId,
            sMaThuoc: $scope.ma_thuoc
        };
        self.loadAndBindDataToGrid();
    }
    $scope.fnUpdateExpriedDate = function () {
        var drug = null;
        self.prepareDataForDialog('update-drug-expried-date-dialog', { drug: drug });
    }
}