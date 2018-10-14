app.instance.controller("DrugSearchController", DrugSearchController);
DrugSearchController.$inject = ['$scope', '$rootScope', '$injector'];
function DrugSearchController($scope, $rootScope, $injector) {
    var self = this;
    this.initAtFirstRunning = this.initAtFirstRunning || function () {
        $scope.searchText = '';
        $scope.items = [];
        $scope.page = 1;
        $scope.data = {
            selected: null
        };
        $scope.useNgMouseover = false;
        $scope.loading = false;

        if ($scope.defaultSelectedDrugItem != null) {
            var selectedItems = [];
            selectedItems.push($scope.defaultSelectedDrugItem);
            $scope.data.selected = selectedItems;
        }       
    }

    this.validateInputParams = this.validateInputParams || function () {
        return true;
    }

    this.onGetDataSuccess = function (response) {
        $scope.loading = false;
        if (response && response.Data && response.Status == HTTP_STATUS_CODE_OK) {
            $scope.items = response.Data;
        } else {
            self.onGetDataFailed(response);
        }
    }

    this.onGetDataFailed = function (response) {
        $scope.loading = false;
        app.notice.error('Không lấy được dữ liệu.');
    }

    $scope.onUpdated = function (selectedItem) {
        //do selectedItem.PropertyName like selectedItem.Name or selectedItem.Key 
        //whatever property your list has.
        if ($scope.onlySingleDrugItem == true && $scope.data.selected.length > 1) {
            var selectedItems = [];
            selectedItems.push($scope.data.selected[$scope.data.selected.length - 1]);
            $scope.data.selected = selectedItems;
        }

        $scope.drugIds = [];
        $scope.drugItems = [];
        if ($scope.data.selected && $scope.data.selected.length > 0) {
            angular.forEach($scope.data.selected, function(item) {
                $scope.drugIds.push(item.DrugId);
                $scope.drugItems.push(item);
            });
        }
        if ($scope.selectChangedCallback != null) {
            $scope.selectChangedCallback($scope.drugIds, $scope.drugItems);
        }
        if ($scope.clearAfterSelected == true) {
            $scope.data.selected = null;
        }       
    }

    $scope.fetch = function ($select, $event) {
        // no event means first load!
        //if (!$event) {
        //    $scope.page = 1;
        //    $scope.items = [];
        //} else {
        //    $event.stopPropagation();
        //    $event.preventDefault();
        //    $scope.page++;
        //}

        if (!$select.search || $select.search.length < 2) {
            $scope.page = 1;
            $scope.items = [];
            return;
        }

        //$scope.loading = true;
   
        self.requestRemoteUrl('/Search/SearchDrugs',
            { ignoreLoadingIndicator: true, searchText: $select.search, searchType: SEARCH_TYPE_DRUG, searchForDrugStore: $scope.searchForDrugStore }, self.onGetDataSuccess, self.onGetDataFailed);
    };

    $injector.invoke(BaseController, this, {
        $scope: $scope,
        $rootScope: $rootScope,
        $injector: $injector
    });
};

app.instance.directive("drugSearchFilter", drugSearchFilter);
drugSearchFilter.$inject = ['$timeout'];
function drugSearchFilter($timeout) {
    return {
        restrict: 'A',
        scope: {
            selectChangedCallback: '=',
            clearAfterSelected: '=',
            onlySingleDrugItem: "=?",
            defaultSelectedDrugItem: "=?",
            searchForDrugStore: "=?"
        },
        replace: false,
        templateUrl: "/tplDrugSearchFilterControl.html",
        controller: 'DrugSearchController'
    }
};
