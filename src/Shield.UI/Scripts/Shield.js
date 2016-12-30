function UmbracoAccessResource($http) {
    var apiRoot = 'backoffice/Shield/UmbracoAccessApi/';

    return {
        PostBackendAccess: function (model) {
            return $http.post(apiRoot + 'PostBackendAccess', angular.toJson(model));
        },
        GetBackendAccessModel: function () {
            return $http.get(apiRoot + 'GetBackendAccessModel');
        }
    };
}

angular.module('umbraco.resources').factory('UmbracoAccessResource', UmbracoAccessResource);

angular.module('umbraco').controller('Shield.Controllers.UmbracoAccess', function ($scope, $routeParams, notificationsService, navigationService, treeService, UmbracoAccessResource) {

    $scope.content = {
        tabs: [{ id: 1, label: 'Backend Access' }]
    };

    $scope.model = UmbracoAccessResource.GetBackendAccessModel();

    $scope.backendAccess = function (model) {
        UmbracoAccessResource.PostBackendAccess(model).then(function (response) {
            if (response.data === 'null') {
                notificationsService.error("Something went wrong, the error has been logged");
            } else {
                notificationsService.success("Successfully added");
            }
        });
    };
});