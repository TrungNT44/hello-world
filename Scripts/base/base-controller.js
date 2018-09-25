/**
 * Define a base controller.
 *
 * @param {Object} $scope
 * @param {Object} $injector
 */
app.instance.controller("BaseController", BaseController);
BaseController.$inject = ['$scope', '$rootScope', '$injector'];
function BaseController($scope, $rootScope, $injector) {
    var self = this;
    var defaultCondition = {
        ActivePage: 0,
    };
    var locationSvr = $injector.get('$location');
    updateTabParentScope.apply(this, [defaultCondition, $scope, locationSvr]);

    this.initAtFirstRunning = this.initAtFirstRunning || function () {
    }

    this.initialize = this.initialize || function () {
        $scope.autoLoadingDataIfViewActivated = true;
        $scope.isViewLoaded = false;
        $scope.rootScope = $rootScope;
        $rootScope.ActivePage = 0;
        $scope.selectedItems = [];
        $scope.model = {};
        $scope.model.input = {};
        $scope.pageSizeList = [{ pageSizeKey: 10, pageSizeValue: 10 },
            { pageSizeKey: 50, pageSizeValue: 50 }, { pageSizeKey: 100, pageSizeValue: 100 },
            { pageSizeKey: 150, pageSizeValue: 150 }, { pageSizeKey: 200, pageSizeValue: 200 },
            { pageSizeKey: 9000, pageSizeValue: '--All--' }];
        $scope.defaultPageSize = 50;
        $scope.model.pageSize = 50;
        $scope.model.paging = true;
        $scope.virtualPageSize = 50;
        $scope.headerHeight = 35;
        $scope.rowHeight = 30;
        $scope.rowSelection = 'single';
        $scope.angularCompileRows = false;
        $scope.pageIndex = 0;
        $scope.urlFilterParams = null;
        $scope.filterByFields = null;
        $scope.gridOptions = null;
        $scope.customSorting = false;
        $scope.externalFiltering = false;
        $scope.externalParams = null;
        $scope.customHeader = false;
        $scope.sortingDirection = 0; // ASC
        $scope.paginationOptions = null;
        $scope.gridCallbackLoadingDataFunc = null;
        $scope.gridCallbackFuncParams = null;
        $scope.isFirstLoad = true;
        $scope.usingTrNgGrid = false;
        $scope.usingVirtualPaging = true;
        $scope.serverSorting = false;
        $scope.serverFiltering = false;
        $scope.onServerSideItemsRequested = self.onServerSideItemsRequested;
        $scope.onPageSizeChanged = self.onPageSizeChanged;
        $scope.loadAndBindDataToView = self.loadAndBindDataToView;
        $scope.ignoreLoadingItemsInFirstLoad = false;
        $scope.errorMessage = null;
        $scope.cachePageSizeOption = true;
        $scope.cachePageSizeKey = null;
        $scope.model.items = null;
        $scope.dialogContainer = null;
        $scope.dialogCallbackFunc = null;
        $scope.cacheDicts = {};
        $scope.reportFromDate = '';
        $scope.reportToDate = '';
        $scope.$on("externalParamsChanged", self.onExternalParamsChanged);
        $scope.$on("removeItems", self.onRemoveItems);
        self.initCacheDictionary();        

        // Call inheritance init.
        self.initReportDates();
        self.initAtFirstRunning();
        self.decodeSearchQuery(null);
        self.readCachePageSize();
        self.onInitializeFinished();
    }

    this.initReportDates = this.initReportDates || function() {
        $scope.reportFromDate = moment(new Date()).format(DEFAULT_MOMENT_DATE_FORMAT);
        $scope.reportToDate = $scope.reportFromDate;
    }

    this.applyReportDates = this.applyReportDates || function (noFilterDates) {
        if (noFilterDates) {
            $scope.model.input.reportFromDate = null;
            $scope.model.input.reportToDate = null;
        } else {
            $scope.model.input.reportFromDate = moment($scope.reportFromDate, DEFAULT_MOMENT_DATE_FORMAT).format('MM-DD-YYYY');
            $scope.model.input.reportToDate = moment($scope.reportToDate, DEFAULT_MOMENT_DATE_FORMAT).format('MM-DD-YYYY');
        }
    }

    this.applyReportDatesToMinMaxDates = this.applyReportDates || function () {
        $scope.model.input.reportFromDate = moment($scope.reportFromDate, DEFAULT_MOMENT_DATE_FORMAT).format('MM-DD-YYYY');
        $scope.model.input.reportToDate = moment($scope.reportToDate, DEFAULT_MOMENT_DATE_FORMAT).format('MM-DD-YYYY');
    }

    this.requestRemoteUrl = this.requestRemoteUrl || function (remoteUrl, extraParams, successFunc, failedFunc) {
        var httpSvr = $injector.get('$http');
        var qSvr = $injector.get('$q');
        var defer = qSvr.defer();
        var args = {};
        $.extend(args, extraParams);
        httpSvr({
            method: 'POST',
            url: remoteUrl,
            data: $.param(args),
            headers: { 'Content-Type': 'application/x-www-form-urlencoded' }
        }).
            success(function (data, status, headers, config) {
                defer.resolve(data);
                if (successFunc != null) {
                    successFunc(data);
                } else {
                    self.onRequestRemoteUrlSuccess(data);
                }
            }).
            error(function (data, status, headers, config) {
                defer.resolve(data);
                if (failedFunc != null) {
                    failedFunc(data);
                } else {
                    self.onRequestRemoteUrlFailed(data);
                }
        });

        return defer.promise;
    }

    this.getPageSize = function() {
        var pageSize = 1000000; // No paging
        if ($scope.model.paging) {
            pageSize = $scope.model.pageSize;
        }

        return pageSize;
    }

    this.loadData = this.loadData || function (remoteUrl, extraParams, bindingFunc) {
        var httpSvr = $injector.get('$http');
        var qSvr = $injector.get('$q');
        var defer = qSvr.defer();
        self.writeCachePageSize();
        self.saveCachedValues();
        self.procesingUrlParams(null);
        var args = { pageIndex: $scope.pageIndex, pageSize: self.getPageSize(), sortingDirection: $scope.sortingDirection };
        $.extend(args, extraParams, $scope.urlFilterParams, $scope.filterByFields);
        if ($scope.externalParams) {
            $.extend(args, $scope.externalParams);
        }
        if ($scope.cacheDicts) {
            $.extend(args, $scope.cacheDicts);
        }
        httpSvr({
            method: 'POST',
            url: remoteUrl,
            data: $.param(args),
            headers: { 'Content-Type': 'application/x-www-form-urlencoded' }
        }).
            success(function (data, status, headers, config) {
                defer.resolve(data);
                if (bindingFunc != null) {
                    bindingFunc(data);
                }
                self.onRequestRemoteUrlSuccess(data);
            }).
            error(function (data, status, headers, config) {
                defer.resolve(data);
                self.onRequestRemoteUrlFailed(data);
            });

        return defer.promise;
    }

    this.decodeSearchQuery = function (defaultObj) {
        var locationSvr = $injector.get('$location');
        var hash = locationSvr.hash();
        if (hash && hash.length > 0) {
            $scope.urlFilterParams = app.utils.decodeQueryObject(hash, defaultObj);
            $scope.model.input = {};
            $.extend($scope.model.input, $scope.urlFilterParams);
        }
    }

    this.encodeSearchQuery = function (defaultObj) {
        $scope.urlFilterParams = $scope.model.input;
        var locationSvr = $injector.get('$location');
        var hash = locationSvr.hash();
        if (hash != null) {
            locationSvr.hash(app.utils.encodeQueryObject($scope.urlFilterParams, defaultObj));
        }
    }

    this.procesingUrlParams = function (defaultObj) {
        if ($scope.isFirstLoad) {
            $scope.isFirstLoad = false;
            self.decodeSearchQuery(defaultObj);
        } else {
            self.encodeSearchQuery(defaultObj);
        }
    }

    this.initGrid = function (serverSorting, serverFiltering, customSorting, externalFiltering, customHeader ,callbackLoadingDataFunc, callbackFuncParams) {
        $scope.gridCallbackLoadingDataFunc = callbackLoadingDataFunc;
        $scope.gridCallbackFuncParams = callbackFuncParams;
        $scope.serverSorting = serverSorting;
        $scope.serverFiltering = serverFiltering;
        $scope.customSorting = customSorting;
        $scope.externalFiltering = externalFiltering;
        $scope.customHeader = customHeader;

        if ($scope.usingTrNgGrid) {
            initTrNgGrid();
        } else {
            initAgGrid();
        }
        self.onInitializeGridFinished();
    }

    function initTrNgGrid() {
       
    }

    function initAgGrid() {
        $scope.gridOptions = {
            enableServerSideSorting: $scope.serverSorting?true:false,
            enableServerSideFilter: $scope.serverFiltering?true:false,
            enableColResize: true,
            virtualPaging: $scope.usingVirtualPaging ? true : false, // this is important, if not set, normal paging will be done
            rowModelType: $scope.usingVirtualPaging ? 'virtual' : null,// 'pagination',
            rowSelection: $scope.rowSelection,
            rowDeselection: true,
            columnDefs: [],
            rowData: null,
            totalItems: 0,
            enableFilter: true,
            enableSorting: true,
            enableRangeSelection: true,
            suppressPreventDefaultOnMouseWheel:true,
            angularCompileRows: $scope.angularCompileRows,
            onBeforeFilterChanged: self.onBeforeFilterChanged,
            onAfterFilterChanged: self.onAfterFilterChanged,
            onBeforeSortChanged: $scope.customSorting ? self.onBeforeSortChanged : null,
            isExternalFilterPresent: $scope.externalFiltering ? self.isExternalFilterPresent : null,
            doesExternalFilterPass: $scope.externalFiltering ? self.doesExternalFilterPass : null,
            headerCellRenderer: $scope.customHeader ? self.renderHeaderCells : null,
            floatingTopRowData: [],
            floatingBottomRowData: [],
            getRowStyle: self.getRowStyle,
            headerHeight: $scope.headerHeight,
            rowHeight: $scope.rowHeight,
            // how big each page in our page cache will be, default is 100
            paginationPageSize: 50,
            // how many extra blank rows to display to the user at the end of the dataset,
            // which sets the vertical scroll and then allows the grid to request viewing more rows of data.
            // default is 1, ie show 1 row.
            paginationOverflowSize: 2,
            // how many server side requests to send at a time. if user is scrolling lots, then the requests
            // are throttled down
            maxConcurrentDatasourceRequests: 2,
            // how many rows to initially show in the grid. having 1 shows a blank row, so it looks like
            // the grid is loading from the users perspective (as we have a spinner in the first col)
            paginationInitialRowCount: 1,
            // how many pages to store in cache. default is undefined, which allows an infinite sized cache,
            // pages are never purged. this should be set for large data to stop your browser from getting
            // full of data
            maxPagesInCache: 5,
        };
    }

    this.getRowStyle = this.getRowStyle || function (params) {
        return null;
    }

    this.createNewDatasource = this.createNewDatasource || function () {
        if (!$scope.model.items) {
            // in case user selected 'onPageSizeChanged()' before the json was loaded
            return;
        }

        self.setRowData($scope.model.items);
    }

    this.setRowData = this.setRowData || function (allOfTheData) {
        if ($scope.usingVirtualPaging) {
            var dataSource = {
                rowCount: null, // behave as infinite scroll
                pageSize: $scope.virtualPageSize,
                overflowSize: $scope.virtualPageSize,
                maxConcurrentRequests: 2,
                maxPagesInCache: 2,
                getRows: function(params) {
                    console.log('asking for ' + params.startRow + ' to ' + params.endRow);
                    // At this point in your code, you would call the server, using $http if in AngularJS.
                    // wait for 500ms before returning
                    setTimeout(function() {
                        // take a slice of the total rows
                        var dataAfterSortingAndFiltering = self.sortAndFilter(allOfTheData, params.sortModel, params.filterModel);
                        var rowsThisPage = dataAfterSortingAndFiltering;
                        if (params.endRow > 0) {
                            rowsThisPage = dataAfterSortingAndFiltering.slice(params.startRow, params.endRow);
                        }
                        // if on or after the last page, work out the last row.
                        var lastRow = -1;
                        if (dataAfterSortingAndFiltering.length <= params.endRow) {
                            lastRow = dataAfterSortingAndFiltering.length;
                        }
                        // call the success callback
                        params.successCallback(rowsThisPage, lastRow);
                    }, 500);
                }
            };

            $scope.gridOptions.api.setDatasource(dataSource);
        } else {
            $scope.gridOptions.api.setRowData(allOfTheData);
        }
    }

    this.sortAndFilter = this.sortAndFilter || function(allOfTheData, sortModel, filterModel) {
        return self.sortData(sortModel, self.filterData(filterModel, allOfTheData));
    }

    this.sortData = this.sortData || function(sortModel, data) {
        var sortPresent = sortModel && sortModel.length > 0;
        if (!sortPresent) {
            return data;
        }
        // do an in memory sort of the data, across all the fields
        var resultOfSort = data.slice();
        resultOfSort.sort(function(a, b) {
            for (var k = 0; k < sortModel.length; k++) {
                var sortColModel = sortModel[k];
                var valueA = a[sortColModel.colId];
                var valueB = b[sortColModel.colId];
                // this filter didn't find a difference, move onto the next one
                if (valueA == valueB) {
                    continue;
                }
                var sortDirection = sortColModel.sort === 'asc' ? 1 : -1;
                if (valueA > valueB) {
                    return sortDirection;
                } else {
                    return sortDirection * -1;
                }
            }
            // no filters found a difference
            return 0;
        });
        return resultOfSort;
    }

    this.filterData = this.filterData || function(filterModel, data) {
        return data;
        //var filterPresent = filterModel && Object.keys(filterModel).length > 0;
        //if (!filterPresent) {
        //    return data;
        //}

        //var resultOfFilter = [];
        //for (var i = 0; i<data.length; i++) {
        //    var item = data[i];

        //    if (filterModel.FilterField) {

        //    }

        //    resultOfFilter.push(item);
        //}

        //return resultOfFilter;
    }

    this.onQuickFilterChanged = this.onQuickFilterChanged || function (value) {
        $scope.gridOptions.api.setQuickFilter(value);
    }    this.setSortModel = this.setSortModel || function (sort) {
        $scope.gridOptions.api.setSortModel(sort);
    }

    this.buildColumnDefs = this.buildColumnDefs || function() {
        $scope.gridOptions.columnDefs.length = 0;
    }

    this.loadAndBindDataToView = this.loadAndBindDataToView || function () {
        self.loadAndBindDataToGrid();
        self.showDialog();
    }

    this.loadAndBindDataToGrid = function () {
        if ($scope.ignoreLoadingItemsInFirstLoad && $scope.isFirstLoad) {
            $scope.isFirstLoad = false;
            return;
        }

        if ($scope.usingTrNgGrid) {
            loadAndBindDataToTrNgGrid();
        } else {
            loadAndBindDataToAgGrid();
        }
    }

    function loadAndBindDataToAgGrid() {
        invokeGridCallbackLoadingDataFunc();
    }

    function loadAndBindDataToTrNgGrid() {
        invokeGridCallbackLoadingDataFunc();
    }

    function invokeGridCallbackLoadingDataFunc() {
        if ($scope.gridCallbackLoadingDataFunc != null) {
            var args = new Array();
            if ($scope.gridCallbackFuncParams != null) {
                var i = 0;
                for (param in $scope.gridCallbackFuncParams) {
                    args[i++] = $scope.gridCallbackFuncParams[param];
                }
            }

            $scope.gridCallbackLoadingDataFunc.apply(null, args);
        }
    }

    this.standardizeReciveData = this.standardizeReciveData || function (response) {
        if (response) {
            var pagingModel = response.Data.PagingResultModel;
            $scope.model.totalItems = pagingModel.TotalSize;
            $scope.model.items = pagingModel.Results;
        }
    }

    this.bindDataToGrid = function (response) {
        self.standardizeReciveData(response);
        self.onBeforBindDataToGridView();
        if ($scope.usingTrNgGrid) {
            bindDataToTrNgGrid();
        } else {
            bindDataToAgGrid();
        }
        self.onAfterBindDataToGridView();
    }

    function bindDataToAgGrid() {
        self.setRowData($scope.model.items);
    }

    function bindDataToTrNgGrid() {
    }

    this.onServerSideItemsRequested = function (currentPage, pageSize, filterBy, filterByFields, orderBy, orderByReverse) {
        $scope.pageIndex = currentPage ? currentPage : 0;
        $scope.model.pageSize = pageSize ? pageSize : 200;
        $scope.filterByFields = filterByFields ? filterByFields : null;
        $scope.sortingDirection = orderBy ? orderBy : 0;
       
        self.loadAndBindDataToGrid();
    }

    this.validateInputParams = this.validateInputParams || function () {
        return true;
    }

    this.readCachePageSize = this.readCachePageSize || function () {
        if (!$scope.model.paging) {
            $scope.model.pageSize = 1000000;
            return;
        }
        var key = $scope.cachePageSizeKey;
        if ($scope.cachePageSizeOption && !app.utils.isStringEmpty(key)) {
            var currPageSize = $scope.defaultPageSize;
            var cacheSvr = $injector.get('cacheService');
            if (cacheSvr) {
                var cache = cacheSvr.cacheInstance();
                currPageSize = cache.get(key);
                if (!currPageSize) {
                    cache.put(key, $scope.defaultPageSize);
                    $scope.model.pageSize = $scope.defaultPageSize;
                } else {
                    $scope.model.pageSize = currPageSize;
                }
            }
        }
    }

    this.writeCachePageSize = this.writeCachePageSize || function () {
        var key = $scope.cachePageSizeKey;
        if ($scope.cachePageSizeOption && !app.utils.isStringEmpty(key)) {
            var currPageSize = $scope.defaultPageSize;
            var cacheSvr = $injector.get('cacheService');
            if (cacheSvr) {
                var cache = cacheSvr.cacheInstance();
                currPageSize = cache.get(key);
                if (currPageSize && currPageSize != $scope.model.pageSize) {
                    cache.remove(key);
                    cache.put(key, $scope.model.pageSize);
                }
            }
        }
    }

    this.onPageSizeChanged = this.onPageSizeChanged || function () {
        self.loadAndBindDataToGrid();
    }

    this.onBeforeSortChanged = this.onBeforeSortChanged || function (event) {
        // Should be overridden in inheritance class
    }

    this.onBeforeFilterChanged = this.onBeforeFilterChanged || function (event) {
        // Should be overridden in inheritance class
    }

    this.renderHeaderCells = this.renderHeaderCells || function (params) {
        // Should be overridden in inheritance class
    }

    this.isExternalFilterPresent = this.isExternalFilterPresent || function () {
        // Should be overridden in inheritance class
    }

    this.doesExternalFilterPass = this.doesExternalFilterPass || function (node) {
        // Should be overridden in inheritance class
    }

    this.externalFilterChanged = this.externalFilterChanged || function (newValue) {
        // Should be overridden in inheritance class and last invoke gridOptions.api.onFilterChanged();
    }

    this.onBeforBindDataToGridView = this.onBeforBindDataToGridView || function () {
        // Should be overridden in inheritance class
    }

    this.onAfterBindDataToGridView = this.onAfterBindDataToGridView || function () {
        // Should be overridden in inheritance class
    }

    this.initCacheDictionary = this.initCacheDictionary || function () {
        // Should be overridden in inheritance class
    }

    this.onBeforeFilterChanged = this.onBeforeFilterChanged || function () {
        // Should be overridden in inheritance class
    }

    this.onAfterFilterChanged = this.onAfterFilterChanged || function () {
        // Should be overridden in inheritance class
    }

    this.onInitializeFinished = this.onInitializeFinished || function () {
        // Should be overridden in inheritance class
    }

    this.onInitializeGridFinished = this.onInitializeGridFinished || function () {
        // Should be overridden in inheritance class
    }

    this.saveCachedValues = this.saveCachedValues || function () {
        var cacheSvr = $injector.get('cacheService');
        if (!cacheSvr) {
            return;
        }
        var cache = cacheSvr.cacheInstance();
        for (var key in $scope.cacheDicts) {
            if ($scope.cacheDicts.hasOwnProperty(key)) {
                var cachedVal = cache.get(key);
                if (cachedVal) {
                    cache.remove(key);
                }
                cache.put(key, $scope.cacheDicts[key]);
            }
        }
    }

    this.readCachedValues = this.readCachedValues || function () {
        var cacheSvr = $injector.get('cacheService');
        if (!cacheSvr) {
            return;
        }
        var cache = cacheSvr.cacheInstance();
        for (var key in $scope.cacheDicts) {
            if ($scope.cacheDicts.hasOwnProperty(key)) {
                var cachedVal = cache.get(key);
                if (cachedVal) {
                    $scope.cacheDicts[key] = cachedVal;
                }
            }
        }
    }
    
    this.broadcastExternalParamsChanged = function (externalParams) {
        $scope.$broadcast("externalParamsChanged", self, externalParams);
    }

    this.onExternalParamsChanged = this.onExternalParamsChanged || function (eventName, sender, externalParams) {
        if (sender != self) {
            //$scope.externalParams = externalParams;
            if (externalParams) {
                for (var key in externalParams) {
                    $scope.cacheDicts[key] = externalParams[key];
                }
                self.saveCachedValues();
            }
            self.loadAndBindDataToGrid();
        }
    }

    this.broadcastRemoveSelectedItems = function () {
        $scope.$broadcast("removeItems", self, $scope.selectedItems);
    }

    this.onRemoveItems = this.onRemoveItems || function (eventName, sender, items) {
        if (sender != self) {
            self.removeItemsRowsOnGrid(items);
        }
    }

    this.onRequestRemoteUrlSuccess = this.onRequestRemoteUrlSuccess || function (data) {
        // Should be overridden in inheritance class
        // app.notice.message("Tác vụ được thực hiện thành công.");
    }

    this.onRequestRemoteUrlFailed = this.onRequestRemoteUrlFailed || function (data) {
        // Should be overridden in inheritance class
        // app.notice.error("Request remote url failed.");
        app.notice.error("Không thực hiện được tác vụ.");
    }

    this.addSelectedItems = function (items, clearOldItems) {
        if (clearOldItems == true) {
            $scope.selectedItems.length = 0;
        }
        if (items instanceof Array) {
            angular.forEach(items, function(item) {
                $scope.selectedItems.push(item);
            });
        } else {
            $scope.selectedItems.push(items);
        }
    }

    this.getSelectedRowItems = function (tableId) {
        var tableEle = self.getTableEleByEvent(tableId);
        return $scope.rootScope.getSelectedRowItems(tableEle);
    }

    this.getTableEleByEvent = function (tableId) {
        return document.querySelector('#' + tableId);
    }

    this.removeItemsRowsOnGrid = function (items) {
        if (items && items.length > 0 && $scope.model && $scope.model.items && $scope.model.items.indexOf(items[0]) > -1) {
            app.utils.deleteItemsInList(items, $scope.model.items);
        }
    }

    this.prepareDataForDialog = this.prepareDataForDialog || function (containerElementId, params, dialogCallbackFunc) {
        var container = $("#" + containerElementId);
        if (container) {
            var scope = angular.element(container).scope();
            scope.externalParams = params;
            scope.dialogContainer = container;
            scope.dialogCallbackFunc = dialogCallbackFunc;
            scope.loadAndBindDataToView();
        }
    }
    
    this.showDialog = this.showDialog || function () {
        if ($scope.dialogContainer) {
            $scope.dialogContainer.modal("show");
            $scope.safeApply();
        }
    }

    this.hideDialog = this.hideDialog || function () {
        if ($scope.dialogContainer) {
            $scope.dialogContainer.modal("hide");
        }
    }

    $scope.safeApply = function (fn) {
        var phase = this.$root.$$phase;
        if (phase == '$apply' || phase == '$digest') {
            if (fn && (typeof (fn) === 'function')) {
                fn();
            }
        } else {
            this.$apply(fn);
        }
    };

    this.isActivedPage = function () {
        return angular.isDefined($scope.isActive) && $scope.isActive();
    }

    this.getElementByScopeId = function (scopeId) {
        var elem;
        $('.ng-scope').each(function () {
            var s = angular.element(this).scope(),
                sid = s.$id;

            if (sid == scopeId) {
                elem = this;
                return false; // stop looking at the rest
            }
        });

        return elem;
    }

    this.getCombinedElement = function () {
        return self.getElementByScopeId($scope.$id);
    }

    this.tryLoadedDetection = function () {
        var jsEle = self.getCombinedElement();
        var ele = $(jsEle);
        if (!ele.size()) {
            window.requestAnimationFrame(tryLoadedDetection);
        } else {
            $scope.isViewLoaded = true;
        }
    };

    $scope.$on("ActiveTableChanged", function () {
        if (self.isActivedPage()) {
            self.decodeSearchQuery(null);
            $scope.model.input.ActivePage = $scope.urlFilterParams.ActivePage;
            $scope.isDirty = false;
            $rootScope.ActivePage = $scope.model.input.ActivePage;
            if ($scope.autoLoadingDataIfViewActivated == true)
            {
                self.loadAndBindDataToView();
            }
        }
    }); 

    this.onViewReady = this.onViewReady || function (params) {
    }

    $scope.isReady = function (params) {
        self.onViewReady(params);
    };
 
    // This will be invoked last of all when a child controller is instantiated.
    self.initialize();
}

$(document).ready(function () {
    $.fn.datepicker.defaults.language = 'vi';
    // Bind date rage
    $('.input-daterange').datepicker({
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

    // Bind date rage
    $('.input-datetime').datepicker({
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
});