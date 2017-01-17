(function(){ 
angular.module('shield', ['umbraco', 'shield.resources']);
angular.module('shield.resources', []);

angular.module('umbraco').directive("ngIsolateApp", function () {
    return {
        "scope": {},
        "restrict": "AEC",
        "compile": function (element, attrs) {
            var html = element.html();
            element.html('');
            return function (scope, element) {
                scope.$destroy();
                setTimeout(function () {
                    var newRoot = document.createElement("div");
                    newRoot.innerHTML = html;
                    angular.bootstrap(newRoot, [attrs["ngIsolateApp"]]);
                    element.append(newRoot);
                });
            }
        }
    }
});
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
/**
 * @ngdoc resource
 * @name UmbracoAccessResource
 * @function
 *
 * @description
 * Handles the Requests for the Umbraco Access area of the custom section
*/
angular.module('shield.resources').factory('UmbracoAccessResource', ['$http', function ($http) {
    var apiRoot = 'backoffice/Shield/UmbracoAccessApi/';

    return {
        PostConfiguration: function (data) {
            return $http.post(apiRoot + 'PostConfiguration', angular.toJson(data));
        },
        GetConfiguration: function () {
            return $http.get(apiRoot + 'GetConfiguration');
        }
    };
}]);
})();