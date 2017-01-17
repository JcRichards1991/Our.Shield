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