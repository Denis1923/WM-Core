function IsParameterItem(id, isChooseFormat) {
    ShowProgress();

    var urlAction = "";

    var pathParts = location.pathname.split('/');

    if (pathParts[1] == '' || pathParts[1] == 'Home')
        urlAction = '/Home/IsParameterItem?lstView=' + id;
    else {

        for (var i = 1; i < pathParts.length; i++) {
            if (pathParts[i] == "Home")
                break;

            urlAction += "/" + pathParts[i];
        }

        urlAction += '/Home/IsParameterItem?lstView=' + id
    }

    $.ajax({
        url: urlAction,
        data: $('.selected').attr('id'),
        type: 'GET',
        dataType: 'html',
        cache: false,
        contentType: 'application/html; charset=utf-8',
        success: function (data) {
            if (data != "" && $(data).filter("input").length > 0 && $(data).filter("input[type='hidden']").length != $(data).filter("input").length) {
                HideProgress();
                $('#WMParametersBlock').append(data);
                ShowParamsDialog(isChooseFormat);
            }
            else if (isChooseFormat.toString().toLowerCase() == "true") {
                HideProgress();
                ShowFormatDialog();
            }
            else {
                BeforeSubmit();
                $("#docForm").submit();
            }
        },
        error: function (request, status, error) {
            HideProgress();
            ShowErrorMessage("Во время выполнения операции произошла ошибка<br/>Статус: " + request.status + "<br/>Информация: " + getErrorMessageFromRequest(request) + "<br/>Обратитесь к администратору системы");
        }
    });
}

function getErrorMessageFromRequest(request) {
    var titles = $(request.responseText).filter("title");
    return titles.length > 0 ? titles[0].text : "";
}

function IsPreviewParameterItem(id) {
    ShowProgress();

    var urlAction = "";

    var pathParts = location.pathname.split('/');

    if (pathParts[1] == '' || pathParts[1] == 'Home')
        urlAction = '/Home/IsParameterItem?lstView=' + id;
    else {

        for (var i = 1; i < pathParts.length; i++) {
            if (pathParts[i] == "Home")
                break;

            urlAction += "/" + pathParts[i];
        }

        urlAction += '/Home/IsParameterItem?lstView=' + id
    }

    $.ajax({
        url: urlAction,
        data: $('.selected').attr('id'),
        type: 'GET',
        dataType: 'html',
        contentType: 'application/html; charset=utf-8',
        success: function (data) {
            if (data != "") {
                HideProgress();
                $('#WMParametersBlock').append(data);
                ShowParamsDialog("false", "true", id);
            }
            else {
                PreviewDocument(id);
            }
        },
        error: function (request, status, error) {
            HideProgress();
            ShowErrorMessage("Во время выполнения операции произошла ошибка<br/>Статус: " + request.status + "<br/>Информация: " + getErrorMessageFromRequest(request) + "<br/>Обратитесь к администратору системы");
        }
    });
}

var clone;
var position;
var imageObj;

function PreviewDocument(id) {
    ShowProgress();

    var urlAction = "";

    var pathParts = location.pathname.split('/');

    if (pathParts[1] == '' || pathParts[1] == 'Home')
        urlAction = '/Home/PreviewDocument?lstView=' + id;
    else {

        for (var i = 1; i < pathParts.length; i++) {
            if (pathParts[i] == "Home")
                break;

            urlAction += "/" + pathParts[i];
        }

        urlAction += '/Home/PreviewDocument?lstView=' + id
    }

    $("#WMParametersBlock :input").each(function (i, obj) {

        urlAction += "&" + $(obj).attr("name") + "=" + $(obj).val();
    });

    $.ajax({
        url: urlAction,
        data: $('.selected').attr('id'),
        type: 'POST',
        dataType: 'html',
        contentType: 'application/html; charset=utf-8',
        success: function (data) {
            if (data != "") {
                HideProgress();
                $('#Preview').html(data);
                $("div#Preview img").each(function (i, obj) {
                    imageObj = obj;
                    clone = $(obj).clone();
                    $(clone).addClass("clonedItem");
                    position = $(obj).position();
                });
                ShowModalDialog('Preview');
            }
        },
        error: function (request, status, error) {
            HideProgress();
            ShowErrorMessage("Во время выполнения операции произошла ошибка<br/>Статус: " + request.status + "<br/>Информация: " + getErrorMessageFromRequest(request) + "<br/>Обратитесь к администратору системы");
        }
    });
}

function BeforeSubmit() {
    var myInterval;
    $(document).ready(
        function (e) {
            myInterval = setInterval(function () { Timer() }, 1000);
        });

    function Timer() {
        if (document.readyState == "complete") {
            HideProgress();
            clearInterval(myInterval);
        }
    }
}

function ShowParamsDialog(isChooseFormat, isPreview, id) {
    dialog = $('#ParamsDialog').dialog({
        autoOpen: false,
        height: 350,
        width: 325,
        modal: true,
        draggable: false,
        buttons: [{
            text: "Выбрать",
            id: "chooseBtn",
            click: function () {
                if (isPreview == "true") {
                    PreviewDocument(id);

                    dialog.dialog("close");
                    $('#WMParametersBlock').empty();
                }
                else if (isChooseFormat.toString().toLowerCase() == "true") {
                    dialog.dialog("close");
                    ShowFormatDialog();
                }
                else {
                    ShowProgress();
                    BeforeSubmit();
                    $("#docForm").submit();

                    dialog.dialog("close");
                    $('#WMParametersBlock').empty();
                }
            }
        },
        {
            text: "Отмена",
            id: "cancelBtn",
            click: function () {
                dialog.dialog("close");
                $('#WMParametersBlock').empty();
            },
        }],
        open: function (event, ui) {

            $(".ui-dialog-titlebar-close").hide();
            $("#ParamsDialog").show();
            $('#chooseBtn').button('disable');

            $.datepicker.setDefaults($.datepicker.regional['ru']);

            $('.dateParameter').datepicker({
                fixFocusIE: false,
                onClose: function (dateText, inst) {
                    dialog.focus();
                },
                beforeShow: function (input, inst) {
                    var result = $.browser.msie ? !this.fixFocusIE : true;
                    this.fixFocusIE = false;
                    return result;
                },
                dateFormat: "dd.mm.yy",
                changeYear: true
            });

            var f = $.find('.WMParameters');

            $(f).each(function (index) {
                $(this).change(WMParameterChanged);
            });

            WMParameterChanged();
        },
    });

    dialog.parent().appendTo($("form:first"));
    dialog.dialog("open");
}

function ShowFilterDialog() {
    dialog = $('#FilterDialog').dialog({
        autoOpen: false,
        height: 350,
        width: 325,
        modal: true,
        draggable: false,
        buttons: [{
            text: "Применить",
            id: "chooseBtn",
            click: function () {
                applyFilter();
                dialog.dialog("close");
            }
        },
        {
            text: "Отмена",
            id: "cancelBtn",
            click: function () {
                dialog.dialog("close");
            },
        }],
        open: function (event, ui) {

            $(".ui-dialog-titlebar-close").hide();
            $("#FilterDialog").show();

            $.datepicker.setDefaults($.datepicker.regional['ru']);

            $('.dateFilter').datepicker({
                fixFocusIE: false,
                onClose: function (dateText, inst) {
                    dialog.focus();
                },
                beforeShow: function (input, inst) {
                    var result = $.browser.msie ? !this.fixFocusIE : true;
                    this.fixFocusIE = false;
                    return result;
                },
                dateFormat: "dd.mm.yy",
                changeYear: true
            });

        }
    });

    dialog.parent().appendTo($("form:first"));
    dialog.dialog("open");
}

function ShowFormatDialog() {
    dialog = $('#FormatDialog').dialog({
        autoOpen: false,
        height: 150,
        width: 280,
        modal: true,
        draggable: false,
        buttons: [{
            text: "Выбрать",
            id: "chooseBtn",
            click: function () {
                $('#SaveFormat').val($('#formatBox').val());
                ShowProgress();
                BeforeSubmit();
                $("#docForm").submit();
                dialog.dialog("close");
                $('#WMParametersBlock').empty();
            }
        },
        {
            text: "Отмена",
            id: "cancelBtn",
            click: function () {
                dialog.dialog("close");
                $('#WMParametersBlock').empty();
            },
        }],
        open: function (event, ui) {

            $(".ui-dialog-titlebar-close").hide();
            $("#FormatDialog").show();           

        }
    });

    dialog.parent().appendTo($("form:first"));
    dialog.dialog("open");
}

function WMParameterChanged() {

    var enableSubmit = true;

    var f = $.find('.WMParameters');

    $(f).each(function (index) {
        if (!$(this).context.classList.contains("nullable") && $(this).val() == "") {
            enableSubmit = false;
        }
    });

    if (enableSubmit)
        $('#chooseBtn').button('enable');
    else
        $('#chooseBtn').button('disable');
}

function Zoom() {
    $(".clonedItem").animate({
        height: "450px",
        width: "250px"
    }, 200, function () { $(this).remove(); });

    $("div#Preview img").css("z-index", 1);

    var top = Math.max($(window).height() / 2 - 650 / 2, 0);
    var left = Math.max($(window).width() / 2 - 450 / 2, 0);
    $(clone).css("top", top).css("left", left).css("z-index", 1000);

    $(clone).appendTo("body").css("position", "absolute").animate({
        height: "650px",
        width: "450px"
    }, 200, function () {

        $(clone).bind("click", function (e) {
            $(clone).animate({
                height: "450px",
                width: "250px"
            }, 200, function () { $(clone).remove(); });

        });
    });
}

function setCustomStyle(backgroundColor) {
    $(".header").css("background-color", backgroundColor);
    $(".header").css("border-color", backgroundColor);
}

function applyFilter() {
    var form = $("#docForm")[0];

    var urlAction = "";

    var pathParts = location.pathname.split('/');

    if (pathParts[1] == '' || pathParts[1] == 'Home')
        urlAction = '/Home/ApplyFilters';
    else {

        for (var i = 1; i < pathParts.length; i++) {
            if (pathParts[i] == "Home")
                break;

            urlAction += "/" + pathParts[i];
        }

        urlAction += '/Home/ApplyFilters';
    }

    form.action = urlAction;

    form.submit();
}

function fieldChanged(id, isCheckBox) {
    var parentValue = isCheckBox ? $("#filter_" + id + "_value").is(":checked").toString() : $("#filter_" + id + "_value")[0].value;

    $(".ParentId").each(function (index) {
        if ($(this).val() == id) {
            var rowId = $(this).context.id.split("_")[1];
            var parentValueList = $("#filter_" + rowId + "_parentValue")[0].value.split(";;");
            var operator = $("#filter_" + rowId + "_parentOperator")[0].value;
            var isVisible = parentValueList.indexOf(parentValue) != -1;

            if (operator == "<>") {
                isVisible = !isVisible;
            }

            if (isVisible) {
                $("#" + rowId).show();
            }
            else {
                $("#" + rowId).hide();
            }
        }
    });
}

function addFontFace(name, url) {
    var newStyle = document.createElement('style');
    newStyle.appendChild(document.createTextNode("\
    @font-face {\
        font-family: " + name + ";\
        src: url('" + url + "');\
    }\
    "));

    document.head.appendChild(newStyle);
}