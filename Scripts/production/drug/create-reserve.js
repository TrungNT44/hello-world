app.instance.controller("CreateReserveController", CreateReserveController);
CreateReserveController.$inject = ['$scope', '$rootScope', '$injector'];
function CreateReserveController($scope, $rootScope, $injector) {
    var self = this;
    this.initAtFirstRunning = this.initAtFirstRunning || function () {
        $scope.ignoreLoadingItemsInFirstLoad = false;
        $scope.usingTrNgGrid = true;
        $scope.model.pageSize = 1000000; // Not paging
        $scope.model.paging = false;
        //$scope.cachePageSizeKey = 'expiry-warning-page-size-cache-key';
        self.readCachedValues();
        $scope.option_filter = {
            type: "0",
            provider: "",
            group_drug: "",
            chi_lay_hang_het: true
        };
    }
    this.onInitializeFinished = function () {
        this.requestRemoteUrl("/DrugManagement/GetListProvider", null, function (data) {
            if (data.Data != null) {
                $scope.provider = data.Data;
            }
        });
        this.requestRemoteUrl("/DrugManagement/GetListGroupDrug", null, function (data) {
            if (data.Data != null) {
                $scope.group_drug = data.Data;
            }
        });
        // Init grid
        this.initGrid(false, false, false, false, false,
            self.loadData, {
                remoteUrl: '/DrugManagement/InitCreateReserve',
                extraParams: {
                    type: 0,
                    provider: 0,
                    group_drug: 0,
                    name_drug: "",
                    get_drug_empty: true
                },
                bindingFunc: self.bindDataToGrid
            });
    }
    this.standardizeReciveData = this.standardizeReciveData || function (response) {
        if (response && response.Data) {
            var pagingModel = response.Data.PagingResultModel;
            $scope.model.totalItems = pagingModel.TotalSize;
            $scope.model.items = pagingModel.Results;
            $scope.model.Total = response.Data.Total;
        } else {
            app.notice.error('Lỗi khi truy cập dữ liệu máy chủ.');
        }
    }
    $scope.fnAddItemReserve = function () {
        switch($scope.option_filter.type){
            case "1":{
                if ($scope.option_filter.provider == "") {
                    app.notice.error('Chọn 1 nhà cung cấp.');
                    return;
                }
            }
                break;
            case "2":{
                if ($scope.option_filter.group_drug == "") {
                    app.notice.error('Chọn 1 nhóm sản phẩm.');
                    return;
                }
            }
                break;
            case "3":{
                if ($scope.option_filter.name_drugs == "") {
                    app.notice.error('Nhập tên sản phẩm.');
                    return;
                }
            }
                break;
        }
        self.loadData("/DrugManagement/InitCreateReserve", {
            type: parseInt("0" + $scope.option_filter.type),
            provider: parseInt("0" + $scope.option_filter.provider),
            group_drug: parseInt("0" + $scope.option_filter.group_drug),
            name_drug: $scope.option_filter.name_drugs,
            get_drug_empty: $scope.option_filter.chi_lay_hang_het
        }, function (response) {
            var a = self;
            if (response && response.Data) {
                var pagingModel = response.Data.PagingResultModel;
                var rows_table = $scope.model.items;
                var num_itemAdd = 0;
                angular.forEach(pagingModel.Results, function (rowItem, idex) {
                    if (rows_table.findIndex(function (ele) {
                        if (ele.MaThuoc == pagingModel.Results[idex].MaThuoc)
                        return true;
                    }) < 0) {
                        rowItem.STT = rows_table.length + 1;
                        rows_table.push(rowItem);
                        num_itemAdd++;
                    }
                });
                app.notice.message('Đã thêm ' + num_itemAdd + " thuốc vào danh sách.");
            }
        });
    }
    $scope.fnRemoveDrug = function () {
        var index = this.$index;
        $scope.model.items.splice(index, 1);
        $scope.model.totalItems = $scope.model.totalItems - 1;
        $scope.model.Total = $scope.model.Total - 1;
        angular.forEach($scope.model.items, function (rowItem, idex) {
            rowItem.STT = ++idex;
        });
    }
    $scope.fnDeleteAllItem = function () {
        var index = this.$index;
        $scope.model.items = [];
        $scope.model.totalItems = 0;
        $scope.model.Total = 0;
    }
    $scope.fnPrint = function (fileType) {
        var url;
        if (fileType == "pdf") {
            url = "/DrugManagement/PrintReserve";
        }
        else{
            url = "/DrugManagement/ExportExcelReserve"
        }
        self.loadData(url, {
            inputItem: $scope.model.items
        }, function (response) {
            if (response != null && response != "") {
                if (response.indexOf("pdf") >= 0) {
                    var anchor = angular.element('<a/>');
                    anchor.attr({
                        href: response,
                        target: '_blank'
                    })[0].click();
                }
                else {
                    var anchor = angular.element('<a/>');
                    anchor.attr({
                        href: response,
                        target: '_blank',
                        download: "dutru.xlsx"
                    })[0].click();
                }
            }
        });
    }
    $injector.invoke(BaseController, this, {
        $scope: $scope,
        $rootScope: $rootScope,
        $injector: $injector
    });
}