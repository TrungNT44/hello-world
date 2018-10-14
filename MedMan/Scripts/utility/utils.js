app.utils = app.utils || {};


function checkDate(field, acceptEmpty) {
    var allowBlank = true;
    var minYear = 1902;
    var maxYear = (new Date()).getFullYear();

    var errorMsg = "";

    // regular expression to match required date format
    re = /^(\d{4})-(\d{1,2})-(\d{1,2})$/;

    if (field.value != '') {
        if (regs = field.value.match(re)) {
            if (regs[3] < 1 || regs[3] > 31) {
                errorMsg = "Invalid value for day: " + regs[1];
            } else if (regs[2] < 1 || regs[2] > 12) {
                errorMsg = "Invalid value for month: " + regs[2];
            } else if (regs[1] < minYear || regs[1] > maxYear) {
                errorMsg = "Invalid value for year: " + regs[1] + " - must be between " + minYear + " and " + maxYear;
            }
        } else {
            errorMsg = "Invalid date format: " + field.value;
        }
    } else if (!allowBlank && !acceptEmpty) {
        errorMsg = "Empty date not allowed!";
    }

    if (errorMsg != "") {
        alert(errorMsg);
        field.focus();
        return false;
    }

    return true;
}

function dataURItoBlob(dataURI) {
    // convert base64/URLEncoded data component to raw binary data held in a string
    var byteString;
    if (dataURI.split(',')[0].indexOf('base64') >= 0)
        byteString = atob(dataURI.split(',')[1]);
    else
        byteString = unescape(dataURI.split(',')[1]);

    // separate out the mime component
    var mimeString = dataURI.split(',')[0].split(':')[1].split(';')[0];

    // write the bytes of the string to a typed array
    var ia = new Uint8Array(byteString.length);
    for (var i = 0; i < byteString.length; i++) {
        ia[i] = byteString.charCodeAt(i);
    }

    return new Blob([ia], { type: mimeString });
}

function createCORSRequest(method, url) {
    var xhr = new XMLHttpRequest();
    if ("withCredentials" in xhr) {

        // Check if the XMLHttpRequest object has a "withCredentials" property.
        // "withCredentials" only exists on XMLHTTPRequest2 objects.
        xhr.open(method, url, true);

    } else if (typeof XDomainRequest != "undefined") {

        // Otherwise, check if XDomainRequest.
        // XDomainRequest only exists in IE, and is IE's way of making CORS requests.
        xhr = new XDomainRequest();
        xhr.open(method, url);

    } else {

        // Otherwise, CORS is not supported by the browser.
        xhr = null;

    }
    return xhr;
}

function upLoadImage(uploadData) {
    $("#loader-div").show();
    var blob = dataURItoBlob(uploadData.imageContent);
    $.ajax({
        url: uploadData.signatureURL,
        data: { bucketId: uploadData.bucketId, contentType: blob.type, key: uploadData.key },
        datatype: JSON,
        type: 'POST',
        error: function () {
            $("#loader-div").hide();
        },
        success: function (res) {
            console.log(res.key);
            $("#loader-div").show();
            var formData = new FormData();
            formData.append('key', res.key);
            formData.append('AWSAccessKeyId', res.AWSAccessKeyId);
            formData.append('acl', "private");
            formData.append('policy', res.policy);
            formData.append('signature', res.signature);
            formData.append('content-type', blob.type);
            formData.append('file', blob);

            var xhr = createCORSRequest("POST", res.url);
            xhr.send(formData);
            xhr.onreadystatechange = function () {
                if (xhr.readyState == 4 && xhr.status == 204) {
                    $("#loader-div").hide();
                    if (callback) callback();
                }
            }
        }
    });
}

function upLoadProfileImage(uploadData, callback) {
    $("#loader-div").show();
    var blob = dataURItoBlob(uploadData.imageContent);
    $.ajax({
        url: "/Admin/RequestAmazonSignatureForAccount",
        data: { workerid: uploadData.workerid, contentType: blob.type, key: uploadData.key },
        datatype: JSON,
        type: 'POST',
        error: function () {
            $("#loader-div").hide();
        },
        success: function (res) {
            console.log(res.key);
            var formData = new FormData();
            formData.append('key', res.key);
            formData.append('AWSAccessKeyId', res.AWSAccessKeyId);
            formData.append('acl', "private");
            formData.append('policy', res.policy);
            formData.append('signature', res.signature);
            formData.append('content-type', blob.type);
            formData.append('file', blob);

            var xhr = createCORSRequest("POST", res.url);
            //xhr.open("POST", res.url, true);
            xhr.send(formData);
            xhr.onreadystatechange = function () {
                if (xhr.readyState == 4) {
                    $("#loader-div").hide();
                    if (callback) callback(uploadData.key, res.url + res.key);
                }
            }
        }
    });
}

function updateTabChildScope(tabIndex, scope) {
    scope.isDirty = true;
    scope.isActive = function isActive() {
        return scope.$parent.isShow(tabIndex);
    }
    scope.$on("InActiveTabDirty", function () {
        if (!scope.$parent.isShow(tabIndex)) {
            scope.isDirty = true;
        }
    });
}

function updateTabParentScope(defaultCondition, scope, $location) {
    scope.tabShownIndex = defaultCondition.ActivePage;
    var hash = $location.hash();
    var result = app.utils.decodeQueryObject(hash, defaultCondition);
    if (result) {
        if (result.ActivePage != undefined) {
            scope.tabShownIndex = result.ActivePage;
        }
    }

    scope.showTabContent = function showTabContent(tabIndex) {
        scope.tabShownIndex = tabIndex;
        scope.model.ActivePage = tabIndex;
        $location.hash(app.utils.encodeQueryObject({ ActivePage: tabIndex }, defaultCondition));
        scope.$broadcast("ActiveTableChanged", tabIndex);
    }

    scope.isShow = function isShow(index) {
        return index == scope.tabShownIndex;
    }
}

app.utils = function () {
    return {
        decodeQueryObject: decodeQueryObject,
        encodeQueryObject: encodeQueryObject,
        deleteItemInList: deleteItemInList,
        deleteItemsInList: deleteItemsInList,
        deSelectItemsInList: deSelectItemsInList,
        getDefaultToDate: getDefaultToDate,
        getBackFromNow: getBackFromNow,
        getDefaultFromDate: getDefaultFromDate,
        getDates: getDates,
        isStringEmpty: isStringEmpty,
        getBytesToText: getBytesToText,
        isEmpty: isEmpty,
        getBaseUrl: getBaseUrl,
        getRootUrl: getRootUrl,
        isValidDate: isValidDate,
        setPickerSelectedDate: setPickerSelectedDate
    }

    function isValidDate(d) {
        return d instanceof Date && !isNaN(d);
    }

    function setPickerSelectedDate(pickerDate, vnDateString) {
        if (isEmpty(vnDateString) || pickerDate == null) return;
        var pkDate = new Date(moment(vnDateString, DEFAULT_MOMENT_DATE_FORMAT).format('MM-DD-YYYY'));
        if (!isValidDate(pkDate)) return;
        
        pkDate = new Date(pkDate.getUTCFullYear(), pkDate.getUTCMonth(), pkDate.getUTCDate() + 1);
        pickerDate.datepicker("setDate", pkDate);
    }

    function getBaseUrl() {
        var re = new RegExp(/^.*\//);
        return re.exec(window.location.href);
    }

    function getRootUrl() {     
        var loca = window.location;
        var rootUrl = loca.origin
            ? loca.origin
            : loca.protocol + '/' + loca.host;
        return rootUrl;
    }

    function isStringEmpty(str) {
        return (!str || 0 === str.length);
    }

    function formatDate(d) {
        var dd = d.getDate();
        var mm = d.getMonth() + 1; //January is 0!
        var yyyy = d.getFullYear();

        if (dd < 10) {
            dd = '0' + dd
        }

        if (mm < 10) {
            mm = '0' + mm
        }
        return yyyy + '-' + mm + '-' + dd;
    }

    function getDefaultToDate() {
        var tomorrow = new Date();
        tomorrow.setDate(tomorrow.getDate() + 1);
        return formatDate(tomorrow);
    }

    function getBackFromNow(date) {
        var resultDate = new Date();
        resultDate.setDate(resultDate.getDate() - date);
        return formatDate(resultDate);
    }

    function getDefaultFromDate() {
        var lastMonth = new Date();
        lastMonth.setDate(lastMonth.getDate() - 30);
        return formatDate(lastMonth);
    }

    function decodeQueryObject(queryString, defaultObj) {
        if (!queryString) return false;
        var obj = $.extend({}, defaultObj);
        var pairs = queryString.split('&');
        for (var i in pairs) {
            var split = pairs[i].split('=');
            var value = decodeURIComponent(split[1]);
            if (value == "true") {
                value = true;
            }
            obj[decodeURIComponent(split[0])] = value;
        }
        return obj;
    }

    function encodeQueryObject(obj, defaultObj) {
        var pairs = [];
        if (obj) {
            for (var property in obj) {
                var value = obj[property];
                if (typeof value == "function") {
                    continue;
                }

                if ((defaultObj && defaultObj[property] != value)
                    || (!defaultObj && value)
                ) {
                    if (value !== undefined && value !== null) {
                        pairs.push(encodeURIComponent(property) + "=" + encodeURIComponent(value.toString()));
                    }
                }
            }
        }
        return pairs.join("&");
    }


    function deleteItemsInList(items, list) {
        for (var i = 0; i < items.length; i++) {
            deleteItemInList(items[i], list);
        }
    }


    function deSelectItemsInList(items, list) {
        for (var i = 0; i < items.length; i++) {
            var index = list.indexOf(items[i]);
            if (index >= 0) {
                var item = list[index];
                item.isSelected = false;
                if (item.updateTeamCountTime && item.updateTeamCountTime > 0) {
                    item.updateTeamCountTime += 1;
                }
            }
        }
    }


    function deleteItemInList(item, list) {
        var index = list.indexOf(item);
        if (index >= 0) {
            list.splice(index, 1);
        }
    }

    Date.prototype.addDays = function (days) {
        var dat = new Date(this.valueOf());
        dat.setDate(dat.getDate() + days);
        return dat;
    }
    function isDate(dateArg) {
        var t = (dateArg instanceof Date) ? dateArg : (new Date(dateArg));
        return !isNaN(t.valueOf());
    }

    function isValidRange(minDate, maxDate) {
        return (new Date(minDate) <= new Date(maxDate));
    }

    function getDates(startDt, endDt) {
        var error = ((isDate(endDt)) && (isDate(startDt)) && isValidRange(startDt, endDt)) ? false : true;
        var between = [];
        if (error) console.log('error occured!!!... Please Enter Valid Dates');
        else {
            var currentDate = new Date(startDt),
                end = new Date(endDt);
            while (currentDate <= end) {
                between.push(new Date(currentDate));
                currentDate.setDate(currentDate.getDate() + 1);
            }
        }
        return between;
    }

    function getBytesToText(sizeInBytes) {
        if (sizeInBytes == 0) return "0";
        if (sizeInBytes < 1024) return sizeInBytes.toFixed(2) + "B";
        var famount = sizeInBytes;
        famount /= 1024;
        if (famount < 1024) return famount.toFixed(2) + "KB"; // String.format("{0:0.0}KB", famount);
        famount /= 1024;
        if (famount < 1024) return famount.toFixed(2) + "MB"; //String.format("{0:0.0}MB", famount);
        famount /= 1024;
        return famount.toFixed(2) + "GB"; // String.format("{0:0.0}GB", famount);
    }

    function isEmpty(str) {
        if (typeof str == 'undefined' || !str || str.length === 0 || str === "" || !/[^\s]/.test(str) || /^\s*$/.test(str) || str.replace(/\s/g, "") === "") {
            return true;
        }
        else {
            return false;
        }
    }
}();

var clickCount = 0, singleClickTimer;
function singleClick(url) {
    window.open(url);
}
function doubleClick(url) {
    var link = document.createElement('a');
    link.href = url;
    link.setAttribute('download', '');
    link.click();
}
function downloadFileGlobal(el) {
    var url = $(el).data("fileurl")
    clickCount++;
    if (clickCount === 1) {
        singleClickTimer = setTimeout(function () {
            clickCount = 0;
            singleClick(url);
        }, 400);
    } else if (clickCount === 2) {
        clearTimeout(singleClickTimer);
        clickCount = 0;
        doubleClick(url);
    }
    return false;
}
function downloadFile(code) {
    $.post($('input[name=baseUrl]').val() + "Utilities/GetLinkDownloadFile", {
        codeFile: code
    }, function (response) {
        if(response){
            var link = document.createElement("a");
            link.id = "linkdown_temp";
            link.target = "_blank";
            link.href = response,
            $('body').append(link);
            $('#linkdown_temp')[0].click(function () {
                $(this).remove()
            });
        }
    });
}