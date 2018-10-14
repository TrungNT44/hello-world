var cache = {};
var tempRow = {};
var currentTerm = "";
var tbl;
//Call all the helper to init
function InitHelper() {
    $('#MaKhachHang,#MaNhaCungCap,#MaBacSy').selectize();
    //InitDatePicker();
    InitAutocomplete();
    InitEdit();
    BindNumberOnly();
    BindSoLuong(false);
    CalculateSum();
    InitMoney();
    $('.edit-mode').hide();
    //Turn the first row to edit mode
    $("table#tblCt tr").eq(1).find(".edit-thuoc").click();

    //Hide options
    $("select[id*='MaDonViTinh']").each(function () {
        HideDonViTinh(this);
    });
    //change ngaytao
    BindNgayTaoPosition();

    //functional buttons
    $("#btnDelete").on("click", function () {
        $("#Xoa").val("True");
        $("#frmPhieuNhap").submit();
    });
    $("#btnRestore").on("click", function () {
        $("#Xoa").val("False");
        $("#frmPhieuNhap").submit();
    });
    $("#btnConfirm").on("click", function () {
        ButtonConfirmClick();
    });
    //checkBox change
    $("#ckVAT").change(function () {
        VATCheckBoxChange(this);
    });

    $($("table#tblCt tr").eq(1).find(".delete-thuoc")).hide();

    tbl = $('#tblCt').DataTable({
        "ordering": false,
        "info": false,
        "scrollY": 250,
        "scrollCollapse": true,
        "paging": false,
        "searching": false

    });
    $("span.pNumber").each(function () {
        $(this).html(numberWithCommas($(this).html()));
    });
    $("b.pNumber").each(function () {
        $(this).html(numberWithCommas($(this).html()));
    });
    $($("table")[0]).css("width", "100%");
    $(".dataTables_scrollHeadInner").css("width", "100%");
    // unmask input
    resizeElements();
}

function CreateInsertRow(maNhaThuoc, maPhieuNhap, slData) {
    var row = $("table#tblCt").find("tr:last");

    if (slData == "") {
        slData = row.find("select[id*='MaDonViTinh']").prop('outerHTML').replace("selected=\"selected\"", "").replace(/_0__/g, "_-1__").replace(/\[0\]/g, "[-1]");
    }
    var btnPrice = (loaiPhieu == '1' ?
        '</div></td>' +
        '<td >' +
        '<div class="input-group"><span data-inputmask="\'alias\': \'numeric\', \'groupSeparator\': \',\', \'autoGroup\': true, \'digits\': 0, \'digitsOptional\': false, \'suffix\': \'\'" class="display-mode pNumber" id="PhieuNhapChiTiets_-1__GiaBanSp"></span><input data-inputmask="\'alias\': \'numeric\', \'groupSeparator\': \',\', \'autoGroup\': true, \'digits\': 0, \'digitsOptional\': false, \'suffix\': \'\': \'0\'" class="form-control edit-mode text-box single-line  new-out-price-l pNumber" data-val="true" data-val-number="Đơn giá phải là số" data-val-required="Chưa nhập đơn giá" id="PhieuNhapChiTiets_-1__GiaBan" format = "number"name="PhieuNhapChiTiets[-1].GiaBan" type="text" value="0" style="display: none;">'+
        '<span class="input-group-btn" onclick="event.preventDefault();"><button class="btn" ng-click="updateInPrice($event); $event.preventDefault();"><a style="cursor: pointer;" id="luuGiaNhapId" title="Cập nhật giá nhập"><i class="glyphicon glyphicon-edit"></i></a></button></span></div>'
        :
        ''
    );
    var slbtn = (loaiPhieu == '1' ?
        '<span class="input-group-btn" onclick="event.preventDefault();"><button class="btn" ng-click="updateBatchExpiryDate($event); $event.preventDefault();"><a style="cursor: pointer;" id="luuHanId" title="Cập nhật lô/hạn dùng"><i class="glyphicon glyphicon-calendar"></i></a></button></span>'
        :
        ''
    );
    var tRow = '<tr style="display:none" id="trFirst">' +
        '<td style="padding: 10px 0">' +
        '<a class="edit-thuoc display-mode" style="cursor: pointer" id="0" title="Sửa"><i class="glyphicon glyphicon-pencil"></i></a> ' +
        '<a class="save-thuoc edit-mode text-success" style="cursor: pointer" id="0" title="Lưu"><i class="glyphicon glyphicon-ok"></i></a>&nbsp' +
        //'<a class="exit-thuoc edit-mode text-warning" style="cursor: pointer" id="0" title="Bỏ qua"><i class="glyphicon glyphicon-remove"></i></a>&nbsp' +
        '<a class="delete-thuoc edit-mode text-danger" style="cursor: pointer" id="0" title="Xóa"><i class="glyphicon glyphicon-trash"></i></a>&nbsp' +
        '<input data-val="true" data-val-number="The field MaPhieuNhapCt must be a number." data-val-required="The MaPhieuNhapCt field is required." id="PhieuNhapChiTiets_-1__MaPhieuNhapCt" name="PhieuNhapChiTiets[-1].MaPhieuNhapCt" type="hidden" value="0">' +
        '<input data-val="true" data-val-number="The field MaPhieuNhap must be a number." data-val-required="The MaPhieuNhap field is required." id="PhieuNhapChiTiets_-1__MaPhieuNhap" name="PhieuNhapChiTiets[-1].MaPhieuNhap" type="hidden" value="' + maPhieuNhap + '">' +
        '<input id="PhieuNhapChiTiets_-1__MaNhaThuoc" name="PhieuNhapChiTiets[-1].MaNhaThuoc" type="hidden" value="' + maNhaThuoc + '">' +
        '</td>' +
        '<td><span id="dSTT_-1__Sp"></span></td>' +
        '<td><span class="display-mode" id="PhieuNhapChiTiets_-1__MaThuocSp"></span><input class="form-control edit-mode thuocFinderByMa ui-autocomplete-input drug-code" type="text" id="PhieuNhapChiTiets_-1__MaThuoc" value="" autocomplete="off" style="display: none;"></td>' +
        '<td>' +
        '<span class="display-mode" style="display: inline;" id="PhieuNhapChiTiets_-1__TenSp"></span>' +
        '<input class="form-control edit-mode thuocFinder ui-autocomplete-input drug-name" type="text" id="PhieuNhapChiTiets_-1__TenThuoc" value="" autocomplete="off" style="display: none;">' +
        '<input id="PhieuNhapChiTiets_-1__ThuocId" name="PhieuNhapChiTiets[-1].ThuocId" type="hidden" value="">' +
        '<input id="PhieuNhapChiTiets_-1__MaThuoc" name="PhieuNhapChiTiets[-1].MaThuoc" type="hidden" value="">' +
        '</td>' +
        '<td align="right">' +
        '<span class="display-mode" style="display: inline;" id="PhieuNhapChiTiets_-1__MaDonViTinhSp"></span>' +
        '<input type="hidden" id="_-1__HaiDonViTinh" value="" />' +
        slData +
        '</td>' +
        '<td align="right">' +
        '<span data-inputmask="\'alias\': \'decimal\', \'groupSeparator\': \',\', \'autoGroup\': true" class="display-mode pNumber" id="PhieuNhapChiTiets_-1__SoLuongSp"></span>' +
        '<input data-inputmask="\'alias\': \'decimal\', \'groupSeparator\': \',\', \'autoGroup\': true" class="form-control edit-mode text-box single-line" data-val="true" data-val-number="The field SoLuong must be a number." data-val-required="The SoLuong field is required." id="PhieuNhapChiTiets_-1__SoLuong" name="PhieuNhapChiTiets[-1].SoLuong" type="text" value="0" style="display: none;">' +
        '</td>' +
        '<td align:right style="float: right">' +
        '<div class="input-group"><span data-inputmask="\'alias\': \'numeric\', \'groupSeparator\': \',\', \'autoGroup\': true, \'digits\': 0, \'digitsOptional\': false, \'suffix\': \'\'" class="display-mode pNumber" id="PhieuNhapChiTiets_-1__GiaNhapSp"></span><input data-inputmask="\'alias\': \'numeric\', \'groupSeparator\': \',\', \'autoGroup\': true, \'digits\': 0, \'digitsOptional\': false, \'suffix\': \'\': \'0\'" class="form-control edit-mode text-box single-line drug-price pNumber" data-val="true" data-val-number="Đơn giá phải là số" data-val-required="Chưa nhập đơn giá" id="PhieuNhapChiTiets_-1__GiaNhap" format = "number"name="PhieuNhapChiTiets[-1].GiaNhap" type="text" value="0" style="display: none;">' +
        
        btnPrice +
        '</td>' +
        '<td>' +
        '<span class="display-mode pNumber" id="PhieuNhapChiTiets_-1__ChietKhauSp"></span>' +
        '<input data-inputmask="\'alias\': \'decimal\', \'groupSeparator\': \',\', \'autoGroup\': true" class="form-control edit-mode text-box single-line" data-val="true" data-val-number="The field ChietKhau must be a number." data-val-required="The ChietKhau field is required." id="PhieuNhapChiTiets_-1__ChietKhau" name="PhieuNhapChiTiets[-1].ChietKhau" type="text" value="0" style="display: none;">' +
        '</td>' +
        '<td>' +
        '<input id="PhieuNhapChiTiets_-1__SoLo" name="PhieuNhapChiTiets[-1].SoLo" type="hidden" class="drug-batch">' +
        '<input id="PhieuNhapChiTiets_-1__HanDung" name="PhieuNhapChiTiets[-1].HanDung" type="hidden" class="drug-date">' +
        slbtn +
        '</td>' +
        '<td><input type="hidden" value="" id="PhieuNhapChiTiets_-1__HeSo" />' +
        '<input type="hidden" value="0" id="PhieuNhapChiTiets_-1__DVX"/>' +
        '<input type="hidden" value="0" id="PhieuNhapChiTiets_-1__DVTN"/>' +
        '<input type="hidden" value="0" id="PhieuNhapChiTiets_-1__BasePrice"/>' +
        '<input type="hidden" value="0" id="PhieuNhapChiTiets_-1__BasePriceOut"/>' +
        '<b name="miniSum" class="pNumber"></b></td>' +
        '</tr>';

    $("table#tblCt thead").append(tRow);

    //Compile it to bind it with angular
    AngularHelper.Compile($("#create-receipt-note"));

    //Set to empty row
    $("table#tblCt tr").eq(1).find("select").val([]);
}

function BindNumberOnly() {
    $("input[type='number']").off("keydown");
    $("input[type='number']").keydown(function (e) {
        // Allow: backspace, delete, tab, escape, enter and .
        if ($.inArray(e.keyCode, [46, 8, 9, 27, 13, 110, 190]) !== -1 ||
            // Allow: Ctrl+A
            (e.keyCode == 65 && e.ctrlKey === true) ||
            // Allow: home, end, left, right, down, up
            (e.keyCode >= 35 && e.keyCode <= 40)) {
            // let it happen, don't do anything
            return;
        }
        // Ensure that it is a number and stop the keypress
        if ((e.shiftKey || (e.keyCode < 48 || e.keyCode > 57)) && (e.keyCode < 96 || e.keyCode > 105)) {
            e.preventDefault();
        }
    });
}

function InitDatePicker() {
    $(".datefield").each(function () {
        BindDatePicker($(this));
    });
}

//Bind the datepicker to all rows
function BindDatePicker(elem) {
    elem.removeClass('hasDatepicker');
    elem.datepicker({
        format: DEFAULT_DATE_PICKER_FORMAT,
        changeMonth: true,
        changeYear: true,
        endDate: 0,
        maxViewMode: 2,
        defaultDate: new Date(),
        minDate: MIN_PRODUCTION_DATA_DATE,
        language: 'vi',
        autoclose: true
    });
}

//Autocomplete init
function InitAutocomplete() {
    $(".thuocFinder").each(function () {
        BindAutocomplete($(this), "tenThuoc");
    });
    $(".thuocFinderByMa").each(function () {
        BindAutocomplete($(this), "maThuoc");
    });


    BindAutocomplete($("#maThuoc"), "maThuoc");


}

//Autocomplete binder
function BindAutocomplete(elem, sName) {
    var type = $("#MaLoaiXuatNhap").val();
    var url = $('input[name=baseUrl]').val() + "Thuocs/GetThuocs";
    if (sName == "maThuoc") {
        url = $('input[name=baseUrl]').val() + "Thuocs/GetThuocsByMaOrTen";
    }
    elem.autocomplete({
        minLength: 2,
        source: function (request, response) {
            var term = request.term;
            currentTerm = sName + term;
            if (term in cache) {
                response(cache[sName + term]);
                return;
            }
            response(null);
            //get json : là hàm truy vấn
            $.getJSON(url, request, function (data, status, xhr) {
                cache[sName + term] = data;
                response(data);
            });
        },
        messages: {
            noResults: "Không tìm thấy"//,
            // results: ""
        },
        focus: function (event, ui) {
            $("#Thuocs").val(ui.item.label);
            return false;
        },
        select: function (event, ui) {
            $(this).val(ui.item.label);
            var tr = $("#trFirst");
            tr.find("input[name*='.ThuocId']").val(ui.item.value);
            tr.find("input[name*='.MaThuoc']").val(ui.item.maThuoc);
            tr.find('input.thuocFinderByMa ').val(ui.item.maThuoc);
            tr.find("input[id$='__MaThuoc']").val(ui.item.maThuoc);
            tr.find("input[id$='__TenThuoc']").val(ui.item.tenThuoc);
            tr.find("span[id$='__TenSp']").text(ui.item.tenThuoc);
            //tr.find("select[name*='.madonvitinh']").val(ui.item.unit1);            
            if ($("select#MaKhachHang").val() == undefined && ui.item.unit2 != "") {
                tr.find("input[id*='GiaNhap']").val(ui.item.price2);
                tr.find("input[id*='GiaBan']").val(ui.item.price1);
                tr.find("select[name*='.MaDonViTinh']").val(ui.item.unit2);
            } else {
                tr.find("input[id*='GiaNhap']").val(ui.item.price1);
                tr.find("select[name*='.MaDonViTinh']").val(ui.item.unit1);
            }

            GetThuocProperty(ui.item.value, tr, type);
            $('input[id*=-1__SoLuong]:first').val(1);
            InsertRow(tr);
            $("#maThuoc").val("");
            return false;
        }
    }).each(function () {
        $(this).autocomplete('instance')._renderItem = function (ul, item) {
            return $("<li>")
                .append("<a>" + item.desc + "</a>")
                .append('<input type="hidden" name="price" value="' + item.price + '"/>')
                .append('<input type="hidden" name="unit" value="' + item.unit + '"/>')
                .appendTo(ul);
        };
    });
}

var tData = {};

function GetThuocProperty(thuocId, tr, type) {
    var tDataList = cache[currentTerm];
    for (var i = 0; i < tDataList.length; i++) {
        if (tDataList[i].value == thuocId) {
            tData = tDataList[i];
        }
    }

    var options = tr.find("select[id*='MaDonViTinh'] option");
    tr.find("input[id*='DVX']").val(tData.unit1);
    tr.find("input[id*='DVTN']").val(tData.unit2);
    if (type == "3")
        tr.find("input[id*='BasePrice']").val(tData.price1);
    else {
        tr.find("input[id*='BasePrice']").val(tData.price2);
        tr.find("input[id*='BasePriceOut']").val(tData.price1);
    }
    HideSelectOpt(options, tData.unit1, tData.unit2);
    tr.find("input[id*='HeSo']").val(tData.heSo);
    var basePrice = tr.find("input[id*='BasePrice']").val();
    var basePriceOut = tr.find("input[id*='BasePriceOut']").val();
    //Bind gia nhap

    tr.find("select[id*='MaDonViTinh']").on("change", function () {
        if ($(this).val() == tr.find("input[id*='DVX']").val()) {
            tr.find("input[id*='GiaNhap']").val(parseInt(basePrice));
            tr.find("input[id*='GiaBan']").val(parseInt(basePriceOut));
        } else {
            tr.find("input[id*='GiaNhap']").val(basePrice * tData.heSo);
            tr.find("input[id*='GiaBan']").val(basePriceOut * tData.heSo);
        }
    }).trigger('change');
    //Heso
    tr.find("input[id*='HeSo']").val(tData.heSo);
}

$("#MaNhaCungCap").on("change", function () {
    if ($(".item").text() == "Hàng nhập lẻ") {
        RecalculateMoney(true);
    }
});

function GetThuocPropertyFromScan(tData, tr, type) {
    var options = tr.find("select[id*='MaDonViTinh'] option");
    tr.find("input[id*='DVX']").val(tData.unit1);
    tr.find("input[id*='DVTN']").val(tData.unit2);
    if (type == "3")
        tr.find("input[id*='BasePrice']").val(tData.price1);
    else {
        tr.find("input[id*='BasePrice']").val(tData.price2);
        tr.find("input[id*='BasePriceOut']").val(tData.price1);
    }
    HideSelectOpt(options, tData.unit1, tData.unit2);
    tr.find("input[id*='HeSo']").val(tData.heSo);
    var basePrice = tr.find("input[id*='BasePrice']").val();
    var basePriceOut = tr.find("input[id*='BasePriceOut']").val();
    //Bind gia nhap
    tr.find("select[id*='MaDonViTinh']").on("change", function () {
        if ($(this).val() == tr.find("input[id*='DVX']").val()) {
            tr.find("input[id*='GiaNhap']").val(parseInt(basePrice));
            tr.find("input[id*='GiaBan']").val(parseInt(basePriceOut));
        } else {
            tr.find("input[id*='GiaNhap']").val(basePrice * tData.heSo);
            tr.find("input[id*='GiaBan']").val(basePriceOut * tData.heSo);
        }
    }).trigger('change');
    //Heso
    tr.find("input[id*='HeSo']").val(tData.heSo);
}

$("#tblCt").on("change", "select[id*='MaDonViTinh']", function () {
    var tr = $(this).closest("tr");
    var heso = tr.find("input[id*='HeSo']").val();
    var price = tr.find("input[id*='GiaNhap']").val();
    var priceOut = tr.find("input[id*='GiaBan']").val();
    if ($(this).val() != tr.find("input[id*='DVX']").val()) {
        tr.find("input[id*='GiaNhap']").val(parseInt(price * heso));
        tr.find("input[id*='GiaBan']").val(parseInt(priceOut * heso));
    }
    else {
        tr.find("input[id*='GiaNhap']").val(parseInt(price) / heso);
        tr.find("input[id*='GiaBan']").val(parseInt(priceOut) / heso);
    }

    tr.find("input[id*='MaDonViTinh']").val($(this).val());

    RecalculateSum(tr);
});

$("#tblCt").on("keyup", "input[id*='SoLuong']", function (event) {
    if (event.keyCode == 13) {
        var tr = $(this).closest("tr");
        SaveRow(tr);
        tr.find('.edit-mode, .display-mode').toggle();
        $("#maThuoc").focus();
    }
});

$("#tblCt").on("keyup", "input[id*='GiaNhap']", function (event) {
    if (event.keyCode == 13) {
        var tr = $(this).closest("tr");
        SaveRow(tr);
        tr.find('.edit-mode, .display-mode').toggle();
        $("#maThuoc").focus();
    }
});
$("#tblCt").on("keyup", "input[id*='GiaBan']", function (event) {
    if (event.keyCode == 13) {
        var tr = $(this).closest("tr");
        SaveRow(tr);
        tr.find('.edit-mode, .display-mode').toggle();
        $("#maThuoc").focus();
    }
});
$("#tblCt").on("keyup", "input[id*='ChietKhau']", function (event) {
    if (event.keyCode == 13) {
        var tr = $(this).closest("tr");
        SaveRow(tr);
        tr.find('.edit-mode, .display-mode').toggle();
        $("#maThuoc").focus();
    }
});

//Hide the unecessary don vi thu nguyen
function HideSelectOpt(options, dvx, dvtn) {
    options.hide().attr('disabled', 'disabled');
    options.each(function () {
        if ($(this).val() == dvx || $(this).val() == dvtn) {
            $(this).show().removeAttr('disabled');
        }
    });
}

//Bind the Edit mechanism to all rows
function InitEdit() {
    //Edit row
    $('body').off('click', '.edit-thuoc');
    $('body').on('click', '.edit-thuoc', function () {
        var tr = $(this).parents('tr:first');
        //Save data to temp table
        if (tr.parent().get(0).tagName != "THEAD") {
            tr.find("input[type=text],[type=number],[type=hidden]").each(function () {
                tempRow[$(this).attr("id")] = $(this).val();
            });
            tr.find("select").each(function () {
                tempRow[$(this).attr("id")] = $(this).val();
            });
        }

        tr.find('.edit-mode, .display-mode').toggle();
        tr.find("input[id*=__SoLuong]").focus();
    });
    //Cancel Edit
    //$('body').off('click', '.exit-thuoc');
    //$('body').on('click', '.exit-thuoc', function () {
    //    var tr = $(this).parents('tr:first');
    //    if (tr.parent().get(0).tagName == "THEAD")
    //        ClearTr(tr);
    //    else {
    //        RestoreOriginalRow(tr);
    //        tr.find('.edit-mode, .display-mode').toggle();
    //    }

    //});
    //Save the row
    $('body').off('click', '.save-thuoc');
    $('body').off('keydown', '.lastInput');
    $('body').on('keydown', '.dataTables_scrollHeadInner tr:eq(1) td:gt(4) input', function (event) {
        if (event.keyCode == 13) {
            var tr = $(this).parents('tr:first');
            if (tr.parent().get(0).tagName == "THEAD") {
                if (tr.find("input[id*='ThuocId']").val() != "") {
                    InsertRow(tr);
                    //tr.find(':input').val('');
                }
            }

        }
    });
    $('body').on('click', '.save-thuoc', function () {
        var tr = $(this).parents('tr:first');
        //the template row
        if (tr.parent().get(0).tagName == "THEAD") {
            if (tr.find("input[id*='ThuocId']").val() != "") {
                InsertRow(tr);
            }

            //save a new row
        } else if ($(this).attr("id") == 0) {
            SaveRow(tr, true);
            tr.find('.edit-mode, .display-mode').toggle();
        } else {
            SaveRow(tr, false);
            tr.find('.edit-mode, .display-mode').toggle();
        }
    });
    //Delete the row
    $('body').off('click', '.delete-thuoc');
    $('body').on('click', '.delete-thuoc', function () {
        var tr = $(this).parents('tr:first');
        tr.find("input[id*='SoLuong']").val(0);
        $('#tblCt').DataTable().row(tr).remove().draw();
        $('#tblCt tbody tr').each(function (index, el) {
            $(this).find('input, select').each(function () {
                if ($(this).attr('name'))
                    if ($(this).attr('name').indexOf('[') > 0) {
                        var name = $(this).attr('name');
                        $(this).attr('name', name.replace(new RegExp("\\[.*\\]", 'gi'), '[' + index + ']'));
                    }
            });
        });
        //tr.remove();
        RecalculateSTT();
        RecalculateSum(tr);
        if ($(".item").text() == "Hàng nhập lẻ") {
            RecalculateMoney(true);
        }
    });
}

var clicked = false;
function fnCheckStatusTable() {
    if (clicked) {
        return false;
    }
    clicked = true;
    //if ($("#tblCt .delete-thuoc:visible").length > 0) {
    //    return false;
    //}
    return true;
}

//Insert a new row
function InsertRow(tr) {
    var newTr = {};
    var isValid = true;
    //validate form
    if (tr.find('input[id*=__MaThuoc]').val().trim() == "") {
        tr.find('input[id*=__MaThuoc]').addClass('input-validation-error');
        isValid = false;
    }
    if (tr.find('input[id*=__TenThuoc]').val().trim() == "") {
        tr.find('input[id*=__TenThuoc]').addClass('input-validation-error');
        isValid = false;
    }
    if (tr.find('input[id*=__MaDonViTinh]').val() <= 0) {
        tr.find('input[id*=__MaDonViTinh]').addClass('input-validation-error');
        isValid = false;
    }

    if (tr.find('input[id*=__SoLuong]').val() <= 0) {
        tr.find('input[id*=__SoLuong]').addClass('input-validation-error');
        isValid = false;
    }
    if (tr.find('input[id*=__GiaNhap]').val() < 0) {
        tr.find('input[id*=__GiaNhap]').addClass('input-validation-error');
        isValid = false;
    }
    if (tr.find('input[id*=__GiaBan]').val() < 0) {
        tr.find('input[id*=__GiaBan]').addClass('input-validation-error');
        isValid = false;
    }
    if (!isValid) {
        tr.find('input.input-validation-error:first').focus();
        return;
    }
    // Chi them khi chua co' ma hang trong bang
    //var maThuoc = tr.find('input[id*=__MaThuoc]').val().trim();       
    //if (jQuery('#tblCt').find('tbody input[value=' + maThuoc + ']').size()) {
    //    var soluong1 = 
    //    //return false;
    //}

    // them doan nay de tinh toan them row hoac update so luong

    var flag = false;
    var maThuoc = tr.find('input[id*=__MaThuoc]').val().trim();
    jQuery.each(jQuery('#tblCt').find('tbody input[value=' + maThuoc + ']'), function () {
        if ($(this).attr("type") == "text") {
            var thisTr = $(this).closest("tr");
            var oldqty = parseFloat(thisTr.find('input[id*=__SoLuong]').val());
            var oldUnit = thisTr.find('select[id*=__MaDonViTinh]').val();
            var oldDiscount = thisTr.find('input[id*=__ChietKhau]').val();
            var oldPrice = thisTr.find('input[id*=__GiaNhap]').val();

            var newqty = parseFloat(tr.find('input[id*=__SoLuong]').val());
            var newUnit = tr.find('select[id*=__MaDonViTinh]').val();
            var newDiscount = tr.find('input[id*=__ChietKhau]').val();
            var newPrice = tr.find('input[id*=__GiaNhap]').val();

            if (oldPrice == newPrice && oldUnit == newUnit && oldDiscount == newDiscount) {
                var total = (oldqty + newqty) * oldPrice;
                var actualTotal = total;

                if (oldDiscount != null) {
                    var actualTotal = total - oldDiscount * total / 100;
                }
                var quantity = round(oldqty + newqty, 2);
                var refNum = $.number(quantity, 2, '.', ',');
                if (Math.floor(quantity) == quantity) {
                    refNum = $.number(quantity, 0, '.', ',');
                }
                thisTr.find('input[id*=__SoLuong]').val(quantity);
                thisTr.find('span[id*=__SoLuong]').text(refNum);
                refNum = $.number(actualTotal, 0, '.', ',');
                thisTr.find('b[name=miniSum]').text(refNum);
                flag = true;
            }
        }
    });
    if (flag) {
        ClearTr(tr);
        $('.dataTables_scrollHeadInner tr:eq(1)  input[id*=_TenThuoc]').focus();
        return false;
    }

    //copy current value to attribute
    tr.find("input[type=text],[type=number],[type=hidden]").each(function () {
        $(this).attr("value", $(this).val());
    });
    var slVal = tr.find("select[id*='MaDonViTinh']").val();
    $(tr.find("select").find(".delete-thuoc")).show();
    //Clone the row
    newTr = tr.clone().prop('outerHTML');

    ClearTr(tr);
    var row = $("table#tblCt").find("tr:last");
    var currentNum = 0;
    if (row.find("td")[1] != undefined)
        currentNum = parseInt(row.find(".stt").text());
    if (isNaN(currentNum))
        currentNum = parseInt(row.find("td")[1].innerHTML);
    if (isNaN(currentNum))
        currentNum = 0;
    //Replace and insert the row
    newTr = newTr.replace(/_-1__/g, "_" + currentNum + "__").replace(/\[-1\]/g, "[" + currentNum + "]");

    //$(newTr).insertAfter($("table#tblCt tbody tr").eq(0));
    var addRow = [];
    $($(newTr).find("td")).each(function () {
        var col = $(this).html();
        addRow.push(col);
    });
    tbl.row.add(addRow).draw();
    //Compile it to bind it with angular
    AngularHelper.Compile($("#create-receipt-note"));

    var fTr = $("table#tblCt tr:last");

    fTr.find("select[id*='MaDonViTinh']").val(slVal);
    fTr.find(".delete-thuoc").show();

    InitEdit();
    RecalculateSTT();
    SaveRow(fTr, fTr.attr("id") == 0);
    fTr.find('.edit-mode, .display-mode').toggle();
    // dont allow to edit mathuoc, tenthuoc
    fTr.find('td:eq(2),td:eq(3)').children().removeClass('edit-mode display-mode');
    fTr.find('td:eq(4)').attr('align', 'right');
    if (loaiPhieu == 1) {
        fTr.find('td:eq(6),td:eq(7)').attr('align', 'right');
    } else {
        fTr.find('td:eq(6)').attr('align', 'right');
    }
    // enable input mask
    fTr.find(":input,span").inputmask();
    //tr.find("input[type=text]").eq(0).focus();
    $('.dataTables_scrollHeadInner tr:eq(1)  input[id*=_TenThuoc]').focus();

}

//Clear the template row
function ClearTr(tr) {
    tr.find("input[type=text]").val('');
    tr.find("input[id*='ThuocId']").val('');
    // tr.find("input[type=hidden]").val('');
    tr.find("input[id*=_SoLuong]").val(0);
    tr.find("input[id*=ChietKhau]").val(0);
    tr.find("input[id*=SoLo]").val('');
    tr.find("input[id*=HanDung]").val('');
    tr.find("select").val([]);
}

//Recalculate the Index
function RecalculateSTT() {
    var cnt = 0;
    $("table#tblCt tr").each(function () {
        if (cnt > 1 && $(this).find("td")[2]) {
            $(this).find("td")[1].innerHTML = cnt - 1;
        }
        cnt++;
    });
}

//Recalculate the Sum
function RecalculateSum(tr) {
    var tds = tr.find("td");
    //var heSo = 1;
    //if (tr.find("select[id*='MaDonViTinh'] option:selected").val() != tr.find("input[id*='DVX']").val())
    //    heSo = tr.find("input[id*='HeSo']").val();
    var miniTotal = 0;
    var refNum = $.number(miniTotal, 0, '.', ',');
    if (loaiPhieu != 3) {
        miniTotal = $(tds[5]).find("input").val() * $(tds[6]).find("input").val() * (100 - $(tds[8]).find("input").val()) / 100;
        refNum = $.number(miniTotal, 0, '.', ',');
        $(tds[10]).find("b").html(refNum);
    }
    else {
        miniTotal = $(tds[5]).find("input").val() * $(tds[6]).find("input").val() * (100 - $(tds[8]).find("input").val()) / 100;
        refNum = $.number(miniTotal, 0, '.', ',');
        $(tds[9]).find("b").html(refNum);
    }
    CalculateSum();
}

//Save a row
function SaveRow(tr, newRow) {
    tr.find("span").each(function () {
        var attId = $(this).attr("id");
        if (attId) {
            var id = attId.slice(0, -2);
            if (id.indexOf("DonViTinh") >= 0) {
                $(this).html(tr.find("select[id*='MaDonViTinh'] option:selected").text());
                return;
            }
            var control = tr.find("#" + id);

            if (id.toString().indexOf("SoLuong") > -1 || id.toString().indexOf("GiaNhap") > -1 || id.toString().indexOf("GiaBan") > -1) {
                $(this).html(numberWithCommas(control.val()));
            }
            else if (id.toString().indexOf("ChietKhau") > -1) {
                if (control.val() == "") {
                    $("#" + id).val("");
                }
                else {
                    $(this).html(control.val() + " %");
                }
            }
            else {
                $(this).html(control.val());
            }
        }
    });
    RecalculateSum(tr);
    resizeElements();
    
    if ($(".item").text() == "Hàng nhập lẻ") {
        RecalculateMoney(true);
    }

    return true;
}

function RestoreOriginalRow(tr) {
    tr.find("input[type=text],[type=number],[type=hidden]").each(function () {
        $(this).val(tempRow[$(this).attr("id")]);
    });
    tr.find("select").each(function () {
        $(this).val(tempRow[$(this).attr("id")]);
    });
}

// #region Money
//Calculate the Sum of the order
function CalculateSum() {
    var sumAll = 0;
    $('b[name="miniSum"]').each(function () {
        if ($(this).text() != "")
            sumAll += parseInt($(this).text().replace(/\,/g, ""));
        $(this).text(numberWithCommas($(this).text()));
        if ($(this).text() == '0')
            $(this).text('');
    });
    var refNum = $.number(sumAll, 0, '.', ',');
    $("#spanSumAll").text(refNum).trigger('change');
}

function RecalculateMoney(fullPay) {
    var sumAll = $("#spanSumAll").text().replace(/\,/g, "");
    var vat = parseInt($("#VAT").val());
    if (isNaN(vat))
        vat = 0;
    var tra = $("input[id*='DaTra']").val().replace(/\,/g, "");
    if (isNaN(tra))
        tra = 0;
    if (isNaN(sumAll))
        sumAll = 0;

    var sumFinal = 0;
    if (vat > 0)
        sumFinal = parseInt(sumAll) + Math.floor(parseInt(sumAll) * (parseInt(vat)) / 100);
    else {
        sumFinal = sumAll;
    }
    var refNum = $.number(sumFinal, 0, '.', ',');
    $("#TongTien").attr("value", sumFinal);
    if ($("#ckVAT").is(':checked') || vat > 0) {
        $("#spSumfinal").text("Tổng Cộng : " + refNum);
    }
    if (tra !== "" && tra >= 0) {
        var total = sumFinal - tra;
        refNum = $.number(Math.abs(total), 0, '.', ',');
        if (total < 0)
            $("#spDebt").text("Trả Lại : " + refNum);
        else {
            $("#spDebt").text("Còn Nợ : " + refNum);
        }
    }
    if (fullPay) {
        refNum = $.number(sumFinal, 0, '.', ',');
        $("input[id*='DaTra']").val(sumFinal);
        $("input[id*='DaTra']").text(refNum);
        $("input[id*='DaTra']").trigger("change");
    }
}

function InitMoney() {
    var sumFinal = 0;
    $("#spanSumAll").on('change', function () {
        //Do calculation and change value of other span2,span3 here
        RecalculateMoney(false);
    });

    $("#VAT").on('keyup', function () {
        //if (!isNaN($(this).val()))
        RecalculateMoney(false);
    });
    $("input[id*='DaTra']").on('change keyup', function () {
        RecalculateMoney(false);
    });
    $("#btnFull").on('click', function () {
        RecalculateMoney(true);
    });
}

$("#btnIn").on('click', function () {
    var id = $("#MaPhieuNhap").val();
    window.location.href = "/PhieuNhaps/In/" + id;
});

function BindSoLuong(type) {
    $("table#tblCt tr").each(function () {
        var tr = $(this);
        var soLuong = tr.find("input[id*='SoLuong']");
        var sl = soLuong.val();
        var ck = tr.find("input[id*='ChietKhau']").val();
        var giaNhap = tr.find("input[id*='GiaNhap']").val();
        tr.find("b[name='miniSum']").text(Math.round(giaNhap * sl * (100 - ck) / 100));
    });
    $("input#DaTra").val($("#DaTra").val().replace(/\,/g, ""));
}

function numberWithCommas(x) {
    return x.toString().replace(/\B(?=(\d{3})+(?!\d))/g, ",").replace('.00', '');
}
//end region

//region util function
function VATCheckBoxChange(ck) {
    if ($(ck).is(":checked")) {
        $("#VAT").show();
        $("#spSumfinal").show();
    } else {
        $("#VAT").hide();
        $("#spSumfinal").hide();
        $("#VAT").val(0);
    }

    RecalculateMoney(false);
}

function ButtonConfirmClick() {
    var today = new Date();
    var dd = today.getDate();
    var mm = today.getMonth() + 1; //January is 0!
    var yyyy = today.getFullYear();

    if (dd < 10) {
        dd = '0' + dd;
    }

    if (mm < 10) {
        mm = '0' + mm;
    }
    today = mm + '/' + dd + '/' + yyyy;
    $("#NgayNhap").val(today);
}

function BindNgayTaoPosition() {
    $(document.body).on('click', '.daterange-picker', function (e) {
        var reportDatePicker = $(e.currentTarget).find('input');
        var currDate = moment($("#NgayNhap").val(), DEFAULT_MOMENT_DATE_FORMAT).toDate();
        reportDatePicker.datepicker("update", currDate);
        reportDatePicker.datepicker('show');
    });

    $('.daterange-picker input').datepicker({
        format: DEFAULT_DATE_PICKER_FORMAT,
        changeMonth: true,
        changeYear: true,
        endDate: 0,
        maxViewMode: 2,
        defaultDate: new Date(),
        minDate: MIN_PRODUCTION_DATA_DATE,
        language: 'vi',
        autoclose: true
    }).on('changeDate', function (dateEvent) {
        var selectedDate = moment(dateEvent.date).format(DEFAULT_MOMENT_DATE_FORMAT);
        $("#inputDate").text(selectedDate);
        $("#NgayNhap").val(selectedDate);
    });
}
function HideDonViTinh(sl) {
    var dvtFilter = $(sl).parent().find("input[id*='HaiDonViTinh']");
    if (dvtFilter.val() != "") {
        var dvtVals = dvtFilter.val().split(';');
        HideSelectOpt($(sl).find("option"), dvtVals[0], dvtVals[1]);
    }
}
var resizeElementsTimeout;
function resizeElements() {
    clearTimeout(resizeElementsTimeout);
    resizeElementsTimeout = setTimeout(function () {
        $('.dataTables_scrollHead').css('width', '100%').find('table').css('width', '100%').find('thead th, thead td').css('width', '');
        $('.dataTables_scrollHeadInner').css('width', '100%');
        $('.dataTables_scroll').css('overflow-x', 'auto');

        if (loaiPhieu == '1') {
            $('.dataTables_scrollHeadInner table thead th:eq(0)').css('width', '5%').css('text-align', 'left');
            $('.dataTables_scrollHeadInner table thead th:eq(1)').css('width', '5%').css('text-align', 'left');
            $('.dataTables_scrollHeadInner table thead th:eq(2)').css('width', '8%').css('text-align', 'left');
            $('.dataTables_scrollHeadInner table thead th:eq(3)').css('width', '26%').css('text-align', 'left');
            $('.dataTables_scrollHeadInner table thead th:eq(4)').css('width', '8%').css('text-align', 'right');
            $('.dataTables_scrollHeadInner table thead th:eq(5)').css('width', '6%').css('text-align', 'right');
            $('.dataTables_scrollHeadInner table thead th:eq(6)').css('width', '9%').css('text-align', 'right');
            $('.dataTables_scrollHeadInner table thead th:eq(7)').css('width', '12%').css('text-align', 'right');
            $('.dataTables_scrollHeadInner table thead th:eq(8)').css('width', '5%').css('text-align', 'right');
            $('.dataTables_scrollHeadInner table thead th:eq(9)').css('width', '7%').css('text-align', 'right');
            $('.dataTables_scrollHeadInner table thead th:eq(10)').css('width', '25%').css('text-align', 'right');
        } else {
            $('.dataTables_scrollHeadInner table thead th:eq(0)').css('width', '7%').css('text-align', 'left');
            $('.dataTables_scrollHeadInner table thead th:eq(1)').css('width', '5%').css('text-align', 'left');
            $('.dataTables_scrollHeadInner table thead th:eq(2)').css('width', '10%').css('text-align', 'left');
            $('.dataTables_scrollHeadInner table thead th:eq(3)').css('width', '28%').css('text-align', 'left');
            $('.dataTables_scrollHeadInner table thead th:eq(4)').css('width', '8%').css('text-align', 'left');
            $('.dataTables_scrollHeadInner table thead th:eq(5)').css('width', '6%').css('text-align', 'right');
            $('.dataTables_scrollHeadInner table thead th:eq(6)').css('width', '12%').css('text-align', 'right');
            
            $('.dataTables_scrollHeadInner table thead th:eq(7)').css('width', '7%').css('text-align', 'right');
            $('.dataTables_scrollHeadInner table thead th:eq(8)').css('width', '7%').css('text-align', 'right');
            $('.dataTables_scrollHeadInner table thead th:eq(9)').css('width', '25%').css('text-align', 'right');
        }

        $('#tblCt').css('width', '100%').find('thead th, thead td').css('width', '');
  
       
        if (loaiPhieu == '1') {
            $('#tblCt thead th:eq(0)').css('width', '5%');
            $('#tblCt thead th:eq(1)').css('width', '5%');
            $('#tblCt thead th:eq(2)').css('width', '9%');
            $('#tblCt thead th:eq(3)').css('width', '25%');
            $('#tblCt thead th:eq(4)').css('width', '9%');
            $('#tblCt thead th:eq(5)').css('width', '7%');
            $('#tblCt thead th:eq(6)').css('width', '10%');
            $('#tblCt thead th:eq(7)').css('width', '12%');
            $('#tblCt thead th:eq(8)').css('width', '7%');
            $('#tblCt thead th:eq(9)').css('width', '7%');
            $('#tblCt thead th:eq(10)').css('width', '25%');
        } else {
            $('#tblCt thead th:eq(0)').css('width', '7%');
            $('#tblCt thead th:eq(1)').css('width', '5%');
            $('#tblCt thead th:eq(2)').css('width', '10%');
            $('#tblCt thead th:eq(3)').css('width', '24%');
            $('#tblCt thead th:eq(4)').css('width', '10%');
            $('#tblCt thead th:eq(5)').css('width', '9%');
            $('#tblCt thead th:eq(6)').css('width', '12%');
            $('#tblCt thead td:eq(6)').attr('align','right');
            //$('#tblCt thead th:eq(6)').css('align', 'right');
            $('#tblCt thead th:eq(7)').css('width', '7%');
            $('#tblCt thead th:eq(8)').css('width', '4%');
            $('#tblCt thead th:eq(9)').css('width', '25%');
        }
        $('.dataTables_scrollBody').css('overflow', '');
    }, 50);

}

var oRowSelected;
function fnInitWhenClosePopup(orow) {
    oRowSelected = orow;
    $('#update-drug-batch-dialog,#update-drug-in-price-dialog').on('hidden.bs.modal', function () {
        $(oRowSelected).closest("tr").find("input[id*='__SoLuong']").focus();
    });
}

//
//Functions for scan barcode on mobile
function scan() {
    var href = window.location.href.split("\#")[0];

    window.addEventListener("storage", zxinglistener, false);
    zxingWindow = window.open("zxing://scan/?ret=" + encodeURIComponent(href + "#{CODE}"), '_self');

};

function zxinglistener(e) {
    localStorage["zxingbarcode"] = "";
    if (e.url.split("\#")[0] == window.location.href) {
        window.focus();
        //alert(e.url);
        processBarcode(decodeURIComponent(e.newValue));
    }
    window.removeEventListener("storage", zxinglistener, false);
}
if (window.location.hash != "") {
    localStorage["zxingbarcode"] = window.location.hash.substr(1);
    self.close();
    window.location.href = "about:blank";//In case self.close is disabled
} else {
    window.addEventListener("hashchange", function (e) {
        window.removeEventListener("storage", zxinglistener, false);
        var hash = window.location.hash.substr(1);
        //alert(window.location.hash);
        if (hash != "") {
            window.location.hash = "";
            processBarcode(decodeURIComponent(hash));
        }
    }, false);
}

function processBarcode(barcode) {
    if (barcode) {
        var type = $("#MaLoaiXuatNhap").val();
        var url = $('input[name=baseUrl]').val() + "Thuocs/GetThuocsByBarcode";
        $.get(url, { barcode: barcode }).done(function (thuoc) {

            var tr = $("#trFirst");
            tr.find("input[name*='.ThuocId']").val(thuoc.value);
            tr.find("input[name*='.MaThuoc']").val(thuoc.maThuoc);
            tr.find('input.thuocFinderByMa ').val(thuoc.maThuoc);
            tr.find("input[id$='__MaThuoc']").val(thuoc.maThuoc);
            tr.find("input[id$='__TenThuoc']").val(thuoc.tenThuoc);
            tr.find("span[id$='__TenSp']").text(thuoc.tenThuoc);
            //alert(thuoc.value);
            if ($("select#MaKhachHang").val() == undefined) {
                tr.find("input[id*='GiaNhap']").val(thuoc.price2);
                tr.find("input[id*='GiaBan']").val(thuoc.price1);
                tr.find("select[name*='.MaDonViTinh']").val(thuoc.unit2);
            } else {
                tr.find("input[id*='GiaNhap']").val(thuoc.price1);
                tr.find("select[name*='.MaDonViTinh']").val(thuoc.unit1);
            }

            GetThuocPropertyFromScan(thuoc, tr, type);
            $('input[id*=-1__SoLuong]:first').val(1);

            InsertRow(tr);
            //$("#maThuoc").val("");
            return false;


        });
    }
}
//end region