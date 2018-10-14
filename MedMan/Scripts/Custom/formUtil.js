function ShowHideFields() {
    $('#trSlide').slideToggle(100);
    $(".msg_head").click(function () {
        $('#trSlide').slideToggle(100);

        if ($(this).hasClass('msg_head_expanded'))
            $(this).children("span").text("[+]");
        else {
            $(this).children("span").text("[-]");
        }
        $(this).toggleClass('msg_head_expanded');
    });
}

function initialForm() {
    // input masks
    inputmask.extendDefaults({
        'autoUnmask': true
    });
    $(":input").inputmask();
    // no space in "maThuoc"
    $("input[name=MaThuoc]").on('keyup', function () {
        $(this).val($(this).val().replace(/ +?/g, ''));
    });

    // show/hid additional fields
    ShowHideFields();
}
$('input.number').keyup(function (event) {

    // skip for arrow keys
    if (event.which >= 37 && event.which <= 40) return;

    // format number
    $(this).val(function (index, value) {
        return value
          .replace(/\D/g, "")
          .replace(/\B(?=(\d{3})+(?!\d))/g, ".")
        ;
    });
});