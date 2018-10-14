app.instance.controller("ListInventoryController", ListInventoryController);
ListInventoryController.$inject = ['$scope', '$rootScope', '$injector'];
function ListInventoryController($scope, $rootScope, $injector) {
    var self = this;
    this.initAtFirstRunning = this.initAtFirstRunning || function () {

        // File angular controller của View Inventory/Index.cshtml
        $scope.InventoryModel = {};
        $scope.searchThuocId = -1; // khởi tạo giá trị id của thuốc muốn tìm = -1
    }
    
    // chuyển đến trang tạo Phiếu Kiểm kê mới
    $scope.Create = function () {
        $window.location.href = '/Inventory/Create';
    }

    // xử lý khi user tìm và chọn 1 thuốc mới
    $scope.onDrugSelectChanged = function (drugIds) {
        if (drugIds == null || drugIds.length < 1) {
            $scope.searchThuocId = -1;
            return;
        }
        var drugId = drugIds[0];
        self.getInventoryDrugItem('', drugId);
    }

    // lấy thông tin thuốc từ server
    this.getInventoryDrugItem = function (maNhomThuoc, drugId) {
        self.requestRemoteUrl('/Inventory/GetDrugInfo',
            { ignoreLoadingIndicator: true, maNhomThuoc: maNhomThuoc, drugIds: drugId }, self.onGetInventoryDrugItemSuccess, self.onGetInventoryDrugItemFailed);
    }

    // Nếu lấy thông tin thuốc OK, gán tên thuốc vừa lấy được vào $scope.searchThuocId
    this.onGetInventoryDrugItemSuccess = function (response) {
        if (response && response.Data && response.Status == 200) {
            $scope.searchThuocId = response.Data[0].ThuocId;
        } else {
            self.onGetInventoryDrugItemFailed(response);
        }
    }

    this.onGetInventoryDrugItemFailed = function (response) {
        app.notice.error('Không lấy được dữ liệu.');
    }

    this.standardizeReciveData = this.standardizeReciveData || function (response) {
        if (response && response.Data) {
            $scope.InventoryModel = response.Data.InventoryDetailModels;
        } else {
            self.onGetDataFailed(response);
        }
    }

    this.onGetDataSuccess = function (response) {
        self.standardizeReciveData(response);
    }

    this.onGetDataFailed = function (response) {
        app.notice.error('Không lấy được dữ liệu.');
    }

    $scope.onDisplayReportData = function (callbackParams) {
        $scope.reportFromDate = callbackParams.reportFromDate;
        $scope.reportToDate = callbackParams.reportToDate;

        // call webservice để lấy dữ liệu
        self.fetchData();

    }

    this.fetchData = function () {
        // set tham số khi gọi webservice lấy danh sách Phiếu Kk
        var externalParams = {
            thuocId: $scope.searchThuocId,
            fromDate: $scope.reportFromDate,
            toDate: $scope.reportToDate
        };

        self.requestRemoteUrl('/Inventory/GetInventoryList',
            externalParams, self.onGetDataSuccess, self.onGetDataFailed);
    }

    $injector.invoke(BaseController, this, {
        $scope: $scope,
        $rootScope: $rootScope,
        $injector: $injector
    });

};
