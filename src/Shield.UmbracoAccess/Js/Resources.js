
/**
 * @ngdoc resource
 * @name UmbracoAccessResource
 * @function
 *
 * @description
 * Api resource for the Umbraco Access area
*/
angular.module('umbraco.resources').factory('ShieldUmbracoAccessResource', ['$http', function ($http) {
    var apiRoot = Umbraco.Sys.ServerVariables.umbracoSettings.umbracoPath + 'backoffice/Shield/UmbracoAccessApi/';

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