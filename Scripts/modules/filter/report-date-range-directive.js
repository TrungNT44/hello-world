
app.instance.controller("ReportDateRangeController", ReportDateRangeController);
ReportDateRangeController.$inject = ["$scope", "$rootScope"];
function ReportDateRangeController($scope, $rootScope) {
    var self = this;
    //$scope.filterType = FILTER_DATE_RANGE;
    if (typeof $scope.filterType === "undefined") {
        $scope.filterType = FILTER_DATE_RANGE;
    }

    $scope.reportFromDate = null;
    $scope.reportToDate = null;
    $scope.callbackParams = null;   
    $scope.onFromDateClick = function () {
        $("#reportFromDate").datepicker("show");
    }

    $scope.onToDateClick = function () {
        $("#reportToDate").datepicker("show");
    } 

    this.validateInputParams = function () {
        $scope.reportFromDate = null;
        $scope.reportToDate = null;
        $scope.callbackParams = null;
        if ($scope.filterType == FILTER_DATE_RANGE) {
            $scope.reportFromDate = $("#reportFromDate")[0].value;
            $scope.reportToDate = $("#reportToDate")[0].value;
        }
        if ($scope.filterType == FILTER_DATE_RANGE && (app.utils.isStringEmpty($scope.reportFromDate)
           || app.utils.isStringEmpty($scope.reportToDate))) {
            var errorMessage = 'Ngày để lọc dữ liệu không hợp lệ.';
            app.notice.error(errorMessage);
            return false;
        }
        
        return true;
    }

    $scope.onDisplayData = function (validInput) {
        if ($scope.displayDataCallback != null) {
            if (validInput && !self.validateInputParams()) return;
            $scope.callbackParams = { filterType: $scope.filterType, reportFromDate: $scope.reportFromDate, reportToDate: $scope.reportToDate };
            $scope.displayDataCallback($scope.callbackParams);
        }
    }

    $scope.onExport = function ($event) {
        if ($scope.exportCallback != null) {
            if (!self.validateInputParams()) return;

            $scope.exportCallback($scope.callbackParams);
        }
        else {
            var anchor = $event.target.closest("a");
            return ExcellentExport.excel(anchor, $scope.displayDataTableId, 'ExportData');
        }

    }

    $scope.onPrint = function () {
        if ($scope.printCallback != null) {
            if (!self.validateInputParams()) return;

            $scope.printCallback($scope.callbackParams);
        }
    }

    $scope.onFilterTypeChange = function () {
        if ($scope.filterType == FILTER_DATE_RANGE) {
            settingDateRangePicker();
        }
    }

    $scope.getExcelFileName = function ()
    {
        var fileName = $scope.excelFileName;
        if (app.utils.isStringEmpty(fileName)) {
            fileName = "DataExport.xls";
        }

        return fileName;
    }

    $scope.isReady = function () {
        initDateRanges();
    }

    function displayDrugSearchControl() {
        if ($('#rdFilterAll').is(':visible')) { //if the container is visible on the page
            
        } else {
            setTimeout(displayDrugSearchControl, 50); //wait 50 ms, then try again
        }
    }

    function initDateRanges() {
        var toDate = new Date();
        var fromDate = new Date(toDate.getFullYear(), toDate.getMonth(), 1);
        $scope.reportFromDate = moment(fromDate).format(DEFAULT_MOMENT_DATE_FORMAT);
        $scope.reportToDate = moment(toDate).format(DEFAULT_MOMENT_DATE_FORMAT);
        if ($scope.callCallbackAfterInit) {
            $scope.onDisplayData(false);
        }     
    }    

    function settingDateRangePicker() {
        if ($('#reportFromDate').is(':visible')) { //if the container is visible on the page
            $(".input-daterange").datepicker({
                format: DEFAULT_DATE_PICKER_FORMAT,
                changeMonth: true,
                changeYear: true,
                endDate: 0,
                maxViewMode: 2,
                defaultDate: new Date(),
                minDate: MIN_PRODUCTION_DATA_DATE,
                language: 'vi',
                autoclose: true
            });
        } else {
            setTimeout(settingDateRangePicker, 50); //wait 50 ms, then try again
        }
    }
};

app.instance.directive("reportDateRageFilter", reportDateRageFilter);
reportDateRageFilter.$inject = ['$timeout'];
function reportDateRageFilter($timeout) {
    return {
        restrict: 'A',
        scope: {
            displayDataTableId: '@?',
            excelFileName: '@?',
            supressExport: '=',
            supressPrint: '=',
            displayDataCallback: '=',
            exportCallback: '=',
            printCallback: '=',
            callCallbackAfterInit: '=',
            hideDateRangeFilter: '=',
            filterType: "=?",
            onlyDisplayFromTo: "=?",
        },
        replace: false,
        templateUrl: "/tplReportDateRageFilterControl.html",
        controller: 'ReportDateRangeController'
    }
};
