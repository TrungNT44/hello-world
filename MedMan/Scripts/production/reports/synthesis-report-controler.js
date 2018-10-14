app.instance.controller("SynthesisReportController", SynthesisReportController);
SynthesisReportController.$inject = ['$scope', '$rootScope', '$injector'];
function SynthesisReportController($scope, $rootScope, $injector) {
    var self = this;
    this.initAtFirstRunning = this.initAtFirstRunning || function () {
        $scope.ignoreLoadingItemsInFirstLoad = true;
        $scope.usingTrNgGrid = false;
        $scope.filterType = FILTER_DATE_RANGE;
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
        if (response && response.Data && response.Status == HTTP_STATUS_CODE_OK) {
            $scope.reportData = response.Data;
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
        var externalParams = null;
        if ($scope.filterType == FILTER_ALL) {
            self.applyReportDates(true);
        } else {
            self.applyReportDates();
            externalParams = { reportFromDate: $scope.model.input.reportFromDate, reportToDate: $scope.model.input.reportToDate };
        }
        self.requestRemoteUrl('/Report/GetSynthesisReportData',
            externalParams, self.onGetDataSuccess, self.onGetDataFailed);
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
        self.loadData, {
            remoteUrl: '/Report/GetRevenueDrugSynthesis',
            extraParams: {reportDate: $scope.model.input.reportDate }, bindingFunc: self.bindDataToGrid
        });
};
