'use strict';
angular.module('ajaxLoader', [])
.factory('httpInterceptor', ["$q", "$rootScope", "$log", function ($q, $rootScope, $log) {
    var numLoadings = 0;
    return {
        request: function (config) {
            numLoadings++;
            var showIndicator = true;
            var hasDataProp = config.hasOwnProperty('data');
            if (hasDataProp && !app.utils.isEmpty(config.data) && config.data.indexOf('ignoreLoadingIndicator') !== -1) {
                showIndicator = false;
            }

            // Show loader
            if (showIndicator) {
                $rootScope.$broadcast("loader_show");
            }
       
            return config || $q.when(config)
        },
        response: function (response) {
            if ((--numLoadings) === 0) {
                // Hide loader
                $rootScope.$broadcast("loader_hide");
            }
            return response || $q.when(response);
        },
        responseError: function (response) {
            if (!(--numLoadings)) {
                // Hide loader
                $rootScope.$broadcast("loader_hide");
            }
            //return $q.reject(response);
            $rootScope.$broadcast("loader_hide");
        }
    };
}])
.config(["$httpProvider", function ($httpProvider) {
    $httpProvider.interceptors.push('httpInterceptor');
}])
.directive("loader", ["$rootScope", function ($rootScope) {
    return function ($scope, element, attrs) {
        $scope.$on("loader_show", function () {
            //$.showLoading({ name: 'circle-turn', allowHide: false });
            return element.css("display", "block");
        });
        return $scope.$on("loader_hide", function () {
            //$.hideLoading();
            return element.css("display", "none");
        });
    };
}])