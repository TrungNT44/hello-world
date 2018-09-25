app.instance.directive("countdownTimer", CountdownTimer);
function CountdownTimer() {
    return {
        scope: {
            intervalInSeconds: '=',
            onUpdateStatus: '=',
            onCounterEnd: '='
        },
        link: function ($scope, element, attrs) {
            var timer;
            var instance = this;
            var seconds = $scope.intervalInSeconds;
            var updateStatus = $scope.onUpdateStatus || function () { };
            var counterEnd = $scope.onCounterEnd || function () { };

            function decrementCounter() {
                updateStatus(seconds);
                if (seconds === 0) {
                    counterEnd();
                    instance.stop();
                }
                seconds--;
            }

            this.start = function () {
                clearInterval(timer);
                timer = 0;
                seconds = $scope.intervalInSeconds;
                timer = setInterval(decrementCounter, 1000);
            };

            this.stop = function () {
                clearInterval(timer);
            };

            angular.element(document).ready(function () {
                instance.start();
            });
        }
    };
   
}
