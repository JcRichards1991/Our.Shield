/**
 * @ngdoc resource
 * @name MediaProtectResource
 * @function
 *
 * @description
 * Handles the Requests for the Media Protect area of the custom section
 */
angular.module('umbraco.resources').factory('MediaProtectResource', function ($http) {
    var apiRoot = 'backoffice/Shield/MediaProtectApi/';

    return {
        PostMediaProtect: function (model) {
            return $http.post(apiRoot + 'PostMediaProtectConfiguration', angular.toJson(model));
        },
        GetMediaProtect: function () {
            return $http.get(apiRoot + 'GetMediaProtectConfiguration');
        }
    };
});