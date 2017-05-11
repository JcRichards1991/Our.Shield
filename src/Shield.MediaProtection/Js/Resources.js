
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