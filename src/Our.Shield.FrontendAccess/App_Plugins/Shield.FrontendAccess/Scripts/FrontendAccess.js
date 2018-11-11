angular
  .module('umbraco')
  .controller('Shield.Editors.FrontendAccess.Edit',
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