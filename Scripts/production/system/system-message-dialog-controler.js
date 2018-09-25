app.instance.controller("SystemMessagesController", SystemMessagesController);
SystemMessagesController.$inject = ['$scope', '$rootScope', '$injector'];
function SystemMessagesController($scope, $rootScope, $injector) {
    var self = this;
    this.initAtFirstRunning = this.initAtFirstRunning || function () {
        $scope.ignoreLoadingItemsInFirstLoad = false;
        $scope.usingTrNgGrid = true;
        $scope.model.pageSize = 1000000; // Not paging
    }

    this.standardizeReciveData = this.standardizeReciveData || function (response) {
        if (response && response.Data) {
            var pagingModel = response.Data.PagingResultModel;
            $scope.model.totalItems = pagingModel.TotalSize;
            $scope.model.items = pagingModel.Results;
        } else {
            //app.notice.error('Lỗi khi truy cập dữ liệu máy chủ.');
        }
    }

    $scope.solveMessage = function(gridItem) {
        $rootScope.showSolveMessage(gridItem, function () {
            angular.forEach($scope.systemMessages, function (message) {
                if (message.SystemMessageId === gridItem.SystemMessageId) {
                    $scope.systemMessages.splice($scope.systemMessages.indexOf(message), 1);
                }
            });
        });
    }

    $scope.solveSelectedMessages = function() {
        //get Rows Selected to solve
        var rowSelecteds = $scope.systemMessages.filter(function (item) { return item.isSelected == true });
        var systemMessageIds = [];
        angular.forEach(rowSelecteds, function (row) {
            systemMessageIds.push(row.SystemMessageId);
        });

        app.notice.confirm("Are you sure you want to solve  selected messages?", function (result) {
            if (result) {
                $scope.solveMessagesProcessing = true;
                SolveMessageFactory.solveMessages(systemMessageIds, "Quick solve").then(function (data) {
                    if (data) {
                        pixelz.notice.message('There messages have solved.');
                        angular.forEach(systemMessageIds, function (messageId) {
                            angular.forEach($scope.systemMessages, function (systemMessage) {
                                if (messageId === systemMessage.SystemMessageId) {
                                    $scope.systemMessages.splice($scope.systemMessages.indexOf(systemMessage), 1);
                                }
                            });
                        });
                    } else {
                        app.notice.error('Solve message fail.');
                    }
                    $scope.solveMessagesProcessing = false;
                });
            }
        });
    }

    $injector.invoke(BaseController, this, {
        $scope: $scope,
        $rootScope: $rootScope,
        $injector: $injector
    });

    // Init grid
    this.initGrid(false, false, false, false, false,
        self.loadData, {
            remoteUrl: '/System/GetSystemMessages',
            extraParams: null, bindingFunc: self.bindDataToGrid
        });
};