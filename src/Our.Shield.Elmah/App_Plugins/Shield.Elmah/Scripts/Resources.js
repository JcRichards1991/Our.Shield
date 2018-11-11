angular.module('umbraco.resources').factory('shieldElmahResource',
    ['$http',
        function ($http) {
            var apiRoot = 'backoffice/Shield/ElmahApi/';

            return {
                getErrors: function (page, resultsPerPage) {
                    return $http.get(apiRoot + 'GetErrors?page=' + page + '&resultsPerPage=' + resultsPerPage);
                },
                getError: function (id) {
                    return $http.get(apiRoot + 'GetError?id=' + id);
                },
                generateTestException: function () {
                    return $http({
                        method: 'POST',
                        url: apiRoot + 'GenerateTestException'
                    });
                }
            };
        }
    ]
);