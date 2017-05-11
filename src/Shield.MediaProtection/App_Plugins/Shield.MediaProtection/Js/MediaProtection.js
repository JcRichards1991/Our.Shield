(function(root){ 
/**
 * @ngdoc controller
 * @name MediaProtection.EditController
 * @function
 *
 * @description
 * Edit Controller for the Media Protection Edit view
 */
angular.module('umbraco').controller('Shield.Editors.MediaProtection.EditController', ['$scope', 'notificationsService', 'localizationService', 'userService', 'ShieldMediaProtectionResource', function ($scope, notificationsService, localizationService, userService, resource) {
    $scope.loading = 0;
    $scope.error = null;

    $scope.init = function () {

    }
}]);

/**
 * @ngdoc resource
 * @name MediaProtectionResource
 * @function
 *
 * @description
 * Api resource for the Media Protection area
*/
angular.module('umbraco.resources').factory('ShieldMediaProtectionResource', ['$http', function ($http) {
    var apiRoot = Umbraco.Sys.ServerVariables.umbracoSettings.umbracoPath + 'backoffice/Shield/MediaProtectionApi/';

    return {
        PostConfiguration: function (data, userId) {
            return $http.post(apiRoot + 'PostConfiguration', angular.toJson({
                curUserId: userId,
                model: data
            }));
        },
        GetConfiguration: function () {
            return $http.get(apiRoot + 'GetConfiguration');
        }
    };
}]);
 }(window));