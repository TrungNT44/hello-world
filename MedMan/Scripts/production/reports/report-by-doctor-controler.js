app.instance.controller("ReportByDoctorController", ReportByDoctorController);
ReportByDoctorController.$inject = ['$scope', '$rootScope', '$injector', '$window'];
function ReportByDoctorController($scope, $rootScope, $injector, $window) {
    var self = this;
    this.initAtFirstRunning = this.initAtFirstRunning || function () {
        $scope.ignoreLoadingItemsInFirstLoad = true;
        $scope.usingTrNgGrid = true;
        $scope.model.paging = true;
        $scope.reportByTypeId = REPORT_BY_TYPE_DOCTOR;
        $scope.cachePageSizeKey = 'report-by-doctor-page-size-cache-key';
        $scope.filterType = FILTER_DATE_RANGE;
        $scope.filterItemType = ITEM_FILTER_TYPE_BY_DOCTOR;
        $scope.groupFilterType = REPORT_FILTER_TYPE_BY_NAME;
        $scope.selectedItemId = 0;
        $scope.currentSelectedItemId = $scope.selectedItemId;
        self.applyReportDates();
        self.readCachedValues();
    }

    $scope.onDetailByGoods = function () {
        if ($scope.selectedItemId <= 0) return;
        var datesString = '';
        if ($scope.filterType != FILTER_ALL) {
            //datesString = String.format('&reportFromDate={0}&reportToDate={1}', $scope.model.input.reportFromDate, $scope.model.input.reportToDate);
        }
       
        var itemLink = String.format('/Report/ReportByGoods?reportByTypeId={0}&filterObjectId={1}{2}',
            REPORT_BY_TYPE_GOODS_BY_DOCTOR, $scope.selectedItemId, datesString);
        $window.open(itemLink, '_blank');       
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
       
        var reportFilterParams = {
            itemIds: itemIds, groupFilterType: $scope.groupFilterType,
            pageSize: $scope.model.pageSize, pageIndex: $scope.pageIndex,
            filterItemType: $scope.filterItemType,
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
