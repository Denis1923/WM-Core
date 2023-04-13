$(document).ready(function () {
    $('#docForm').keydown(function (event) {
        if (event.keyCode == 13) {
            event.preventDefault();
            return false;
        }
    });
});

function ShowProgress() {
    setTimeout(function () {
        var modal = $('<div/>');
        modal.addClass("modal");
        $(modal).appendTo('body');

        var loading = $(".loading");
        loading.show();
        var top = Math.max($(window).height() / 2 - loading[0].offsetHeight / 2, 0);
        var left = Math.max($(window).width() / 2 - loading[0].offsetWidth / 2, 0);
        loading.css({ top: top, left: left });
    }, 50);
}

function HideProgress() {
    setTimeout(function () {
        $('.modal').remove();
        $('.loading').hide();
    }, 60);
}

function HideError() {
    if ($("#closeWindow").val() == "True") {
        CloseApp();
    }
    else {
        $('.modalError').remove();
        $('#error').hide();
    }
}

function HideConditionError() {
    if ($("#closeWindow").val() == "True") {
        CloseApp();
    }
    else {
        $('.modalError').remove();
        $('#conditionError').hide();
    }
}

function HideResult() {
    if ($("#closeWindow").val() == "True") {
        CloseApp();
    }
    else {
        $('.modal').remove();
        $('.result').hide();
    }
}

function ShowError(ajaxContext) {
    setTimeout(function () {
        var modal = $('<div/>');
        modal.addClass("modalError");
        $('body').append(modal);
        $('#errorText').html(ajaxContext.statusText);
        var error = $("#error");
        error.show();
        var top = Math.max($(window).height() / 2 - error[0].offsetHeight / 2, 0);
        var left = Math.max($(window).width() / 2 - error[0].offsetWidth / 2, 0);
        error.css({ top: top, left: left });
    }, 50);
}

function ShowErrorMessage(text) {
    setTimeout(function () {
        var modal = $('<div/>');
        modal.addClass("modalError");
        $('body').append(modal);
        $('#errorText').html(text);
        $('#errorText').css({ 'word-wrap': 'break-word', 'white-space': 'pre-wrap'});
        var error = $("#error");
        error.show();
        var top = Math.max($(window).height() / 2 - error[0].offsetHeight / 2, 0);
        var left = Math.max($(window).width() / 2 - error[0].offsetWidth / 2, 0);
        error.css({ top: top, left: left });
    }, 50);
}

function ShowConditionErrorMessage(text) {
    setTimeout(function () {
        var modal = $('<div/>');
        modal.addClass("modalError");
        $('body').append(modal);
        $('#conditionErrorText').html(text);
        $('#conditionErrorText').css({ 'word-wrap': 'break-word', 'white-space': 'pre-wrap' });
        var error = $("#conditionError");
        error.show();
        var top = Math.max($(window).height() / 2 - error[0].offsetHeight / 2, 0);
        var left = Math.max($(window).width() / 2 - error[0].offsetWidth / 2, 0);
        error.css({ top: top, left: left });
    }, 50);
}

function ShowResultMessage(text) {
    setTimeout(function () {
        var modal = $('<div/>');
        modal.addClass("modal");
        $('body').append(modal);
        $('#resultText').html(text);
        var error = $(".result");
        error.show();
        var top = Math.max($(window).height() / 2 - error[0].offsetHeight / 2, 0);
        var left = Math.max($(window).width() / 2 - error[0].offsetWidth / 2, 0);
        error.css({ top: top, left: left });
    }, 50);
}

function ShowModalDialog(id) {
    setTimeout(function () {
        var modal = $('<div/>');
        modal.addClass("modal");
        $(modal).appendTo('body');

        var loading = $('#' + id);
        loading.show();
        var top = Math.max($(window).height() / 2 - loading[0].offsetHeight / 2, 0);
        var left = Math.max($(window).width() / 2 - loading[0].offsetWidth / 2, 0);
        loading.css({ top: top, left: left });
    }, 100);
}

function HideModalDialog(id) {
    $('.modal').remove();
    $('#' + id).hide();
}

function CloseApp() {
    window.open('', '_self', '');
    window.close();
}