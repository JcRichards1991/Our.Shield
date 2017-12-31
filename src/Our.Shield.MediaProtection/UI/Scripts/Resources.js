/**
    * @ngdoc resource
    * @name shieldMediaProtectionResource
    * @function
    *
    * @description
    * Api resource for the Media Protection App
*/
angular.module('umbraco.resources').factory('shieldMediaProtectionResource',
    ['$http',
        function ($http) {
            var apiRoot = 'backoffice/Shield/MediaProtectionApi/';

            return {
                getDirectories: function () {
                    return $http.get(apiRoot + 'GetDirectories');
                }
            };
        }
    ]
);