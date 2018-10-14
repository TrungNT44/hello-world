var app = {};
agGrid.initialiseAgGridWithAngular1(angular);
app.instance = angular.module("appInstance", ['ajaxLoader', 'ui.bootstrap', 'trNgGrid', 'ngSanitize', 'ui.select', 'ngRoute', 'angular-cache', 'googlechart', 'agGrid', 'ngNumeraljs', 'dynamicNumber', 'dateParser', 'number-input'])
.run(function () {
    //Change default settings
    TrNgGrid.defaultPagerMinifiedPageCountThreshold = 10;
})
.run(["$templateCache", function ($templateCache) {
    //Overwrite pager template
            $templateCache.put(
                TrNgGrid.footerPagerTemplateId,
                '<span class="pull-right form-group">'
                + ' <ul class="pagination">'
                + '   <li ng-class="{disabled:!pageCanGoBack}" ng-if="extendedControlsActive">'
                + '     <a href="" ng-click="pageCanGoBack&&navigateToPage(0)" ng-attr-title="{{\'Trang đầu\'|' + TrNgGrid.translateFilter + ':gridOptions.locale}}">'
                //+ '         <span class="glyphicon glyphicon-fast-backward"></span>' 
                + '         <span>&laquo;</span>'
                + '     </a>'
                + '   </li>'
                + '   <li ng-class="{disabled:!pageCanGoBack}" ng-if="extendedControlsActive">'
                + '     <a href="" ng-click="pageCanGoBack&&navigateToPage(gridOptions.currentPage - 1)" ng-attr-title="{{\'Trang trước\'|' + TrNgGrid.translateFilter + ':gridOptions.locale}}">'
                //+ '         <span class="glyphicon glyphicon-step-backward"></span>' 
                + '         <span>&lsaquo;</span>'
                + '     </a>'
                + '   </li>'
                + '   <li ng-if="pageSelectionActive" ng-repeat="pageIndex in pageIndexes track by $index" ng-class="{disabled:pageIndex===null, active:pageIndex===gridOptions.currentPage}">'
                + '      <span ng-if="pageIndex===null">...</span>'
                + '      <a href="" ng-click="navigateToPage(pageIndex)" ng-if="pageIndex!==null" ng-attr-title="{{\'Trang\'|' + TrNgGrid.translateFilter + ':gridOptions.locale}}">{{pageIndex+1}}</a>'
                + '   </li>'
                + '   <li ng-class="{disabled:!pageCanGoForward}" ng-if="extendedControlsActive">'
                + '     <a href="" ng-click="pageCanGoForward&&navigateToPage(gridOptions.currentPage + 1)" ng-attr-title="{{\'Trang tiếp\'|' + TrNgGrid.translateFilter + ':gridOptions.locale}}">'
                //+ '         <span class="glyphicon glyphicon-step-forward"></span>' 
                + '         <span>&rsaquo;</span>'
                + '     </a>'
                + '   </li>'
                + '   <li ng-class="{disabled:!pageCanGoForward}" ng-if="extendedControlsActive">'
                + '     <a href="" ng-click="pageCanGoForward&&navigateToPage(lastPageIndex)" ng-attr-title="{{\'Trang cuối\'|' + TrNgGrid.translateFilter + ':gridOptions.locale}}">'
                //+ '         <span class="glyphicon glyphicon-fast-forward"></span>' 
                + '         <span>&raquo;</span>'
                + '     </a>'
                + '   </li>'
                + '   <li class="disabled" style="white-space: nowrap;">'
                + '     <span ng-hide="totalItemsCount || !displayTotalItemsCount">{{\'Không có dữ liệu\'|' + TrNgGrid.translateFilter + ':gridOptions.locale}}</span>'
                + '     <span ng-show="totalItemsCount && displayTotalItemsCount">'
                + '       {{startItemIndex+1}} - {{endItemIndex+1}} {{\'displayed\'|' + TrNgGrid.translateFilter + ':gridOptions.locale}}'
                + '       <span>, {{totalItemsCount}} {{\'in total\'|' + TrNgGrid.translateFilter + ':gridOptions.locale}}</span>'
                + '     </span > '
                + '   </li>'
                + ' </ul>'
                + '</span>'
            );
        }])
;

app.instance.filter('propsFilter', function () {
    return function (items, props) {
        var out = [];

        if (angular.isArray(items)) {
            var keys = Object.keys(props);

            items.forEach(function (item) {
                var itemMatches = false;

                for (var i = 0; i < keys.length; i++) {
                    var prop = keys[i];
                    var text = props[prop].toLowerCase();
                    if (item[prop].toString().toLowerCase().indexOf(text) !== -1) {
                        itemMatches = true;
                        break;
                    }
                }

                if (itemMatches) {
                    out.push(item);
                }
            });
        } else {
            // Let the output be the input untouched
            out = items;
        }

        return out;
    };
});

app.instance.filter('newLine', function () {
    return function (text) {
        if (text) {
            return text.replace(/\n/g, '<br />');
        }
    }
});

app.instance.filter("unsafeHtml", unsafeHtml);
unsafeHtml.$inject = ['$sce'];
function unsafeHtml($sce) {
    return $sce.trustAsHtml;
}

app.instance.config(['$numeraljsConfigProvider', function ($numeraljsConfigProvider) {
    var language = {
        delimiters: {
            thousands: '.',
            decimal: ','
        },
        abbreviations: {
            thousand: 'k',
            million: 'm',
            billion: 'b',
            trillion: 't'
        },
        ordinal: function (number) {
            var b = number % 10;
            return (~~(number % 100 / 10) === 1) ? 'th' :
                (b === 1) ? 'st' :
                (b === 2) ? 'nd' :
                (b === 3) ? 'rd' : 'th';
        },
        currency: {
            symbol: '$'
        }
    };

    $numeraljsConfigProvider.setLanguage('vi', language);
}]);

app.instance.config(['dynamicNumberStrategyProvider', function (dynamicNumberStrategyProvider) {
    dynamicNumberStrategyProvider.addStrategy('app-decimal-number', {
        numInt: 8,
        numFract: 2,
        numSep: '.',
        numPos: true,
        numNeg: false,
        numRound: 'round',
        numThousand: true,
        numThousandSep: ','
    });
}]);
app.instance.run(['dynamicNumberStrategy', function (dynamicNumberStrategy) {
        console.log(dynamicNumberStrategy);
        console.log(dynamicNumberStrategy.getStrategies());
        dynamicNumberStrategy.addStrategy('app-decimal-number1', {
            numInt: 8,
            numFract: 2,
            numSep: '.',
            numPos: true,
            numNeg: false,
            numRound: 'round',
            numThousand: true,
            numThousandSep: ','
        });
        //console.log(dynamicNumberStrategy.getStrategies());
        //console.log(dynamicNumberStrategy.getStrategy('price1'));
    }]);

moment.locale('vi');
bootbox.addLocale('vi', {
    OK: 'Chấp nhận',
    CANCEL: 'Hủy',
    CONFIRM: 'Xác nhận',
    YES: 'Có',
    NO: 'Không'
});

bootbox.setDefaults({
    /**
     * @optional String
     * @default: en
     * which locale settings to use to translate the three
     * standard button labels: OK, CONFIRM, CANCEL
     */
    locale: "vi",

    /**
     * @optional Boolean
     * @default: true
     * whether the dialog should be shown immediately
     */
    show: true,

    /**
     * @optional Boolean
     * @default: true
     * whether the dialog should be have a backdrop or not
     */
    backdrop: true,

    /**
     * @optional Boolean
     * @default: true
     * show a close button
     */
    closeButton: true,

    /**
     * @optional Boolean
     * @default: true
     * animate the dialog in and out (not supported in < IE 10)
     */
    animate: true,

    /**
     * @optional String
     * @default: null
     * an additional class to apply to the dialog wrapper
     */
    className: "my-modal"

});