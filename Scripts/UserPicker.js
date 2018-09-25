jQuery('#UserPickerModal').on('show.bs.modal', function (event) {
    var sender = jQuery(event.relatedTarget) // Button that triggered the modal
    jQuery(this).data('updatevalue', sender.data('updatevalue'));
    jQuery(this).data('updatetext', sender.data('updatetext'));
});
jQuery('#UserPickerModal table tr').on('click', function () {
    jQuery('#UserPickerModal table tr').removeClass('active');
    jQuery(this).addClass('active');
});
jQuery('#userpick').on('click', function () {
    makeSelection();
    jQuery('#UserPickerModal').modal('hide');
});
jQuery('#UserPickerModal table tr').on('dblclick', function () {
    makeSelection();
    jQuery('#UserPickerModal').modal('hide');
});
function makeSelection() {
    var targetValue = jQuery('#UserPickerModal').data('updatevalue');
    var targetText = jQuery('#UserPickerModal').data('updatetext');
    if (jQuery('#UserPickerModal table tr.active').size()) {
        var tr = jQuery('#UserPickerModal table tr.active:first');
        jQuery(targetValue).val(tr.find("input[id*='hdfUserId']").val());        
        jQuery(targetText).val(tr.find('td:eq(1)').text());
    }
}

$('#unassigneduser').change(function () {
    if ($(this).is(':checked')) {
        $('table.table-userpicker').find('tr[data-hasstore=1]').hide();
    } else {
        $('table.table-userpicker').find('tr[data-hasstore=1]').css('display','');
    }
    $('table.table-userpicker').find('tr:eq(0)').css('display', '');
});
$('.btn-finduser').click(function () {
    var str = $('input[name=finduser]').val();
    if (str.length > 0) {
        $('table.table-userpicker').find("tr").hide();
        $('table.table-userpicker').find("tr").filter(function () {
            if ($(this).find('td:eq(1)').text().toUpperCase().indexOf(str.toUpperCase()) > -1) {
                $(this).css('display', '');
                return true;
            }
            return false;
        });
    } else {
        $('table.table-userpicker').find("tr").css('display','');
    }
    if ($('#unassigneduser').is(':checked')) {
        $('table.table-userpicker').find('tr[data-hasstore=1]').hide();
    }
    $('table.table-userpicker').find('tr:eq(0)').css('display', '');
});
