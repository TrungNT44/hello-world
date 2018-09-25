app.instance.controller("ListRecruitActiveController", ListRecruitActiveController);
ListRecruitActiveController.$inject = ['$scope', '$rootScope', '$injector'];
function ListRecruitActiveController($scope, $rootScope, $injector) {
    var self = this;
    this.initAtFirstRunning = this.initAtFirstRunning || function () {
        self.readCachedValues();
    }
    this.onInitializeFinished = function () {
        self.LoadDataForDDL_Province();
        self.loadData("/Recruitment/GetListRecruitActive", null, function (response) {
            if (response) {
                $scope.items = response;
            }
        });
    }
    $scope.Search = function () {
        self.loadData("/Recruitment/GetListRecruitActive", {
            TieuDe: $scope.InputData.TieuDe,
            IdTinhThanh: $scope.InputData.IdTinhThanh
        }, function (response) {
            if (response) {
                $scope.items = response;
            }
        });
    }
    this.LoadDataForDDL_Province = function (defautValue) {
        self.loadData("/Recruitment/GetListProvinces", null, function (response) {
            $scope.ArrTinhThanhs = response;
        });
    }
    $injector.invoke(BaseController, this, {
        $scope: $scope,
        $rootScope: $rootScope,
        $injector: $injector
    });
}