angular.module('umbraco').controller('Shield.Editors.UmbracoAccess.EditController', function ($scope, $routeParams, notificationsService, navigationService, treeService, UmbracoAccessResource) {

    UmbracoAccessResource.GetUmbracoAccess().then(function (response) {
        if (response.data === 'null' || response.data === undefined) {
            notificationsService.error("Something went wrong getting the configuration, the error has been logged");
            $scope.umbracoAccess = {};
        } else {
            $scope.umbracoAccess = response.data;
        }
    });

    $scope.submitUmbracoAccess = function (model) {
        UmbracoAccessResource.PostUmbracoAccess(model).then(function (response) {
            if (response.data === 'null' || response.data === undefined || response.data === 'false') {
                notificationsService.error("Something went wrong, the error has been logged");
            } else {
                notificationsService.success("Successfully added");
            }
        });
    };
});