var cache = {};
var tempRow = {};
var currentTerm = "";
var tbl;
//Call all the helper to init
function InitHelper() {
    $('#MaKhachHang,#MaNhaCungCap,#MaBacSy').selectize();
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
        $("#frmPhieuXuat").submit();
    });
    $("#btnRestore").on("click", function () {
        $("#Xoa").val("False");
        $("#frmPhieuXuat").submit();
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
    // resize
    resizeElements();
}

function CreateInsertRow(maNhaThuoc, maPhieuXuat, slData) {
    var row = $("table#tblCt").find("tr:last");

    if (slData == "") {
        slData = row.find("select[id*='MaDonViTinh']").prop('outerHTML').replace("selected=\"selected\"", "").replace(/_0__/g, "_-1__").replace(/\[0\]/g, "[-1]");
    }

    var tRow = '<tr id="trFirst" style="display:none">' +
                 '<td style="padding: 10px 0">' +
                 '<a class="edit-thuoc display-mode" style="cursor: pointer" id="0" title="Sửa"><i class="glyphicon glyphicon-pencil"></i></a> ' +
                 '<a class="save-thuoc edit-mode text-success" style="cursor: pointer" id="0" title="Lưu"><i class="glyphicon glyphicon-ok"></i></a>&nbsp' +
                 //'<a class="exit-thuoc edit-mode text-warning" style="cursor: pointer" id="0" title="Bỏ qua"><i class="glyphicon glyphicon-remove"></i></a>&nbsp' +
                 '<a class="delete-thuoc edit-mode text-danger" style="cursor: pointer" id="0" title="Xóa"><i class="glyphicon glyphicon-trash"></i></a>&nbsp' +
                 '<input type="hidden" name="giabanbuon_tp" value=""/>' +
                 '<input type="hidden" name="giabanle_tp" value=""/>' +
                 '<input type="hidden" name="gianhap_tp" value=""/>' +
                 '<input data-val="true" data-val-number="The field MaPhieuXuatCt must be a number." data-val-required="The MaPhieuXuatCt field is required." id="PhieuXuatChiTiets_-1__MaPhieuXuatCt" name="PhieuXuatChiTiets[-1].MaPhieuXuatCt" type="hidden" value="0">' +
         '<input data-val="true" data-val-number="The field MaPhieuXuat must be a number." data-val-required="The MaPhieuXuat field is required." id="PhieuXuatChiTiets_-1__MaPhieuXuat" name="PhieuXuatChiTiets[-1].MaPhieuXuat" type="hidden" value="' + maPhieuXuat + '">' +
         '<input id="PhieuXuatChiTiets_-1__MaNhaThuoc" name="PhieuXuatChiTiets[-1].MaNhaThuoc" type="hidden" value="' + maNhaThuoc + '">' +
                 '</td>' +                 
                 '<td><span id="dSTT_-1__Sp"></span></td>' +
                 '<td><span class="display-mode" id="PhieuXuatChiTiets_-1__MaThuocSp"></span><input class="form-control edit-mode thuocFinderByMa ui-autocomplete-input drug-code" type="text" id="PhieuXuatChiTiets_-1__MaThuoc" value="" autocomplete="off" style="display: none;"></td>' +
                 '<td>' +
                 '<span class="display-mode" style="display: inline;" id="PhieuXuatChiTiets_-1__TenSp"></span>' +
                 '<input class="form-control edit-mode thuocFinder ui-autocomplete-input drug-name" type="text" id="PhieuXuatChiTiets_-1__TenThuoc" value="" autocomplete="off" style="display: none;">' +
                 '<input id="PhieuXuatChiTiets_-1__ThuocId" name="PhieuXuatChiTiets[-1].ThuocId" type="hidden" value="">' +
                 '<input id="PhieuXuatChiTiets_-1__MaThuoc" name="PhieuXuatChiTiets[-1].MaThuoc" type="hidden" value="">' +
                 '</td>' +
                 '<td>' +
                 '<span class="display-mode" style="display: inline;" id="PhieuXuatChiTiets_-1__MaDonViTinhSp"></span>' +
                 '<input type="hidden" id="_-1__HaiDonViTinh" value="" />' +
    slData +
    '</td>' +
    '<td>' +    
    '<span data-inputmask="\'alias\': \'decimal\', \'groupSeparator\': \',\', \'autoGroup\': true" class="display-mode pNumber" id="PhieuXuatChiTiets_-1__SoLuongSp"></span>' +
    '<input data-inputmask="\'alias\': \'decimal\', \'groupSeparator\': \',\', \'autoGroup\': true" class="form-control edit-mode text-box single-line" data-val="true" data-val-number="The field SoLuong must be a number." data-val-required="The SoLuong field is required." id="PhieuXuatChiTiets_-1__SoLuong" name="PhieuXuatChiTiets[-1].SoLuong" type="text" value="0" style="display: none;">' +
    '</td>' +
    '<td>' +
    '<div class="input-group"><span data-inputmask="\'alias\': \'numeric\', \'groupSeparator\': \',\', \'autoGroup\': true, \'digits\': 0, \'digitsOptional\': false, \'suffix\': \'\'" class="display-mode pNumber" id="PhieuXuatChiTiets_-1__GiaXuatSp"></span><input data-inputmask="\'alias\': \'numeric\', \'groupSeparator\': \',\', \'autoGroup\': true, \'digits\': 0, \'digitsOptional\': false, \'suffix\': \'\': \'0\'" class="form-control edit-mode text-box single-line drug-price" data-val="true" data-val-number="Đơn giá phải là số" data-val-required="Chưa nhập đơn giá" id="PhieuXuatChiTiets_-1__GiaXuat" format = "number"name="PhieuXuatChiTiets[-1].GiaXuat" type="text" value="0" style="display: none;">' +
    '<span class="input-group-btn" id="PhieuXuatChiTiets_-1__IconGiaXuat" onclick="event.preventDefault();"><button class="btn" ng-click="updateOutPrice($event); $event.preventDefault();"><a style="cursor: pointer;" id="luuGiaXuatId" title="Cập nhật giá xuất"><i class="glyphicon glyphicon-edit"></i></a></button></span></div>' +
    '</td>' +
    '<td>' +    
    '<span class="display-mode pNumber" id="PhieuXuatChiTiets_-1__ChietKhauSp"></span>' +
    '<input data-inputmask="\'alias\': \'decimal\', \'groupSeparator\': \',\', \'autoGroup\': true" class="form-control edit-mode text-box single-line" data-val="true" data-val-number="The field ChietKhau must be a number." data-val-required="The ChietKhau field is required." id="PhieuXuatChiTiets_-1__ChietKhau" name="PhieuXuatChiTiets_[-1].ChietKhau" type="text" value="0" style="display: none;">' +
    '</td>' +

    
    '<td><input type="hidden" value="" id="PhieuXuatChiTiets_-1__HeSo" />' +
    '<input type="hidden" value="0" id="PhieuXuatChiTiets_-1__DVX"/>' +
    '<input type="hidden" value="0" id="PhieuXuatChiTiets_-1__DVTN"/>' +
    '<b name="miniSum" class="pNumber"></b></td>' +
    '</tr>';
    $("table#tblCt thead").append(tRow);
    //Compile it to bind it with angular
    AngularHelper.Compile($("#create-delivery-note"));
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
            //tr.find("select[name*='.MaDonViTinh']").val(ui.item.unit1);
            if ($('input[name=MaLoaiXuatNhap]').val() == 4) {
                //tr.find("input[id*='GiaXuat']").val(ui.item.price2);
                if (ui.item.unit2 != "")
                    tr.find("select[name*='.MaDonViTinh']").val(ui.item.unit2);
                else
                    tr.find("select[name*='.MaDonViTinh']").val(ui.item.unit1);
            } else {
                tr.find("input[id*='GiaXuat']").val(ui.item.price1);
                tr.find("select[name*='.MaDonViTinh']").val(ui.item.unit1);
            }
            // cache prices for switching sale type
            tr.find('input[name=giabanbuon_tp]').val(ui.item.price3);
            tr.find('input[name=giabanle_tp]').val(ui.item.price1);
            tr.find('input[name=gianhap_tp]').val(ui.item.price2);
            GetThuocProperty(ui.item.value, tr);
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

function GetThuocProperty(thuocId, tr) {
    var tDataList = cache[currentTerm];
    for (var i = 0; i < tDataList.length; i++) {
        if (tDataList[i].value == thuocId) {
            tData = tDataList[i];
        }
    }

    var options = tr.find("select[id*='MaDonViTinh'] option");
    tr.find("input[id*='DVX']").val(tData.unit1);
    tr.find("input[id*='DVTN']").val(tData.unit2);
    HideSelectOpt(options, tData.unit1, tData.unit2);
    tr.find("input[id*='__HeSo']").val(tData.heSo);
    //Bind gia Xuat
    tr.find("select[id*='MaDonViTinh']").on("change", function () {
        CalculatePrice(tr);
    }).trigger('change');
    //Heso

}

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
    //Save the row
    $('body').off('click', '.save-thuoc');
    $('body').off('keydown', '.lastInput');
    $('body').on('keydown', '.dataTables_scrollHeadInner tr:eq(1) td:gt(4) input', function (event) {
        if (event.keyCode == 13) {
            var tr = $(this).parents('tr:first');
            if (tr.parent().get(0).tagName == "THEAD") {
                if (tr.find("input[id*='ThuocId']").val() != "") {
                    InsertRow(tr);
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
        if ($(".khachang-item").text() == "Khách hàng lẻ") {
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

    if (tr.find('input[id*=__GiaXuat]').val() < 0) {
        tr.find('input[id*=__GiaXuat]').addClass('input-validation-error');
        isValid = false;
    }
    if (!isValid) {
        tr.find('input.input-validation-error:first').focus();
        return;
    }

    // Chi them khi chua co' ma hang trong bang

    //if (jQuery('#tblCt').find('tbody input[value=' + maThuoc + ']').size()) {
    //    return false;
    //}
    var flag = false;
    var maThuoc = tr.find('input[id*=__MaThuoc]').val().trim();
    jQuery.each(jQuery('#tblCt').find('tbody input[value=' + maThuoc + ']'), function () {
        if ($(this).attr("type") == "text") {
            var thisTr = $(this).closest("tr");
            var oldqty = parseFloat(thisTr.find('input[id*=__SoLuong]').val());
            var oldUnit = thisTr.find('select[id*=__MaDonViTinh]').val();
            var oldDiscount = thisTr.find('input[id*=__ChietKhau]').val();
            var oldPrice = thisTr.find('input[id*=GiaXuat]').val();

            var newqty = parseFloat(tr.find('input[id*=__SoLuong]').val());
            var newUnit = tr.find('select[id*=__MaDonViTinh]').val();
            var newDiscount = tr.find('input[id*=__ChietKhau]').val();
            var newPrice = tr.find('input[id*=GiaXuat]').val();

            if (oldPrice == newPrice && oldUnit == newUnit && oldDiscount == newDiscount) {
                var total = (oldqty + newqty) * oldPrice;
                var actualTotal = total;
                if (oldDiscount != null && oldDiscount != "0") {
                    var actualTotal = total - oldDiscount * total / 100;
                }
                var quantity = round(oldqty + newqty, 2);
                var refNum = $.number(quantity, 2, '.', ',');
                if (Math.floor(quantity) == quantity) {
                    refNum = $.number(quantity, 0, '.', ',');
                }
               
                thisTr.find('input[id*=__SoLuong]').val(quantity);
                thisTr.find('input[id*=__SoLuong]').text(refNum);
                thisTr.find('span[id*=__SoLuong]').text(refNum);
                refNum = $.number(actualTotal, 0, '.', ',');
                thisTr.find('b[name=miniSum]').text(refNum);
                flag = true;
            }
        }
    });

    if (flag) {
        ClearTr(tr);
        CalculateSum();
        if ($(".khachang-item").text() == "Khách hàng lẻ") {
            RecalculateMoney(true);
        }

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
    AngularHelper.Compile($("#create-delivery-note"));

    var fTr = $("table#tblCt tr:last");

    fTr.find("select[id*='MaDonViTinh']").val(slVal);
    fTr.find(".delete-thuoc").show();

    InitEdit();
    RecalculateSTT();
    SaveRow(fTr, fTr.attr("id") == 0);
    fTr.find('.edit-mode, .display-mode').toggle();
    // dont allow to edit mathuoc, tenthuoc
    fTr.find('td:eq(2),td:eq(3)').children().removeClass('edit-mode display-mode');
    // remove sale type    
    // enable input mask
    fTr.find(":input,span").inputmask();

    //Neu la khach le thi tong tra = tong luong
    if ($(".khachang-item").text() == "Khách hàng lẻ") {
        RecalculateMoney(true);
    }
}

//Clear the template row
function ClearTr(tr) {
    tr.find("input[type=text]").val('');
    // tr.find("input[type=hidden]").val('');
    tr.find("input[id*=_SoLuong]").val(0);
    tr.find("input[id*=ChietKhau]").val(0);
    tr.find("select:gt(0)").val([]);
}

//Recalculate the Index
function RecalculateSTT() {
    var cnt = 0;
    $("table#tblCt tr").each(function () {
        if (cnt > 1 && $(this).find("td")[1]) {
            $(this).find("td")[1].innerHTML = cnt - 1;
        }
        cnt++;
    });
}

// Tinh toan lai gia cho san pham
function CalculatePrice(tr) {
    var dvx = tr.find('input[id*=__DVX]').val();
    var dvtn = tr.find('input[id*=__DVTN]').val();
    var dv = tr.find('select[id*=__MaDonViTinh]').val();
    var heso = tr.find('input[id*=__HeSo]').val();
    var gia = 0;
    var giaxuat = 0;
    giaxuat = tr.find('input[name=giabanle_tp]').val();
    var type = $("#MaLoaiXuatNhap").val();
    if (type == "4") {
        giaxuat = tr.find('input[name=gianhap_tp]').val();
    }

    if (dv == dvx) {
        gia = giaxuat;
    }
    else {
        gia = giaxuat * heso;
    }

    tr.find('input[id*=__GiaXuat]').val(gia);
}

//Recalculate the Sum
function RecalculateSum(tr) {
    var tds = tr.find("td");
    //var heSo = 1;
    //if (tr.find("select[id*='MaDonViTinh'] option:selected").val() != tr.find("input[id*='DVX']").val())
    //    heSo = tr.find("input[id*='HeSo']").val();
    $(tds[8]).find("b").html(Math.round($(tds[5]).find("input").val() * $(tds[6]).find("input").val() * (100 - $(tds[7]).find("input").val()) / 100));
    CalculateSum();
}

//Save a row
function SaveRow(tr, newRow) {
    tr.find("span").each(function () {
        if (!$(this).attr("id"))
            return;
        var id = $(this).attr("id").slice(0, -2);
        if (id.indexOf("DonViTinh") >= 0) {
            $(this).html(tr.find("select[id*='MaDonViTinh'] option:selected").text());
            return;
        }
        var control = tr.find("#" + id);
        if (id.toString().indexOf("SoLuong") > -1 || id.toString().indexOf("GiaXuat") > -1 ) {
            $(this).html(numberWithCommas(control.val()));
        }
        else if (id.toString().indexOf("ChietKhau") > -1) {
            if (control.val() == "")
                $("#" + id).val("");
            else
                $(this).html(control.val() + " %");
        }
        else {
            $(this).html(control.val());
        }
    });
    RecalculateSum(tr);
    resizeElements();
    return true;
}

$("#btnIn").on('click', function () {
    var id = $("#MaPhieuXuat").val();
    window.location.href = "/PhieuXuats/In/" + id;
});

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

    $("#spanSumAll").text(numberWithCommas(sumAll)).trigger('change');
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

    $("#TongTien").attr("value", sumFinal);
    if ($("#ckVAT").is(':checked') || vat > 0) {
        $("#spSumfinal").text("Tổng Cộng : " + numberWithCommas(sumFinal));
    }
    if (tra !== "" && tra >= 0) {
        var total = sumFinal - tra;
        if (total < 0)
            $("#spDebt").text("Trả Lại : " + numberWithCommas((Math.abs(total))));
        else {
            $("#spDebt").text("Còn Nợ : " + numberWithCommas((Math.abs(total))));
        }
    }
    if (fullPay) {
        $("input[id*='DaTra']").val(numberWithCommas(sumFinal));
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
        //if(!isNaN ($(this).val()))
        RecalculateMoney(false);
    });
    $("input[id*='DaTra']").on('change keyup', function () {
        RecalculateMoney(false);
    });
    $("#btnFull").on('click', function () {
        RecalculateMoney(true);
    });
}

function BindSoLuong(type) {
    $("table#tblCt tr").each(function () {
        var tr = $(this);
        var soLuong = tr.find("input[id*='SoLuong']");
        var sl = soLuong.val();
        var ck = tr.find("input[id*='ChietKhau']").val();
        var giaXuat = tr.find("input[id*='GiaXuat']").val();
        tr.find("b[name='miniSum']").text(Math.round(giaXuat * sl * (100 - ck) / 100));
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
    $("#NgayXuat").val(today);
    $("#frmPhieuXuat").submit();
}

function BindNgayTaoPosition() {
    $(document.body).on('click', '.daterange-picker', function (e) {
        var reportDatePicker = $(e.currentTarget).find('input');
        var currDate = moment($("#NgayXuat").val(), DEFAULT_MOMENT_DATE_FORMAT).toDate();
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
        $("#NgayXuat").val(selectedDate);
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
        //$('.dataTables_scrollHead').css('width', '100%').find('table').css('width', '100%').find('thead th, thead td').css('width', '');
        //$('.dataTables_scrollHeadInner').css('width', '100%');
        //$('.dataTables_scroll').css('overflow-x', 'auto');
        //$('.dataTables_scrollHeadInner table thead th:eq(0)').css('width', '5%');
        //$('.dataTables_scrollHeadInner table thead th:eq(1)').css('width', '5%');
        //$('.dataTables_scrollHeadInner table thead th:eq(2)').css('min-width', '10%');
        //$('.dataTables_scrollHeadInner table thead th:eq(3)').css('width', '25%');
        //$('.dataTables_scrollHeadInner table thead th:eq(4)').css('min-width', '15%');
        //$('.dataTables_scrollHeadInner table thead th:eq(5)').css('width', '10%');
        //$('.dataTables_scrollHeadInner table thead th:eq(6)').css('width', '10%').css('text-align', 'right');
        //$('.dataTables_scrollHeadInner table thead th:eq(7)').css('width', '10%').css('text-align', 'right');
        //$('.dataTables_scrollHeadInner table thead th:eq(8)').css('width', '10%').css('text-align', 'right');

        //$('#tblCt').css('width', '100%').find('thead th, thead td').css('width', '');
        //$('#tblCt thead th:eq(0)').css('width', '5%');
        //$('#tblCt thead th:eq(1)').css('width', '5%');
        //$('#tblCt thead th:eq(2)').css('width', '10%');
        //$('#tblCt thead th:eq(3)').css('width', '25%');
        //$('#tblCt thead th:eq(4)').css('width', '15%');
        //$('#tblCt thead th:eq(5)').css('width', '10%');
        //$('#tblCt thead th:eq(6)').css('width', '10%');
        //$('#tblCt thead th:eq(7)').css('width', '10%');
        //$('#tblCt thead th:eq(8)').css('width', '10%');
        //$('.dataTables_scrollBody').css('overflow', '');
        $('.dataTables_scrollHead').css('width', '100%').find('table').css('width', '100%').find('thead th, thead td').css('width', '');
        $('.dataTables_scrollHeadInner').css('width', '100%');
        $('.dataTables_scroll').css('overflow-x', 'auto');
        $('.dataTables_scrollHeadInner table thead th:eq(0)').css('width', '7%');
        $('.dataTables_scrollHeadInner table thead th:eq(1)').css('width', '5%');
        $('.dataTables_scrollHeadInner table thead th:eq(2)').css('width', '10%');
        $('.dataTables_scrollHeadInner table thead th:eq(3)').css('width', '20%');
        $('.dataTables_scrollHeadInner table thead th:eq(4)').css('width', '10%');
        $('.dataTables_scrollHeadInner table thead th:eq(5)').css('width', '7%').css('text-align', 'right');
        $('.dataTables_scrollHeadInner table thead th:eq(6)').css('width', '13%').css('text-align', 'right');
        $('.dataTables_scrollHeadInner table thead th:eq(7)').css('width', '7%').css('text-align', 'right');
        //$('.dataTables_scrollHeadInner table thead th:eq(8)').css('width', '7%').css('text-align', 'right');
        $('.dataTables_scrollHeadInner table thead th:eq(8)').css('width', '10%').css('text-align', 'right');

        $('#tblCt').css('width', '100%').find('thead th, thead td').css('width', '');
        $('#tblCt thead th:eq(0)').css('width', '7%');
        $('#tblCt thead th:eq(1)').css('width', '5%');
        $('#tblCt thead th:eq(2)').css('width', '10%');
        $('#tblCt thead th:eq(3)').css('width', '20%');
        $('#tblCt thead th:eq(4)').css('width', '10%');
        $('#tblCt thead th:eq(5)').css('width', '7%');
        $('#tblCt thead th:eq(6)').css('width', '13%');
        $('#tblCt thead th:eq(7)').css('width', '7%');
        //$('#tblCt thead th:eq(8)').css('width', '7%');
        $('#tblCt thead th:eq(8)').css('width', '10%');
        $('.dataTables_scrollBody').css('overflow', '');
    }, 50);

}

function getDrug(barcode) {
    return window.drugs[barcode];
}

function handleInputBarcode(barcode) {
    var drug = getDrug(barcode);
    if (drug == null) {
        alert("Không tồn tại thuốc có mã vạch '" + barcode + "' trong CSDL.");
    } else {
        InsertDrugItemRow(drug);
    }
}

//Insert a new row
function InsertDrugItemRow(drug) {
    var flag = false;
    var maThuoc = drug.Code;
    jQuery.each(jQuery('#tblCt').find('tbody input[value=' + maThuoc + ']'), function () {
        if ($(this).attr("type") == "text") {
            var thisTr = $(this).closest("tr");
            var oldqty = parseInt(thisTr.find('input[id*=__SoLuong]').val());
            var oldUnit = thisTr.find('select[id*=__MaDonViTinh]').val();
            var oldDiscount = thisTr.find('input[id*=__ChietKhau]').val();
            var oldPrice = thisTr.find('input[id*=GiaXuat]').val();

            var newqty = 1;
            var newUnit = drug.Unit;
            var newDiscount = 0;
            var newPrice = drug.Price;

            if (oldPrice == newPrice && oldUnit == newUnit && oldDiscount == newDiscount) {
                var total = (oldqty + newqty) * oldPrice;
                var actualTotal = total;
                if (oldDiscount != null && oldDiscount != "0") {
                    actualTotal = total - oldDiscount * total / 100;
                }

                thisTr.find('input[id*=__SoLuong]').val(oldqty + newqty);
                thisTr.find('span[id*=__SoLuong]').text(oldqty + newqty);
                thisTr.find('b[name=miniSum]').text(numberWithCommas(actualTotal));
                flag = true;
            }
        }
    });

    if (flag) {
        return false;
    }
    var table = document.getElementById("#tblCt");

    // Create an empty <tr> element and add it to the 1st position of the table:
    var newTr = table.insertRow(0);
    var row = $("table#tblCt").find("tr:last");
    var currentNum = 0;
    if (row.find("td")[2] != undefined)
        currentNum = parseInt(row.find("td")[2].innerHTML);
    if (isNaN(currentNum))
        currentNum = 0;
    //Replace and insert the row
    newTr = newTr.replace(/_-1__/g, "_" + currentNum + "__").replace(/\[-1\]/g, "[" + currentNum + "]");
    var addRow = [];
    $($(newTr).find("td")).each(function () {
        var col = $(this).html();
        addRow.push(col);
    });
    tbl.row.add(addRow).draw();

    var fTr = $("table#tblCt tr:last");

    fTr.find("select[id*='MaDonViTinh']").val(slVal);
    fTr.find(".delete-thuoc").show();

    InitEdit();
    RecalculateSTT();
    SaveRow(fTr, fTr.attr("id") == 0);
    fTr.find('.edit-mode, .display-mode').toggle();
    // dont allow to edit mathuoc, tenthuoc
    fTr.find('td:eq(3),td:eq(4)').children().removeClass('edit-mode display-mode');
    // enable input mask
    fTr.find(":input,span").inputmask();
    //tr.find("input[type=text]").eq(0).focus();
    $('.dataTables_scrollHeadInner tr:eq(1)  input[id*=_MaThuoc]').focus();
}
//end region

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
        if (hash != "") {
            window.location.hash = "";
            processBarcode(decodeURIComponent(hash));
        }
    }, false);
}

function processBarcode(barcode) {
    if (barcode) {
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
            if ($('input[name=MaLoaiXuatNhap]').val() == 4) {
                tr.find("input[id*='GiaXuat']").val(thuoc.price2);
                tr.find("select[name*='.MaDonViTinh']").val(thuoc.unit2);
            } else {
                tr.find("input[id*='GiaXuat']").val(thuoc.price1);
                tr.find("select[name*='.MaDonViTinh']").val(thuoc.unit1);
            }

            tr.find('input[name=giabanbuon_tp]').val(thuoc.price3);
            tr.find('input[name=giabanle_tp]').val(thuoc.price1);
            tr.find('input[name=gianhap_tp]').val(thuoc.price2);

            GetThuocPropertyFromScan(thuoc, tr);
            $('input[id*=-1__SoLuong]:first').val(1);

            InsertRow(tr);
            //$("#maThuoc").val("");
            return false;


        });
    }
}

function GetThuocPropertyFromScan(tData, tr) {
    var options = tr.find("select[id*='MaDonViTinh'] option");
    tr.find("input[id*='DVX']").val(tData.unit1);
    tr.find("input[id*='DVTN']").val(tData.unit2);
    tr.find("input[id*='BasePrice']").val(tData.price2);
    HideSelectOpt(options, tData.unit1, tData.unit2);
    tr.find("input[id*='HeSo']").val(tData.heSo);
    //Bind gia nhap
    tr.find("select[id*='MaDonViTinh']").on("change", function () {
        CalculatePrice(tr);
    }).trigger('change');
    //Heso
    tr.find("input[id*='HeSo']").val(tData.heSo);
}

$("#MaKhachHang").on("change", function () {
    if ($(".khachang-item").text() == "Khách hàng lẻ") {
        RecalculateMoney(true);
    }
})


$("#tblCt").on("change", "select[id*='MaDonViTinh']", function () {
    var tr = $(this).closest("tr");
    var heso = tr.find("input[id*='HeSo']").val();
    var price = tr.find("input[id*='GiaXuat']").val();
    if ($(this).val() != tr.find("input[id*='DVX']").val()) {
        tr.find("input[id*='GiaXuat']").val(parseInt(price * heso));
    } else {
        tr.find("input[id*='GiaXuat']").val(parseInt(price) / heso);
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
        if ($(".khachang-item").text() == "Khách hàng lẻ") {
            RecalculateMoney(true);
        }
    }
});

$("#tblCt").on("keyup", "input[id*='GiaXuat']", function (event) {
    if (event.keyCode == 13) {
        var tr = $(this).closest("tr");
        SaveRow(tr);
        tr.find('.edit-mode, .display-mode').toggle();
        $("#maThuoc").focus();
        if ($(".khachang-item").text() == "Khách hàng lẻ") {
            RecalculateMoney(true);
        }
    }
});

$("#tblCt").on("keyup", "input[id*='ChietKhau']", function (event) {
    if (event.keyCode == 13) {
        var tr = $(this).closest("tr");
        SaveRow(tr);
        tr.find('.edit-mode, .display-mode').toggle();
        $("#maThuoc").focus();
        if ($(".khachang-item").text() == "Khách hàng lẻ") {
            RecalculateMoney(true);
        }
    }
});
var oRowSelected;
function fnInitWhenClosePopup(orow) {
    oRowSelected = orow;
    $('#update-drug-out-price-dialog').on('hidden.bs.modal', function () {
        $(oRowSelected).closest("tr").find("input[id*='__SoLuong']").focus();
    });
}
//end region
