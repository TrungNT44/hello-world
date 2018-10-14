app.instance.controller("RevenueDetailsByDayController", RevenueDetailsByDayController);
RevenueDetailsByDayController.$inject = ['$scope', '$rootScope', '$injector'];
function RevenueDetailsByDayController($scope, $rootScope, $injector) {
    var self = this;
    this.initAtFirstRunning = this.initAtFirstRunning || function () {
        $scope.ignoreLoadingItemsInFirstLoad = false;
        $scope.usingTrNgGrid = true;
        $scope.model.pageSize = 1000000; // Not paging
        $scope.model.paging = false;
        $scope.cachePageSizeKey = 'revenue-details-by-day-page-size-cache-key';
        $scope.reportDate = moment(new Date()).format(DEFAULT_MOMENT_DATE_FORMAT);
        self.readCachedValues();
    }

    this.applyReportDate = function () {
        $scope.model.input.reportFromDate = moment($scope.reportDate, DEFAULT_MOMENT_DATE_FORMAT).format('MM-DD-YYYY');
        self.loadAndBindDataToGrid();
    }

    this.validateInputParams = this.validateInputParams || function () {
        return true;
    }

    this.initCacheDictionary = this.initCacheDictionary || function () {
        //$scope.cacheDicts["locationQuery"] = 0;
    }

    this.onInitializeFinished = this.onInitializeFinished || function () {
        if ($scope.model.input.reportFromDate) {
            $scope.reportDate = moment($scope.model.input.reportFromDate, "MM-DD-YYYY").format(DEFAULT_MOMENT_DATE_FORMAT);
        }
        self.encodeSearchQuery();
        self.applyReportDate();
    }

    this.standardizeReciveData = this.standardizeReciveData || function (response) {
        if (response && response.Data) {
            var pagingModel = response.Data.PagingResultModel;
            $scope.model.totalItems = pagingModel.TotalSize;
            $scope.model.items = pagingModel.Results;
            $scope.model.Total = response.Data.Total;
            $scope.model.Revenue = response.Data.Revenue;
            $scope.model.DeliveryTotal = response.Data.DeliveryTotal;
            $scope.model.DebtTotal = response.Data.DebtTotal;
            $scope.model.HasDebtValue = response.Data.HasDebtValue;
        } else {
            app.notice.error('Lỗi khi truy cập dữ liệu máy chủ.');
        }
    }

    $scope.getNoteDetailLink = function (item) {
        var link = '/PhieuXuats/Details/' + item.DeliveryNoteId;
        if (item.IsReturnFromCustomer) {
            link = '/PhieuNhaps/Details/' + item.DeliveryNoteId;
        }
    }

    $scope.deleteAllSelectedItems = function () {
        var selectedItems =  self.getSelectedRowItems('table-id-revenue-details-by-day');
    }

    $scope.onReportDateChanged = function () {
        self.applyReportDate();
    }

    $injector.invoke(BaseController, this, {
        $scope: $scope,
        $rootScope: $rootScope,
        $injector: $injector
    });

    // Init grid
    this.initGrid(false, false, false, false, false,
        self.loadData, {
            remoteUrl: '/Report/GetRevenueDrugSynthesis',
            extraParams: null, bindingFunc: self.bindDataToGrid
        });

    $(document.body).on('click', '.daterange-picker', function (e) {
        var datePicker = $(e.currentTarget).find('input');
        app.utils.setPickerSelectedDate(datePicker, $scope.reportDate);             
        datePicker.datepicker('show');
    });


    var onSelectDateChanged = function (dateEvent) {
        var selectedDate = moment(dateEvent.date).format(DEFAULT_MOMENT_DATE_FORMAT);
        $scope.reportDate = selectedDate;
        $scope.onReportDateChanged();
    };

    $scope.getItemBackground = function (item) {
        var itemBgClass = '';
        if (item.ShallWarning) {
            itemBgClass = 'row-item-warning';
        }

        return itemBgClass;
    }

    $('input').datepicker({
        format: DEFAULT_DATE_PICKER_FORMAT,
        changeMonth: true,
        changeYear: true,
        endDate: 0,
        maxViewMode: 2,
        defaultDate: new Date(),
        minDate: MIN_PRODUCTION_DATA_DATE,
        language: 'vi',
        autoclose: true
    });//.on('changeDate', onSelectDateChanged);
};
