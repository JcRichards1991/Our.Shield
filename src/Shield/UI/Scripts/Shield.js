function UmbracoAccessResource($http) {
    var apiRoot = 'backoffice/Shield/UmbracoAccessApi/';

    return {
        PostIP: function (ip) {
            return $http.post(apiRoot + 'Post', angular.toJson(ip));
        },
        DeleteIP: function (name) {
            return $http.delete(apiRoot + 'Delete?name=' + name);
        },
        GetIP: function (name) {
            return $http.post(apiRoot + "Get?name=" + name);
        },
        GetAllIP: function () {
            return $http.post(apiRoot + "Get");
        },
        GetLog: function () {
            return $http.post(apiRoot + "GetLog");
        }
    };
}

angular.module('umbraco.resources').factory('UmbracoAccessResource', UmbracoAccessResource);

angular.module('umbraco').controller('Shield.Controllers.UmbracoAccess', function ($scope, $routeParams, notificationsService, navigationService, treeService, UmbracoAccessResource) {

    $scope.content = {
        tabs: [{ id: 1, label: 'Whitelist' },
            { id: 2, label: 'Blacklist' },
            { id: 3, label: 'Log' }]
    };

    $scope.ip = {};

    $scope.AddIP = function (ip) {
        UmbracoAccessResource.PostIP(ip).then(function (response) {
            if (response.data == 'null') {
                notificationsService.error("Something went wrong, the error has been logged")
            } else {
                notificationsService.success("Successfully added");
            }
        });
    };
});