app.instance.controller("InOutCommingNoteController", InOutCommingNoteController);
InOutCommingNoteController.$inject = ['$scope', '$rootScope', '$injector', '$filter', '$window'];
function InOutCommingNoteController($scope, $rootScope, $injector, $filter, $window) {
    var self = this;
    this.initAtFirstRunning = this.initAtFirstRunning || function () {
        $scope.selectedItemId = 0;
        $scope.selectedItem = null;
        $scope.model = viewModel;
        $scope.model.ReceiverNoteIds = [];
        $scope.viewMode = viewModel.TaskMode == 0;
        $scope.createMode = viewModel.TaskMode == 1;
        $scope.editMode = viewModel.TaskMode == 2;
        $scope.deleteMode = viewModel.TaskMode == 3;
       
        $scope.model.DebtNotes = [];
        //$scope.model.DebtNotes.push({ NoteId: -1, NoteInfo: '--Phiếu trống--' });
        $scope.model.DebtNotes.push({ NoteId: 0, NoteInfo: '--Tất cả--' });
        $scope.selectedNoteId = $scope.model.ReceiverNoteId;
        $scope.paymentAmountEditable = true;
        $scope.selectedNote = $scope.model.DebtNotes[0]; 
        $scope.noteDate = moment(viewModel.NoteDate).format(DEFAULT_MOMENT_DATE_FORMAT);
        $scope.filterItemTypeId = ITEM_FILTER_TYPE_BY_CUSTOMER;
        if ($scope.model.NoteTypeId == 2) {
            $scope.filterItemTypeId = ITEM_FILTER_TYPE_BY_SUPPLYER;
        }
        $scope.title = "Phiếu Thu";
        if (viewModel.NoteTypeId == 2) {
            $scope.title = "Phiếu Chi";
        }
        $window.document.title = $scope.title;
    }

    this.setDefaultValues = function () 
    {
        if ($scope.model == null) return;

        $scope.selectedItemId = $scope.model.ReceiverId;
        $scope.selectedNoteId = $scope.model.ReceiverNoteId;
        if ($scope.selectedItemId > 0) {
            self.fetchDebtInfo($scope.selectedItemId, $scope.model.NoteTypeId, $scope.model.NoteId);
        }
        // $scope.$broadcast("selectedItemChangedEvent", $scope.selectedItemId);
    }    

    this.validateInputParams = this.validateInputParams || function () {
        return true;
    }

    this.initCacheDictionary = this.initCacheDictionary || function () {
    }

    this.onInitializeFinished = this.onInitializeFinished || function () {
        
    }

    this.onGetDebtInfoSuccess = function (response) {
        var debtInfo = response.Data;
        $scope.model.DebtAmount = debtInfo.DebtAmount;
        $scope.model.DebtNotes = debtInfo.DebtNotes;
        if ($scope.selectedNoteId > 0) {
            var items = $filter('filter')($scope.model.DebtNotes, { NoteId: $scope.selectedNoteId }, true);
            if (items != null && items.length > 0) {
                $scope.selectedNote = items[0];
            }
        }
        else
        {
            $scope.selectedNote = $scope.model.DebtNotes[0];
        }
        if ($scope.createMode) {
            $scope.model.PaymentAmount = 0;
        }
        
        $scope.onNoteItemChanged($scope.selectedNote);
    }

    $scope.getReceiverTitle = function () {
        var title = "Khách hàng";
        if ($scope.model.NoteTypeId == 2) {
            title = "Nhà cung cấp";
        }

        return title;
    }

    $scope.getReceiverNoteTitle = function () {
        var title = "Phiếu xuất";
        if ($scope.model.NoteTypeId == 2) {
            title = "Phiếu nhập";
        }

        return title;
    }

    this.fetchDebtInfo = function (receiverId, noteTypeId, inOutComingNoteId) {
        self.requestRemoteUrl('/InOutCommingNote/GetReceiverDebtInfo',
            { receiverId: receiverId, noteTypeId: noteTypeId, inOutComingNoteId: inOutComingNoteId }, self.onGetDebtInfoSuccess, null);
    }

    $scope.onNoteItemChanged = function (item)
    {
        $scope.model.DebtAmount = item.DebtAmount;
        $scope.paymentAmountEditable = item.NoteId != 0; 
        $scope.model.DebtNote = item;
        if ($scope.createMode) {
            $scope.onPayFull();
        }
        
        $scope.model.ReceiverNoteId = item.NoteId;
    }

    $scope.onReceiverItemChanged = function (item)
    {
        $scope.model.ReceiverId = item.ItemId;
        self.fetchDebtInfo(item.ItemId, $scope.model.NoteTypeId, $scope.model.NoteId);
    }

    $scope.isNormalReceiver = function ()
    {
        return $scope.selectedItem != null && $scope.selectedItem.ItemTypeId == 0;
    }

    $scope.onPayFull = function () {
        $scope.model.PaymentAmount = $scope.model.DebtAmount;
    }

    this.getReceiverNoteIds = function ()
    {
        // Empty note
        if ($scope.selectedNote == null || $scope.selectedNote.NoteId == -1) return null;
       
        var noteIds = [];
        if ($scope.selectedNote.NoteId == 0) {   // All notes          
            angular.forEach($scope.model.DebtNotes, function (item) {
                if (item.NoteId > 0) {
                    noteIds.push(item.NoteId);
                }                
            });            
        }
        else // Specific note
        {
            noteIds.push($scope.selectedItem.NoteId);
        }

        return noteIds;
    }

    this.onSaveInOutCommingNoteSuccess = function (response) {
        app.notice.message("Phiếu đã được lưu.");       
        var rootUrl = app.utils.getRootUrl();
        var detailUrl = String.format('{0}/InOutCommingNote/InOutcommingNoteScreen?noteId={1}&noteTypeId={2}&taskMode=0',
            rootUrl, response.Data, $scope.model.NoteTypeId);
        if ($scope.editMode == true) {
            detailUrl = String.format('{0}/PhieuThuChis?loaiPhieu={1}', rootUrl, $scope.model.NoteTypeId);         
        }
        window.location.href = detailUrl;
    }

    $scope.onSave = function () {
        $scope.model.ReceiverNoteIds = self.getReceiverNoteIds();
        var noteDate = moment($scope.noteDate, DEFAULT_MOMENT_DATE_FORMAT).format('YYYY-MM-DD');
        $scope.model.NoteDate = noteDate;

        self.requestRemoteUrl('/InOutCommingNote/SaveInOutCommingNote',
            { model: $scope.model}, self.onSaveInOutCommingNoteSuccess, null);
    } 

    this.onDeleteInOutCommingNoteSuccess = function (response) {
        app.notice.message("Phiếu đã được xóa.");
        var rootUrl = app.utils.getRootUrl();
        var detailUrl = String.format('{0}/PhieuThuChis?loaiPhieu={1}', rootUrl, $scope.model.NoteTypeId);
      
        window.location.href = detailUrl;
    }

    $scope.onDelete = function () {        
        self.requestRemoteUrl('/InOutCommingNote/DeleteInOutCommingNote',
            { noteId: $scope.model.NoteId }, self.onDeleteInOutCommingNoteSuccess, null);
    }     

    $injector.invoke(BaseController, this, {
        $scope: $scope,
        $rootScope: $rootScope,
        $injector: $injector
    });

    $(document.body).on('click', '.daterange-picker', function (e) {
        var datePicker = $(e.currentTarget).find('input');
        datePicker.datepicker('show');
    });

    var onSelectDateChanged = function (dateEvent) {
        var selectedDate = moment(dateEvent.date).format(DEFAULT_MOMENT_DATE_FORMAT);
        $scope.noteDate = selectedDate;
    };

    $('#note-date-id').datepicker({
        format: DEFAULT_DATE_PICKER_FORMAT,
        changeMonth: true,
        changeYear: true,
        endDate: 0,
        maxViewMode: 2,
        defaultDate: new Date(),
        minDate: MIN_PRODUCTION_DATA_DATE,
        language: 'vi',
        autoclose: true
    }).on('changeDate', onSelectDateChanged);

    $(document).ready(function () {
        self.setDefaultValues();
    });
};
