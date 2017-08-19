/**
    * @ngdoc resource
    * @name shieldResource
    * @function
    *
    * @description
    * Api resource for the Shield Framework
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
        postEnvironment: function (environment) {
            return $http({
                method: 'POST',
                url: apiRoot + 'WriteEnvironment',
                data: JSON.stringify(environment),
                dataType: 'json',
                headers: {
                    'Content-Type': 'application/json'
                }
            });
        },
        deleteEnvironment: function (id) {
            return $http({
                method: 'POST',
                url: apiRoot + 'DeleteEnvironment?id=' + id,
                dataType: 'json',
                headers: {
                    'Content-Type': 'application/json'
                }
            });
        },
        getEnvironments: function () {
            return $http.get(apiRoot + 'GetEnvironments');
        },
        setEnvironmentsSortOrder: function (environments) {
            return $http({
                method: 'POST',
                url: apiRoot + 'SortEnvironments',
                data: JSON.stringify(environments),
                dataType: 'json',
                headers: {
                    'Content-Type': 'application/json'
                }
            });
        },
        getJournals: function (id, page, orderBy, orderByDirection) {
            return $http.get(apiRoot + 'Journals?id=' + id + '&page=' + page + "&orderBy=" + orderBy + "&orderByDirection=" + orderByDirection);
        }
    };
}]);