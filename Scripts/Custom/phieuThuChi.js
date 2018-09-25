var id = "";
var isLoaiPhieu = false;
function InitPage(isEdit) {
    BindNgayTaoPosition();
    ChonDoiTac();
    // tra het event
    $('button[name=trahet]').on('click', function () {
        $('input[name=Amount]').val($('#spNo').val());
        return false;
    });
}

function BindNgayTaoPosition() {
    $(document.body).on('click', '.daterange-picker', function (e) {
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

function ChonDoiTac() {
    $("select#MaKhachHang").change(function () {
        if ($(this).val() != 'null') {
            id = $(this).val();
            GetCongNo(id, true);
        } else {
            id = "";
            $("#dvNo").hide();
        }

        isLoaiPhieu = true;
    });

    $("select#MaNhaCungCap").change(function () {
        if ($(this).val() != 'null') {
            id = $(this).val();
            GetCongNo($(this).val(), false);
        }
        else
        {
            id = "";
        }

        isLoaiPhieu = false;
    });
}

function GetCongNo(id, loaiPhieu) {
    if (id != "") {
        var date = $("#NgayTao").val();
        $.ajax({
            url: $('input[name=baseUrl]').val() + '/PhieuThuChis/GetNo',
            data: { ma: id, isKhachHang: loaiPhieu, ngaytao: date },
            cache: false,
            traditional: true,
            success: function (result) {
                $("#dvNo").show();
                $("#spNo").val(result);
                if (result <= 0) {
                    $("#spNo").hide();
                    $("#spNoZero").show();
                    $('button[name=trahet]').hide();//.parent().width('10%');;
                } else {
                    $("#spNo").show();
                    $("#spNoZero").hide();
                    $('button[name=trahet]').show();//.parent().width('10%');
                }
            }
        });
    }
    else
    {
        $("#dvNo").hide();
        $("#spNo").val("");
    }
}