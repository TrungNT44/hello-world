app.instance.controller("ListRecruitController", ListRecruitController);
ListRecruitController.$inject = ['$scope', '$rootScope', '$injector'];
function ListRecruitController($scope, $rootScope, $injector) {
    var self = this;
    this.initAtFirstRunning = this.initAtFirstRunning || function () {
        $scope.ignoreLoadingItemsInFirstLoad = false;
        $scope.usingTrNgGrid = true;
        $scope.model.pageSize = 1000000; // Not paging
        $scope.cachePageSizeKey = 'expiry-warning-page-size-cache-key';
        self.readCachedValues();
        $scope.InputData = {
            TieuDe: ""
        };
    }
    this.onInitializeFinished = function () {
        self.initGrid(false, false, false, false, false,
        self.loadData, {
            remoteUrl: '/Recruitment/GetListRecruitsOfGrugStore',
            extraParams: null, bindingFunc: self.bindDataToGrid
        });
    }
    $scope.ActiveRecruit = function (idRecruit) {
        self.loadData("/Recruitment/ActiveRecruit", {
            idRecruit: idRecruit
        }, function (response) {
            if (response === "OK")
                app.notice.message('Tin đã được ưu tiên');
            else
                app.notice.error('Không thể ưu tiên hoặc do bạn đã hết quyền ưu tiên trong ngày hôm nay');
        });
    }
    $scope.SubmitSearch = function ($event) {
        if ($event.which == 13)
            $scope.FindRecruit();
    }
    $scope.FindRecruit = function () {
        $scope.gridCallbackFuncParams.extraParams = {
            TieuDe: $scope.InputData.TieuDe
        };
        self.loadAndBindDataToGrid();
    }
    $injector.invoke(BaseController, this, {
        $scope: $scope,
        $rootScope: $rootScope,
        $injector: $injector
    });
}