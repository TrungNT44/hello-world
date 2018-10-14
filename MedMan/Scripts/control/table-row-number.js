app.instance.directive("appModuleRowNumber", appModuleRowNumber);
function appModuleRowNumber() {
    return {
        scope: true,
        require: "^trNgGrid",
        template: '<div>{{rowNumber}}</div>',
        link: function (scope, ele, attr, trNgController) {
            scope.grid = trNgController;
            scope.rowNumber = scope.grid.gridOptions.items.indexOf(scope.gridItem) + 1;
        }
    }
}
