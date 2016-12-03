function UmbracoAccessResource($http) {
    var apiRoot = 'backoffice/Shield/UmbracoAccessApi/';

    return {
        PostIp: function (ip) {
            return $http.post(apiRoot + 'Post', angular.toJson(ip));
        },
        DeleteIp: function (name) {
            return $http.delete(apiRoot + 'Delete?name=' + name);
        },
        GetIps: function () {
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
        tabs: [{ id: 1, label: 'Content' }]
    };

    $scope.ip = {};

    $scope.AddIp = function (ip) {
        UmbracoAccessResource.PostIp(ip).then(function (response) {
            if (response.data === 'null') {
                notificationsService.error("Something went wrong, the error has been logged")
            } else {
                notificationsService.success("Successfully added");
            }
        });
    };
});