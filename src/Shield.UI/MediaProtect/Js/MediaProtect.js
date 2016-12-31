function MediaProtectResource($http) {
    var apiRoot = 'backoffice/Shield/MediaProtectApi/';

    return {
        PostMediaProtect: function (model) {
            return $http.post(apiRoot + 'PostMediaProtectConfiguration', angular.toJson(model));
        },
        GetMediaProtect: function () {
            return $http.get(apiRoot + 'GetMediaProtectConfiguration');
        }
    };
}

angular.module('umbraco.resources').factory('MediaProtectResource', MediaProtectResource);