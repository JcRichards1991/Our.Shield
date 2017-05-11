$(function (root) {
    /**
     * @ngdoc resource
     * @name UmbracoAccessResource
     * @function
     *
     * @description
     * Api resource for the Umbraco Access area
    */
    angular.module('umbraco.resources').factory('ShieldResource', ['$http', function ($http) {
        var apiRoot = Umbraco.Sys.ServerVariables.umbracoSettings.umbracoPath + 'backoffice/Shield/ShieldApi/';

        return {
            PostConfiguration: function (id, data) {
                return $http.post(apiRoot + 'PostConfiguration', angular.toJson({
                    id: id,
                    model: data
                }));
            },
            GetConfiguration: function (id) {
                return $http.get(apiRoot + 'GetConfiguration', id);
            }
        };
    }]);
}(window));