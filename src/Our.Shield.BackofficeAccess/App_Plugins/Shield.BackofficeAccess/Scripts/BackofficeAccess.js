angular
  .module('umbraco')
  .controller('Shield.Editors.BackofficeAccess.Edit',
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