$(document).ready(function () {
    //window.baseUrl = $("#baseUrl").val();
    window.baseUrl = window.location.protocol + "//" + window.location.hostname + ":" + window.location.port;
    console.log(window.baseUrl);
    //Setting
    $('.btn-setting-new').on('click', function () {
        var url = window.baseUrl + $(this).attr('href');
        $.get(url).done(function (data) {
            $('#setting-form-holder').html(data);
            $(data).find('script').each(function () {
                eval($(this).text());
            });
        });
        return false;
    });

    $('#settings').on('click', '.setting-edit', function (e) {
        var url = window.baseUrl + $(this).attr('href');
        $.get(url).done(function (data) {
            $('#setting-form-holder').html(data);
            $(data).find('script').each(function () {
                eval($(this).text());
            });
        });

        e.preventDefault();

    });


    // Don vi tinh
    //window.baseUrl = window.location.protocol + "//" + window.location.hostname;

    /// them moi
    $('.btn-dvt-new').on('click', function () {
        var url = window.baseUrl + $(this).attr('href');
        $.get(url).done(function (data) {
            $('#dvt-form-holder').html(data);
            $(data).find('script').each(function () {
                eval($(this).text());
            });
        });
        return false;
    });
    $('#unit').on('click', '.dvt-edit,.dvt-delete', function (e) {
        if ($(this).hasClass('dvt-edit')) {
            editDvt(window.baseUrl + $(this).attr('href'), $(this).data('id'));
        } else if ($(this).hasClass('dvt-delete')) {
            deleteDvt(window.baseUrl + $(this).attr('href'), $(this).data('id'));
        }
        e.preventDefault();

    });
    function deleteDvt(url, id) {
        $.get(url).done(function (data) {
            $('#dvt-form-holder').html(data);
            $(data).find('script').each(function () {
                eval($(this).text());
            });
        });
    }
    function editDvt(url, id) {
        $.get(url).done(function (data) {
            $('#dvt-form-holder').html(data);
            $(data).find('script').each(function () {
                eval($(this).text());
            });
        });
    }

    // dang bao che
    $('.btn-dbc-new').on('click', function () {
        var url = window.baseUrl + $(this).attr('href');
        $.get(url).done(function (data) {
            $('#dbc-form-holder').html(data);
            $(data).find('script').each(function () {
                eval($(this).text());
            });
        });
        return false;
    });
    $('#dangbaoche').on('click', '.dbc-edit,.dbc-delete', function (e) {
        if ($(this).hasClass('dbc-edit')) {
            editDbc(window.baseUrl + $(this).attr('href'), $(this).data('id'));
        } else if ($(this).hasClass('dbc-delete')) {
            deleteDbc(window.baseUrl + $(this).attr('href'), $(this).data('id'));
        }
        e.preventDefault();

    });
    function deleteDbc(url, id) {
        $.get(url).done(function (data) {
            $('#dbc-form-holder').html(data);
            $(data).find('script').each(function () {
                eval($(this).text());
            });
        });
    }
    function editDbc(url, id) {
        $.get(url).done(function (data) {
            $('#dbc-form-holder').html(data);
            $(data).find('script').each(function () {
                eval($(this).text());
            });
        });
    }

});