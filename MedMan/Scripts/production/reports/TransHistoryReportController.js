app.instance.controller("TransHistoryReportController", TransHistoryReportController);
TransHistoryReportController.$inject = ['$scope', '$rootScope', '$injector'];
function TransHistoryReportController($scope, $rootScope, $injector) {
    var self = this;
    this.initAtFirstRunning = this.initAtFirstRunning || function () {
        $scope.drugId = 0;
        $scope.viewModel = viewModel;
        $scope.hasViewDeliveryNotePrivilage = viewModel.HasViewDeliveryNotePrivilage;
        $scope.hasViewReceiptNotePrivilage = viewModel.HasViewReceiptNotePrivilage;
        $scope.defaultSelectedDrugItem = null;       
        if (viewModel.Drug.DrugId > 0) {
            $scope.defaultSelectedDrugItem = viewModel.Drug;
            $scope.drugId = viewModel.Drug.DrugId;
        }
        $scope.ignoreLoadingItemsInFirstLoad = true;
        $scope.usingTrNgGrid = true;
        $scope.model.pageSize = 1000000; // Not paging
        $scope.cachePageSizeKey = 'trans-history-page-size-cache-key';
        $scope.filterType = FILTER_DATE_RANGE;
        $scope.drugIds = [];
       
        if ($scope.drugId > 0) {
            $scope.filterType = FILTER_ALL;
            $scope.drugIds.push($scope.drugId);
        }        
        $scope.selectedItemId = 0;
        $scope.groupFilterType = REPORT_FILTER_TYPE_BY_NAME; // By Drug
        $scope.filterItemType = ITEM_FILTER_TYPE_BY_DRUG;
        $rootScope.ReceiptTransHistoryCount = 0;
        $rootScope.DeliveryTransHistoryCount = 0;

        self.applyReportDates();
        self.readCachedValues();
    }

    this.validateInputParams = this.validateInputParams || function () {
        return true;
    }

    this.initCacheDictionary = this.initCacheDictionary || function () {
       
    }

    this.onInitializeFinished = this.onInitializeFinished || function () {
        if (!$scope.hasViewReceiptNotePrivilage && $scope.hasViewDeliveryNotePrivilage) {
            $scope.showTabContent(1);
        }

        self.fetchData(false);
    }   

    $scope.getTableId = function () {
        return $rootScope.ActivePage == 0 ? 'table-id-trans-history-receipt' : 'table-id-trans-history-delivery';
    }

    $scope.onDisplayReportData = function (callbackParams) {
        $scope.reportFromDate = callbackParams.reportFromDate;
        $scope.reportToDate = callbackParams.reportToDate;
        $scope.filterType = callbackParams.filterType;

        self.fetchData(true);
    }

    this.fetchData = function (clickByUser) {
        $rootScope.validFilterParams = false;
        if ($scope.drugIds == null || $scope.drugIds.length < 1) {
            if (clickByUser) {
                app.notice.error("Nhập thuốc muốn xem lịch sử giao dịch.");
            }
            return;
        }

        var externalParams = {};
        if ($scope.filterType == FILTER_ALL) {
            self.applyReportDates(true);
        } else {
            self.applyReportDates();            
        }
        externalParams = { reportFromDate: $scope.model.input.reportFromDate, reportToDate: $scope.model.input.reportToDate };

        var reportFilterParams = {
            itemIds: $scope.drugIds, groupFilterType: $scope.groupFilterType,
            filterItemType: $scope.filterItemType
        };

        $.extend(externalParams, reportFilterParams);

        self.broadcastExternalParamsChanged(externalParams);
    }

    this.onViewReady = this.onViewReady || function (params) {
        self.fetchData(false);
    }

    $injector.invoke(BaseController, this, {
        $scope: $scope,
        $rootScope: $rootScope,
        $injector: $injector
    });
};
