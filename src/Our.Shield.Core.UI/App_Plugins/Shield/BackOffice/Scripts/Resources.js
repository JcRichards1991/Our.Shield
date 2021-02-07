angular
  .module('umbraco.resources')
  .factory('shieldResourceHelper',
    [
      '$http',
      '$q',
      function ($http, $q) {
        return {
          delete: function (url) {
            if (!url) {
              throw Error('url is required');
            }

            var deferred = $q.defer();

            $http({
              method: 'DELETE',
              url: url,
              headers: {
                'Content-Type': 'application/json'
              }
            }).then(function (response) {
              return deferred.resolve(response.data);
            }, function (response) {
              console.log(response);

              return deferred.resolve(false);
            });

            return deferred.promise;
          },
          get: function (url, data) {
            if (!url) {
              throw Error('url is required');
            }

            var deferred = $q.defer();

            data = data || {};

            $http
              .get(url,
                {
                  params: data
                })
              .then(function (response) {
                return deferred.resolve(response.data);
              }, function (response) {
                console.log(response);

                return deferred.resolve(false);
              });

            return deferred.promise;
          },
          post: function (url, data) {
            if (!url) {
              throw Error('url is required');
            }

            if (!data) {
              throw Error('data is required');
            }

            var deferred = $q.defer();

            $http({
              method: 'POST',
              url: url,
              data: JSON.stringify(data),
              dataType: 'json',
              contentType: 'application/json'
            }).then(function (response) {
              return deferred.resolve(response.data);
            }, function (response) {
              console.log(response);

              return deferred.resolve(false);
            });

            return deferred.promise;
          }
        };
      }
    ]);

angular
  .module('umbraco.resources')
  .factory('shieldResource',
    [
      'shieldResourceHelper',
      function (shieldResourceHelper) {
        var apiRoot = 'backoffice/shield/ShieldApi/';

        return {
          deleteEnvironment: function (key) {
            return shieldResourceHelper.delete(apiRoot + 'DeleteEnvironment?key=' + key);
          },
          getApp: function (key) {
            return shieldResourceHelper.get(apiRoot + 'GetApp', { key: key });
          },
          getEnvironment: function (key) {
            return shieldResourceHelper.get(apiRoot + 'GetEnvironment', { key: key });
          },
          getEnvironments: function () {
            return shieldResourceHelper.get(apiRoot + 'GetEnvironments');
          },
          getJournals: function (method, id, page, orderBy, orderByDirection) {
            return shieldResourceHelper.get(
              apiRoot + 'Journals',
              {
                method: method,
                id: id,
                page: page,
                orderBy: orderBy,
                orderByDirection: orderByDirection
              });
          },
          getView: function (id) {
            return shieldResourceHelper.get(apiRoot + 'View', { id: id });
          },
          postConfiguration: function (key, config) {
            return shieldResourceHelper.post(apiRoot + 'WriteConfiguration?key=' + key, config);
          },
          setEnvironmentsSortOrder: function (environments) {
            return shieldResourceHelper.post(apiRoot + 'SortEnvironments', environments);
          },
          upsertEnvironment: function (environment) {
            return shieldResourceHelper.post(apiRoot + 'UpsertEnvironment', environment);
          }
        };
      }
    ]
  );