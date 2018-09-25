
app.instance.directive('requiredNumberDirective', function () {
    return {
        restrict: 'A',
        scope: {
            valueCtrl: '=',
        },
        link: function (scope, element, attrs) {
            //Min Max
            var min = attrs.numberMinValue;
            var max = attrs.numberMaxValue;
            var valueCtrl = attrs.valueCtrl;
            if (parseInt(valueCtrl) > parseInt(min) && parseInt(valueCtrl) < parseInt(max)) {
                $(element).addClass("ng-hide");
            }
            //Regex Number
            //var regexNumberInt = attrs.regexNumberInt;
            //var regNumberInt = new RegExp('^[0-9]$');
            //if (regexNumberInt != undefined && regNumberInt.test(regexNumberInt)) {
            //    $(element).removeClass("ng-hide");
            //}
            scope.$watch('valueCtrl', function (value) {
                if (value != undefined) {
                    if (parseInt(value) < parseInt(min)) {
                        $(element).removeClass("ng-hide");
                    }
                    if (parseInt(value) > parseInt(max)) {
                        $(element).removeClass("ng-hide");
                    }
                    if (parseInt(value) > parseInt(min) && parseInt(value) < parseInt(max)) {
                        $(element).addClass("ng-hide");
                    }
                }
            });
        }
    };
});