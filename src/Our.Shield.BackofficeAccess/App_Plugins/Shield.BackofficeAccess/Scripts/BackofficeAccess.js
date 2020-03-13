angular
  .module('umbraco')
  .controller('Shield.Editors.BackofficeAccess.Edit',
    [
      '$scope',
      function ($scope) {
        var vm = this;

        $scope.$on('formSubmitting', function () {
          vm.configuration.excludeUrls = [];
          angular.forEach(vm.excludeUrls, function (item) {
            if (item.value !== '') {
              vm.configuration.excludeUrls.push(item.value);
            }
          });
        });

        angular.extend(vm, {
          configuration: $scope.$parent.configuration,
          excludeUrls: [],
          init: function () {
            if (vm.configuration.excludeUrls) {
              angular.forEach(vm.configuration.excludeUrls, function (item) {
                vm.excludeUrls.push({ value: item });
              });
            }
          },
          addExcludeUrl: function() {
            vm.excludeUrls.push({ value: '' });
            // focus new value
            vm.excludeUrls[vm.excludeUrls.length - 1].hasFocus = true;
          },
          removeExcludeUrl: function (index) {
            var remainder = [];
            for (var x = 0; x < vm.excludeUrls.length; x++) {
              if (x !== index) {
                remainder.push(vm.excludeUrls[x]);
              }
            }
            vm.excludeUrls = remainder;
          }
        });
      }
    ]
  );