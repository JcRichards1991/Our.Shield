(function (root) {
  angular
    .module('umbraco')
    .controller('Shield.Editors.Swagger.Edit',
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
}(window));