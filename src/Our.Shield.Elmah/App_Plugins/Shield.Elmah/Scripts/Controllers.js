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
      'shieldElmahResource',
      function ($scope,
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
            shieldElmahResource.getErrors(vm.pageNumber, vm.resultsPerPage).then(function (response) {
              vm.errors = response.data.errors;
              vm.totalPages = response.data.totalPages;
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
            shieldElmahResource.generateTestException().then(function () {
              vm.getErrors();
              vm.loading = false;
            });
          }
        });
      }
    ]
  );