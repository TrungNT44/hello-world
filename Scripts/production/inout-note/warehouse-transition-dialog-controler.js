app.instance.controller("WarehouseTransitionDialogController", WarehouseTransitionDialogController);
WarehouseTransitionDialogController.$inject = ['$scope', '$rootScope', '$injector', '$filter'];
function WarehouseTransitionDialogController($scope, $rootScope, $injector, $filter) {
    var self = this;
    this.initAtFirstRunning = this.initAtFirstRunning || function () {
        $scope.drugStores = [];
        $scope.selectedDrugStore = null;    
    }     

    this.validateInputParams = this.validateInputParams || function () {
        return true;
    }

    this.initCacheDictionary = this.initCacheDictionary || function () {
    }

    this.onInitializeFinished = this.onInitializeFinished || function () {
        
    }

    this.loadAndBindDataToView = this.loadAndBindDataToView || function () {
        $scope.drugStores = $scope.externalParams.drugStores;
        $scope.noteId = $scope.externalParams.noteId;
        if ($scope.drugStores != null && $scope.drugStores.length > 0) {
            $scope.selectedDrugStore = $scope.drugStores[0]; 
        }

        self.showDialog();
    }
    $scope.onDrugStoreItemChanged = function (item) {       
            $scope.selectedDrugStore = item;      
    }

    this.onTransitWarehouseSuccess = function (response) {
        $("#warehouse-transition-dialog").modal("hide");
        if ($scope.dialogCallbackFunc != null) {
            $scope.dialogCallbackFunc();
        }   
    }    

    $scope.transit = function () {
        self.requestRemoteUrl('/InOutCommingNote/TransitWarehouse',
            { targetDrugStoreCode: $scope.selectedDrugStore.DrugStoreCode,  deliveryNoteId: $scope.noteId }, self.onTransitWarehouseSuccess, null);
    }   

    $injector.invoke(BaseController, this, {
        $scope: $scope,
        $rootScope: $rootScope,
        $injector: $injector
    });
};
