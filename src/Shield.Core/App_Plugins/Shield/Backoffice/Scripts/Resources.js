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
                return $http({
                    method: 'POST',
                    url: apiRoot + 'PostConfiguration?id=' + id,
                    data: angular.toJson(model),
                    headers: {
                        'Content-Type': 'application/json'
                    },
                });

                //return $.ajax({
                //    type: "POST",
                //    data: JSON.stringify(model),
                //    url: apiRoot + 'PostConfiguration?id=' + id,
                //    contentType: "application/json"
                //});

                //return $http.post(apiRoot + 'PostConfiguration?id=' + id, { model: model });
            },
            GetConfiguration: function (id) {
                return $http.get(apiRoot + 'GetConfiguration?id=' + id);
            }
        };
    }]);
}(window));