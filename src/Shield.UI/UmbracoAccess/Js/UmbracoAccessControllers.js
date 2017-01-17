/**
 * @ngdoc controller
 * @name UmbracoAccess.EditController
 * @function
 *
 * @description
 * Handles the Umbraco Access area of the custom section
 */
angular.module('shield').controller('UmbracoAccess.EditController', ['$scope', 'notificationsService', 'UmbracoAccessResource', function ($scope, notificationsService, resource) {
    resource.GetConfiguration().then(function (response) {
        if (response.data === 'null' || response.data === undefined) {
            notificationsService.error("Something went wrong getting the configuration, the error has been logged");
            $scope.configuration = {
                backendAccessUrl: '~/umbraco'
            };
        } else {
            $scope.configuration = response.data;
        }

        $scope.defaultConfiguration = {
            backendAccessUrl: $scope.configuration.backendAccessUrl
        }
    });

    $scope.displayUmbracoAccessWarningMessage = false;

    $scope.$watchCollection('configuration', function () {
        if ($scope.defaultConfiguration.backendAccessUrl !== $scope.configuration.backendAccessUrl) {
            $scope.displayUmbracoAccessWarningMessage = true;
        }
        else {
            $scope.displayUmbracoAccessWarningMessage = false;
        }
    });

    $scope.submitUmbracoAccess = function (configuration) {
        resource.PostConfiguration(configuration).then(function (response) {
            if (response.data === 'null' || response.data === undefined || response.data === 'false') {
                notificationsService.error("Something went wrong, the error has been logged");
            } else {
                notificationsService.success("Successfully updated");

                $scope.defaultConfiguration = {
                    backendAccessUrl: $scope.configuration.backendAccessUrl
                }
                $scope.displayUmbracoAccessWarningMessage = false;
            }
        });
    };
}]);