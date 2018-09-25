app.instance.directive("reloadDirective", reloadDirective);
reloadDirective.$inject = ['$timeout'];
function reloadDirective($timeout) {
    return {
        restrict: 'A',
        scope: {
            input: "=timePerMinute"
        },
        template: '<div><span>{{updateAt}}</span><br /><span>(Automatically reload after: {{countDown}} seconds </span><a href="javascript: " ng-click="reload()">Reload Now</a><span> )</span></div>',
        link: function (scope, ele, attr, trNgController) {
            var reloadInMinutes = scope.input;
            var startAt = new Date();
            var nextReloadTime = startAt.getTime() + (reloadInMinutes * 60 + 1) * 1000;
            scope.updateAt = "Update at: " + startAt.toTimeString();
            scope.countDown = nextReloadTime;


            scope.reload = function () {
                location.reload();
            }

            $timeout(function () {
                scope.reload();
            }, reloadInMinutes * 60 * 1000);

            updateCountDown();

            function updateCountDown() {
                var startAtTimeInSeconds = (nextReloadTime - (new Date()).getTime()) / 1000;
                scope.countDown = startAtTimeInSeconds.toFixed(0);
                $timeout(function () { updateCountDown() }, 500);
            }
        },
    }
}
