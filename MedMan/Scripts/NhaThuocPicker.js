jQuery('#NhaThuocPickerModal').on('show.bs.modal', function (event) {
    var sender = jQuery(event.relatedTarget) // Button that triggered the modal
    jQuery(this).data('updatevalue', sender.data('updatevalue'));
    jQuery(this).data('updatetext', sender.data('updatetext'));
});
jQuery('#NhaThuocPickerModal table tr').on('click', function () {
    jQuery('#NhaThuocPickerModal table tr').removeClass('active');
    jQuery(this).addClass('active');
});
jQuery('#nhathuocpick').on('click', function () {
    nhaThuocMakeSelection();
    jQuery('#NhaThuocPickerModal').modal('hide');
});
jQuery('#NhaThuocPickerModal table tr').on('dblclick', function () {
    nhaThuocMakeSelection();
    jQuery('#NhaThuocPickerModal').modal('hide');
});
function nhaThuocMakeSelection() {
    var targetValue = jQuery('#NhaThuocPickerModal').data('updatevalue');
    var targetText = jQuery('#NhaThuocPickerModal').data('updatetext');
    if (jQuery('#NhaThuocPickerModal table tr.active').size()) {
        var tr = jQuery('#NhaThuocPickerModal table tr.active:first');
        jQuery(targetValue).val(tr.find("input[id*='hdfMaNhaThuoc']").val());
        jQuery(targetText).val(tr.find('td:eq(1)').text());
    }
};
$("#findnhathuoc").keyup(function (event) {
    if (event.keyCode === 13) {
        $("#btnFindNhaThuoc").click();
    }
});
//$('#unassignednhathuoc').change(function () {
//    if ($(this).is(':checked')) {
//        $('table.table-nhathuocpicker').find('tr[data-hasstore=1]').hide();
//    } else {
//        $('table.table-nhathuocpicker').find('tr[data-hasstore=1]').css('display', '');
//    }
//    $('table.table-nhathuocpicker').find('tr:eq(0)').css('display', '');
//});
$('.btn-findnhathuoc').click(function () {
    var str = $('input[name=findnhathuoc]').val();
    if (str.length > 0) {
        $('table.table-nhathuocpicker').find("tr").hide();
        $('table.table-nhathuocpicker').find("tr").filter(function () {
            if( $(this).find('td:eq(1)').text().toUpperCase().indexOf(str.toUpperCase()) > -1)
            {
                $(this).css('display', '');
                return true;
            }
            return false;
        });
    } else {
        $('table.table-nhathuocpicker').find("tr").css('display', '');
    }
    //if ($('#unassignednhathuoc').is(':checked')) {
    //    $('table.table-nhathuocpicker').find('tr[data-hasstore=1]').hide();
    //}
    $('table.table-nhathuocpicker').find('tr:eq(0)').css('display', '');
});
