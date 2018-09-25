/**
 * AngularHelper : Contains methods that help using angular without being in the scope of an angular controller or directive
 * Note: The only problem with this is that I can't make this code work if the template contains ng-include
 */
var AngularHelper = (function () {
    var AngularHelper = function () { };

    /**
     * ApplicationName : Default application name for the helper
     */
    var defaultApplicationName = "appInstance";

    /**
         * Compile : Compile html with the rootScope of an application
         *  and replace the content of a target element with the compiled html
         * @$targetDom : The dom in which the compiled html should be placed
         * @htmlToCompile : The html to compile using angular
         * @applicationName : (Optionnal) The name of the application (use the default one if empty)
         */
    AngularHelper.CompileTemplate = function ($targetDom, htmlToCompile, applicationName) {
        var $injector = angular.injector(["ng", applicationName || defaultApplicationName]);

        $injector.invoke(["$compile", "$rootScope", function ($compile, $rootScope) {
            //Get the scope of the target, use the rootScope if it does not exists
            var $scope = $targetDom.html(htmlToCompile).scope();
            $compile($targetDom)($scope || $rootScope);
            $rootScope.$digest();
        }]);
    }

    /**
    * Compile : Re-compile html with the scope of target DOM
    * @$targetDom : The target DOM shall be re-compiled.
    */
    AngularHelper.Compile = function ($targetDom, $scope) {
        angular.element(document).injector().invoke(function ($compile) {
            var scope = $scope;
            if (!scope) {
                var scope = angular.element($targetDom).scope();
            }
            
            $compile($targetDom)(scope);
        });
    }

    return AngularHelper;
})();