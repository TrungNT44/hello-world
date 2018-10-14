app.instance.controller("EditInventoryController", EditInventoryController);
EditInventoryController.$inject = ['$scope', '$rootScope', '$injector', '$filter', '$window'];
function EditInventoryController($scope, $rootScope, $injector, $filter, $window) {
    var self = this;
    this.initAtFirstRunning = this.initAtFirstRunning || function () {
        // Đây là file angular controller của các View ( Inventory/Create.cshtml, Inventory/Edit.cshtml)
        $scope.InventoryModel = {};

        // nhan gia tri tu ViewBag.viewModel
        if (typeof viewModel != "undefined") {
            $scope.InventoryModel = viewModel;
        }
        $scope.InventoryModel.items = [];

        if ($scope.InventoryModel.MedicineList instanceof Array) {
            $scope.InventoryModel.items = $scope.InventoryModel.MedicineList;
        }

        // nhan gia tri tu ViewBag.NhomThuoc
        if (typeof nhomThuoc != "undefined") {
            $scope.nhomThuoc = nhomThuoc;
        }
        // currentEditingItem - thuốc đang edit
        $scope.currentEditingItem = null;
        // reportDate - ngày lập phiếu
        $scope.reportDate = moment(new Date()).format(DEFAULT_MOMENT_DATE_FORMAT);
        if (typeof $scope.InventoryModel.CreateTime != "undefined") {
            $scope.reportDate = moment($scope.InventoryModel.CreateTime).format(DEFAULT_MOMENT_DATE_FORMAT);
        }

        // model.selectedNhomThuoc - nhóm thuốc lựa chọn để thêm vào phiếu
        $scope.selectedNhomThuoc = null;

        // tên nhóm thuốc chọn để thêm vào phiếu
        $scope.selectedMaNhomThuoc = "";

        // lấy danh sách Id thuốc gửi từ màn hình Thuốc chưa kiểm kê (DrugNotInventoried.cshtml) thông qua sessionStorage
        if (sessionStorage.getItem('NotInventoriedDrugIds')) {
            var drugId = JSON.parse(sessionStorage.getItem('NotInventoriedDrugIds'));
            self.getInventoryDrugItem('', drugId, $scope.reportDate);
        }
        sessionStorage.removeItem('NotInventoriedDrugIds');
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

        // truyền oData đến webservice /Inventory/SaveInventory
        var oData = {};
        oData.Id = $scope.InventoryModel.Id;
        oData.MedicineList = medicineList;
        oData.DaCanKho = cankho;
        oData.CreateTime = $scope.InventoryModel.CreateTime ? $scope.InventoryModel.CreateTime : $scope.reportDate;
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
        var maNhomThuoc = $scope.selectedNhomThuoc;
        $scope.selectedMaNhomThuoc = maNhomThuoc;
        if (maNhomThuoc == null || maNhomThuoc == "") return;
        $scope.selectedNhomThuoc = null;
        var ngayTao = $scope.reportDate;
        self.getInventoryDrugItem(maNhomThuoc, [], ngayTao);
    }

    // lấy thông tin thuốc từ server
    this.getInventoryDrugItem = function (maNhomThuoc, drugId, ngayTao, barcode) {
        self.requestRemoteUrl('/Inventory/GetDrugInfo',
            { ignoreLoadingIndicator: true, maNhomThuoc: maNhomThuoc, drugIds: drugId, ngayTao: ngayTao, barcode: barcode }, self.onGetInventoryDrugItemSuccess, self.onGetInventoryDrugItemFailed);
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
            var maNhomThuoc = $scope.selectedMaNhomThuoc;
            if (maNhomThuoc != "" && currentDrugsCount < $scope.InventoryModel.items.length) {
                angular.forEach($scope.nhomThuoc, function (nhomThuoc) {
                    if (nhomThuoc.MaNhomThuoc == maNhomThuoc) {
                        app.notice.message("Đã thêm thành công nhóm thuốc " + nhomThuoc.TenNhomThuoc);
                    }
                });
            }
            $scope.selectedMaNhomThuoc = "";
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
        if (typeof $scope.InventoryModel.CreateTime == "undefined") {
            $scope.reportDate = moment(dateEvent.date).format(DEFAULT_MOMENT_DATE_FORMAT);
        }
    };

    // mở dialog để cập nhật giá/lô/hạn dùng của thuốc
    $scope.updateDrug = function (item) {
        var inventoryId = -1;
        var daCanKho = false;
        if (typeof $scope.InventoryModel.Id != "undefined") {
            inventoryId = $scope.InventoryModel.Id;
        }
        if (typeof $scope.InventoryModel.DaCanKho != "undefined") {
            daCanKho = $scope.InventoryModel.DaCanKho;
        }
        var drug = {
            InventoryId: inventoryId,
            DaCanKho: daCanKho,
            MaThuoc: item.MaThuoc,
            TenThuoc: item.TenThuoc,
            Donvi: item.TenDonViTinh,
            Gia: item.Gia,
            SoLo: item.SoLo,
            HanDung: item.HanDung != null ? moment(item.HanDung).toDate() : null,
            onlyUpdateDrugAtClient: true
        };
        $scope.currentEditingItem = item;
        self.prepareDataForDialog('update-inventory-item-dialog', { drug: drug }, self.afterUpdated);
    }

    this.afterUpdated = function (data) {
        if ($scope.currentEditingItem == null) return;
        $scope.currentEditingItem.Gia = data.Gia;
        $scope.currentEditingItem.SoLo = data.SoLo;
        $scope.currentEditingItem.HanDung = data.HanDung;
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

    // show confirm dialog khi ấn nút Cân kho phiếu
    $scope.confirmCankho = function () {
        if ($scope.InventoryModel.items.length <= 0) {
            app.notice.error('Không có thuốc nào trong danh sách!');
            return;
        }
        app.notice.confirm('Bạn có muốn cân kho phiếu kiểm kê này không?', function (result) {
            if (result) {
                $scope.saveInventory(true);
            }
        });
    };
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