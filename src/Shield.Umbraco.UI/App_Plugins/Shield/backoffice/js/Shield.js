function UmbracoAccessResource($http) {
    var apiRoot = 'backoffice/Shield/UmbracoAccessApiController/';

    return {
        PostIP: function (ip, command) {
            return $http.post(apiRoot + 'Post', angular.toJson(new {
                ip: ip,
                command: command
            }));
        },
        DeleteIP: function (name) {
            return $http.delete(apiRoot + 'Delete?name=' + name);
        },
        GetIP: function (name) {
            return $http.post(apiRoot + "Get?name=" + name);
        },
        GetAllIP: function () {
            return $http.post(apiRoot + "Get");
        }
    };
}

angular.module('umbraco.resources').factory('UmbracoAccessService', UmbracoAccessResource);

angular.module('umbraco').controller('Shield.Controllers.UmbracoAccess', function ($scope, $routeParams, notificationsService, navigationService, treeService, UmbracoAccessService) {

    $scope.content = { tabs: [{ id: 1, label: 'Whitelist' }, { id: 2, label: 'Blacklist' }, { id: 3, label: 'Log' }] };

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