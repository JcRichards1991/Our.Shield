angular
  .module('umbraco')
  .controller('Shield.Editors.SiteMaintenance.Edit',
    [
      '$scope',
      'localizationService',
      function ($scope,
        localizationService) {
        var vm = this;
        angular.extend(vm, {
          configuration: $scope.$parent.configuration
        });
      }
    ]
  );