angular.module("umbraco").controller("Our.Umbraco.MigrationsViewer.Controller", function ($scope, $http, $routeParams, $route) {

    $http.get("/umbraco/backoffice/api/MigrationsViewerApi/Get?productName=" + $routeParams.id)
        .then(function (response) {
            $scope.items = response.data;
        });

    $scope.refreshData = function () { $route.reload(); }

    $scope.product = $routeParams.id;
});