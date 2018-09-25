app.instance.controller("DrugCatalogController", DrugCatalogController);
DrugCatalogController.$inject = ['$scope', '$rootScope', '$injector'];
function DrugCatalogController($scope, $rootScope, $injector) {
    var self = this;
    this.initAtFirstRunning = this.initAtFirstRunning || function () {
        $scope.ignoreLoadingItemsInFirstLoad = false;
        $scope.usingTrNgGrid = true;
        $scope.model.paging = true;
        self.readCachedValues();
        $scope.keySearch = "";
        $scope.pageSizeList = [{
            pageSizeKey: 10,
            pageSizeValue: 10
        },
        {
            pageSizeKey: 20,
            pageSizeValue: 20
        },
        {
            pageSizeKey: 50,
            pageSizeValue: 50
            }];
        $scope.totalItems = 0;
    }
    this.onInitializeFinished = function () {
        //self.fetchData();
    }
    $scope.fnFindDrug = function () {
        self.fetchData();
    }
    $scope.fnSubmitSearch = function ($event) {
        if ($event.which == 13)
            self.fetchData();
    }
    this.fetchData = function () {
        var keyWord = {
            keyWord: $scope.keySearch
        };
        self.loadData('/Thuocs/SearchDrugCatalogByName',
            keyWord, self.onGetDataSuccess, self.onGetDataFailed);
    }
    this.onGetDataSuccess = function (response) {
        if (response && response.results) {
            var pagingModel = response.results;
            $scope.model.totalItems = response.totalSize;
            $scope.totalItems = response.totalSize;
            $scope.model.items = pagingModel;
        } else {
            self.onGetDataFailed(response);
        }
    };
    this.onGetDataFailed = function (response) {
        app.notice.error('Không lấy được dữ liệu.');
    };
    $injector.invoke(BaseController, this, {
        $scope: $scope,
        $rootScope: $rootScope,
        $injector: $injector
    });

    // Init grid
    this.initGrid(false, false, false, false, false,
        self.fetchData, null);
}