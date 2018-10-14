    
app.instance.factory('ajaxService', ajaxService);

ajaxService.$inject = ['$http', '$q', '$timeout'];

function ajaxService($http, $q, $timeout) {
    var service = {
        post: postData,
        get: getData,
        LazyBinding: LazyBinding,
        getPageSizeList: getPageSizeList
    };

    return service;

    function LazyBinding(model, result, callback) {
        var i = 0;
        while (result.length > 0) {
            model.push(result[0]);
            result.splice(0, 1);
            i++;
            if (i > 20) {
                break;
            }
        }
        if (result.length > 0) {
            $timeout(function () { LazyBinding(model, result, callback); }, 1000);
        }
        if (result.length == 0) {
            if (callback) callback();
        }
    }

    function getPageSizeList() {
        return [
            { value: 5, text: "5" },
            { value: 10, text: "10" },
            { value: 25, text: "25" },
            { value: 50, text: "50" },
            { value: 100, text: "100" }
        ];
    }

    function postData(url, params) {
        var deferred = $q.defer();
        $http({
            method: 'POST',
            url: url,
            data: $.param(params),
            headers: { 'Content-Type': 'application/x-www-form-urlencoded' }
        }).
            success(function(data, status, headers, config) {
                // this callback will be called asynchronously
                // when the response is available
                //return data;
                deferred.resolve(data);
            }).
            error(function(data, status, headers, config) {
                // called asynchronously if an error occurs
                // or server returns response with an error status.
                //data["err"] = "Error getting product list data.";
                //return data;
                deferred.resolve(data);
            });
        return deferred.promise;
    }

    function getData(url) {
        var deferred = $q.defer();
        $http({
            method: 'GET',
            url: url
        }).
            success(function(data, status, headers, config) {
                // this callback will be called asynchronously
                // when the response is available
                //return data;
                deferred.resolve(data);
            }).
            error(function(data, status, headers, config) {
                // called asynchronously if an error occurs
                // or server returns response with an error status.
                //data["err"] = "Error getting product list data.";
                //return data;
                deferred.resolve(data);
            });
        return deferred.promise;
    }
}
