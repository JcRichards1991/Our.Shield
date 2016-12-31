function UmbracoAccessResource($http) {
    var apiRoot = 'backoffice/Shield/UmbracoAccessApi/';

    return {
        PostUmbracoAccess: function (model) {
            return $http.post(apiRoot + 'PostUmbracoAccess', angular.toJson(model));
        },
        GetUmbracoAccess: function () {
            return $http.get(apiRoot + 'GetUmbracoAccess');
        }
    };
}

angular.module('umbraco.resources').factory('UmbracoAccessResource', UmbracoAccessResource);

angular.module('umbraco').controller('Shield.Controllers.UmbracoAccess', function ($scope, $routeParams, notificationsService, navigationService, treeService, UmbracoAccessResource) {
    
    $scope.umbracoAccess = UmbracoAccessResource.GetUmbracoAccess();

    $scope.submitUmbracoAccess = function (model) {
        UmbracoAccessResource.PostUmbracoAccess(model).then(function (response) {
            if (response.data === 'null' || response.data === 'false') {
                notificationsService.error("Something went wrong, the error has been logged");
            } else {
                notificationsService.success("Successfully added");
            }
        });
    };
});