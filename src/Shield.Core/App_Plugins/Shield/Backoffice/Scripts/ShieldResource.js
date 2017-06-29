$(function (root) {
    "use strict";

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
            getView: function (id) {
                return $http.get(apiRoot + 'View?id=' + id);
            },
            postView: function (id, model) {
                return $http({
                    method: 'POST',
                    url: apiRoot + 'View?id=' + id,
                    data: angular.toJson(model),
                    headers: {
                        'Content-Type': 'application/json'
                    },
                });
            }
        };
    }]);
}(window));