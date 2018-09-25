app.instance.run(["$rootScope", function ($rootScope) {
    $rootScope.getSelectedRowItems = function ($tableEle) {
        var scope = angular.element($tableEle).isolateScope();
        var items = scope.items;
        var result = [];
        for (var i = 0; i < items.length; i++) {
            if (items[i].isSelected) {
                result.push(items[i]);
            }
        }
        return result;
    };
}]);


app.instance.directive("appModuleSelectAll", appModuleSelectAll);
function appModuleSelectAll() {
    return {
        scope: true,
        require: "trNgGrid",
        template: "<input type='checkbox' class ='check-all-directive' title='check all' ng-model='isSelectedAll' ng-change='toggleSelectAll()'/>",
        controller: ["$scope", function ($scope) {
            $scope.$on('ClearSelectedAll', function () {
                $scope.isSelectedAll = false;
            });
            $scope.$watchCollection("[" + "gridOptions.filterBy," + "gridOptions.currentPage" + "gridOptions.filterByFields," + "gridOptions.orderBy," + "gridOptions.orderByReverse," + "gridOptions.pageItems" + "]", function (newValue, oldValue) {
                $scope.isSelectedAll = false;
            });

            $scope.$on("UpdateSelectAll", function (event, selectAllState) {
                $scope.isSelectedAll = selectAllState;
                $scope.toggleSelectAll();
            });

            $scope.toggleSelectAll = function () {
                var items = $scope.gridOptions.items;
                var currentPageIndex = $scope.gridOptions.currentPage;
                var isInServerSideMode = $scope.isInServerSideMode;
                var itemPerPage = $scope.gridOptions.pageItems;
                if (isInServerSideMode) {
                    for (var i = 0; i < items.length ; i++) {
                        if ((!items[i].hasOwnProperty("isSelectAble") || items[i].isSelectAble)
                            && (!items[i].hasOwnProperty("IsDisabled") || !items[i].IsDisabled))
                        items[i].isSelected = $scope.isSelectedAll;
                    }
                }
                else {
                    var fromIndex = 0;
                    var toIndex = items.length;

                    if (itemPerPage > 0) {
                        fromIndex = currentPageIndex * itemPerPage;

                        var pageToIndex = (currentPageIndex + 1) * itemPerPage;
                        if(pageToIndex < toIndex){
                            toIndex = pageToIndex;
                        }
                    }

                    for (var i = fromIndex; i < toIndex; i++) {
                        if ((!items[i].hasOwnProperty("isSelectAble") || items[i].isSelectAble)
                            && (!items[i].hasOwnProperty("IsDisabled") || !items[i].IsDisabled))
                        items[i].isSelected = $scope.isSelectedAll;
                    }
                }

            }
        }],
    }
}

app.instance.directive("appModuleSelectRow", appModuleSelectRow);
function appModuleSelectRow() {
    return {
        scope: true,
        require: "trNgGrid",
        template: "<input type='checkbox' title='select row' ng-model='gridItem.isSelected' ng-disabled='gridItem.IsDisabled' ng-click='selectItem(gridItem, $event)'/> ",
        controller: ["$scope", "$rootScope", function ($scope, $rootScope) {
            $scope.gridItem.isSelected = false;
            $scope.selectItem = function selectItem(gridItem, $event) {
                var selectedItems = [];
                if (!$event.ctrlKey && !$event.shiftKey && !$event.metaKey) {
                    // if neither key modifiers are pressed, clear the selection and start fresh
                    //gridItem.isSelected = !gridItem.isSelected;
                    $scope.gridOptions.lastSelected = gridItem;
                    selectedItems.push(gridItem);
                    $rootScope.$broadcast("app-row-selected", selectedItems);
                }
                else {
                    if ($event.ctrlKey || $event.metaKey) {
                        // the ctrl key deselects or selects the item
                        //gridItem.isSelected = !gridItem.isSelected;
                        //$scope.gridOptions.lastSelected = gridItem;
                        selectedItems.push(gridItem);
                        $rootScope.$broadcast("app-row-selected", selectedItems);
                    }
                    else if ($event.shiftKey && $scope.gridOptions.lastSelected != null) {
                        var lastSelectedindex = $scope.gridOptions.items.indexOf($scope.gridOptions.lastSelected);
                        var currentIndex = $scope.gridOptions.items.indexOf(gridItem);
                        var firstItemIndex = lastSelectedindex > currentIndex ? currentIndex : lastSelectedindex;
                        var lastItemIndex = lastSelectedindex > currentIndex ? lastSelectedindex : currentIndex;
                        for (var index = firstItemIndex; index <= lastItemIndex; index++) {
                            var item = $scope.gridOptions.items[index];
                            if (!item.hasOwnProperty("isSelectAble") || item.isSelectAble) {
                                item.isSelected = gridItem.isSelected;
                                selectedItems.push(item);
                            }
                        }
                        $rootScope.$broadcast("app-row-selected", selectedItems);
                    }
                }
            }
        }]
    }
}



app.instance.directive("appModuleSettingRow", appModuleSettingRow);
function appModuleSettingRow() {
    return {
        scope: true,
        require: "trNgGrid",
        template: "<input type='checkbox' title='select row' ng-model='gridItem.isSelected' ng-click='selectItem(gridItem, $event)'/> ",
        controller: ["$scope", "$rootScope", function ($scope, $rootScope) {
            $scope.selectItem = function selectItem(gridItem, $event) {
                var selectedItems = [];
                if (!$event.ctrlKey && !$event.shiftKey && !$event.metaKey) {
                    // if neither key modifiers are pressed, clear the selection and start fresh
                    //gridItem.isSelected = !gridItem.isSelected;
                    $scope.gridOptions.lastSelected = gridItem;
                    selectedItems.push(gridItem);
                    $rootScope.$broadcast("app-row-selected", selectedItems);
                }
                else {
                    if ($event.ctrlKey || $event.metaKey) {
                        // the ctrl key deselects or selects the item
                        //gridItem.isSelected = !gridItem.isSelected;
                        //$scope.gridOptions.lastSelected = gridItem;
                        selectedItems.push(gridItem);
                        $rootScope.$broadcast("app-row-selected", selectedItems);
                    }
                    else if ($event.shiftKey && $scope.gridOptions.lastSelected != null) {
                        var lastSelectedindex = $scope.gridOptions.items.indexOf($scope.gridOptions.lastSelected);
                        var currentIndex = $scope.gridOptions.items.indexOf(gridItem);
                        var firstItemIndex = lastSelectedindex > currentIndex ? currentIndex : lastSelectedindex;
                        var lastItemIndex = lastSelectedindex > currentIndex ? lastSelectedindex : currentIndex;
                        for (var index = firstItemIndex; index <= lastItemIndex; index++) {
                            var item = $scope.gridOptions.items[index];
                            if (!item.hasOwnProperty("isSelectAble") || item.isSelectAble) {
                                item.isSelected = gridItem.isSelected;
                                selectedItems.push(item);
                            }
                        }
                        $rootScope.$broadcast("app-row-selected", selectedItems);
                    }
                }
            }
        }]
    }
}

