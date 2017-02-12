
/**
 * @ngdoc resource
 * @name UmbracoAccessResource
 * @function
 *
 * @description
 * Handles the Requests for the Umbraco Access area of the custom section
*/
angular.module('umbraco.resources').factory('ShieldUmbracoAccessResource', ['$http', function ($http) {
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