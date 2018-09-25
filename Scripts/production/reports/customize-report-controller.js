app.instance.controller("CustomizeReportController", CustomizeReportController);
CustomizeReportController.$inject = ['$scope', '$rootScope', '$injector'];
function CustomizeReportController($scope, $rootScope, $injector) {
    var self = this;

    this.onInitializeFinished = this.onInitializeFinished || function() {
        $scope.model.pageSize = 100;
        $scope.fromDate = moment(new Date()).format(DEFAULT_MOMENT_DATE_FORMAT);
        $scope.toDate = moment(new Date()).format(DEFAULT_MOMENT_DATE_FORMAT);
        $scope.rowRenderIndex = 0;
    }

    $scope.changePageSize = function() {
        self.fetchData();
    };

    this.addColumns = function() {
        var shtml = "";
        angular.forEach($scope.model.DrugNames, function(item) {
            shtml += '<th rowspan="1" style="text-align: center;">Số lượng</th>';
            shtml += '<th style="text-align: center;">Giá sau CK</th>';
        });
        document.getElementById('idDynaicColumn').innerHTML = shtml;
    };

    this.standardizeReciveData = this.standardizeReciveData || function(response) {
        if (response && response.Data && response.Status == 0) {
            $scope.reportData = response.Data;
        } else {
            self.onGetDataFailed(response);
        }
    }

    this.onGetDataSuccess = function(response) {
        $scope.model.items = response.Data.CustomizeReportItems;
        $scope.model.DrugNames = response.Data.DrugNames;
        $scope.model.TotalQuantityAndPrice = response.Data.TotalQuantityAndPrice;
        self.addColumns();
    }

    this.onGetDataFailed = function(response) {
        app.notice.error('Không lấy được dữ liệu.');
    }

    $scope.showReport = function (selfRefreshing) {
        self.fetchData();
    }

    this.fetchData = function () {
        self.requestRemoteUrl('/Report/CustomizeReport',
        {
            fromDate: $scope.fromDate,
            toDate: $scope.toDate
        }, self.onGetDataSuccess, self.onGetDataFailed);
    }

    $scope.exportToExcel = function() { // ex: '#my-table'
        var exportHref = Excel.tableToExcel("#customizeTable", 'Bao cao');
        $timeout(function() { location.href = exportHref; }, 100); // trigger download
    }

    $injector.invoke(BaseController, this, {
        $scope: $scope,
        $rootScope: $rootScope,
        $injector: $injector
    });

    $(".input-datetimepicker").datepicker({
        format: DEFAULT_DATE_PICKER_FORMAT,
        changeMonth: true,
        changeYear: true,
        endDate: 0,
        maxViewMode: 2,
        defaultDate: new Date(),
        minDate: MIN_PRODUCTION_DATA_DATE
    }).on('changeDate', function() {
        $('.datepicker').hide();
    });

    //$(".input-datetimepicker").datepicker({
    //    dateFormat: "dd/mm/yy",
    //    changeMonth: true,
    //    changeYear: true,
    //    onSelect: function (dateText, inst) {
    //        $(inst).closest('div').find('input.datepicker-target').val(dateText);
    //    },
    //    onClose: function (selectedDate) {
    //        if ($(this).hasClass('from')) {
    //            $(".input-datetimepicker.to").datepicker("option", "minDate", selectedDate);
    //        }
    //        else {
    //            $(".input-datetimepicker.from").datepicker("option", "maxDate", selectedDate);
    //        }
            
    //    }
    //});
    //$('.btn-datepicker').on('click', function () {
    //    $(this).closest('div').find('input.input-datetimepicker').datepicker('show');
    //});

}

