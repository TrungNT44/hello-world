app.instance.controller("MappingCatalogController", MappingCatalogController);
MappingCatalogController.$inject = ['$scope', '$rootScope', '$injector'];
var i = 0;
function MappingCatalogController($scope, $rootScope, $injector) {
    var self = this;
    self.loadDone = false;
    this.initAtFirstRunning = this.initAtFirstRunning || function () {
        $scope.ignoreLoadingItemsInFirstLoad = false;
        $scope.usingTrNgGrid = true;
        $scope.model.paging = true;
        self.readCachedValues();
        $scope.keySearch = "";
        $scope.StoreDrugFilter = {
        };
        $scope.isGetAll = 1;
        //console.log($scope.pageSizeList);
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
    }
    this.onInitializeFinished = function () {
        self.loadData('/Recruitment/GetListDrugStores',
            null, function (response) {
                if (response) {
                    var arrDrugStore = [];
                    response.forEach(function (item) {
                        if (item.MaNhaThuoc !== "0012")
                            arrDrugStore.push(item);
                    });
                    $scope.StoreDrug = arrDrugStore;
                    $scope.StoreDrugFilter = { selected: arrDrugStore[0] };
                }
                else {
                    $scope.StoreDrug = [];
                }
            },null);
    }
    //Lấy danh sách thuốc của nhà thuốc
    $scope.$watch("StoreDrugFilter.selected", function (newVal, oldVal) {
        if ($scope.StoreDrugFilter.selected) {
            self.fetchData();
        }
    }, true);
    $scope.$watch("isGetAll", function (newVal, oldVal) {
        if (self.loadDone === true) {
            self.fetchData();
        }
    });
    $scope.fnSubmitSearch = function ($event) {
        if ($event.which == 13) {
            if (self.loadDone === true) {
                self.fetchData();
            }
        }
    };
    $scope.fnFindDrug = function () {
        self.fetchData();
    }
    this.fetchData = function () {
        if ($scope.StoreDrugFilter.selected) {
            self.loadData('/Thuocs/GetDrugByStoreDrugCode',
           {
               storeDrugCode: $scope.StoreDrugFilter.selected.MaNhaThuoc,
               drugName: $scope.keySearch,
               getAll: $scope.isGetAll
           }, self.onGetDataSuccess, self.onGetDataFailed);
        }
    }
    this.onGetDataSuccess = function (response) {
        if (response && response.Data) {
            self.loadDone = true;
            var pagingModel = response.Data;
            $scope.model.totalItems = response.TotalSize;
            $scope.model.items = pagingModel;
        } else {
            self.loadDone = true;
            self.onGetDataFailed(response);
        }
    };
    this.onGetDataFailed = function (response) {
        app.notice.error('Không lấy được dữ liệu.');
    };
    this.onAfterBindDataToGridView = initEventSearch;
    $injector.invoke(BaseController, this, {
        $scope: $scope,
        $rootScope: $rootScope,
        $injector: $injector
    });
    this.initGrid(false, false, false, false, false,
                        self.fetchData, null);
}
var count = 0;
app.instance.directive('afterRender', ['$timeout', function ($timeout) {
    var def = {
        link: function (scope, element, attrs) {
            function rended() {
                count++;
                if (count == scope.model.items.length)
                    initEventSearch();//do something
            }
            $timeout(rended, 0);
            count = 0;
        }
    };
    return def;
}]);