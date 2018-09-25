app.instance.directive("appModuleFilter", appModuleFilter);
function appModuleFilter() {
    return {
        scope: {
            input: "=appModuleFilter"
        },
        require: "^trNgGrid",
        template: '<div tr-ng-title>{{input.displayName}}</div><input placeholder="{{input.placeholder}}" class="form-control input-sm ng-valid ng-dirty ng-touched" type="text" ng-model="filterInput" ng-class="filterInput ? \'editing\' : \'\'" ng-press/>',
        link: function (scope, ele, attr, trNgController) {
            scope.grid = trNgController;
            scope.$watch("filterInput", function (newValue, oldValue) {
                if (newValue !== oldValue) {
                    scope.grid.gridOptions.filterByFields[scope.input.filterName] = newValue;
                    scope.grid.gridOptions.filterByFields = angular.extend({}, scope.grid.gridOptions.filterByFields);
                }
            });
        },
    }
}
