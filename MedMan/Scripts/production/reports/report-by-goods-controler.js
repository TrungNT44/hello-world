app.instance.controller("ReportByGoodsController", ReportByGoodsController);
ReportByGoodsController.$inject = ['$scope', '$rootScope', '$injector'];
function ReportByGoodsController($scope, $rootScope, $injector) {
    var self = this;
    this.initAtFirstRunning = this.initAtFirstRunning || function () {
        $scope.ignoreLoadingItemsInFirstLoad = true;
        $scope.usingTrNgGrid = true;
        $scope.model.paging = true;
        $scope.reportByTypeId = REPORT_BY_TYPE_GOODS;
        $scope.cachePageSizeKey = 'report-by-goods-page-size-cache-key';
        $scope.filterType = FILTER_DATE_RANGE;
       
        $scope.groupFilterType = REPORT_FILTER_TYPE_BY_NAME;
        $scope.selectedItemId = 0;

        $scope.secondGroupFilterType = REPORT_FILTER_TYPE_ALL;
        $scope.secondFilterItemType = ITEM_FILTER_TYPE_BY_DRUG_GROUP;
        $scope.secondSelectedItemId = 0;
        $scope.currentSelectedItemId = $scope.selectedItemId;
        $scope.reportTitle = "BÁO CÁO THEO MẶT HÀNG";
        self.setDefaultParams();
        self.applyReportDates();
        self.readCachedValues();
    }

    this.setDefaultParams = function () {
        $scope.viewModel = viewModel;
        if (viewModel != null) {
            $scope.filterItemType = viewModel.FilterItemTypeId;
            $scope.selectedItemId = viewModel.FilteObjectId;
            $scope.reportTitle = $scope.filterItemType == 9 ?"BÁO CÁO BÁC SỸ THEO MẶT HÀNG" : "BÁO CÁO THEO MẶT HÀNG";
        }
        else {
            $scope.filterItemType = ITEM_FILTER_TYPE_BY_STAFF;
        }
    }

    $scope.getGroupFilterLabel = function () {
        var label = "Nhân viên";
        if ($scope.filterItemType == ITEM_FILTER_TYPE_BY_DOCTOR) {
            label = "Bác sỹ";
        }

        return label;
    }

    this.validateInputParams = this.validateInputParams || function () {
        return true;
    }

    this.initCacheDictionary = this.initCacheDictionary || function () {
        //$scope.cacheDicts["locationQuery"] = 0;
    }

    this.onInitializeFinished = this.onInitializeFinished || function () {
        //self.fetchData();
        //$("#btnDisplayDataByDateRange").trigger("click");
    }

    this.standardizeReciveData = this.standardizeReciveData || function (response) {
        if (response && response.Data) {
            var pagingModel = response.Data.PagingResultModel;
            $scope.model.totalItems = pagingModel.TotalSize;
            $scope.model.items = pagingModel.Results;
            $scope.model.TotalAmount = response.Data.TotalAmount;
            $scope.model.TotalRevenue = response.Data.TotalRevenue;
            $scope.currentSelectedItemId = $scope.selectedItemId;
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
            itemIds.push($scope.selectedItemId);
        }
        var filterItemType = $scope.filterItemType;        

        var secondItemIds = [];
        if ($scope.secondGroupFilterType == REPORT_FILTER_TYPE_BY_NAME) {
            secondItemIds = $scope.drugIds;
        } else if ($scope.secondGroupFilterType == REPORT_FILTER_TYPE_BY_GROUP) {
            secondItemIds.push($scope.secondSelectedItemId);
        }

        var secondFilterItemType = $scope.secondFilterItemType;
        if ($scope.secondFilterItemType == REPORT_FILTER_TYPE_BY_NAME) {
            secondFilterItemType = ITEM_FILTER_TYPE_BY_DRUG;
        }
       
        var reportFilterParams = {
            itemIds: itemIds, secondItemIds: secondItemIds, groupFilterType: $scope.groupFilterType,
            pageSize: $scope.model.pageSize, pageIndex: $scope.pageIndex,
            filterItemType: filterItemType,
            secondFilterItemType: secondFilterItemType,
            reportByTypeId: $scope.reportByTypeId
        };

        $.extend(externalParams, reportFilterParams);
       
        self.requestRemoteUrl('/Report/GetReportByData',
            externalParams, self.onGetDataSuccess, self.onGetDataFailed);
    }

    $scope.getItemBackground = function(item) {
        var itemBgClass = '';
        if (item.ReturnedItem)
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
