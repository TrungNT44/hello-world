var currentNum = 0;
var currentTerm = "";
var cache = {};
var tbl;
function InitPage() {
    BindAutocomplete($("#txtByMa"), "maThuoc");
    BindAutocomplete($("#txtByTen"), "tenThuoc");
    
    tbl = $('#tbl').DataTable({
        "ordering": false,
        "info": false,
        "scrollY": "600px",
        "scrollCollapse": true,
        "paging": false
    });
    $("select#MaNhomThuoc").prepend("<option value='0'>Tất Cả</option>");
    $("select#MaNhomThuoc").prepend("<option value=''></option>").val('');
    $(".DTTT_container").find("a").each(function () {
        if ($(this).hide());
    });
    $(".DTTT_button_xls").show();
    BindSoSanh();
    BindNgayTaoPosition();
}

function BindNgayTaoPosition() {
    $(document.body).on('click', '.daterange-picker', function (e) {
        debugger;
        var reportDatePicker = $(e.currentTarget).find('input');

        var currDate = moment($("#NgayTao").val(), DEFAULT_MOMENT_DATE_FORMAT).toDate();
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
        $("#NgayTao").val(selectedDate);
    });
}

function BindSoSanh() {
    $('body').off('change', '.thucte');
    $('body').on('change', '.thucte', function () {
        var tr = $(this).parents('tr:first');
        tr.find("span[id*='ChenhLech']").html(tr.find("input[id*='ThucTe']").val() - tr.find("input[id*='TonKho']").val());
    });
}

function BindAutocomplete(elem, sName) {
    var url = "/Thuocs/GetThuocs";
    if (sName == "maThuoc") {
        url = "/Thuocs/GetThuocsByMa";
    }
    elem.autocomplete({
        minLength: 2,
        source: function (request, response) {
            var term = request.term;
            currentTerm = term;
            if (term in cache) {
                response(cache[term]);
                return;
            }
            response(null);
            //get json : là hàm truy vấn
            $.getJSON(url, request, function (data, status, xhr) {
                cache[term] = data;
                response(data);
            });
        },
        messages: {
            noResults: "",
            results: ""
        },
        focus: function (event, ui) {
            return false;
        },
        select: function (event, ui) {
            if (sName == "maThuoc") {
                $(this).val(ui.item.maThuoc);
                $("input#txtByTen").val(ui.item.label);
            } else {
                $(this).val(ui.item.label);
                $("input#txtByMa").val(ui.item.maThuoc);
            }
            $("input#txtThuocId").val(ui.item.value);
            $("input#txtSoLuong").val(ui.item.soLuong);

            return false;
        }
    }).each(function () {
        $(this).autocomplete('instance')._renderItem = function (ul, item) {
            return $("<li>")
                .append("<a>" + item.label + "<br>" + item.desc + "</a>")
                .appendTo(ul);
        };
    });
}

function GetThuocsByNhom(nhomId) {
    $.ajax({
        url: '/Thuocs/GetThuocsByNhom',
        data: { nhomId: nhomId },
        traditional: true,
        success: function (result) {
            if (result[0].Message != null)
                alert(result[0].Message);
            for (var i = 0; i < result.length; i++) {
                var maThuoc = result[i].MaThuoc;
                if (maThuoc == "")
                    return;
                var thuocId = result[i].ThuocId;
                var tenThuoc = result[i].TenThuoc;
                if (result[i].SoLuong == null)
                    result[i].SoLuong = 0;
                var soLuong = result[i].SoLuong;
                AddRow(thuocId, maThuoc, tenThuoc, soLuong);

            }
        }
    });
}

//Handle the insert data
function AddThuocs(parameters) {
    if ($("select#MaNhomThuoc").val() != '') {
        GetThuocsByNhom($("select#MaNhomThuoc").val());
    } else {
        var maThuoc = $("input#txtByMa").val();
        if (maThuoc == "")
            return;
        var tenThuoc = $("input#txtByTen").val();
        var soLuong = $("input#txtSoLuong").val();
        var thuocId = $("input#txtThuocId").val();
        AddRow(thuocId, maThuoc, tenThuoc, soLuong);
    }
}

function AddRow(thuocId, maThuoc, tenThuoc, soLuong) {
    $('table#tbl tbody tr').each(function () {

    });

    var row = [
        GenRowData(thuocId, "thuocId") + GenRowData(maNhaThuoc, "maNhaThuoc"), maThuoc,
        tenThuoc,
        GenRowData(soLuong, "tonKho"),
        GenRowData(0, "thucTe"), '<span class="display-mode pNumber" id="Thuocs_'+ currentNum +'__ChenhLech"> ' + 0 +'</span>'
    ];
    tbl.row.add(row).draw();
    BindSoSanh();
    currentNum++;
}


function GenRowData(data, dType) {
    var thuocTemplate = '<input class="form-control edit-mode text-box single-line" data-val="true" data-val-number="The field Thuoc must be a number." data-val-required="The Thuoc field is required." id="PhieuKiemKeChiTiets_-1__ThuocId" name="PhieuKiemKeChiTiets[-1].ThuocId" type="number" value="0" style="display: none;">';
    var tonKhoTemplate = '<span class="display-mode" style="display: inline;" id="PhieuNhapChiTiets_-1__TonKhoSp">' + data + '</span><input class="form-control edit-mode text-box single-line" data-val="true" data-val-number="The field Ton Kho must be a number." data-val-required="The Ton Kho field is required." id="PhieuKiemKeChiTiets_-1__TonKho" name="PhieuKiemKeChiTiets[-1].TonKho" type="number" value="0" style="display: none;">';
    var thucTeTemplate = '<input class="form-control edit-mode text-box single-line thucte" data-val="true" data-val-number="The field Thuc Te must be a number." data-val-required="The Thuc Te field is required." id="PhieuKiemKeChiTiets_-1__ThucTe" name="PhieuKiemKeChiTiets[-1].ThucTe" type="number" value="0">';
    var maNhaThuocTemplate = '<input class="form-control edit-mode text-box single-line" data-val="true" data-val-number="The field Ma Nha Thuoc must be a number." data-val-required="The Ma Nha Thuoc field is required." id="PhieuKiemKeChiTiets_-1__MaNhaThuoc" name="PhieuKiemKeChiTiets[-1].MaNhaThuoc" value="0" style="display: none;">';

    var result = "";
    if (dType == "thuocId")
        result = thuocTemplate.replace(/_-1__/g, "_" + currentNum + "__").replace(/\[-1\]/g, "[" + currentNum + "]").replace('value="0"', 'value="' + data + '"');
    else if (dType == "tonKho")
        result = tonKhoTemplate.replace(/_-1__/g, "_" + currentNum + "__").replace(/\[-1\]/g, "[" + currentNum + "]").replace('value="0"', 'value="' + data + '"');
    else if (dType == "thucTe")
        result = thucTeTemplate.replace(/_-1__/g, "_" + currentNum + "__").replace(/\[-1\]/g, "[" + currentNum + "]").replace('value="0"', 'value="' + data + '"');
    else if (dType == "maNhaThuoc")
        result = maNhaThuocTemplate.replace(/_-1__/g, "_" + currentNum + "__").replace(/\[-1\]/g, "[" + currentNum + "]").replace('value="0"', 'value="' + data + '"');

    return result;
}

function PostCanKho() {
    $("input#MaPhieuNhap").val(0);
    $("input#MaPhieuXuat").val(0);
}


function KiemKePostModel(thuocId, tonKho, thucTe) {
    this.ThuocId = thuocId,
        this.TonKho = tonKho,
        this.ThucTe = thucTe;
}
