angular
  .module('umbraco')
  .controller('Shield.Editors.Dialogs.EditException',
    [
      '$scope',
      function ($scope) {
        var vm = this;

        angular.extend(vm, {
          exception: $scope.dialogData,
          buttonKey: '',
          init: function () {
            vm.buttonKey = 'general_' + (vm.exception.fromIpAddress === '' ? 'add' : 'update');
          },
          close: $scope.close,
          submit: function () {
            $scope.submit(vm.exception);
          }
        });
      }
    ]
  );