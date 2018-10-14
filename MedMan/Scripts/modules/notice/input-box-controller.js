app.instance.controller("inputBoxController", inputBoxController);
inputBoxController.$inject = ["$scope"];
function inputBoxController($scope) {
    //public
    $scope.show = show;

    //private
    $scope.model = {};
    $scope.onSubmit = onSubmit;

    function show(title, value, callback) {
        $scope.model.title = title;
        $scope.model.value = value;
        $scope.callback = callback;

        $("#inputBox").modal('show');
    }

    function hide() {
        $("#inputBox").modal('hide');
    }

    function onSubmit() {
        hide();
        if ($scope.callback) {
            $scope.callback($scope.model.value);
        }
    }
}
