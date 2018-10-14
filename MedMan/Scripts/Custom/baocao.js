$(".input-datetimepicker").datepicker({
    format: DEFAULT_DATE_PICKER_FORMAT,
    changeMonth: true,
    changeYear: true,
    endDate: 0,
    maxViewMode: 2,
    defaultDate: new Date(),
    minDate: MIN_PRODUCTION_DATA_DATE,
    language: 'vi',
    autoclose: true,
    onSelect: function (dateText, inst) {
        $(inst).closest('div').find('input.datepicker-target').val(dateText);
    },
    onClose: function (selectedDate) {
        if ($(this).hasClass('from')) {
            $(".input-datetimepicker.to").datepicker("option", "minDate", selectedDate);
        }
            else
        {
            $(".input-datetimepicker.from").datepicker("option", "maxDate", selectedDate);
        }
            
    }
}).on('changeDate', function (dateEvent) {
    var selectedDate = moment(dateEvent.date).format(DEFAULT_MOMENT_DATE_FORMAT);
    var inputTarget = $(dateEvent.currentTarget).find('input.datepicker-target');
    inputTarget.val(selectedDate);
    if ($(this).hasClass('from')) {
        $(".input-datetimepicker.to").datepicker("option", "minDate", selectedDate);
    }
    else {
        $(".input-datetimepicker.from").datepicker("option", "maxDate", selectedDate);
    }
});

$('.btn-datepicker').on('click', function () {
    $(this).closest('div').find('input.input-datetimepicker').datepicker('show');
});
//function theoKhachHang() {
//    // show hide by type
//    $('select[name=Type]').on('change', function () {
//        var val = $(this).val();
//        if (val == "all") {
//            $('select[name=MaKhachHang]').hide().attr('disabled', 'disabled');
//            $('select[name=MaNhomKhachHang]').hide().attr('disabled', 'disabled');

//        } else if (val == "byname") {
//            $('select[name=MaKhachHang]').show().removeAttr('disabled');
//            $('select[name=MaNhomKhachHang]').hide().attr('disabled', 'disabled');
            
//        } else if (val == "bygroup") {
//            $('select[name=MaKhachHang]').hide().attr('disabled', 'disabled');
//            $('select[name=MaNhomKhachHang]').show().removeAttr('disabled');
//        }
//    });
//    var val =  $('select[name=Type]').val();
//    if (val == "all") {
//        $('select[name=MaKhachHang]').hide().attr('disabled', 'disabled');
//        $('select[name=MaNhomKhachHang]').hide().attr('disabled', 'disabled');

//    } else if (val == "byname") {
//        $('select[name=MaKhachHang]').show().removeAttr('disabled');
//        $('select[name=MaNhomKhachHang]').hide().attr('disabled', 'disabled');

//    } else if (val == "bygroup") {
//        $('select[name=MaKhachHang]').hide().attr('disabled', 'disabled');
//        $('select[name=MaNhomKhachHang]').show().removeAttr('disabled');
//    }
//    // show hide from/ to date
//    $('input[name=Period]').on('change', function () {
//        if ($(this).val() == "period") {
//            $('#period').show();
//        } else {
//            $('#period').hide();
//        }
//    });
//    if ($('input[name=Period]:checked').val() == "period") {
//        $('#period').show();
//    } else {
//        $('#period').hide();
//    }
//    // prepare for printing
//    $('.btn-print').on('click', function () {
//        var userFilter = $('select[name=Type]').val();
//        var periodFilter = $('input[name=Period]:checked').val();
//        if (userFilter == "all") {
//            $('#user-filter').html("Tổng kết hết");
//        } else if (userFilter == "byname") {
//            $('#user-filter').html("Tổng kết theo tên");
//        } else if (userFilter == "bygroup") {
//            $('#user-filter').html("Tổng kết theo nhóm");
//        }
//        if (periodFilter == 'period') {
//            $('#report-period').html("Từ ngày " + $('input[name=From]').val() + " đến ngày " + $('input[name=To]').val());
//        } else {
//            $('#report-period').html("Tổng kết hết");
//        }
//        window.print();
//    });
//    $('.btn-excel').on('click', function () {
//        $(this).parents('form').find('input[name=export]').val(1);
//    });
//}
function theoNhanVien() {
    // show hide from/ to date
    $('input[name=Period]').on('change', function () {
        if ($(this).val() == "period") {
            $('#period').show();
        } else {
            $('#period').hide();
        }
    });
    if ($('input[name=Period]:checked').val() == "period") {
        $('#period').show();
    } else {
        $('#period').hide();
    }
    // prepare for printing
    $('.btn-print').on('click', function () {
        var userFilter = $('select[name=UserId]').val();
        var periodFilter = $('input[name=Period]:checked').val();
        if (userFilter) {
            $('#user-filter').html("Tổng kết theo tên");
        } else {
            $('#user-filter').html("Tổng kết hết");
        }
        if (periodFilter == 'period') {
            $('#report-period').html("Từ ngày " + $('input[name=From]').val() + " đến ngày " + $('input[name=To]').val());
        } else {
            $('#report-period').html("Tổng kết hết");
        }
        window.print();
    });
    // prepare for exporting to excel
    $('.btn-excel').on('click', function () {
        $(this).parents('form').find('input[name=export]').val(1);
    });
    $('input[value=Xem]').on('click', function () {
        $(this).parents('form').find('input[name=export]').val('');
    });
}
function tongHop() {
    // show hide from/ to date
    $('input[name=Period]').on('change', function () {
        if ($(this).val() == "period") {
            $('#period').show();
        } else {
            $('#period').hide();
        }
    });
    if ($('input[name=Period]:checked').val() == "period") {
        $('#period').show();
    } else {
        $('#period').hide();
    }
   
}

function theoBacSy() {
    // show hide from/ to date
    $('input[name=Period]').on('change', function () {
        if ($(this).val() == "period") {
            $('#period').show();
        } else {
            $('#period').hide();
        }
    });
    if ($('input[name=Period]:checked').val() == "period") {
        $('#period').show();
    } else {
        $('#period').hide();
    }
    // prepare for printing
    $('.btn-print').on('click', function () {
        var userFilter = $('select[name=BacSyId]').val();
        var periodFilter = $('input[name=Period]:checked').val();
        if (userFilter) {
            $('#user-filter').html("Tổng kết theo tên");
        } else {
            $('#user-filter').html("Tổng kết hết");
        }
        if (periodFilter == 'period') {
            $('#report-period').html("Từ ngày " + $('input[name=From]').val() + " đến ngày " + $('input[name=To]').val());
        } else {
            $('#report-period').html("Tổng kết hết");
        }
        window.print();
    });
    $('.btn-excel').on('click', function () {
        $(this).parents('form').find('input[name=export]').val(1);
    });
    $('input[value=Xem]').on('click', function () {
        $(this).parents('form').find('input[name=export]').val('');
    });
}
function theoMatHang() {
    // show hide by type
    $('select[name=Type]').on('change', function () {
        var val = $(this).val();
        if (val == "all") {
            $('input[name=SearchTerm]').hide().attr('disabled', 'disabled');
            $('select[name=MaNhomThuoc]').hide().attr('disabled', 'disabled');

        } else if (val == "byname") {
            $('input[name=SearchTerm]').show().removeAttr('disabled');
            $('select[name=MaNhomThuoc]').hide().attr('disabled', 'disabled');

        } else if (val == "bygroup") {
            $('input[name=SearchTerm]').hide().attr('disabled', 'disabled');
            $('select[name=MaNhomThuoc]').show().removeAttr('disabled');
        }
    });
    var val = $('select[name=Type]').val();
    if (val == "all") {
        $('input[name=SearchTerm]').attr('disabled', 'disabled').hide();
        $('select[name=MaNhomThuoc]').hide().attr('disabled', 'disabled');

    } else if (val == "byname") {
        $('input[name=SearchTerm]').removeAttr('disabled').show();
        $('select[name=MaNhomThuoc]').hide().attr('disabled', 'disabled');

    } else if (val == "bygroup") {
        $('input[name=SearchTerm]').attr('disabled', 'disabled').hide();
        $('select[name=MaNhomThuoc]').show().removeAttr('disabled');
    }
    // show hide from/ to date
    $('input[name=Period]').on('change', function () {
        if ($(this).val() == "period") {
            $('#period').show();
        } else {
            $('#period').hide();
        }
    }); 
    if ($('input[name=Period]:checked').val() == "period") {
        $('#period').show();
    } else {
        $('#period').hide();
    }
    // prepare for printing
    $('.btn-print').on('click', function () {
        var userFilter = $('select[name=Type]').val();
        var periodFilter = $('input[name=Period]:checked').val();
        if (userFilter == "all") {
            $('#user-filter').html("Tổng kết hết");
        } else if (userFilter == "byname") {
            $('#user-filter').html("Tổng kết theo tên");
        } else if (userFilter == "bygroup") {
            $('#user-filter').html("Tổng kết theo nhóm");
        }
        if (periodFilter == 'period') {
            $('#report-period').html("Từ ngày " + $('input[name=From]').val() + " đến ngày " + $('input[name=To]').val());
        } else {
            $('#report-period').html("Tổng kết hết");
        }
        window.print();
    });
    $('.btn-excel').on('click', function () {
        $(this).parents('form').find('input[name=export]').val(1);
    });
    $('input[value=Xem]').on('click', function() {
        $(this).parents('form').find('input[name=export]').val('');
    });
}
function theoKhachHang() {
    // show hide by type
    $('select[name=Type]').on('change', function () {
        var val = $(this).val();
        if (val == "all") {
            $('select[name=MaKhachHang]').hide().attr('disabled', 'disabled');
            $('select[name=MaNhomKhachHang]').hide().attr('disabled', 'disabled');

        } else if (val == "byname") {
            $('select[name=MaKhachHang]').show().removeAttr('disabled');
            $('select[name=MaNhomKhachHang]').hide().attr('disabled', 'disabled');

        } else if (val == "bygroup") {
            $('select[name=MaKhachHang]').hide().attr('disabled', 'disabled');
            $('select[name=MaNhomKhachHang]').show().removeAttr('disabled');
        }
    });
    var val = $('select[name=Type]').val();
    if (val == "all") {
        $('select[name=MaKhachHang]').hide().attr('disabled', 'disabled');
        $('select[name=MaNhomKhachHang]').hide().attr('disabled', 'disabled');

    } else if (val == "byname") {
        $('select[name=MaKhachHang]').show().removeAttr('disabled');
        $('select[name=MaNhomKhachHang]').hide().attr('disabled', 'disabled');

    } else if (val == "bygroup") {
        $('select[name=MaKhachHang]').hide().attr('disabled', 'disabled');
        $('select[name=MaNhomKhachHang]').show().removeAttr('disabled');
    }
    // show hide from/ to date
    $('input[name=Period]').on('change', function () {
        if ($(this).val() == "period") {
            $('#period').show();
        } else {
            $('#period').hide();
        }
    });
    if ($('input[name=Period]:checked').val() == "period") {
        $('#period').show();
    } else {
        $('#period').hide();
    }
    // prepare for printing
    $('.btn-print').on('click', function () {
        var userFilter = $('select[name=Type]').val();
        var periodFilter = $('input[name=Period]:checked').val();
        if (userFilter == "all") {
            $('#user-filter').html("Tổng kết hết");
        } else if (userFilter == "byname") {
            $('#user-filter').html("Tổng kết theo tên");
        } else if (userFilter == "bygroup") {
            $('#user-filter').html("Tổng kết theo nhóm");
        }
        if (periodFilter == 'period') {
            $('#report-period').html("Từ ngày " + $('input[name=From]').val() + " đến ngày " + $('input[name=To]').val());
        } else {
            $('#report-period').html("Tổng kết hết");
        }
        window.print();
    });
    $('.btn-excel').on('click', function () {
        $(this).parents('form').find('input[name=export]').val(1);
    });
    $('input[value=Xem]').on('click', function () {
        $(this).parents('form').find('input[name=export]').val('');
    });
}
function theoNhaCungCap() {
    // show hide by type
    $('select[name=Type]').on('change', function () {
        var val = $(this).val();
        if (val == "all") {
            $('select[name=MaNhaCungCap]').hide().attr('disabled', 'disabled');
            $('select[name=MaNhomNhaCungCap]').hide().attr('disabled', 'disabled');

        } else if (val == "byname") {
            $('select[name=MaNhaCungCap]').show().removeAttr('disabled');
            $('select[name=MaNhomNhaCungCap]').hide().attr('disabled', 'disabled');

        } else if (val == "bygroup") {
            $('select[name=MaNhaCungCap]').hide().attr('disabled', 'disabled');
            $('select[name=MaNhomNhaCungCap]').show().removeAttr('disabled');
        }
    });
    var val = $('select[name=Type]').val();
    if (val == "all") {
        $('select[name=MaNhaCungCap]').hide().attr('disabled', 'disabled');
        $('select[name=MaNhomNhaCungCap]').hide().attr('disabled', 'disabled');

    } else if (val == "byname") {
        $('select[name=MaNhaCungCap]').show().removeAttr('disabled');
        $('select[name=MaNhomNhaCungCap]').hide().attr('disabled', 'disabled');

    } else if (val == "bygroup") {
        $('select[name=MaNhaCungCap]').hide().attr('disabled', 'disabled');
        $('select[name=MaNhomNhaCungCap]').show().removeAttr('disabled');
    }
    // show hide from/ to date
    $('input[name=Period]').on('change', function () {
        if ($(this).val() == "period") {
            $('#period').show();
        } else {
            $('#period').hide();
        }
    });
    if ($('input[name=Period]:checked').val() == "period") {
        $('#period').show();
    } else {
        $('#period').hide();
    }
    // prepare for printing
    $('.btn-print').on('click', function () {
        var userFilter = $('select[name=Type]').val();
        var periodFilter = $('input[name=Period]:checked').val();
        if (userFilter == "all") {
            $('#user-filter').html("Tổng kết hết");
        } else if (userFilter == "byname") {
            $('#user-filter').html("Tổng kết theo tên");
        } else if (userFilter == "bygroup") {
            $('#user-filter').html("Tổng kết theo nhóm");
        }
        if (periodFilter == 'period') {
            $('#report-period').html("Từ ngày " + $('input[name=From]').val() + " đến ngày " + $('input[name=To]').val());
        } else {
            $('#report-period').html("Tổng kết hết");
        }
        window.print();
    });
    $('.btn-excel').on('click', function () {
        $(this).parents('form').find('input[name=export]').val(1);
    });
    $('input[value=Xem]').on('click', function () {
        $(this).parents('form').find('input[name=export]').val('');
    });
}
function theoKhoHang() {
    // show hide by type
    $('select[name=Type]').on('change', function () {
        var val = $(this).val();
        if (val == "all") {
            $('input[name=SearchTerm]').hide().attr('disabled', 'disabled');
            $('select[name=MaNhomThuoc]').hide().attr('disabled', 'disabled');

        } else if (val == "byname") {
            $('input[name=SearchTerm]').show().removeAttr('disabled');
            $('select[name=MaNhomThuoc]').hide().attr('disabled', 'disabled');

        } else if (val == "bygroup") {
            $('input[name=SearchTerm]').hide().attr('disabled', 'disabled');
            $('select[name=MaNhomThuoc]').show().removeAttr('disabled');
        }
    });
    var val = $('select[name=Type]').val();
    if (val == "all") {
        $('input[name=SearchTerm]').attr('disabled', 'disabled').hide();
        $('select[name=MaNhomThuoc]').hide().attr('disabled', 'disabled');

    } else if (val == "byname") {
        $('input[name=SearchTerm]').removeAttr('disabled').show();
        $('select[name=MaNhomThuoc]').hide().attr('disabled', 'disabled');

    } else if (val == "bygroup") {
        $('input[name=SearchTerm]').attr('disabled', 'disabled').hide();
        $('select[name=MaNhomThuoc]').show().removeAttr('disabled');
    }
    // show hide from/ to date
    $('input[name=Period]').on('change', function () {
        if ($(this).val() == "period") {
            $('#period').show();
        } else {
            $('#period').hide();
        }
    });
    if ($('input[name=Period]:checked').val() == "period") {
        $('#period').show();
    } else {
        $('#period').hide();
    }
    // prepare for printing
    $('.btn-print').on('click', function () {
        var userFilter = $('select[name=Type]').val();
        var periodFilter = $('input[name=Period]:checked').val();
        if (userFilter == "all") {
            $('#user-filter').html("Tổng kết hết");
        } else if (userFilter == "byname") {
            $('#user-filter').html("Tổng kết theo tên");
        } else if (userFilter == "bygroup") {
            $('#user-filter').html("Tổng kết theo nhóm");
        }
        if (periodFilter == 'period') {
            $('#report-period').html("Từ ngày " + $('input[name=From]').val() + " đến ngày " + $('input[name=To]').val());
        } else {
            $('#report-period').html("Tổng kết hết");
        }
        window.print();
    });
    $('.btn-excel').on('click', function () {
        $(this).parents('form').find('input[name=export]').val(1);
    });
    $('input[value=Xem]').on('click', function () {
        $(this).parents('form').find('input[name=export]').val('');
    });
}
var cache = {};
var currentTerm = "";
function BindAutocomplete(selectorElem, selectorOutput) {
    var url = $('input[name=baseUrl]').val() + "Thuocs/GetThuocsByMaOrTen";
    $(selectorElem).autocomplete({
        minLength: 2,
        source: function (request, response) {
            var term = request.term;
            currentTerm = selectorElem + term;
            if (term in cache) {
                response(cache[selectorElem + term]);
                return;
            }
            $.getJSON(url, request, function (data, status, xhr) {
                cache[selectorElem + term] = data;
                response(data);
            });
        },
        messages: {
            noResults: "Không tìm thấy"//,
            // results: ""
        },
        focus: function (event, ui) {
            $(this).val(ui.item.tenThuoc);
            return false;
        },
        select: function (event, ui) {
            $(this).val(ui.item.tenThuoc);
            $(selectorOutput).val(ui.item.value);
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