app.instance.controller("EditInventoryController", EditInventoryController);
EditInventoryController.$inject = ['$scope', '$rootScope', '$injector', '$filter', '$window'];
function EditInventoryController($scope, $rootScope, $injector, $filter, $window) {
    var self = this;
    this.initAtFirstRunning = this.initAtFirstRunning || function () {
        // Đây là file angular controller của các View ( Inventory/Create.cshtml, Inventory/Edit.cshtml, Inventory/DrugsNotInventoried.cshtml )
        $scope.InventoryModel = {};
        if (typeof viewModel != "undefined") {
            $scope.InventoryModel = viewModel;
        }
        // model.items - danh sách thuốc

        $scope.InventoryModel.items = [];
        if (typeof notInventoriedDrugs != "undefined" && notInventoriedDrugs != null) {
            $scope.InventoryModel.items = notInventoriedDrugs;
        }

        if ($scope.InventoryModel.MedicineList instanceof Array) {
            $scope.InventoryModel.items = $scope.InventoryModel.MedicineList;
        }
        if (typeof nhomThuoc != "undefined") {
            $scope.InventoryModel.nhomThuoc = nhomThuoc;
        }
        // currentEditingItem - thuốc đang edit
        $scope.currentEditingItem = null;
        // reportDate - ngày lập phiếu
        $scope.reportDate = moment(new Date()).format(DEFAULT_MOMENT_DATE_FORMAT);
        if ($scope.InventoryModel.CreateTime != "undefined") {
            $scope.reportDate = moment($scope.InventoryModel.CreateTime).format(DEFAULT_MOMENT_DATE_FORMAT);
        }

        // model.ItemPerPage - quy định số thuốc hiển thị trên 1 trang danh sách thuốc
        $scope.InventoryModel.ItemPerPage = 10;
        // model.selectedNhomThuoc - nhóm thuốc lựa chọn để thêm vào phiếu
        $scope.InventoryModel.selectedNhomThuoc = null;

        // tên nhóm thuốc chọn để thêm vào phiếu
        $scope.InventoryModel.maNhomThuoc = "";

        if (sessionStorage.getItem('NotInventoridDrugIds')) {
            var drugId = JSON.parse(sessionStorage.getItem('NotInventoridDrugIds'));
            console.log(drugId);
            self.getInventoryDrugItem('', drugId, $scope.reportDate);
        }
        sessionStorage.removeItem('NotInventoridDrugIds');
    }

    // Nếu lưu OK, chuyển đến trang chi tiết
    this.onSaveInventorySuccess = function (response) {
        app.notice.message("Phiếu đã được lưu.");
        var rootUrl = app.utils.getRootUrl();
        var detailUrl = String.format('{0}/Inventory/Details/{1}',
            rootUrl, response.Data);

        window.location.href = detailUrl;
    }

    // lưu phiếu Kiểm kê đã tạo/edit
    $scope.saveInventory = function (cankho) {
        var medicineList = $scope.InventoryModel.items;
        if (medicineList.length <= 0) {
            app.notice.error('Không có thuốc nào trong danh sách!');
            return;
        }

        if (cankho == true) {
            var r = confirm("Đồng ý cân kho phiếu Kiểm kê này?");
            if (r == false) {
                return;
            }
        }

        // truyền oData đến webservice /Inventory/SaveInventory
        var oData = {};
        oData.Id = $scope.InventoryModel.Id;
        oData.MedicineList = medicineList;
        oData.IsCompareStore = cankho;
        oData.CreateTime = $scope.InventoryModel.CreateTime ? $scope.InventoryModel.CreateTime : $scope.reportDate;
        console.log(oData.CreateTime);
        self.requestRemoteUrl('/Inventory/SaveInventory',
            { model: oData }, self.onSaveInventorySuccess, null);
    }

    // xóa thuốc khỏi danh sách hiện tại
    $scope.onDelete = function (item) {
        var index = $scope.InventoryModel.items.indexOf(item);
        if (index >= 0) {
            $scope.InventoryModel.items.splice(index, 1);
        }
    }

    // xử lý khi user tìm và chọn 1 thuốc mới
    $scope.onDrugSelectChanged = function (drugIds) {
        if (drugIds == null || drugIds.length < 1) return;
        var isExists = false;
        var drugId = drugIds[0];
        var ngayTao = $scope.reportDate;
        self.getInventoryDrugItem('', drugId, ngayTao);

    }

    // xử lý khi user chọn thêm 1 nhóm thuốc vào phiếu
    $scope.AddThuocTheoNhom = function () {
        // neu ma nhom thuoc nhap vao khac null thi goi service lay thong tin thuoc
        var maNhomThuoc = $scope.InventoryModel.selectedNhomThuoc;
        $scope.InventoryModel.maNhomThuoc = maNhomThuoc;
        if (maNhomThuoc == null || maNhomThuoc == "") return;
        $scope.InventoryModel.selectedNhomThuoc = null;
        var ngayTao = $scope.reportDate;
        self.getInventoryDrugItem(maNhomThuoc, [], ngayTao);
    }

    // lấy thông tin thuốc từ server
    this.getInventoryDrugItem = function (maNhomThuoc, drugId, ngayTao, inventoryId, barcode) {
        self.requestRemoteUrl('/Inventory/GetDrugInfo',
            { ignoreLoadingIndicator: true, maNhomThuoc: maNhomThuoc, drugIds: drugId, ngayTao: ngayTao, inventoryId: inventoryId, barcode: barcode }, self.onGetInventoryDrugItemSuccess, self.onGetInventoryDrugItemFailed);
    }

    // Nếu lấy thông tin thuốc OK, cập nhật danh sách thuốc đang hiển thị
    this.onGetInventoryDrugItemSuccess = function (response) {
        if (response && response.Data && response.Status == 200) {
            var drugItems = response.Data;
            if (drugItems == null || drugItems.length <= 0) {
                app.notice.error('Không lấy được dữ liệu.');
                return;
            }
            self.updateInventoryDrugItem(drugItems, true);
        } else {
            self.onGetInventoryDrugItemFailed(response);
        }
    }

    // cập nhật danh sách thuốc đang hiển thị
    this.updateInventoryDrugItem = function (drugItems, newMode) {
        if (newMode) {
            // TH drugItems là 1 mảng các thuốc, duyệt mảng này để thêm thuốc vào danh sách
            //if (drugItems instanceof Array) {
            var thuocDaKiemKes = [];
            var phieuDaKiemKes = [];
            var thuocDaTonTais = [];
            var currentDrugsCount = $scope.InventoryModel.items.length;
            angular.forEach(drugItems, function (newDrugItem) {
                // isExistsInCurrentList để kiểm tra thuốc đã có trong danh sách hiện tại chưa
                newDrugItem.isExistsInCurrentList = false;
                if (currentDrugsCount > 0) {
                    angular.forEach($scope.InventoryModel.items, function (thuoc) {
                        if (thuoc.ThuocId === newDrugItem.ThuocId) {
                            newDrugItem.isExistsInCurrentList = true;
                            $scope.pushNotExistItemToArray(thuocDaTonTais, newDrugItem.MaThuoc);
                        }

                    });
                }

                // Những thuốc không có trong danh sách hiện tại
                if (newDrugItem.isExistsInCurrentList == false) {
                    // nếu thuốc có trong phiếu Kiểm kê khác thì alert
                    if (newDrugItem.MaPhieuKiemKeTonTai > 0) {
                        $scope.pushNotExistItemToArray(thuocDaKiemKes, newDrugItem.MaThuoc);
                        $scope.pushNotExistItemToArray(phieuDaKiemKes, newDrugItem.MaPhieuKiemKeTonTai);
                    }
                    // Thêm thuốc
                    else {
                        $scope.InventoryModel.items.push(newDrugItem);
                    }
                }
            });

            // alert danh sách thuốc đã nằm trong phiếu kiểm kê hiện tại
            if (thuocDaTonTais.length > 0) {
                var alertDetail = String.format("Mã thuốc {0} đã tồn tại trong danh sách", thuocDaTonTais);
                alert(alertDetail);
            }

            // alert danh sách thuốc đang có trong phiếu kiểm kê khác mà chưa cân kho
            if (thuocDaKiemKes.length > 0 && phieuDaKiemKes.length > 0) {
                var alertDetail = String.format("Mã thuốc {0} đã tồn tại trong phiếu {1}", thuocDaKiemKes, phieuDaKiemKes);
                alert(alertDetail);
            }
            

            // thông báo khi thêm được thuốc trong nhóm thuốc đã chọn vào danh sách hiện tại
            var maNhomThuoc = $scope.InventoryModel.maNhomThuoc;
            if (maNhomThuoc != "" && currentDrugsCount < $scope.InventoryModel.items.length) {
                angular.forEach($scope.InventoryModel.nhomThuoc, function (nhomThuoc) {
                    if (nhomThuoc.MaNhomThuoc == maNhomThuoc) {
                        app.notice.message("Đã thêm thành công nhóm thuốc " + nhomThuoc.TenNhomThuoc);
                    }
                });
                $scope.InventoryModel.maNhomThuoc = "";

            }
            //}
            //else {
            //    $scope.InventoryModel.items.push(drugItems);
            //}
        }

    }

    this.onGetInventoryDrugItemFailed = function (response) {
        app.notice.error('Không lấy được dữ liệu.');
    }

    $injector.invoke(BaseController, this, {
        $scope: $scope,
        $rootScope: $rootScope,
        $injector: $injector
    });

    $(document.body).on('click', '.daterange-picker', function (e) {
        var reportDatePicker = $(e.currentTarget).find('input');
        reportDatePicker.datepicker('show');
    });

    // khi user thay đổi ngày tạo/edit phiếu
    var onSelectDateChanged = function (dateEvent) {
        console.log($scope.reportDate);
        if (typeof $scope.InventoryModel.CreateTime == "undefined") {
            $scope.reportDate = moment(dateEvent.date).format(DEFAULT_MOMENT_DATE_FORMAT);
        }
        console.log($scope.reportDate);
    };

    

    // mở dialog để cập nhật giá/lô/hạn dùng của thuốc
    $scope.updateDrug = function (item) {
        var inventoryId = -1;
        var isCompareStore = false;
        if (typeof $scope.InventoryModel.Id != "undefined") {
            inventoryId = $scope.InventoryModel.Id;
        }
        if (typeof $scope.InventoryModel.IsCompareStore != "undefined") {
            isCompareStore = $scope.InventoryModel.IsCompareStore;
        }
        //console.log(item.HanDungDateString);
        var drug = {
            InventoryId: inventoryId,
            IsCompareStore: isCompareStore,
            MaThuoc: item.MaThuoc,
            TenThuoc: item.TenThuoc,
            Gia: item.Gia,
            SoLo: item.SoLo,
            HanDung: item.HanDung != null ? moment(item.HanDung).format(DEFAULT_MOMENT_DATE_FORMAT) : null
        };
        $scope.currentEditingItem = item;
        self.prepareDataForDialog('update-inventory-item-dialog', { drug: drug }, self.afterUpdated);
    }

    this.afterUpdated = function (data) {
        if ($scope.currentEditingItem == null) return;
        $scope.currentEditingItem.Gia = data.Gia;
        $scope.currentEditingItem.SoLo = data.SoLo;
        
        $scope.currentEditingItem.HanDung = data.HanDung;
        $scope.currentEditingItem.HanDungDateString = data.HanDung.toString();
        console.log(typeof $scope.currentEditingItem.HanDung);
        console.log(typeof $scope.currentEditingItem.HanDungDateString);
        console.log($scope.currentEditingItem.HanDung);
        console.log($scope.currentEditingItem.HanDungDateString);
    }

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
            if (event.keyCode == 13) {
                var barcode = $("#barcode").val();
                if (barcode != null && barcode.length > 0) {
                    var ngayTao = $scope.reportDate;
                    self.getInventoryDrugItem(-1, [], ngayTao, barcode);
                }
                //else {
                //    $("#save-note-btn-id").click();
                //}
                $("#barcode").val('');
            }
        });
        $("#barcode").focus();
    });

    $scope.moveToNextDrug = function ($event) {
        if ($event.which == 13) {
            angular.element($event.currentTarget).closest("tr").next().find('.row-item-quantiy').focus();
        }
    }

    $scope.pushNotExistItemToArray = function (currentArray, item) {
        if (currentArray.indexOf(item) == -1) {
            currentArray.push(item);
        }
    }

};

app.instance.filter("DrugQuantityFilter", ['numberFilter', '$locale',
    function (number, $locale) {

        var formats = $locale.NUMBER_FORMATS;

        return function (input, fractionSize) {
            //Get formatted value
            if (!input) return 0;
            var formattedValue = number(input, fractionSize);

            //get the decimalSepPosition
            var decimalIdx = formattedValue.indexOf(formats.DECIMAL_SEP);

            //If no decimal just return
            if (decimalIdx == -1) return formattedValue;


            var whole = formattedValue.substring(0, decimalIdx);
            var decimal = (Number(formattedValue.substring(decimalIdx)) || "").toString();

            return whole + decimal.substring(1);
        };
    }
]);