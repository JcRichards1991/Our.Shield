angular
  .module('umbraco')
  .controller('Shield.Editors.Elmah.Edit',
    [
      '$scope',
      function ($scope) {
        var vm = this;
        angular.extend(vm, {
          configuration: $scope.$parent.configuration
        });
      }
    ]
  );

angular
  .module('umbraco')
  .controller('Shield.Editors.Elmah.Reporting',
    [
      '$scope',
      '$routeParams',
      '$timeout',
      'shieldElmahResource',
      function ($scope,
        $routeParams,
        $timeout,
        shieldElmahResource) {
        var vm = this;
        angular.extend(vm, {
          loading: true,
          pageNumber: 1,
          resultsPerPage: 100,
          totalPages: 0,
          errors: [],
          selectedError: null,
          getErrors: function () {
            vm.loading = true;
            shieldElmahResource
              .getErrors($routeParams.id, vm.pageNumber, vm.resultsPerPage)
              .then(function (response) {
                vm.errors = response.errors;
                vm.totalPages = response.totalPages;
                vm.loading = false;
              });
          },
          viewError: function (id) {
            vm.loading = true;
            shieldElmahResource
              .getError(id)
              .then(function (error) {
                vm.selectedError = error;
                if (vm.selectedError.error._webHostHtmlMessage) {
                  $timeout(function() {
                    var iframe = document.getElementById('webHostingHtmlIframe');
                    iframe.contentWindow.document.write(vm.selectedError.error._webHostHtmlMessage);
                  });
                }
                vm.loading = false;
              });
          },
          prevPage: function () {
            vm.pageNumber--;
            vm.getErrors();
          },
          nextPage: function () {
            vm.pageNumber++;
            vm.getErrors();
          },
          gotoPage: function (page) {
            vm.pageNumber = page;
            vm.getErrors();
          },
          generateTestException: function () {
            vm.loading = true;
            shieldElmahResource
              .generateTestException()
              .then(function () {
                vm.getErrors();
                vm.loading = false;
              });
          }
        });
      }
    ]
  );
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