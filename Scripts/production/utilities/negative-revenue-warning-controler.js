app.instance.controller("NegativeRevenueWarningController", NegativeRevenueWarningController);
NegativeRevenueWarningController.$inject = ['$scope', '$rootScope', '$injector', '$window'];
function NegativeRevenueWarningController($scope, $rootScope, $injector, $window) {
    var self = this;
    this.initAtFirstRunning = this.initAtFirstRunning || function () {
        $scope.ignoreLoadingItemsInFirstLoad = true;
        $scope.usingTrNgGrid = true;
        $scope.model.paging = false;
        $scope.cachePageSizeKey = 'negative-revenue-warning-page-size-cache-key';
        $scope.filterType = FILTER_DATE_RANGE;        
        self.applyReportDates();
        self.readCachedValues();
    }

    this.validateInputParams = this.validateInputParams || function () {
        return true;
    }

    this.initCacheDictionary = this.initCacheDictionary || function () {
    }

    this.onInitializeFinished = this.onInitializeFinished || function () {
    }

    this.standardizeReciveData = this.standardizeReciveData || function (response) {
        if (response && response.Data) {
            var pagingModel = response.Data.PagingResultModel;
            $scope.model.totalItems = pagingModel.TotalSize;
            $scope.model.items = pagingModel.Results;          
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
       
        self.requestRemoteUrl('/Utilities/GetNegativeRevenueWarningData',
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

    $scope.onReceiptItemLink = function (gridItem, noteItem)
    {
        var itemLink = '';
        if (noteItem.NoteId > 0) {
            itemLink = String.format('/PhieuNhaps/Details/{0}', noteItem.NoteId);
            $window.open(itemLink, '_blank');
        }
        else
        {
            itemLink = String.format('/Thuocs/DialogDetail/{0}', gridItem.ItemId);
            var dialogThuocDetails = {};
            $.get(itemLink).done(function (data) {
                if (data) {
                    dialogThuocDetails[gridItem.ItemId] = $(data).hide().appendTo(document.body).on('hide.bs.modal', function () { $('.modal-backdrop').remove(); }
                    ).modal();
                }
            });
        }

        return itemLink;
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
