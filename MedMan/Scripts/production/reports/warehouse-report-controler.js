app.instance.controller("WarehouseReportController", WarehouseReportController);
WarehouseReportController.$inject = ['$scope', '$rootScope', '$injector'];
function WarehouseReportController($scope, $rootScope, $injector) {
    var self = this;
    this.initAtFirstRunning = this.initAtFirstRunning || function () {
        $scope.ignoreLoadingItemsInFirstLoad = true;
        $scope.usingTrNgGrid = true;
        $scope.model.pageSize = 1000000; // Not paging
        $scope.cachePageSizeKey = 'drug-warehouse-page-size-cache-key';
        $scope.filterType = FILTER_DATE_RANGE;
        $scope.drugIds = [];
        $scope.selectedItemId = 0;
        $scope.groupFilterType = REPORT_FILTER_TYPE_ALL;
        $scope.filterItemType = ITEM_FILTER_TYPE_BY_DRUG_GROUP;
        self.applyReportDates();
        self.readCachedValues();
    }

    this.validateInputParams = this.validateInputParams || function () {
        return true;
    }

    this.initCacheDictionary = this.initCacheDictionary || function () {
        //$scope.cacheDicts["locationQuery"] = 0;
    }

    this.onInitializeFinished = this.onInitializeFinished || function () {
        //self.fetchData();
    }

    this.standardizeReciveData = this.standardizeReciveData || function (response) {
        if (response && response.Data) {
            var pagingModel = response.Data.PagingResultModel;
            $scope.model.totalItems = pagingModel.TotalSize;
            $scope.model.items = pagingModel.Results;
            $scope.model.FirsInventoryValueTotal = response.Data.FirsInventoryValueTotal;
            $scope.model.ReceiptValueTotal = response.Data.ReceiptValueTotal;
            $scope.model.DeliveryValueTotal = response.Data.DeliveryValueTotal;
            $scope.model.LastInventoryValueTotal = response.Data.LastInventoryValueTotal;
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

    this.fetchData = function () {
        var externalParams = {};
        if ($scope.filterType == FILTER_ALL) {
            self.applyReportDates(true);
        } else {
            self.applyReportDates();
            externalParams = { reportFromDate: $scope.model.input.reportFromDate, reportToDate: $scope.model.input.reportToDate };
        }
        var itemIds = [];
        if ($scope.groupFilterType == REPORT_FILTER_TYPE_BY_NAME) {
            itemIds = $scope.drugIds;
        } else if ($scope.groupFilterType == REPORT_FILTER_TYPE_BY_GROUP) {
            itemIds.push($scope.selectedItemId);
        }

        var filterItemType = $scope.filterItemType;
        if ($scope.groupFilterType == REPORT_FILTER_TYPE_BY_NAME) {
            filterItemType = ITEM_FILTER_TYPE_BY_DRUG;
        }
        var reportFilterParams = {
            itemIds: itemIds, groupFilterType: $scope.groupFilterType,
            pageSize: $scope.model.pageSize, pageIndex: $scope.pageIndex,
            filterItemType: filterItemType
        };

        $.extend(externalParams, reportFilterParams);
       
        self.requestRemoteUrl('/Report/GetDrugWarehouses',
            externalParams, self.onGetDataSuccess, self.onGetDataFailed);
    }

    $scope.getItemBackground = function(item) {
        var itemBgClass = '';
        if (item.LastInventoryQuantity < 0)
        {
            itemBgClass = 'row-item-warning';
        }

        return itemBgClass;
    }

    $scope.onDisplayReportData = function (callbackParams) {
        $scope.reportFromDate = callbackParams.reportFromDate;
        $scope.reportToDate = callbackParams.reportToDate;
        $scope.filterType = callbackParams.filterType;

        self.fetchData();
    }

    $injector.invoke(BaseController, this, {
        $scope: $scope,
        $rootScope: $rootScope,
        $injector: $injector
    });

    // Init grid
    this.initGrid(false, false, false, false, false,
        self.fetchData, null);
};
