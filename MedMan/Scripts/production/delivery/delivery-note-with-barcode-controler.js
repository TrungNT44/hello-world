app.instance.controller("DeliveryNoteWithBarcodeController", DeliveryNoteWithBarcodeController);
DeliveryNoteWithBarcodeController.$inject = ['$scope', '$rootScope', '$injector', '$filter'];
function DeliveryNoteWithBarcodeController($scope, $rootScope, $injector, $filter) {    
    var self = this;
    this.initAtFirstRunning = this.initAtFirstRunning || function () {
        $scope.drugIds = [];
        $scope.model.items = [];
        $scope.model.TotalAmount = 0.0;
        $scope.reportDate = moment(new Date()).format(DEFAULT_MOMENT_DATE_FORMAT);
        $scope.selectedCustomerItemId = -1;
        $scope.selectedDoctorItemId = -1;
        $scope.model.Description = '';
        $scope.model.PaymentAmount = 0;
        $scope.currentEditingItem = null;
        $scope.model.NoteNumber = viewModel.NoteNumber;
        $scope.allowToChangeTotalAmount = viewModel.AllowToChangeTotalAmount;
        $scope.debtLabel = 'Còn nợ';
        $scope.doseNumber = 1.0;
        $scope.saving = false;

        //$scope.$watch('doseNumber', function (newValue, oldValue) {
        //    if (newValue === undefined || oldValue === undefined || Math.abs(newValue - oldValue) < 0.1 || newValue < 1 ){
        //        $scope.doseNumber = 1.0;
        //        return;
        //    }            
        //});
    }

    this.validateInputParams = this.validateInputParams || function () {
        if ($scope.model.items.length < 1) {
            app.notice.error('Phiếu bán chưa có dữ liệu.');            
            return false;
        }

        return true;
    }

    this.getItemByDrug = function (drugId)
    {
        var items = self.getAllItemsByDrug(drugId);
        if (items == null || items.length < 1) return null;

        return items[0];
    }

    this.getAllItemsByDrug = function (drugId) {
        var items = $filter('filter')($scope.model.items, { DrugId: drugId }, true);
        return items;
    }

    this.getTotalAmountFromCurrentDeliveryItems = function ()
    {
        var amount = 0.0;
        angular.forEach($scope.model.items, function (item) {
            amount += item.TotalAmount;
        });

        return amount;
    }

    this.updateTotalAmount = function ()
    {
        var amount = self.getTotalAmountFromCurrentDeliveryItems();

        $scope.model.TotalAmount = amount;
        $scope.model.PaymentAmount = amount;
        self.focusToBarcode();
    }

    this.focusToBarcode = function ()
    {
        $("#barcode").focus();
        fromBCScanner = false;
    }

    this.updateTotalAmount4Item = function (item, updatePrice) {
        if (item == null || item.DrugUnits == null || item.DrugUnits.length < 1) return;
        var selectedUnit = $filter('filter')(item.DrugUnits, { UnitId: item.SelectedUnitId }, true)[0];
        if (selectedUnit == null) return;
        //if (updatePrice) {           
        //    if (selectedUnit.UnitId != item.RetailUnitId) {
        //        item.Price = item.Factors * item.Price;
        //    }
        //    else {
        //        item.Price = item.Price / item.Factors;
        //    }
        //}

        item.TotalAmount = item.Price * item.Quantity;
    }

    this.shallCombineDrugItems = function (firstItem, secondItem)
    {
        if (firstItem == null || secondItem == null) return false;

        if (firstItem.SelectedUnitId == secondItem.SelectedUnitId && Math.abs(firstItem.Price - secondItem.Price) < 0.1) return true;

        return false;
    }    

    this.alertIfOverInventoryQuantity = function (drugItem, existingDrugItems) {
        // var existingDrugItems = self.getAllItemsByDrug(drugItem.DrugId);
        var totalQuantities = 0;
        angular.forEach(existingDrugItems, function (item) {
            if (item.SelectedUnitId != item.RetailUnitId) {
                totalQuantities += item.Factors * item.Quantity;
            }
            else {
                totalQuantities += item.Quantity;
            }            
        });
        var remainQuantity = drugItem.LastInventoryQuantity - totalQuantities;
        
        if (remainQuantity <= 0) {
            app.notice.error("Số lượng xuất ra vượt quá số lượng tồn kho hiện tại (" + drugItem.LastInventoryQuantity + " đơn vị)");
            drugItem.IsOverQuantity = true;
        }
        else
        {
            drugItem.IsOverQuantity = false;
        }
        
        angular.forEach(existingDrugItems, function (item) {
            item.LastInventoryQuantity = drugItem.LastInventoryQuantity;
            if (item.SelectedUnitId != item.RetailUnitId) {
                item.RemainQuantity = item.LastInventoryQuantity / item.Factors;
            }
            else
            {
                item.RemainQuantity = drugItem.LastInventoryQuantity;
            }
        });
    }

    this.onDoseChanged = function () {        
        angular.forEach($scope.model.items, function (item) {
            self.updateQuantityByDose(item, true);
            self.updateTotalAmount4Item(item, false);            
        });
        self.updateTotalAmount();
        $scope.safeApply();
    }

    this.updateQuantityByDose = function (drugItem, updateDose) {
        if (updateDose) {
            drugItem.Quantity = $scope.doseNumber * drugItem.QuantityPerOneDose;
        }
        else {
            drugItem.QuantityPerOneDose = drugItem.Quantity;
            if ($scope.doseNumber > 1.0) {
                drugItem.QuantityPerOneDose = drugItem.Quantity / $scope.doseNumber;
            }
        }
    }

    this.updateDeliveryDrugItem = function (drugItem, newMode, updateDose) {
        if (newMode) {
            $scope.model.items.push(drugItem);
        }
        self.updateQuantityByDose(drugItem, updateDose);

        var existingDrugItems = self.getAllItemsByDrug(drugItem.DrugId);
        if (existingDrugItems == null || existingDrugItems.length < 1) return;
        self.alertIfOverInventoryQuantity(drugItem, existingDrugItems);

        angular.forEach(existingDrugItems, function (item) {
            if (item != drugItem && self.shallCombineDrugItems(item, drugItem)) {
                drugItem.Quantity += item.Quantity;
                var index = $scope.model.items.indexOf(item);
                if (index >= 0) {
                    $scope.model.items.splice(index, 1);
                }
            }
        });
       
        self.updateTotalAmount4Item(drugItem, !newMode);
        self.updateTotalAmount();
    }
   
    this.onGetDrugDeliveryItemSuccess = function (response) {
        if (response && response.Data && response.Status == HTTP_STATUS_CODE_OK) {
            var drugItem = response.Data;
            self.updateDeliveryDrugItem(drugItem, true);
        } else {
            self.onGetDrugDeliveryItemFailed(response);
        }
        $("#barcode").val("");
        self.focusToBarcode();
    }

    this.onGetDrugDeliveryItemFailed = function (response) {
        app.notice.error('Không lấy được dữ liệu.');
        $("#barcode").val("");
        self.focusToBarcode();
    }

    this.getDrugDeliveryItem = function (drugId, barcode) {
        self.requestRemoteUrl('/DeliveryNote/GetDrugDeliveryItem',
            { ignoreLoadingIndicator: true, drugId: drugId, barcode: barcode }, self.onGetDrugDeliveryItemSuccess, self.onGetDrugDeliveryItemFailed);
    }

    $scope.onDrugSelectChanged = function (drugIds) {
        if (drugIds == null || drugIds.length < 1) return;
        var drugId = drugIds[0];
        self.getDrugDeliveryItem(drugId, ''); 
        self.focusToBarcode();
    }

    $scope.onEdit = function (item)
    {
        item.EditMode = true;
        $scope.currentEditingItem = item;
        self.focusToQuantityEle(item);
    }

    $scope.onSave = function (item) {
        $scope.currentEditingItem = null;
        self.updateDeliveryDrugItem(item, false); 
        item.EditMode = false;
        $scope.safeApply();
    }

    $scope.onDelete = function (item) {
        var index = $scope.model.items.indexOf(item);
        if (index >= 0) {
            $scope.model.items.splice(index, 1);
        }
        self.updateTotalAmount();
    }

    $scope.getUnitName = function (item)
    {
        var unitName = '';
        if (item == null || item.DrugUnits == null || item.DrugUnits.length < 1) return unitName;
        var selectedUnit = $filter('filter')(item.DrugUnits, { UnitId: item.SelectedUnitId }, true)[0];
        if (selectedUnit == null) return unitName;

        unitName = selectedUnit.UnitName;

        return unitName;
    }

    $scope.getDebtAmount = function ()
    {
        var debtVal = $scope.model.TotalAmount - $scope.model.PaymentAmount;
        $scope.debtLabel = debtVal < 0 ? 'Tiền thừa' : 'Còn nợ';

        return Math.abs(debtVal);
    }

    $scope.onUnitChanged = function (item)
    {
        //self.updateTotalAmount4Item(item, true);       
        if (item.SelectedUnitId != item.RetailUnitId) {
            item.Price = item.Factors * item.Price;
            item.RemainQuantity = item.LastInventoryQuantity / item.Factors;
        }
        else {
            item.Price = item.Price / item.Factors;
            item.RemainQuantity = item.LastInventoryQuantity;
        }
        self.focusToQuantityEle(item);
    }

    $scope.onPayFull = function () {
         $scope.model.PaymentAmount = $scope.model.TotalAmount;
    }

    $scope.getItemBackground = function (item) {
        var itemBgClass = '';
        if (item.IsOverQuantity == true) {
            itemBgClass = 'row-item-warning';
        }

        return itemBgClass;
    }

    this.waitFocusToQuantityTextBox = function (qtyTdEle)
    {
        var qtyTextBox = qtyTdEle.find('input[type="text"]:visible')[0];
        if (qtyTextBox != null) {
            qtyTextBox.focus();
        }
        else
        {
            setTimeout(self.waitFocusToQuantityTextBox, 50, qtyTdEle); //wait 50 ms, then try again
        }
    }

    this.focusToQuantityEle = function (gridItem){
        var rowIndex = $scope.model.items.indexOf(gridItem);
        if (rowIndex < 0) return;
        
        try {
            //$('#table-id-delivery-note-with-barcode tbody tr').eq(0).find('.td-barcode input[type="text"]:visible')[0]
            var td = $('#table-id-delivery-note-with-barcode tbody tr').eq(rowIndex).find('.td-barcode');         
            if (td == null) return;
          
            self.waitFocusToQuantityTextBox(td);
        } catch (ex) {
            console.log(ex.message);
        }
    }

    this.onSaveDeliveryNoteSuccess = function (response) {      
        if (response && response.Data && response.Status == HTTP_STATUS_CODE_OK) {
            app.notice.message('Lưu phiếu thành công.');
            var rootUrl = app.utils.getRootUrl();
            var detailUrl = String.format('{0}/PhieuXuats/InDetails/{1}?BillType=Barcode', rootUrl, response.Data);
            window.location.href = detailUrl;
        } else {
            self.onSaveDeliveryNoteFailed(response);
        }
    }

    this.onSaveDeliveryNoteFailed = function (response) {
        $scope.saving = false;
        app.notice.error('Gặp lỗi trong quá trình lưu phiếu.');
    }

    $scope.saveDeliveryNote = function () {
        if (!self.validateInputParams() || $scope.saving) return;
        $scope.saving = true;
        var noteDate = moment($scope.reportDate, DEFAULT_MOMENT_DATE_FORMAT).format('YYYY-MM-DD');
        self.requestRemoteUrl('/DeliveryNote/SaveDeliveryNote',
            {
                deliveryItems: $scope.model.items, paymentAmount: $scope.model.PaymentAmount, noteNumber: $scope.model.NoteNumber,
                noteDate: noteDate, customerId: $scope.selectedCustomerItemId,
                doctorId: $scope.selectedDoctorItemId, description: $scope.model.Description
            }, self.onSaveDeliveryNoteSuccess, self.onSaveDeliveryNoteFailed);
    }

    $scope.canSaveDeliveryNote = function () {
        if ($scope.saving) return false;
        if ($scope.model.items == null || $scope.model.items.length < 1) return false;
        for (var i = 0; i < $scope.model.items.length; i++) {
            var item = $scope.model.items[i];
            if (item.EditMode) return false;
        }
    }

    this.updateNewPriceForItem = function (item, factors) {
        var newPrice = math.round(item.Price * factors, 0);       
        item.Price = newPrice;

        var newTotalAmount = math.round(item.Price * item.Quantity, 0);
        item.TotalAmount = newTotalAmount;

        return newTotalAmount;
    }

    $scope.onTotalAmountChanged = function ($event) {
        var keyCode = $event.which || $event.keyCode;
        if (keyCode != 13) return;       

        if (!$scope.allowToChangeTotalAmount || $scope.model.items == null || $scope.model.items.length < 1) return;
       
        var userTotalAmount = $scope.model.TotalAmount;
        if (userTotalAmount < 0.5) {
            app.notice.error('Nhập tổng tiền không không hợp lệ ( > 0.5).');
            return;
        }

        var totalAmount = self.getTotalAmountFromCurrentDeliveryItems();
        if (totalAmount < 0.5) {
            return;
        }

        var item = $scope.model.items[0];
        var factors = math.round(userTotalAmount / totalAmount, 1);
        var newTotalAmount = 0.0;
        var totalItems = $scope.model.items.length;

        for (var i = 0; i < totalItems - 1; i++) {
            var item = $scope.model.items[i];
            newTotalAmount += self.updateNewPriceForItem(item, factors);
        }

        var lastItemAmount = math.round(userTotalAmount - newTotalAmount, 0);        
        var lastItem = $scope.model.items[totalItems - 1];
        lastItem.Price = math.round(lastItemAmount / lastItem.Quantity, 1);
        lastItem.TotalAmount = lastItemAmount;

        $scope.onPayFull();

        $("#tbxPaymentAmount").focus();
    }   

    $injector.invoke(BaseController, this, {
        $scope: $scope,
        $rootScope: $rootScope,
        $injector: $injector
    });
    $(document.body).on('click', '.daterange-picker', function (e) {
        var datePicker = $(e.currentTarget).find('input');
        app.utils.setPickerSelectedDate(datePicker, $scope.reportDate);
        datePicker.datepicker('show');
    });

    var onSelectDateChanged = function (dateEvent) {
        var selectedDate = moment(dateEvent.date).format(DEFAULT_MOMENT_DATE_FORMAT);
        $scope.reportDate = selectedDate;
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
        $("#barcode").keyup(function (event) {
            if (event.keyCode == 13 && !fromBCScanner) {
                var barcode = $("#barcode").val();
                if (barcode != null && barcode.length > 0) {
                    self.getDrugDeliveryItem(0, barcode);
                }
                //else {
                //    $("#save-note-btn-id").click();
                //}                
            }
        });

        $("#doseNumber").keyup(function (event) {
            if (event.keyCode == 13 && !fromBCScanner) {
                self.onDoseChanged();                             
            }
        });
        
        Mousetrap.bind(['f9'], function (e) {
            $("#save-note-btn-id").click();
        });
        Mousetrap.bindGlobal('enter', function () {
            if ($scope.currentEditingItem != null && $scope.currentEditingItem.EditMode) {
                $scope.onSave($scope.currentEditingItem);
            }
        });
        $("#barcode").focus();
    }); 
    
    $(document).BCScanner(
    {
        timeBeforeScanTest: 200, // wait for the next character for upto 200ms
        startChar: [], // Prefix character for the cabled scanner (YHD-3100 - No prefix chars for YHD )
        endChar: [13,71], // be sure the scan is complete if key 13 (enter) and 71 (g) are detected
        avgTimeByChar: 40, // it's not a barcode if a character takes longer than 40ms
        onComplete: function(barcode, qty) // main callback function
        {
            fromBCScanner = true;
            $("#barcode").val(barcode);
            self.getDrugDeliveryItem(0, barcode);           
        }
        //,onKeyDetect: function(event){console.log(event.which); return false;}
        });

    //$(document).scannerDetection({
    //    timeBeforeScanTest: 200, // wait for the next character for upto 200ms
    //    endChar: [13], // be sure the scan is complete if key 13 (enter) is detected
    //    avgTimeByChar: 40, // it's not a barcode if a character takes longer than 40ms
    //    ignoreIfFocusOn: 'input', // turn off scanner detection if an input has focus
    //    onComplete: function (barcode, qty) { ... }, // main callback function
    //    scanButtonKeyCode: 116, // the hardware scan button acts as key 116 (F5)
    //    scanButtonLongPressThreshold: 5, // assume a long press if 5 or more events come in sequence
    //    onScanButtonLongPressed: showKeyPad, // callback for long pressing the scan button
    //    onError: function (string) { alert('Error ' + string); }
    //});
};