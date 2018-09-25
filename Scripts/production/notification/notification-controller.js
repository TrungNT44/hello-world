app.instance.controller("NotificationController", NotificationController);
NotificationController.$inject = ['$scope', '$rootScope', '$injector'];
function NotificationController($scope, $rootScope, $injector) {
    var self = this;
    this.initAtFirstRunning = this.initAtFirstRunning || function () {
        $scope.ignoreLoadingItemsInFirstLoad = false;
        self.readCachedValues();
        $scope.Notification = {
            NotificationTypeID: "2"
        };
        $scope.StoreDrugFilter = {};
        $scope.DrugStoreCode = "";
        $scope.taskComplete = 0;
        $scope.action = action;
        if (action == "view")
            $scope.Title = "Chi tiết thông báo";
        if (action == "create")
            $scope.Title = "Tạo thông báo";
        if (action == "update")
            $scope.Title = "Cập nhật thông báo";
    }
    this.onInitializeFinished = function () {
        self.LoadDataForDDL_DrugStores();
        self.LoadDataForDDL_NotificationType();
    }
    $scope.$watch("taskComplete", function (newVal, oldVal) {
        if (newVal == 2 && notificationId) {
            self.fnLoadNotificationInfo();
        }
    }, true);
    this.LoadDataForDDL_DrugStores = function (defautValue) {
        self.loadData('/Recruitment/GetListDrugStores',
            null, function (response) {
                $scope.taskComplete++;
                if (response) {
                    var arrDrugStore = response;
                    arrDrugStore.unshift({
                        MaNhaThuoc: "",
                        TenNhaThuoc: "--Tất cả nhà thuốc--"
                    });
                    $scope.StoreDrug = arrDrugStore;
                    $scope.StoreDrugFilter = { selected: arrDrugStore[0] };
                }
                else {
                    $scope.StoreDrug = [];
                }
            }, null);
    }
    this.LoadDataForDDL_NotificationType = function (defautValue) {
        self.loadData("/Notification/GetListNotificationType", null, function (response) {
            $scope.taskComplete++;
            $scope.ArrNotificationType = response;
        });
    }
    this.fnLoadNotificationInfo = function () {
        self.loadData("/Notification/GetNotificationInfo", { id: notificationId }, function (response) {
            $scope.StoreDrug.forEach(function (el) {
                if (el.MaNhaThuoc === response.DrugStoreID)
                    $scope.StoreDrugFilter.selected = el;
            });
            $scope.Notification = response;
            $scope.Notification.NotificationTypeID = response.NotificationTypeID.toString();
        }, null);
    }
    $scope.Save = function (FormVali) {
        $scope.submited = true;
        if (FormVali) {
            var oDataPost = {
                ID: $scope.Notification.ID,
                DrugStoreID: $scope.StoreDrugFilter.selected.MaNhaThuoc,
                NotificationTypeID: $scope.Notification.NotificationTypeID,
                Title: $scope.Notification.Title,
                Link: $scope.Notification.Link
            };
            self.loadData("/Notification/SaveNotification", oDataPost, function (response) {
                if (response === "OK") {
                    app.notice.message('Lưu thành công');
                    //window.location.href = "/Notification/List";
                }
                else
                    app.notice.error('Không lưu được thông báo');
            });
        }
    }
    $injector.invoke(BaseController, this, {
        $scope: $scope,
        $rootScope: $rootScope,
        $injector: $injector
    });
}