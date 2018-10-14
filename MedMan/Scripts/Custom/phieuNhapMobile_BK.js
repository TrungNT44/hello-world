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
    //$("table#tblCt tr").eq(1).find(".edit-thuoc").click();

    //Hide options
    $("select[id*='MaDonViTinh']").each(function () {
        HideDonViTinh(this);
    });
    //change ngaytao
    BindNgayTaoPosition();

    //functional buttons
    $("#dvNgayNhap").on("click", function () {
        $("#NgayNhap").datepicker("show");
    });
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

    var tRow = '<tr>' +
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
                '<td><span class="display-mode" id="PhieuNhapChiTiets_-1__MaThuocSp"></span><input class="form-control edit-mode thuocFinderByMa ui-autocomplete-input" type="text" id="PhieuNhapChiTiets_-1__MaThuoc" value="" autocomplete="off" style="display: none;"></td>' +
                '<td>' +
                '<span class="display-mode" style="display: inline;" id="PhieuNhapChiTiets_-1__TenSp"></span>' +
                '<input class="form-control edit-mode thuocFinder ui-autocomplete-input" type="text" id="PhieuNhapChiTiets_-1__TenThuoc" value="" autocomplete="off" style="display: none;">' +
                '<input id="PhieuNhapChiTiets_-1__ThuocId" name="PhieuNhapChiTiets[-1].ThuocId" type="hidden" value="">' +
                '<input id="PhieuNhapChiTiets_-1__MaThuoc" name="PhieuNhapChiTiets[-1].MaThuoc" type="hidden" value="">' +
                '</td>' +
                '<td>' +
                '<span class="display-mode" style="display: inline;" id="PhieuNhapChiTiets_-1__MaDonViTinhSp"></span>' +
                '<input type="hidden" id="_-1__HaiDonViTinh" value="" />' +
    slData +
    '</td>' +
    '<td>' +
    '<span data-inputmask="\'alias\': \'decimal\', \'groupSeparator\': \',\', \'autoGroup\': true" class="display-mode pNumber" id="PhieuNhapChiTiets_-1__SoLuongSp"></span>' +
    '<input data-inputmask="\'alias\': \'decimal\', \'groupSeparator\': \',\', \'autoGroup\': true" class="form-control edit-mode text-box single-line" data-val="true" data-val-number="The field SoLuong must be a number." data-val-required="The SoLuong field is required." id="PhieuNhapChiTiets_-1__SoLuong" name="PhieuNhapChiTiets[-1].SoLuong" type="text" value="0" style="display: none;">' +
    '</td>' +
    '<td>' +
   '<span data-inputmask="\'alias\': \'numeric\', \'groupSeparator\': \',\', \'autoGroup\': true, \'digits\': 0, \'digitsOptional\': false, \'suffix\': \'\', \'placeholder\': \'0\'" class="display-mode pNumber" style="display: inline;" id="PhieuNhapChiTiets_-1__GiaNhapSp"></span>' +
    '<input data-inputmask="\'alias\': \'numeric\', \'groupSeparator\': \',\', \'autoGroup\': true, \'digits\': 0, \'digitsOptional\': false, \'suffix\': \'\', \'placeholder\': \'0\'" class="form-control edit-mode text-box single-line" data-val="true" data-val-number="Đơn giá phải là số" data-val-required="Chưa nhập đơn giá" id="PhieuNhapChiTiets_-1__GiaNhap" format = "number"name="PhieuNhapChiTiets[-1].GiaNhap" type="text" value="0" style="display: none;">' +
    '</td>' +
    '<td>' +
    '<span class="display-mode pNumber" id="PhieuNhapChiTiets_-1__ChietKhauSp"></span>' +
    '<input data-inputmask="\'alias\': \'decimal\', \'groupSeparator\': \',\', \'autoGroup\': true" class="form-control edit-mode text-box single-line" data-val="true" data-val-number="The field ChietKhau must be a number." data-val-required="The ChietKhau field is required." id="PhieuNhapChiTiets_-1__ChietKhau" name="PhieuNhapChiTiets[-1].ChietKhau" type="text" value="0" style="display: none;">' +
    '</td>' +
    //'<td>' +
    //'<span class="display-mode pNumber" style="display: inline;" id="PhieuNhapChiTiets_-1__HanDungSp"></span>' +
    //'<input value="" class="form-control datefield edit-mode" data-val="true" data-val-date="The field HanDung must be a date." data-val-required="The HanDung field is required." id="PhieuNhapChiTiets_-1__HanDung" name="PhieuNhapChiTiets[-1].HanDung" type="text" style="display: none;">' +
    //'</td>' +
    '<td><input type="hidden" value="" id="PhieuNhapChiTiets_-1__HeSo" />' +
    '<input type="hidden" value="0" id="PhieuNhapChiTiets_-1__DVX"/>' +
     '<input type="hidden" value="0" id="PhieuNhapChiTiets_-1__DVTN"/>' +
     '<input type="hidden" value="0" id="PhieuNhapChiTiets_-1__BasePrice"/>' +
    '<b name="miniSum" class="pNumber"></b></td>' +
    '</tr>';

    $("table#tblCt thead").append(tRow);
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
        dateFormat: "dd/mm/yy",
        changeMonth: true,
        changeYear: true,
        onClose: function () {
            $(this).focus();
        }
    });
}

//Autocomplete init
function InitAutocomplete() {
    //$(".thuocFinder").each(function () {
    //    BindAutocomplete($(this), "tenThuoc");
    //});
    //$(".thuocFinderByMa").each(function () {
    //    BindAutocomplete($(this), "maThuoc");
    //});
    BindAutocomplete($("#maThuoc"), "maThuoc");
}

//Autocomplete binder
function BindAutocomplete(elem, sName) {
    var url = $('input[name=baseUrl]').val() + "Thuocs/GetThuocs";
    if (sName == "maThuoc") {
        url = $('input[name=baseUrl]').val() + "Thuocs/GetThuocsByMaOrTen";
    }
    elem.autocomplete({
        minLength: 3,
        source: function (request, response) {
            var term = request.term;
            currentTerm = sName + term;
            if (term in cache) {
                response(cache[sName + term]);
                return;
            }
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
            var tr = $(this).parent().parent();
            tr.find("input[name*='.ThuocId']").val(ui.item.value);
            tr.find("input[name*='.MaThuoc']").val(ui.item.maThuoc);
            tr.find('input.thuocFinderByMa ').val(ui.item.maThuoc);
            tr.find("input[id$='__MaThuoc']").val(ui.item.maThuoc);
            tr.find("input[id$='__TenThuoc']").val(ui.item.tenThuoc);
            tr.find("span[id$='__TenSp']").text(ui.item.tenThuoc);
            //tr.find("select[name*='.madonvitinh']").val(ui.item.unit1);
            tr.find("input[id*='SoLuong']").val(1);
            if ($("select#MaKhachHang").val() == undefined) {
                tr.find("input[id*='GiaNhap']").val(ui.item.price2);
                tr.find("select[name*='.MaDonViTinh']").val(ui.item.unit2);
            } else {
                tr.find("input[id*='GiaNhap']").val(ui.item.price1);
                tr.find("select[name*='.MaDonViTinh']").val(ui.item.unit1);
            }
            
            GetThuocProperty(ui.item.value, tr);
            //$('input[id*=-1__SoLuong]:first').focus();            
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
    tr.find("input[id*='BasePrice']").val(tData.price2);
    HideSelectOpt(options, tData.unit1, tData.unit2);
    tr.find("input[id*='HeSo']").val(tData.heSo);
    //Bind gia nhap
    tr.find("select[id*='MaDonViTinh']").on("change", function () {
        if ($(this).val() == tr.find("input[id*='DVX']").val()) {            
            tr.find("input[id*='GiaNhap']").val(parseInt(tData.price2));
        } else {            
            tr.find("input[id*='GiaNhap']").val(tData.price2 * tData.heSo);
        }
    }).trigger('change');
    //Heso
    tr.find("input[id*='HeSo']").val(tData.heSo);
}

$("#tblCt").on("change", "select[id*='MaDonViTinh']", function () {
    var tr = $(this).closest("tr");
    var heso = tr.find("input[id*='HeSo']").val();
    var price = tr.find("input[id*='GiaNhap']").val();
    if ($(this).val() != tr.find("input[id*='DVX']").val()) {        
        tr.find("input[id*='GiaNhap']").val(parseInt(price * heso));
    }
    else {        
        tr.find("input[id*='GiaNhap']").val(parseInt(price)/heso);
    }

    RecalculateSum(tr);
});

$("#tblCt").on("keyup", "input[id*='SoLuong']", function (event) {
    if (event.keyCode == 13) {
        var tr = $(this).closest("tr");
        SaveRow(tr);
        tr.find('.edit-mode, .display-mode').toggle();
    }
});

$("#tblCt").on("keyup", "input[id*='GiaNhap']", function (event) {
    if (event.keyCode == 13) {
        var tr = $(this).closest("tr");
        SaveRow(tr);
        tr.find('.edit-mode, .display-mode').toggle();
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
$('$maThuoc').on('keyup', function (event)
{
    if (event.keyCode == 13)
    {
        InsertRow(tr);
        //var tr = $(this).parents('tr:first');
        //if (tr.parent().get(0).tagName == "THEAD")
        //{
        //    if (tr.find("input[id*='ThuocId']").val() != "")
        //    {
        //        InsertRow(tr);                   
        //    }
        //}

    }
});
//Bind the Edit mechanism to all rows
function InitEdit() {
    //Insert Row
    
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
    $('body').off('keyup', '.lastInput');
    $('body').on('keyup', '.dataTables_scrollHeadInner tr:eq(1) td:gt(4) input', function (event) {
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
                // clear input form
                //tr.find(':input').val('');
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
        tr.find("input[id*='SoLuong']").val(1);
        $('#tblCt').DataTable().row(tr).remove().draw();
        $('#tblCt tbody tr').each(function (index, el) {
            $(this).find('input').each(function () {
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
    });
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
    if (tr.find('input[id*=__GiaNhap]').val() <= 0) {
        tr.find('input[id*=__GiaNhap]').addClass('input-validation-error');
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
            var oldqty = parseInt(thisTr.find('input[id*=__SoLuong]').val());
            var oldUnit = thisTr.find('select[id*=__MaDonViTinh]').val();
            var oldDiscount = thisTr.find('input[id*=__ChietKhau]').val();
            var oldPrice = thisTr.find('input[id*=__GiaNhap]').val();

            var newqty = parseInt(tr.find('input[id*=__SoLuong]').val());
            var newUnit = tr.find('select[id*=__MaDonViTinh]').val();
            var newDiscount = tr.find('input[id*=__ChietKhau]').val();
            var newPrice = tr.find('input[id*=__GiaNhap]').val();

            if (oldPrice == newPrice && oldUnit == newUnit && oldDiscount == newDiscount) {
                var total = (oldqty + newqty) * oldPrice;
                var actualTotal = total;

                if (oldDiscount != null) {
                    var actualTotal = total - oldDiscount * total / 100;
                }

                thisTr.find('input[id*=__SoLuong]').val(oldqty + newqty);
                thisTr.find('span[id*=__SoLuong]').text(oldqty + newqty);
                thisTr.find('b[name=miniSum]').text(numberWithCommas(actualTotal));
                flag = true;
            }
        }
    });

    if (flag) {
        ClearTr(tr);
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

    var fTr = $("table#tblCt tr:last");

    fTr.find("select[id*='MaDonViTinh']").val(slVal);
    fTr.find(".delete-thuoc").show();

    //BindAutocomplete(fTr.find(".thuocFinder"));

    console.log(fTr.find('td:eq(2),td:eq(3)'));
    //BindDatePicker(fTr.find(".datefield"));
    InitEdit();
    RecalculateSTT();
    SaveRow(fTr, fTr.attr("id") == 0);
    fTr.find('.edit-mode, .display-mode').toggle();
    // dont allow to edit mathuoc, tenthuoc
    fTr.find('td:eq(2),td:eq(3)').children().removeClass('edit-mode display-mode');
    // enable input mask
    fTr.find(":input,span").inputmask();
    //tr.find("input[type=text]").eq(0).focus();
    $('.dataTables_scrollHeadInner tr:eq(1)  input[id*=_MaThuoc]').focus();

}

//Clear the template row
function ClearTr(tr) {
    tr.find("input[type=text]").val('');
    // tr.find("input[type=hidden]").val('');
    tr.find("input[type=number]").val(0);
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
    $(tds[8]).find("b").html(Math.floor($(tds[5]).find("input").val() * $(tds[6]).find("input").val() * (100 - $(tds[7]).find("input").val()) / 100));
    CalculateSum();
}

//Save a row
function SaveRow(tr, newRow) {
    tr.find("span").each(function () {        
        var id = $(this).attr("id").slice(0, -2);
        if (id.indexOf("DonViTinh") >= 0) {
            $(this).html(tr.find("select[id*='MaDonViTinh'] option:selected").text());
            return;
        }
        var control = tr.find("#" + id);
        if (control.attr("format") == "number") {
            $(this).html(numberWithCommas(control.val()));
        }
        else {
            if (id.toString().indexOf("ChietKhau") > -1) {                
                if (control.val() == "0" || control.val() == "") {
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
        $("#spSumfinal").text("Tổng cộng: " + numberWithCommas(sumFinal));
    }
    if (tra !== "" && tra >= 0) {
        var total = sumFinal - tra;
        if (total < 0)
            $("#spDebt").text("Trả lại : " + numberWithCommas((Math.abs(total))));
        else {
            $("#spDebt").text("Còn nợ : " + numberWithCommas((Math.abs(total))));
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
        //if (tr.find("select[id*='MaDonViTinh']").val() != tr.find("input[id*='DVX']").val()) {
        //    if (type)
        //        soLuong.val(soLuong.val() * tr.find("input[id*='HeSo']").val());
        //    else {
        //        var heSo = tr.find("input[id*='HeSo']").val();
        //        if (heSo != "") {
        //            sl = sl / heSo;
        //            soLuong.val(sl);
        //            tr.find("span[id*='SoLuongSp']").text(sl);
        //        }
        //    }
        //}
        var ck = tr.find("input[id*='ChietKhau']").val();
        var giaNhap = tr.find("input[id*='GiaNhap']").val();
        tr.find("b[name='miniSum']").text(giaNhap * sl * (100 - ck) / 100);
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
    $("#frmPhieuNhap").submit();
}

function BindNgayTaoPosition() {

    $("#NgayNhap").datepicker({
        dateFormat: "dd/mm/yy",
        changeMonth: true,
        changeYear: true,
        onSelect: function (dateText, inst) {
            var fDate = dateText.split("/");
            //var startDate = new Date(fDate[2], fDate[1], fDate[0]);
            //var day = startDate.getDate();
            //var month = startDate.getMonth();
            //var year = startDate.getFullYear();
            //$("#dvNgayXuat").html("Ngày " + day + " Tháng " + month + " Năm " + year);
            $("#dvNgayNhap").html(fDate[0] + "/" + fDate[1] + "/" + fDate[2]);
        },
        beforeShow: function (event, ui) {
            var $button = $("#dvNgayNhap"),
                left = $button.offset().left,
                top = $button.offset().top + $button.height();
            setTimeout(function () {
                ui.dpDiv.css({ left: left + "px", top: top + "px" });
            }, 10);
        }
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
        //$('.dataTables_scrollHead').css('width', '100%').css('min-width', '750px').find('table').css('width', '100%').css('min-width', '750px').find('thead th, thead td').css('width', '');
        //$('.dataTables_scrollHeadInner').css('width', '100%');
        //$('.dataTables_scroll').css('overflow-x', 'auto');
        //$('.dataTables_scrollHeadInner table thead th:eq(0)').css('width', '55px');

        //$('.dataTables_scrollHeadInner table thead th:eq(1)').css('width', '50px');
        //$('.dataTables_scrollHeadInner table thead th:eq(2)').css('width', '90px');
        //$('.dataTables_scrollHeadInner table thead th:eq(3)').css('min-width', '90px');
        //$('.dataTables_scrollHeadInner table thead th:eq(4)').css('width', '90px');
        //$('.dataTables_scrollHeadInner table thead th:eq(5)').css('width', '70px').css('text-align', 'right');
        //$('.dataTables_scrollHeadInner table thead th:eq(6)').css('width', '120px').css('text-align', 'right');
        //$('.dataTables_scrollHeadInner table thead th:eq(7)').css('width', '50px').css('text-align', 'right');
        //$('.dataTables_scrollHeadInner table thead th:eq(8)').css('min-width', '80px');

        //$('#tblCt').css('width', '100%').css('min-width', '750px').find('thead th, thead td').css('width', '');
        $('#tblCt').css('width', '100%');
        $('#tblCt thead th:eq(0)').css('width', '55px');
        $('#tblCt thead th:eq(1)').css('width', '30px');
        $('#tblCt thead th:eq(2)').css('width', '90px');
        $('#tblCt thead th:eq(3)').css('min-width', '70px');
        $('#tblCt thead th:eq(4)').css('width', '90px');
        $('#tblCt thead th:eq(5)').css('width', '70px');
        $('#tblCt thead th:eq(6)').css('width', '120px');
        $('#tblCt thead th:eq(7)').css('width', '50px');
        $('#tblCt thead th:eq(8)').css('min-width', '80px');
        $('.dataTables_scrollBody').css('overflow', '');
    }, 50);

   


}
//end region