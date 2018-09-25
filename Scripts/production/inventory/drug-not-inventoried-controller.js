app.instance.controller("DrugNotInventoriedController", DrugNotInventoriedController);
DrugNotInventoriedController.$inject = ['$scope', '$rootScope', '$injector'];
function DrugNotInventoriedController($scope, $rootScope, $injector) {
    var self = this;
    this.initAtFirstRunning = this.initAtFirstRunning || function () {

        // File angular controller của View Inventory/DrugsNotInventoried.cshtml
        $scope.NotInventoriedDrugs = [];
    }


    this.standardizeReciveData = this.standardizeReciveData || function (response) {
        if (response && response.Data) {
            $scope.NotInventoriedDrugs = response.Data;
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

        self.fetchData();

    }

    this.fetchData = function () {
        var externalParams = {
            fromDate: $scope.reportFromDate,
            toDate: $scope.reportToDate
        };

        self.requestRemoteUrl('/Inventory/GetDrugsHaveNotInventoried',
            externalParams, self.onGetDataSuccess, self.onGetDataFailed);
    }

    // xóa thuốc khỏi danh sách hiện tại
    $scope.onDelete = function (item) {
        var index = $scope.NotInventoriedDrugs.indexOf(item);
        if (index >= 0) {
            $scope.NotInventoriedDrugs.splice(index, 1);
        }
    }

    // Khi user click nút Lập phiếu Kiêm kê ở màn hình các thuốc chưa Kiểm kê (DrugsNotInventoried.cshtml)
    $scope.lapPhieuKiemKe = function () {
        if ($scope.NotInventoriedDrugs.length <= 0) {
            app.notice.error('Không có thuốc nào trong danh sách!');
            return;
        }
        var notInventoriedDrugIds = [];
        // thêm ThuocId của các thuốc chưa kiểm kê vào notInventoriedDrugIds
        angular.forEach($scope.NotInventoriedDrugs, function (drug) {
            notInventoriedDrugIds.push(drug.ThuocId);
        });
        sessionStorage.setItem('NotInventoridDrugIds', JSON.stringify(notInventoriedDrugIds));
        var rootUrl = app.utils.getRootUrl();
        var detailUrl = String.format('{0}/Inventory/Create',
            rootUrl);
        window.location.href = detailUrl;

    }

    $injector.invoke(BaseController, this, {
        $scope: $scope,
        $rootScope: $rootScope,
        $injector: $injector
    });

};

