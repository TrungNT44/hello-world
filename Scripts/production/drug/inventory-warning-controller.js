app.instance.controller("InventoryWarningController", InventoryWarningController);
InventoryWarningController.$inject = ['$scope', '$rootScope', '$injector'];
function InventoryWarningController($scope, $rootScope, $injector) {
    var self = this;
    this.initAtFirstRunning = this.initAtFirstRunning || function () {
        $scope.ignoreLoadingItemsInFirstLoad = false;
        $scope.usingTrNgGrid = true;
        $scope.model.pageSize = 100000; // Not paging
        $scope.model.paging = false;
        //$scope.cachePageSizeKey = 'expiry-warning-page-size-cache-key';
        self.readCachedValues();
        $scope.option_filter = {
            type: "0",
            provider: "",
            group_drug: "",
            chi_lay_hang_het: false
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
                remoteUrl: '/DrugManagement/InitInventoryWarning',
                extraParams: {
                    type: 0,
                    provider: 0,
                    group_drug: 0,
                    name_drug: "",
                    get_drug_empty: false
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

            angular.forEach($scope.model.items, function (rowItem, idex) {
                rowItem.STT = ++idex;
            });

        } else {
            app.notice.error('Lỗi khi truy cập dữ liệu máy chủ.');
        }
    }
    $scope.searchData = function () {
        switch ($scope.option_filter.type) {
            case "1": {
                if ($scope.option_filter.provider == "") {
                    app.notice.error('Chọn 1 nhà cung cấp.');
                    return;
                }
            }
                break;
            case "2": {
                if ($scope.option_filter.group_drug == "") {
                    app.notice.error('Chọn 1 nhóm sản phẩm.');
                    return;
                }
            }
                break;
            case "3": {
                if ($scope.option_filter.name_drugs == "") {
                    app.notice.error('Nhập tên sản phẩm.');
                    return;
                }
            }
                break;
        }
        self.loadData("/DrugManagement/InitInventoryWarning", {
            type: parseInt("0" + $scope.option_filter.type),
            provider: parseInt("0" + $scope.option_filter.provider),
            group_drug: parseInt("0" + $scope.option_filter.group_drug),
            name_drug: $scope.option_filter.name_drugs,
            get_drug_empty: $scope.option_filter.chi_lay_hang_het
        }
        , function (response) {
            var a = self;
            if (response && response.Data) {
                var pagingModel = response.Data.PagingResultModel;
                $scope.model.totalItems = pagingModel.TotalSize;
                $scope.model.items = pagingModel.Results;
                $scope.model.Total = response.Data.Total;

                angular.forEach($scope.model.items, function (rowItem, idex) {
                    rowItem.STT = ++idex;
                });

            } else {
                app.notice.error('Lỗi khi truy cập dữ liệu máy chủ.');
            }
        });
    }
    
    $scope.fnPrint = function (fileType) {
        var url;
        if (fileType == "pdf") {
            url = "/DrugManagement/PrintInventoryWarning";
        }
        else {
            url = "/DrugManagement/ExportExcelInventoryWarning";
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
                        download: "danhsachhanghet.xlsx"
                    })[0].click();
                }
            }
        });
    }
    $scope.fnDuTru = function () {
        var anchor = angular.element('<a/>');
        anchor.attr({
            href: "/DrugManagement/CreateReserve",
            target: '_blank'
        })[0].click();        
    }
    $injector.invoke(BaseController, this, {
        $scope: $scope,
        $rootScope: $rootScope,
        $injector: $injector
    });
}