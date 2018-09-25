app.instance.controller("TransHistoryReceiptItemTableController", TransHistoryReceiptItemTableController);
TransHistoryReceiptItemTableController.$inject = ['$scope', '$rootScope', '$injector'];
function TransHistoryReceiptItemTableController($scope, $rootScope, $injector) {
    var self = this;
    this.initAtFirstRunning = this.initAtFirstRunning || function () {
        $scope.ignoreLoadingItemsInFirstLoad = true;
        $scope.usingTrNgGrid = true;
        $scope.cachePageSizeKey = 'trans-history-receipt-page-size-cache-key';
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
            $rootScope.ReceiptTransHistoryCount = $scope.model.totalItems;
        }
    }

    $injector.invoke(BaseController, this, {
        $scope: $scope,
        $rootScope: $rootScope,
        $injector: $injector
    });

    // Init grid
    this.initGrid(false, false, false, false, false,
        self.loadData, { remoteUrl: '/Report/GetDrugTransHistoryData', extraParams: { noteTypeId: 1 }, bindingFunc: self.bindDataToGrid });
};
