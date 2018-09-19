angular.module('umbraco.resources').factory('shieldElmahResource',
    ['$http',
        function ($http) {
            var apiRoot = 'backoffice/Shield/ElmahApi/';

            return {
                getErrors: function (page, resultsPerPage) {
                    return $http.get(apiRoot + 'GetErrors?page=' + page + '&resultsPerPage=' + resultsPerPage);
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