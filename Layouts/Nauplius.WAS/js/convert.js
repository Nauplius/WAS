function hideWaitDiv() {
    var spinDiv = $('#spinwait');
    var textDiv = $('#textwait');
    spinDiv.hide();
    textDiv.hide();
}

_spBodyOnLoadFunctionNames.push("hideWaitDiv");

function RewriteOutput(elementId, inputFile, dropDownList) {
    var text = document.getElementById(elementId.id);
    var ddl = document.getElementById(dropDownList.id).value;

    if (text.value.indexOf('.') !== -1) {
        text.value = text.value.substr(0, text.value.lastIndexOf('.'));
    }

    text.value = text.value + "." + ddl;

    if (text.value == "." + ddl) {
        text.value = "";
    }
}

function ShowLocationTree(elementId) {
    var tBox = document.getElementById(elementId.id);
    var siteBrowserUrl = "";

    if (_spPageContextInfo.siteServerRelativeUrl == "/") {
        siteBrowserUrl = "/_layouts/15/Nauplius.WAS/SiteBrowser.aspx?ParentElement=" + tBox.id + "&IsDlg=1";
    } else {
        siteBrowserUrl = _spPageContextInfo.siteServerRelativeUrl + "/_layouts/15/Nauplius.WAS/SiteBrowser.aspx?ParentElement=" + tBox.id + "&IsDlg=1";
    }

    var options = {
        url: siteBrowserUrl,
        args: null,
        title: 'Save Location',
        dialogReturnValueCallback: dialogCallback,
    };
    SP.SOD.execute('sp.ui.dialog.js', 'SP.UI.ModalDialog.showModalDialog', options);

    function dialogCallback(dialogResult, returnValue) {
        if (returnValue != null) {
            var tBox1 = document.getElementById(returnValue[1]);
            if (document.all) {
                tBox1.innerText = returnValue[0]; //IE8 and below support
            } else {
                tBox1.textContent = returnValue[0]; //Everything else
            }
        }
    }
}

function ShowSettings(rowId, fileTypeDropDownList, fileName, fileSettings) {

    var itemSettingsUrl = "";
    var fileTypeDdl = document.getElementById(fileTypeDropDownList);
    var fileType = fileTypeDdl.options[fileTypeDdl.selectedIndex].value;
    var tbox1 = document.getElementById(fileSettings);
    /*
    var loc = window.location.href.substr(0, window.location.href.indexOf('?'));

    $.ajax({
        type: "GET",
        url: loc + "/LoadData",
        async: false,
        contentType: "application/json; charset=utf-8",
    })
    */

    if (_spPageContextInfo.siteServerRelativeUrl == "/") {
        itemSettingsUrl = "/_layouts/15/Nauplius.WAS/ConversionSettings.aspx?ParentElement=" + rowId + "&FileType=" + fileType +
            "&FileName=" + fileName + "&Settings=" + fileSettings + "&j=" + tbox1.innerText + "&IsDlg=1";
    } else {
        itemSettingsUrl = _spPageContextInfo.siteServerRelativeUrl + "/_layouts/15/Nauplius.WAS/ConversionSettings.aspx?ParentElement=" + rowId + "&FileType=" + fileType +
            "&FileName=" + fileName + "&Settings=" + fileSettings + "&IsDlg=1";
    }

    var options = {
        url: itemSettingsUrl,
        args: null,
        title: 'Conversion Settings for ' + fileName,
        dialogReturnValueCallback: dialogCallback,
    };
    SP.SOD.execute('sp.ui.dialog.js', 'SP.UI.ModalDialog.showModalDialog', options);

    function dialogCallback(dialogResult, returnValue) {
        if (dialogResult == SP.UI.DialogResult.OK) {
            var loc = window.location.href.substr(0, window.location.href.indexOf('?'));
            var jsonData = returnValue[0];

            $.ajax({
                type: "POST",
                url: loc + "/SaveData",
                data: JSON.stringify({ data: jsonData }),
                dataType: "json",
                contentType: "application/json; charset=utf-8",
                async:false,
                done: function (msg) { alert(msg.d); },
                fail: function (xhr, ajaxOptions, thrownError) {
                    console.log('error:');
                    console.log(xhr.status);
                    console.log(thrownError);
                }
            });
            /*
            var settings = returnValue[0];

            document.getElementById(fileSettings).innerText = settings;

            var tBox1 = document.getElementById(returnValue[3]);
            if (document.all) {
                tBox1.innerText = settings; //IE8 and below support
            } else {
                tBox1.textContent = settings; //Everything else
            }*/
        }
    }
}

var opts = {
    lines: 11,
    length: 13,
    width: 4,
    radius: 15,
    corners: 0,
    rotate: 0,
    direction: 1,
    color: '#000',
    speed: 1.1,
    trail: 47,
    shadow: true,
    hwaccel: false,
    className: 'wait',
    zIndex: 2e9
};
var spinner;

function runSpinner() {
    var target = document.getElementById('spinwait');
    var ph1 = $('#ctl00_PlaceHolderMain_p1');
    var table = $('#ctl00_PlaceHolderMain_gvItems');
    var spinDiv = $('#spinwait');
    var textDiv = $('#textwait');
    var btnOk = $('#ctl00_PlaceHolderMain_btnConvert');
    var btnCan = $('#ctl00_PlaceHolderMain_btnCancel');
    btnOk.attr("disabled", "disabled");
    btnCan.attr("disabled", "disabled");
    table.hide();
    ph1.hide();
    spinDiv.show();
    if (typeof (spinner) == 'undefined') {
        spinner = new Spinner(opts).spin(target);
    }
    textDiv.show();
}