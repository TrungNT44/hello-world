app.instance.controller("NearExpiredDrugWarningController", NearExpiredDrugWarningController);
NearExpiredDrugWarningController.$inject = ['$scope', '$rootScope', '$injector', '$window'];
function NearExpiredDrugWarningController($scope, $rootScope, $injector, $window) {
    var self = this;
    this.initAtFirstRunning = this.initAtFirstRunning || function () {
        $scope.ignoreLoadingItemsInFirstLoad = true;
        $scope.usingTrNgGrid = true;
        $scope.model.paging = false;
        $scope.cachePageSizeKey = 'near-expired-drug-page-size-cache-key';
        $scope.filterType = FILTER_DATE_RANGE;
        $scope.drugIds = [];
        $scope.selectedItemId = 0;
        $scope.groupFilterType = REPORT_FILTER_TYPE_ALL;
        $scope.filterItemType = ITEM_FILTER_TYPE_BY_DRUG_GROUP;
        $scope.currentItem = null;
        $scope.selectedExpiredOption = 1;
        $scope.expiredOptions = [{ key: 0, value: 'Tất cả hạn dùng' }, { key: 1, value: 'Hàng sắp hết hạn' }];        

        self.readCachedValues();
    }

    this.validateInputParams = this.validateInputParams || function () {
        return true;
    }

    this.initCacheDictionary = this.initCacheDictionary || function () {
    }

    this.onInitializeFinished = this.onInitializeFinished || function () {
    }

    this.standardizeReciveData = this.standardizeReciveData || function (response) {
        if (response && response.Data) {
            var pagingModel = response.Data.PagingResultModel;
            $scope.model.totalItems = pagingModel.TotalSize;
            $scope.model.items = pagingModel.Results;          
        } else {
            self.onGetDataFailed(response);
        }
    }

    this.onGetDataSuccess = function (response) {
        self.standardizeReciveData(response);
    }

    this.onGetDataFailed = function (response) {
        app.notice.error('Không thực hiện được tác vụ.');
    }

    this.fetchData = function () {
        var externalParams = {};
       
        var itemIds = [];
        var filterItemType = $scope.filterItemType
        if ($scope.groupFilterType == REPORT_FILTER_TYPE_BY_NAME) {
            filterItemType = ITEM_FILTER_TYPE_BY_DRUG;
            itemIds = $scope.drugIds;
        } else if ($scope.groupFilterType == REPORT_FILTER_TYPE_BY_GROUP) {
            itemIds.push($scope.selectedItemId);
            filterItemType = ITEM_FILTER_TYPE_BY_DRUG_GROUP;
        }
      
        var externalParams = {
            itemIds: itemIds, groupFilterType: $scope.groupFilterType,
            filterItemType: filterItemType,
            expiredOption: $scope.selectedExpiredOption
        };
       
        self.requestRemoteUrl('/Utilities/GetNearExpiredDrugWarningData',
            externalParams, self.onGetDataSuccess, self.onGetDataFailed);
    }

    $scope.onReceiptItemLink = function (noteItem) {
        var itemLink = '';
        if (noteItem.NoteId > 0) {
            itemLink = String.format('/PhieuNhaps/Details/{0}', noteItem.NoteId);
            $window.open(itemLink, '_blank');
        }
        else {
            itemLink = String.format('/Thuocs/DialogDetail/{0}', noteItem.ItemId);
            var dialogThuocDetails = {};
            $.get(itemLink).done(function (data) {
                if (data) {
                    dialogThuocDetails[noteItem.ItemId] = $(data).hide().appendTo(document.body).on('hide.bs.modal', function () { $('.modal-backdrop').remove(); }
                    ).modal();
                }
            });
        }

        return itemLink;
    }

    $scope.getItemBackground = function(item) {
        var itemBgClass = '';
        if (item.IsExpired)
        {
            itemBgClass = 'row-item-warning';
        }

        return itemBgClass;
    }

    $scope.onDisplayReportData = function (callbackParams) {
        self.fetchData();
    }

    $scope.updateDrug = function (item) {
        var drug = {
            SoPhieuNhap: item.ItemNumber,
            NgayPhieuNhap: item.ItemDate,
            drugCode: item.ItemCode,
            drugName: item.ItemName,
            drugBatch: item.SerialNumber,
            drugDate: item.ExpiredDate != null ? moment(item.ExpiredDate).toDate() : null,
            noteItemId: item.NoteItemId
        };
        $scope.currentItem = item;
        self.prepareDataForDialog('update-drug-expried-date-dialog', { drug: drug }, self.afterUpdated);
    }

    this.afterUpdated = function (data) {
        if ($scope.currentItem == null) return;
   
        $scope.currentItem.ExpiredDate = moment(data.drugDate).format("YYYY-MM-DD") + "T00:00:00Z";
        $scope.currentItem.SerialNumber = data.drugBatch;
    }

    $injector.invoke(BaseController, this, {
        $scope: $scope,
        $rootScope: $rootScope,
        $injector: $injector
    });

    // Init grid
    this.initGrid(false, false, false, false, false,
        self.fetchData, null);
};
