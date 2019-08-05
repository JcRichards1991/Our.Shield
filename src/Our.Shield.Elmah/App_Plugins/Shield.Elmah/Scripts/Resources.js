angular
  .module('umbraco.resources')
  .factory('shieldElmahResource',
    [
      '$http',
      '$q',
      function ($http, $q) {
        var apiRoot = 'backoffice/Shield/ElmahApi/';

        var get = function (url, data) {
          var deferred = $q.defer();

          data = data || {};

          $http
            .get(apiRoot + url,
              {
                params: data
              })
            .then(function (response) {
              return deferred.resolve(response.data);
            }, function (response) {
              return deferred.resolve(response);
            });

          return deferred.promise;
        };

        var post = function (url, data) {
          var deferred = $q.defer();

          $http({
            method: 'POST',
            url: apiRoot + url,
            data: JSON.stringify(data),
            dataType: 'json',
            headers: {
              'Content-Type': 'application/json'
            }
          }).then(function (response) {
            return deferred.resolve(response.data);
          }, function (response) {
            return deferred.resolve(response);
          });

          return deferred.promise;
        };

        return {
          getErrors: function (appKey, page, resultsPerPage) {
            return get('GetErrors', {
              appKey: appKey,
              page: page,
              resultsPerPage: resultsPerPage
            });
          },
          getError: function (id) {
            return get('GetError', {
              id: id
            });
          },
          generateTestException: function () {
            return post('GenerateTestException');
          }
        };
      }
    ]);