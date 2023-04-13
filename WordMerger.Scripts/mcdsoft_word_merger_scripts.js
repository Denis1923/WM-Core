function StartWordMergerUI(isPackage, quiredType, autoUpload, enableIgnoreCondition) {
    try {
        Xrm.Page.data.save().then(function success() {
            var pageUrl = GetSysParamValue("mcdsoft_wordmerger_ui_web_url");

            var url = pageUrl + "?";

            if (isPackage == null || isPackage == "undefined") {
                url += "reportType=all";
            }
            else if (isPackage && isPackage == true) {
                url += "reportType=package";
            }
            else {
                url += "reportType=document";
            }

            if (autoUpload == true) {
                url += "&autoUpload=true";
            }

            url += "&entityName=" + escape(Xrm.Page.data.entity.getEntityName());
            url += "&quiredType=" + (quiredType == null || quiredType == undefined ? "docsreps" : escape(quiredType));
            url += "&ids=" + Xrm.Page.data.entity.getId();
            url += "&userId=" + Xrm.Page.context.getUserId();

            if (enableIgnoreCondition != null)
                url += "&enableIgnoreCondition=" + enableIgnoreCondition;

            var width = 570;
            var height = 620;
            var left = (screen.width / 2) - (width / 2);
            var top = (screen.height / 2) - (height / 2);
            window.open(url + "#", "x", "width=" + width + "px,height=" + height + "px,toolbar=no, location=no, directories=no, status=no, menubar=no, scrollbars=no, resizable=no, copyhistory=no, top=" + top + ", left=" + left);
        },
            function error(message) { console.log(message); });
    }
    catch (ex) {
        alert("Ошибка: " + ex.message);
    }
}

function StartWordMergerUIFromHome(entityName, quiredType, isPackage, ids) {
    try {
        var pageUrl = GetSysParamValue("mcdsoft_wordmerger_ui_web_url");
        var url = pageUrl + "?";

        if (isPackage == null || isPackage == "undefined") {
            url += "reportType=all";
        }
        else if (isPackage && isPackage == true) {
            url += "reportType=package";
        }
        else {
            url += "reportType=document";
        }

        if (ids == null || ids == "undefined")
            ids = "{00000000-0000-0000-0000-000000000000}";

        url += "&entityName=" + escape(entityName);
        url += "&quiredType=" + (quiredType == null || quiredType == undefined ? "docsreps" : escape(quiredType));
        url += "&ids=" + ids;
        url += "&userId=" + Xrm.Page.context.getUserId();

        var width = 570;
        var height = 620;
        var left = (screen.width / 2) - (width / 2);
        var top = (screen.height / 2) - (height / 2);
        window.open(url + "#", "x", "width=" + width + "px,height=" + height + "px,toolbar=no, location=no, directories=no, status=no, menubar=no, scrollbars=yes, resizable=no, copyhistory=no, top=" + top + ", left=" + left);
    }
    catch (ex) {
        alert("Ошибка: " + ex.message);
    }
}

function GetSysParamValue(paramName) {
    var sysParamEntity = "mcdsoft_syst_parameters";
    var query = "/" + Xrm.Page.context.getOrgUniqueName() + "/xrmservices/2011/organizationdata.svc/" + sysParamEntity + "Set?$select=" + paramName + "&$filter=statecode/Value eq 0";
    var xmlhttp = new XMLHttpRequest();
    xmlhttp.open('GET', query, false);
    xmlhttp.send();
    var resultXml = xmlhttp.responseXML;
    if (resultXml.getElementsByTagName("d:" + paramName).length != 0) {
        if (resultXml.getElementsByTagName("d:" + paramName)[0].textContent == null) {
            return resultXml.getElementsByTagName("d:" + paramName)[0].text;
        }
        return resultXml.getElementsByTagName("d:" + paramName)[0].textContent;
    }
    else {
        return resultXml.getElementsByTagName(paramName)[0].textContent;
    }
}

function OpenDocument(documentName, isPackage) {

    if (!documentName) {
        alert("Имя документа является обязательным параметром.");
        return;
    }

    var pageUrl = GetSysParamValue("cmdsoft_single_document_service");

    if (pageUrl == null) {
        alert("Не задан адрес страницы WordMerger");
        return;
    }

    var url = pageUrl;
    url += '?ids=' + Xrm.Page.data.entity.getId();
    url += '&userid=' + Xrm.Page.context.getUserId();
    url += '&docname=' + escape(documentName);

    window.open(url);
}