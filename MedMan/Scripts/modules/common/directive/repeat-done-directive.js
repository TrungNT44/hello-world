app.instance.directive('repeatDone', function () {
    return function (scope, element, attrs) {
        if (scope.$last) { // all are rendered
            scope.$eval(attrs.repeatDone);
        }
    }
});

// Example

//<ul>
//	<li ng-repeat="feed in feedList" repeat-done="layoutDone()" ng-cloak>
//		<a href="{{feed}}" title="view at {{feed | hostName}}" data-toggle="tooltip">{{feed | strip_http}}</a>
//	</li>
//</ul>

//.controller('AppCtrl', function($scope, $timeout) {

//    $scope.feedList = [
//        'http://feeds.feedburner.com/TEDTalks_video',
//        'http://feeds.nationalgeographic.com/ng/photography/photo-of-the-day/',
//        'http://sfbay.craigslist.org/eng/index.rss',
//        'http://www.slate.com/blogs/trending.fulltext.all.10.rss',
//        'http://feeds.current.com/homepage/en_US.rss',
//        'http://feeds.current.com/items/popular.rss',
//        'http://www.nytimes.com/services/xml/rss/nyt/HomePage.xml'
//    ];

//    $scope.layoutDone = function() {
//        //$('a[data-toggle="tooltip"]').tooltip(); // NOT CORRECT!
//        $timeout(function() { $('a[data-toggle="tooltip"]').tooltip(); }, 0); // wait...
//    }

//})
