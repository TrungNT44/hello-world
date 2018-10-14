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
    $("#dvNgayXuat").on("click", function () {
        $("#NgayXuat").datepicker("show");
    });
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
    var btn = (loaiPhieu == "4" ? "" : '<button class="btn" ng-click="updateOutPrice($event); $event.preventDefault();"><a style="cursor: pointer;" id="luuGiaXuatId" title="Cập nhật giá xuất"><i class="glyphicon glyphicon-edit"></i></a></button>');
    var tRow = '<tr>' +
                '<td style="padding: 10px 0">' +
                '<a class="edit-thuoc display-mode" style="cursor: pointer" id="0" title="Sửa"><i class="glyphicon glyphicon-pencil"></i></a> ' +
                '<a class="save-thuoc edit-mode text-success" style="cursor: pointer" id="0" title="Lưu"><i class="glyphicon glyphicon-ok"></i></a>&nbsp' +
                '<a class="delete-thuoc edit-mode text-danger" style="cursor: pointer" id="0" title="Xóa"><i class="glyphicon glyphicon-trash"></i></a>&nbsp' +
                '<input type="hidden" name="giabanbuon_tp" value=""/>' +
                '<input type="hidden" name="giabanle_tp" value=""/>' +
                 '<input type="hidden" name="gianhap_tp" value=""/>' +
                '<input data-val="true" data-val-number="The field MaPhieuXuatCt must be a number." data-val-required="The MaPhieuXuatCt field is required." id="PhieuXuatChiTiets_-1__MaPhieuXuatCt" name="PhieuXuatChiTiets[-1].MaPhieuXuatCt" type="hidden" value="0">' +
        '<input data-val="true" data-val-number="The field MaPhieuXuat must be a number." data-val-required="The MaPhieuXuat field is required." id="PhieuXuatChiTiets_-1__MaPhieuXuat" name="PhieuXuatChiTiets[-1].MaPhieuXuat" type="hidden" value="' + maPhieuXuat + '">' +
        '<input id="PhieuXuatChiTiets_-1__MaNhaThuoc" name="PhieuXuatChiTiets[-1].MaNhaThuoc" type="hidden" value="' + maNhaThuoc + '">' +
                '</td>' +
                '<td><select name="kieuban" class="form-control"><option value="banle">B.Lẻ</option><option value="banbuon">B.Buôn</option></select></td>' +
                '<td><a href="javascript:void(0)" action="/Thuocs/DialogDetail?id={thuocid}" id="spOrdinalNumber" class="a_view"><span id="dSTT_-1__Sp" class="stt"></span></a></td>' +
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
    '<span data-inputmask="\'alias\': \'decimal\', \'groupSeparator\': \',\', \'autoGroup\': true" class="display-mode pNumber" format="number" id="PhieuXuatChiTiets_-1__SoLuongSp"></span>' +
    '<input data-inputmask="\'alias\': \'decimal\', \'groupSeparator\': \',\', \'autoGroup\': true" class="form-control edit-mode text-box single-line" data-val="true" data-val-number="The field SoLuong must be a number." data-val-required="The SoLuong field is required." id="PhieuXuatChiTiets_-1__SoLuong" name="PhieuXuatChiTiets[-1].SoLuong" type="text" value="0" style="display: none;">' +
    '</td>' +
    '<td>' +
    '<div class="input-group"><span data-inputmask="\'alias\': \'numeric\', \'groupSeparator\': \',\', \'autoGroup\': true, \'digits\': 0, \'digitsOptional\': false, \'suffix\': \'\'" class="display-mode pNumber" id="PhieuXuatChiTiets_-1__GiaXuatSp"></span><input data-inputmask="\'alias\': \'numeric\', \'groupSeparator\': \',\', \'autoGroup\': true, \'digits\': 0, \'digitsOptional\': false, \'suffix\': \'\': \'0\'" class="form-control edit-mode text-box single-line drug-price" data-val="true" data-val-number="Đơn giá phải là số" data-val-required="Chưa nhập đơn giá" id="PhieuXuatChiTiets_-1__GiaXuat" format = "number"name="PhieuXuatChiTiets[-1].GiaXuat" type="text" value="0" style="display: none;">' +
    '<span class="input-group-btn" id="PhieuXuatChiTiets_-1__IconGiaXuat" onclick="event.preventDefault();">'+ btn +'</span></div>' +
    '</td>' +
    '<td>' +
    '<span class="display-mode pNumber" id="PhieuXuatChiTiets_-1__ChietKhauSp"></span>' +
    '<input data-inputmask="\'alias\': \'decimal\', \'groupSeparator\': \',\', \'autoGroup\': true" class="form-control edit-mode text-box single-line" data-val="true" data-val-number="The field ChietKhau must be a number." data-val-required="The ChietKhau field is required." id="PhieuXuatChiTiets_-1__ChietKhau" name="PhieuXuatChiTiets[-1].ChietKhau" type="text" value="0" style="display: none;">' +
    '</td>' +
        '<td style="text-align: right;">' + '<span name="lbSoTon"></span><input type="hidden" name="lbSoTon">' + '</td>' +
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
    $('select[name=kieuban] option:first').prop('selected', true);
    // Tra lai nha cung cap 
    if ($('input[name=MaLoaiXuatNhap]').val() == 4) {
        $('select[name=kieuban]').remove();
    }
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
}

//Autocomplete binder
function BindAutocomplete(elem, sName) {
    var url = $('input[name=baseUrl]').val() + "Thuocs/GetThuocs";
    if (sName == "maThuoc") {
        url = $('input[name=baseUrl]').val() + "Thuocs/GetThuocsByMa";
    }

    if (loaiPhieu == "4")
    {
        url = $('input[name=baseUrl]').val() + "Thuocs/GetThuocs_GiaPhieu";
        if (sName == "maThuoc") {
            url = $('input[name=baseUrl]').val() + "Thuocs/GetThuocsByMa_GiaPhieu";
        }
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
            var tr = $(this).closest("tr");
            tr.find("input[name*='.ThuocId']").val(ui.item.value);
            tr.find("input[name*='.MaThuoc']").val(ui.item.maThuoc);
            tr.find('input.thuocFinderByMa ').val(ui.item.maThuoc);
            tr.find("input[id$='__MaThuoc']").val(ui.item.maThuoc);
            tr.find("input[id$='__TenThuoc']").val(ui.item.tenThuoc);
            tr.find("span[id$='__TenSp']").text(ui.item.tenThuoc);
            tr.find("span[name=lbSoTon]").text(ui.item.soton);
            tr.find("input[type=hidden][name=lbSoTon]").val(ui.item.soton);
            if ($('input[name=MaLoaiXuatNhap]').val() == 4) {
                tr.find("input[id*='GiaXuat']").val(ui.item.price2);
                if (ui.item.unit2 == "")
                {
                    tr.find("select[name*='.MaDonViTinh']").val(ui.item.unit1);
                }
                else
                {
                    tr.find("select[name*='.MaDonViTinh']").val(ui.item.unit2);
                }
                
            }
            else {
                if ($("select[name=kieuban]").val() == 'banle') {
                    tr.find("input[id*='GiaXuat']").val(ui.item.price1);
                } else {
                    tr.find("input[id*='GiaXuat']").val(ui.item.price3);
                }

                tr.find("select[name*='.MaDonViTinh']").val(ui.item.unit1);
            }
            // cache prices for switching sale type
            tr.find('input[name=giabanbuon_tp]').val(ui.item.price3);
            tr.find('input[name=giabanle_tp]').val(ui.item.price1);
            tr.find('input[name=gianhap_tp]').val(ui.item.price2);
            GetThuocProperty(ui.item.value, tr);
            $('input[id*=-1__SoLuong]:first').focus();
            fnInitEventDDLUnit();
            tr.find(".drug-unit").trigger("change");
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
        var tr = $(this).closest("tr");
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

    $('body').on('change', 'select[name=kieuban]', function () {
        var tr = $(this).closest("tr");
        CalculatePrice(tr);
        //$(this).parents('tr').find('select[id*=__MaDonViTinh]').val($(this).parents('tr').find('input[id*=__DVX]').val());
    });

    //Save the row
    $('body').off('click', '.save-thuoc');
    $('body').off('keydown', '.lastInput');
    $('body').on('keydown', '.dataTables_scrollHeadInner tr:eq(1) td:gt(4) input', function (event) {
        if (event.keyCode == 13) {
            //console.log("122");
            var tr = $(this).parents('tr:first');
            if (tr.parent().get(0).tagName == "THEAD") {
                var id_thuoc = tr.find("input[id*='ThuocId']").val();
                if (id_thuoc != "") {
                    //Check số lượng tồn kho có đủ cho số lượng nhập vào ko.
                    $(this).closest("tr").find(".save-thuoc").trigger("click");
                    //InsertRow(tr);
                    //tr.find(':input').val('');
                }
            }

        }
    });
    var SLThuoc_DaNhap;
    $('body').on('click', '.save-thuoc', function () {
        SLThuoc_DaNhap = "";
        var tr = $(this).parents('tr:first');
        //the template row
        if (tr.parent().get(0).tagName == "THEAD") {
            var id_thuoc = tr.find("input[id*='ThuocId']").val();
            if (id_thuoc != "") {
                SLThuoc_DaNhap = tr.find("input[id*=__SoLuong]").val();
                var url = $('input[name=baseUrl]').val() + "Thuocs/GetSLTonKhoCuaThuocs_Warming";
                var request = "id_thuoc=" + id_thuoc + "&donvi=" + tr.find("select[id*=__MaDonViTinh]").val();
                if (loaiPhieu == "4") {
                    $.getJSON(url, request, function (data, status, xhr) {
                        var sl_tonkho = parseFloat(data);
                        if (isNaN(sl_tonkho) === false) {
                            var sl_danhap = parseFloat(SLThuoc_DaNhap);
                            if (isNaN(sl_danhap) === false) {
                                if (sl_tonkho < sl_danhap) {
                                    app.notice.error("Số lượng xuất ra vượt quá số lượng tồn kho hiện tại (" + sl_tonkho + " đơn vị)");
                                }
                                else {
                                    InsertRow(tr);
                                }
                            }
                        }
                        else {
                            app.notice.error("Không lấy được số lượng thuốc tồn kho");
                        }
                        //console.log(data)
                    });
                }
                else {
                    $.getJSON(url, request, function (data, status, xhr) {
                        var sl_tonkho = parseFloat(data);
                        if (isNaN(sl_tonkho) === false) {
                            var sl_danhap = parseFloat(SLThuoc_DaNhap);
                            if (isNaN(sl_danhap) === false) {
                                if (sl_tonkho < sl_danhap) {
                                    app.notice.message("Số lượng xuất ra vượt quá số lượng tồn kho hiện tại (" + sl_tonkho + " đơn vị)");
                                }
                            }
                        }
                        //console.log(data)
                    });
                    InsertRow(tr);
                }
                // clear input form
                //tr.find(':input').val('');
            }
            //save a new row
        } else if ($(this).attr("id") == 0) {
            var id_thuoc = tr.find("input[id*='ThuocId']").val();
            if (id_thuoc != "") {
                SLThuoc_DaNhap = tr.find("input[id*=__SoLuong]").val();
                var url = $('input[name=baseUrl]').val() + "Thuocs/GetSLTonKhoCuaThuocs_Warming";
                var request = "id_thuoc=" + id_thuoc + "&donvi=" + tr.find("select[id*=__MaDonViTinh]").val();
                if (loaiPhieu == "4") {
                    $.getJSON(url, request, function (data, status, xhr) {
                        var sl_tonkho = parseFloat(data);
                        if (isNaN(sl_tonkho) === false) {
                            var sl_danhap = parseFloat(SLThuoc_DaNhap);
                            if (isNaN(sl_danhap) === false) {
                                if (sl_tonkho >= sl_danhap) {
                                    tr.find('.edit-mode, .display-mode').toggle();
                                    SaveRow(tr, true);
                                }
                                else {
                                    app.notice.error("Số lượng xuất ra vượt quá số lượng tồn kho hiện tại (" + sl_tonkho + " đơn vị)");
                                }
                            }
                        }
                        else {
                            app.notice.error("Không lấy được số lượng thuốc tồn kho");
                        }
                        //console.log(data)
                    });
                }
                else {
                    $.getJSON(url, request, function (data, status, xhr) {
                        var sl_tonkho = parseFloat(data);
                        if (isNaN(sl_tonkho) === false) {
                            var sl_danhap = parseFloat(SLThuoc_DaNhap);
                            if (isNaN(sl_danhap) === false) {
                                if (sl_tonkho < sl_danhap) {
                                    app.notice.message("Số lượng xuất ra vượt quá số lượng tồn kho hiện tại (" + sl_tonkho + " đơn vị)");
                                }
                            }
                        }
                        //console.log(data)
                    });
                    SaveRow(tr, true);
                    tr.find('.edit-mode, .display-mode').toggle();
                }
                
            }
        } else {
            SaveRow(tr, false);            
            tr.find('.edit-mode, .display-mode').toggle();
        }
        if ($(".khachang-item").text() == "Khách hàng lẻ") {
            RecalculateMoney(true);
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
   
    if ($("#tblCt .delete-thuoc:visible").length > 0) {
        return false;
    }

    if (clicked) {
        return false;
    }
    clicked = true;
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
        //tr.find('input.input-validation-error:first').focus();
        //return false;
        $('.dataTables_scrollHeadInner tr:eq(1)  input[id*=_TenThuoc]').focus();
        return false;
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
            //console.log(12);
            RecalculateMoney(true);
        }
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
    if (row.find("td")[2] != undefined)
        currentNum = parseInt(row.find(".stt").text());
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
    $(fTr).find("span[name=lbSoTon]").parent().css({ "text-align": "right" });
    fTr.find("select[id*='MaDonViTinh']").val(slVal);
    fTr.find(".delete-thuoc").show();

    //BindDatePicker(fTr.find(".datefield"));
    InitEdit();
    RecalculateSTT();
    SaveRow(fTr, fTr.attr("id") == 0);
    fTr.find('.edit-mode, .display-mode').toggle();
    // dont allow to edit mathuoc, tenthuoc
    fTr.find('td:eq(3),td:eq(4)').children().removeClass('edit-mode display-mode');
    // remove sale type
    fTr.find('select[name=kieuban]').remove();
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
    tr.find("span[name=lbSoTon]").text(0);
    tr.find("input[type=hidden][name=lbSoTon]").val(0);
    tr.find("select:gt(0)").val([]);
}

//Recalculate the Index
function RecalculateSTT() {
    var cnt = 0;
    $("table#tblCt tr").each(function () {
        if (cnt > 1 && $(this).find("td")[2]) {
            $(this).find(".stt").text(cnt - 1);
            var thuocid = $(this).find("input[name*='.ThuocId']").val();
            $(this).find(".a_view").attr("action", $(this).find(".a_view").attr("action").replace("{thuocid}", thuocid));
            //$(this).find("td")[2].innerHTML = cnt - 1;
        }
        cnt++;
    });
    FnReInitLinkDetail();
}
function FnReInitLinkDetail() {
    $('.a_view').unbind().on('click', function () {
        $.get($(this).attr("action")).done(function (data) {
            if (data) {
                $(data).hide().appendTo(document.body).on('hide.bs.modal', function () { $('.modal-backdrop').remove(); }).modal();
            }
        });
    });
}
// Tinh toan lai gia cho san pham
function CalculatePrice(tr) {
    var type = $("#MaLoaiXuatNhap").val();
    var kieuban = tr.find('select[name=kieuban]').val();
    var dvx = tr.find('input[id*=__DVX]').val();
    var dvtn = tr.find('input[id*=__DVTN]').val();
    var dv = tr.find('select[id*=__MaDonViTinh]').val();
    var heso = tr.find('input[id*=__HeSo]').val();

    var gia = 0;
    var giaxuat = 0;
    if (kieuban == 'banle') {
        giaxuat = tr.find('input[name=giabanle_tp]').val();
    } else if (kieuban == 'banbuon') {
        giaxuat = tr.find('input[name=giabanbuon_tp]').val();
    }
    else {
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
    var miniTotal = $(tds[6]).find("input").val() * $(tds[7]).find("input").val() * (100 - $(tds[8]).find("input").val()) / 100;
    var refNum = $.number(miniTotal, 0, '.', ','); // Outputs: 5,020.24
    tr.find("td:last-child b").text(refNum);
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
        if (id.toString().indexOf("SoLuong") > -1 || id.toString().indexOf("GiaXuat") > -1) {
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
    if ($(".khachang-item").text() == "Khách hàng lẻ") {
        RecalculateMoney(true);
    }

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
    var skip0 = 0;
    $('b[name="miniSum"]').each(function () {
        if ($(this).text() != "")
            sumAll += parseInt($(this).text().replace(/\,/g, ""));
        $(this).text(numberWithCommas($(this).text()));
        if ($(this).text() == '0') {
            if (skip0 == 0)
                $(this).text('');
            else
                $(this).text('0');
        }
        skip0++;
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
        //if(!isNaN ($(this).val()))
        if ($(".khachang-item").text() == "Khách hàng lẻ")
            RecalculateMoney(true);
        else RecalculateMoney(false);
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
        var giaXuat = tr.find("input[id*='GiaXuat']").val();
        var refNum = $.number(giaXuat * sl * (100 - ck) / 100, 0, '.', ',');
        tr.find("b[name='miniSum']").text(refNum);
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
    if ($(".khachang-item").text() == "Khách hàng lẻ")
        RecalculateMoney(true);
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
        $('.dataTables_scrollHead').css('width', '100%').css('min-width', '750px').find('table').css('width', '100%').css('min-width', '750px').find('thead th, thead td').css('width', '');
        $('.dataTables_scrollHeadInner').css('width', '100%');
        $('.dataTables_scroll').css('overflow-x', 'auto');
        $('.dataTables_scrollHeadInner table thead th:eq(0)').css('width', '45px');
        $('.dataTables_scrollHeadInner table thead th:eq(1)').css('width', '130px');
        $('.dataTables_scrollHeadInner table thead th:eq(2)').css('width', '45px');
        $('.dataTables_scrollHeadInner table thead th:eq(3)').css('width', '80px');
        $('.dataTables_scrollHeadInner table thead th:eq(4)').css('width', '316px');
        $('.dataTables_scrollHeadInner table thead th:eq(5)').css('width', '70px');
        $('.dataTables_scrollHeadInner table thead th:eq(6)').css('width', '70px').css('text-align', 'right');
        $('.dataTables_scrollHeadInner table thead th:eq(7)').css('width', '120px').css('text-align', 'right');
        $('.dataTables_scrollHeadInner table thead th:eq(8)').css('width', '50px').css('text-align', 'right');
        $('.dataTables_scrollHeadInner table thead th:eq(9)').css('min-width', '30px').css('text-align', 'right');;
        $('.dataTables_scrollHeadInner table thead th:eq(10)').css('min-width', '70px');

        $('#tblCt').css('width', '100%').css('min-width', '750px').find('thead th, thead td').css('width', '');
        $('#tblCt thead th:eq(0)').css('width', '45px');
        $('#tblCt thead th:eq(1)').css('width', '130px');
        $('#tblCt thead th:eq(2)').css('width', '45px');
        $('#tblCt thead th:eq(3)').css('width', '80px');
        $('#tblCt thead th:eq(4)').css('width', '316px');
        $('#tblCt thead th:eq(5)').css('width', '70px');
        $('#tblCt thead th:eq(6)').css('width', '70px');
        $('#tblCt thead th:eq(7)').css('width', '120px');
        $('#tblCt thead th:eq(8)').css('width', '50px');
        $('#tblCt thead th:eq(9)').css('min-width', '30px').css('text-align', 'right');
        $('#tblCt thead th:eq(10)').css('min-width', '70px');
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

                thisTr.find('input[id*=__SoLuong]').val(round(oldqty + newqty, 2));
                thisTr.find('span[id*=__SoLuong]').text(oldqty + newqty);
                var refNum = $.number(actualTotal, 0, '.', ',');
                thisTr.find('b[name=miniSum]').text(refNum);
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
    // remove sale type
    fTr.find('select[name=kieuban]').remove();
    // enable input mask
    fTr.find(":input,span").inputmask();
    //tr.find("input[type=text]").eq(0).focus();
    $('.dataTables_scrollHeadInner tr:eq(1)  input[id*=_MaThuoc]').focus();
}

$("#MaKhachHang").on("change", function() {
    if ($(".khachang-item").text() == "Khách hàng lẻ") {
        RecalculateMoney(true);
    }
});

$("#tblCt").on("change", "select[id*='MaDonViTinh']", function () {
    var tr = $(this).closest("tr");
    var heso = tr.find("input[id*='HeSo']").val();
    var price = tr.find("input[id*='GiaXuat']").val();
    var soton_goc = +(tr.find("input[type=hidden][name=lbSoTon]").val() || 1);
    if ($(this).val() != tr.find("input[id*='DVX']").val()) {
        //quy đổi theo đơn vị nguyên
        tr.find("input[id*='GiaXuat']").val(parseInt(price * heso));
        tr.find("span[name=lbSoTon]").text((soton_goc / heso).toFixed(2)); 
    } else {
        //quy đổi ra đơn vị lẻ
        tr.find("input[id*='GiaXuat']").val(parseInt(price) / heso);
        tr.find("span[name=lbSoTon]").text(soton_goc);     
    }

    RecalculateSum(tr);
});
function fnInitEventDDLUnit() {
    $($("#tblCt_wrapper table")[0]).find("select").change(function () {
        var tr = $(this).closest("tr");
        var heso = tr.find("input[id*='HeSo']").val();
        var soton_goc = +(tr.find("input[type=hidden][name=lbSoTon]").val() || 1);
        if ($(this).val() != tr.find("input[id*='DVX']").val()) {
            //quy đổi theo đơn vị nguyên
            tr.find("span[name=lbSoTon]").text((soton_goc / heso).toFixed(2));
        } else {
            //quy đổi ra đơn vị lẻ
            tr.find("span[name=lbSoTon]").text(soton_goc);
        }
    });
}
$("#tblCt").on("keyup", "input[id*='SoLuong']", function (event) {
    if (event.keyCode == 13) {
        $(this).closest("tr").find(".save-thuoc").trigger("click");
        //SaveRow(tr);
        //tr.find('.edit-mode, .display-mode').toggle();
        //$("#PhieuXuatChiTiets_-1__TenThuoc").focus();

        //save-thuoc
    
    }
});

$("#tblCt").on("keyup", "input[id*='GiaXuat']", function (event) {
    if (event.keyCode == 13) {
        //var tr = $(this).closest("tr");
        //SaveRow(tr);
        //tr.find('.edit-mode, .display-mode').toggle();
        //$("#PhieuXuatChiTiets_-1__TenThuoc").focus();
        $(this).closest("tr").find(".save-thuoc").trigger("click");

    }
});

$("#tblCt").on("keyup", "input[id*='ChietKhau']", function (event) {
    if (event.keyCode == 13) {
        //var tr = $(this).closest("tr");
        //SaveRow(tr);
        //tr.find('.edit-mode, .display-mode').toggle();
        //$("#PhieuXuatChiTiets_-1__TenThuoc").focus();
        $(this).closest("tr").find(".save-thuoc").trigger("click");
      
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