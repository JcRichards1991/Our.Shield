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
        var apiRoot = 'backoffice/Shield/ShieldApi/';

        return {
            PostConfiguration: function (id, model) {
                var data = {
                    id: id,
                    model: model
                }
                return $http.post(apiRoot + 'PostConfiguration', data);
            },
            GetConfiguration: function (id) {
                return $http.Get(apiRoot + 'GetConfiguration?id=' + id);
            }
        };
    }]);
}(window));