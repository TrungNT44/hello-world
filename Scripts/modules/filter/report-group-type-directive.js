app.instance.controller("ReportGroupTypesController", ReportGroupTypesController);
ReportGroupTypesController.$inject = ['$scope', '$rootScope', '$injector', '$filter'];
function ReportGroupTypesController($scope, $rootScope, $injector, $filter) {
    var self = this;
    this.initAtFirstRunning = this.initAtFirstRunning || function () {
        $scope.groupTypes = [];
        $scope.groupTypes.push(
            {
                groupTypeId: 0,
                groupTypeName: '--Tất cả--'
            }
        );
        $scope.groupTypes.push(
            {
                groupTypeId: 1,
                groupTypeName: 'Theo nhóm'
            }
        );
        $scope.groupTypes.push(
            {
                groupTypeId: 2,
                groupTypeName: 'Theo tên'
            }
        );
        $scope.filterByGroupItems = [];
        $scope.filterByNameItems = []; 
        $scope.preFilterItemType = -1;
    }

    this.validateInputParams = this.validateInputParams || function () {
        return true;
    }

    this.onInitializeFinished = this.onInitializeFinished || function () {
        self.getFilterItems();
    }

    $scope.onGroupTypeChanged = function () {
        if ($scope.filterByNameForDrug && $scope.groupFilterType == REPORT_FILTER_TYPE_BY_NAME) {
            self.displayDrugSearchControl();
            $scope.filterItemType = ITEM_FILTER_TYPE_BY_DRUG;
        } else {
            self.displayFilterGroupItemsControl();
            $scope.filterItemType = $scope.preFilterItemType;
        }
    };

    $scope.onItemChanged = function (item) {
        $scope.selectedItemId = item.ItemId;
        if ($scope.itemChangedCallback != null) {
            $scope.itemChangedCallback(item);
        }
    };

    $scope.onDrugSelectChanged = function (drugIds) {
        $scope.drugIds = drugIds;
    };

    this.focusToSelectedItem = function ()
    {
        if ($scope.selectedItemId <= 0) return;

        if ($scope.groupFilterType == REPORT_FILTER_TYPE_BY_GROUP) {
            if ($scope.filterByGroupItems != null && $scope.filterByGroupItems.length > 0) {
                var items = $filter('filter')($scope.filterByGroupItems, { ItemId: $scope.selectedItemId }, true);
                if (items != null && items.length > 0) {
                    $scope.selectedItem = items[0];
                }                             
            }
        }
        else if ($scope.groupFilterType == REPORT_FILTER_TYPE_BY_NAME) {
            if ($scope.filterByNameItems != null && $scope.filterByNameItems.length > 0) {
                var items = $filter('filter')($scope.filterByNameItems, { ItemId: $scope.selectedItemId }, true);
                if (items != null && items.length > 0) {
                    $scope.selectedItem = items[0];
                }
            }
        }
    }

    $scope.$on("selectedItemChangedEvent", function (evt, data)
    {
        $scope.selectedItemId = data;
        self.focusToSelectedItem();
    });

    $scope.$watch("filterItemType", function (newVal, oldVal) {
        if (newVal != oldVal) {
            self.getFilterItems();
        }
    }, true);   

    this.onGetDataSuccess = function (response) {
        if (response && response.Data && response.Status == HTTP_STATUS_CODE_OK) {
            $scope.filterByGroupItems = response.Data.GroupItems;
            $scope.filterByNameItems = response.Data.NameItems;
            $scope.safeApply();
            if ($scope.selectedItemId > 0) {
                self.focusToSelectedItem();
                return;
            }

            if ($scope.ignoreAutoSelectFirstItem == true) return;

            if ($scope.groupFilterType == REPORT_FILTER_TYPE_BY_GROUP) {
                if ($scope.filterByGroupItems != null && $scope.filterByGroupItems.length > 0) {
                    $scope.selectedItemId = $scope.filterByGroupItems[0].ItemId;
                    $scope.selectedItem = $scope.filterByGroupItems[0];
                }
            }
            else if ($scope.groupFilterType == REPORT_FILTER_TYPE_BY_NAME) {
                if ($scope.filterByNameItems != null && $scope.filterByNameItems.length > 0) {
                    $scope.selectedItemId = $scope.filterByNameItems[0].ItemId;
                    $scope.selectedItem = $scope.filterByNameItems[0];
                }
            }
            if ($scope.raiseEventOnFirstSelectedByDefault == true &&  $scope.selectedItem != null) {
                $scope.onItemChanged($scope.selectedItem);
            }
        } else {
            self.onGetDataFailed(response);
        }
    }

    this.onGetDataFailed = function (response) {
        app.notice.error('Không lấy được dữ liệu.');
    }

    this.getFilterItems = function () {
        if ($scope.preFilterItemType < 0 && $scope.filterItemType != ITEM_FILTER_TYPE_BY_DRUG) {
            $scope.preFilterItemType = $scope.filterItemType;
        }
        self.requestRemoteUrl('/Search/GetFilterItems',
            { ignoreLoadingIndicator: true, filterItemType: $scope.filterItemType, optionAllItems: $scope.optionAllItems }, self.onGetDataSuccess, self.onGetDataFailed);
    }

    this.displayDrugSearchControl = function () {
        if ($('#drugSearchControlId').is(':visible')) { //if the container is visible on the page
            var groupCtrl = $("#groupTypeContainerId");
            var searchCtrl = $("#drugSearchControlId");
            searchCtrl.width(groupCtrl.width());
            searchCtrl.height(groupCtrl.height());
        } else {
            setTimeout(self.displayDrugSearchControl, 50); //wait 50 ms, then try again
        }
    }

    this.displayFilterGroupItemsControl = function () {
        var groupControlId = '#ddlFilterByGroupItems';
        if ($scope.groupFilterType == REPORT_FILTER_TYPE_BY_NAME) {
            groupControlId = '#ddlFilterByNameItems';
        }

        if ($(groupControlId).is(':visible')) { //if the container is visible on the page
            AngularHelper.Compile($(groupControlId), $scope);
        } else {
            setTimeout(self.displayFilterGroupItemsControl, 50); //wait 50 ms, then try again
        }
    }

    $injector.invoke(BaseController, this, {
        $scope: $scope,
        $rootScope: $rootScope,
        $injector: $injector
    });
};

app.instance.directive("reportGroupTypeFilter", reportGroupTypeFilter);
reportGroupTypeFilter.$inject = ['$timeout'];
function reportGroupTypeFilter($timeout) {
    return {
        restrict: 'A',
        scope: {
            groupFilterTitle: '@?',
            groupFilterType: "=",
            showGroupFilterType: "=",
            filterItemType: '=',
            selectedItemId: '=',
            selectedItem: '=',
            filterByNameForDrug: '=',
            drugIds: "=",
            ignoreAutoSelectFirstItem: "=?",
            singleMode: "=?",
            disabledMode: "=?",
            optionAllItems: "=?",
            singleModeWithTitle: "=?",
            itemChangedCallback: '=',
            raiseEventOnFirstSelectedByDefault: "=?",
            disableGroupFilterType: "=?",
            onlySingleDrugItem: "=?",
            defaultSelectedDrugItem: "=?",
        },
        replace: false,
        templateUrl: "/tplReportGroupTypeFilterControl.html",
        controller: 'ReportGroupTypesController'
    }
};
