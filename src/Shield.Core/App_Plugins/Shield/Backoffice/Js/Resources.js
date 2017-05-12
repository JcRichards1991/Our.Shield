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
            PostConfiguration: function (id, data) {
                return $http({
                    method: 'POST',
                    url: apiRoot + 'PostConfiguration',
                    data: {
                        'id': id,
                        'model': data
                    },
                });
            },
            GetConfiguration: function (id) {
                return $http({
                    method: 'GET',
                    url: apiRoot + 'GetConfiguration',
                    params: {
                        id: id
                    }
                });
            }
        };
    }]);
}(window));