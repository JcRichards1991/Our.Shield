/**
 * @ngdoc resource
 * @name UmbracoAccessResource
 * @function
 *
 * @description
 * Handles the Requests for the Umbraco Access area of the custom section
 */
angular.module('umbraco.resources').factory('UmbracoAccessResource', function ($http) {
    var apiRoot = 'backoffice/Shield/UmbracoAccessApi/';

    return {
        PostConfiguration: function (configuration) {
            return $http.post(apiRoot + 'PostConfiguration', angular.toJson(configuration));
        },
        GetConfiguration: function () {
            return $http.get(apiRoot + 'GetConfiguration');
        }
    };
});