app.instance.controller("ListInventoryController", ListInventoryController);
ListInventoryController.$inject = ['$scope', '$rootScope', '$injector'];
function ListInventoryController($scope, $rootScope, $injector) {
    var self = this;
    this.initAtFirstRunning = this.initAtFirstRunning || function () {

        // File angular controller của View Inventory/Index.cshtml
        $scope.InventoryModel = {};//viewModel;
        $scope.tenThuocTimKiem = "";
        $scope.viewNotInventoriedDrugs = false;

        //$scope.filterType = FILTER_DATE_RANGE;     
        //self.applyReportDates();
        //self.readCachedValues();
        
        
    }

    //this.onInitializeFinished = this.onInitializeFinished || function () {
    //    //fetchData();
    //}


    // chuyển đến trang tạo Phiếu Kiểm kê mới
    $scope.Create = function () {
        $window.location.href = '/Inventory/Create';
    }

    // xử lý khi user tìm và chọn 1 thuốc mới
    $scope.onDrugSelectChanged = function (drugIds) {
        if (drugIds == null || drugIds.length < 1) {
            $scope.tenThuocTimKiem = drugIds;
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

    // Nếu lấy thông tin thuốc OK, gán tên thuốc vừa lấy được vào $scope.tenThuocTimKiem
    this.onGetInventoryDrugItemSuccess = function (response) {
        if (response && response.Data && response.Status == 200) {
            $scope.tenThuocTimKiem = response.Data[0].TenThuoc;
        } else {
            self.onGetInventoryDrugItemFailed(response);
        }
    }

    this.onGetInventoryDrugItemFailed = function (response) {
        app.notice.error('Không lấy được dữ liệu.');
    }   

    // Chuyển đến màn hình xem danh sách thuốc chưa được Kiểm kế
    $scope.XemHangChuaKiemKe = function () {
        var rootUrl = app.utils.getRootUrl();
        var detailUrl = String.format('{0}/Inventory/DrugsNotInventoried',
            rootUrl);
        window.location.href = detailUrl;
    }

    this.standardizeReciveData = this.standardizeReciveData || function (response) {
        if (response && response.Data) {
            $scope.InventoryModel = response.Data;
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
        //$scope.filterType = callbackParams.filterType;
        if ($scope.viewNotInventoriedDrugs) {
            $scope.XemHangChuaKiemKe();
        }
        else {
            self.fetchData();
        }
        
    }

    this.fetchData = function () {
        
        var externalParams = {
            searchTen: $scope.tenThuocTimKiem,
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

app.instance.filter('moment', function () {
    return function (input, momentFn /*, param1, param2, ...param n */) {
        var args = Array.prototype.slice.call(arguments, 2),
            momentObj = moment(input);
        return momentObj[momentFn].apply(momentObj, args);
    };
});
