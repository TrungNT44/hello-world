app.instance.controller("ListNotificationController", ListNotificationController);
ListNotificationController.$inject = ['$scope', '$rootScope', '$injector'];
function ListNotificationController($scope, $rootScope, $injector) {
    var self = this;
    this.initAtFirstRunning = this.initAtFirstRunning || function () {
        $scope.ignoreLoadingItemsInFirstLoad = false;
        self.readCachedValues();
        $scope.StoreDrugFilter = {};
        $scope.Notification = {};
        $scope.DrugStoreCode = "";
        $scope.action = action;
        $scope.isAdmin = isAdmin;
        $scope.taskComplete = 0;
        $scope.pageSizeList = [{
            pageSizeKey: 10,
            pageSizeValue: 10
        },
        {
            pageSizeKey: 20,
            pageSizeValue: 20
        },
        {
            pageSizeKey: 50,
            pageSizeValue: 50
        }];
    }
    this.onInitializeFinished = function () {
        self.fnLoadDataForDDL_DrugStores();
        self.fnLoadDataForDDL_NotificationType();
    }
    this.fnLoadDataForDDL_DrugStores = function () {
        self.loadData('/Recruitment/GetListDrugStores',
            null, function (response) {
                if (response) {
                    var arrDrugStore = response;
                    if (action == "list")
                        arrDrugStore.unshift({
                            MaNhaThuoc: "",
                            TenNhaThuoc: "--Tất cả nhà thuốc--"
                        });
                    $scope.StoreDrug = arrDrugStore;
                    if (currentDrugStoreCode) {
                        //Nếu tồn tại mã nhà thuốc
                        $scope.StoreDrug.forEach(function (el) {
                            if (el.MaNhaThuoc === currentDrugStoreCode)
                                $scope.StoreDrugFilter.selected = el;
                        });
                    }
                    else {
                        $scope.StoreDrugFilter = { selected: arrDrugStore[0] };
                    }
                }
                else {
                    $scope.StoreDrug = [];
                }
                $scope.taskComplete++;
            }, null);
    }
    this.fnLoadDataForDDL_NotificationType = function (defautValue) {
        self.loadData("/Notification/GetListNotificationType", null, function (response) {
            if (response) {
                response.unshift({
                    ID: "",
                    Name: "--Tất cả--"
                });
            }
            $scope.ArrNotificationType = response;
            $scope.taskComplete++;
        });
    };
    $scope.fnSubmitSearchHistory = function ($event) {
        if ($event.which == 13) {
            self.fnGetNotificationHistory();
        }
    };
    $scope.fnGetNotificationHistory = this.fnGetNotificationHistory = function () {
        var notification = {};
        notification.DrugStoreID = $scope.StoreDrugFilter.selected ? $scope.StoreDrugFilter.selected.MaNhaThuoc : "";
        notification.NotificationTypeID = $scope.Notification.NotificationTypeID;
        notification.Title = $scope.Notification.Title;
        self.loadData('/Notification/GetNotificationHistory',
            notification, function (response) {
                if (response && response.results) {
                    $scope.model.totalItems = response.totalSize;
                    $scope.model.items = response.results;
                }
            }, function () {
                app.notice.error('Không lấy được dữ liệu.');
            });
    };
    $scope.fnSubmitSearch = function ($event) {
        if ($event.which == 13) {
            self.fnFind();
        }
    };
    $scope.fnFind = this.fnFind = function () {
        var notification = $scope.Notification;
        notification.DrugStoreID = $scope.StoreDrugFilter.selected ? $scope.StoreDrugFilter.selected.MaNhaThuoc : "";
        self.loadData('/Notification/SearchNotification',
            notification, function (response) {
                if (response && response.results) {
                    $scope.model.totalItems = response.totalSize;
                    $scope.model.items = response.results;
                }
            }, function () {
                app.notice.error('Không lấy được dữ liệu.');
            });
    };
    $scope.fnDelete = function (id) {
        app.notice.confirm('Bạn có chắc chắn xóa?', function (result) {
            if (result) {
                self.loadData("/Notification/DeleteNotification", { id: id }, function (response) {
                    if (response === "OK") {
                        app.notice.message('Xóa thành công');
                        self.fnFind();
                    }
                    else
                        app.notice.error('Không thể xóa thông báo');
                });
            }
        });
    }
    $scope.fnEvict = function (id) {
        app.notice.confirm('Bạn có chắc chắn thu hồi thông báo này?', function (result) {
            if (result) {
                self.loadData("/Notification/EvictNotification", { id: id }, function (response) {
                    if (response === "OK") {
                        app.notice.message('Thu hồi thành công');
                        self.fnFind();
                    }
                    else
                        app.notice.error('Không thể thu hồi thông báo');
                });
            }
        });
    }
    $scope.fnRelease = function (id) {
        self.loadData("/Notification/ReleaseNotification", { id: id }, function (response) {
            if (response === "OK") {
                app.notice.message('Phát hành thành công');
                self.fnFind();
            }
            else
                app.notice.error('Không thể phát hành thông báo');
        });
    }
    $scope.$watch("taskComplete", function (newVal, oldVal) {
        if (newVal == 2) {
            if (action == "history") {
                self.initGrid(false, false, false, false, false,
                    self.fnGetNotificationHistory, null);
                self.fnGetNotificationHistory();
            }
            else {
                self.initGrid(false, false, false, false, false,
                    self.fnFind, null);
                self.fnFind();
            }
        }
    }, true);
    $injector.invoke(BaseController, this, {
        $scope: $scope,
        $rootScope: $rootScope,
        $injector: $injector
    });
}