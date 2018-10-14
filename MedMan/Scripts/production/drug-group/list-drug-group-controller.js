app.instance.controller("ListDrugGroupController", ListDrugGroupController);
ListDrugGroupController.$inject = ['$scope', '$rootScope', '$injector', '$filter', '$window'];
function ListDrugGroupController($scope, $rootScope, $injector, $filter, $window) {
    var self = this;
    this.initAtFirstRunning = this.initAtFirstRunning || function () {
        // File angular controller của View DrugGroup/Index.cshtml
        $scope.newDrugGroup = {};
        //$scope.model = {};
        if (typeof viewModel != "undefined") {
            $scope.model = viewModel;
        }
        //$scope.model.tenThuocTimKiem = "";
        $scope.model.ItemPerPage = 10;
    }

    // Nếu lưu OK, chuyển về màn hình chính 
    this.onSaveDrugGroupSuccess = function (response) {
        console.log(response.Data);
        if (response.Data == -1) {
            app.notice.error('Nhóm thuốc này đã được tạo. Hãy nhập tên khác');
            return;
        }

        app.notice.message("Nhóm thuốc đã được lưu.");
        var rootUrl = app.utils.getRootUrl();
        var detailUrl = String.format('{0}/DrugGroup/Index', rootUrl);

        window.location.href = detailUrl;
    }

    // lưu phiếu Kiểm kê đã tạo/edit
    $scope.saveDrugGroup = function () {
        var tenNhomThuoc = $scope.newDrugGroup.TenNhomThuoc;
        if (tenNhomThuoc == null || tenNhomThuoc == "") {
            app.notice.error('Tên nhóm thuốc không hợp lệ');
            return;
        }

        // truyền oData đến webservice /DrugGroup/SaveDrugGroup
        var oData = $scope.newDrugGroup;

        self.requestRemoteUrl('/DrugGroup/SaveDrugGroup',
            { model: oData }, self.onSaveDrugGroupSuccess, null);
    }
    

    $injector.invoke(BaseController, this, {
        $scope: $scope,
        $rootScope: $rootScope,
        $injector: $injector
    });

};
