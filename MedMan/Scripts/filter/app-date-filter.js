app.instance.filter("appDate", appDateFilter);
appDateFilter.$inject = ["$filter"];
function appDateFilter($filter) {
    return function (input, format, timezone) {
        if (!format) {
            //format = "dd-MM-yyyy HH:mm";
            format = "dd/MM/yyyy";
        }
        if (!timezone) {
            timezone = "UTC+7"; // Viet Nam
        }
        return $filter('date')(input, format, timezone); 
    }
}