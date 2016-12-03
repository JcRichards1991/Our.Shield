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
        tabs: [{ id: 1, label: 'Whitelist' },
            { id: 2, label: 'Blacklist' },
            { id: 3, label: 'Log' }]
    };

    $scope.ip = {};
    $scope.whitelist = [];
    $scope.blacklist = [];

    $scope.ips = UmbracoAccessResource.GetIps().then(function (response) {
        if (response.data !== 'null' && response.data.length !== 0) {
            for (var i = 0; (i < response.data.length + 1) ; i++) {
                if (response.data[i].Allow) {
                    $scope.whitelist.push(response.data[i])
                } else {
                    $scope.blacklist.push(response.data[i]);
                }
            }
        }
    });

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