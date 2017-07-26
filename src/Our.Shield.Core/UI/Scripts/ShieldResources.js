/**
    * @ngdoc resource
    * @name UmbracoAccessResource
    * @function
    *
    * @description
    * Api resource for the Umbraco Access area
*/
angular.module('umbraco.resources').factory('shieldResource', ['$http', function ($http) {

    var apiRoot = 'backoffice/Shield/ShieldApi/';

    return {
        getView: function (id) {
            return $http.get(apiRoot + 'View?id=' + id);
        },
        postConfiguration: function (id, config) {
            return $http({
                method: 'POST',
                url: apiRoot + 'Configuration?id=' + id,
                data: JSON.stringify(config),
                dataType: 'json',
                headers: {
                    'Content-Type': 'application/json'
                },
            });
        },
        postEnvironment: function (id, data) {
            return $http({
                method: 'POST',
                url: apiRoot + 'Environment?id=' + id,
                data: JSON.stringify(data),
                dataType: 'json',
                headers: {
                    'Content-Type': 'application/json'
                },
            });
        },
        getJournals: function (id, page) {
            return $http.get(apiRoot + 'Journals?id=' + id + '&page=' + page);
        },
        getAppIds: function () {
            return $http.get(apiRoot + 'AppIds');
        }
    };
}]);