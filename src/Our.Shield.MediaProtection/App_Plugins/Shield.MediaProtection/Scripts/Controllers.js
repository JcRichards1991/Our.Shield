﻿angular
  .module('umbraco')
  .controller('Shield.Editors.MediaProtection.Edit',
    [
      '$scope',
      'shieldMediaProtectionResource',
      function ($scope,
        shieldMediaProtectionResource) {
        var vm = this;
        angular.extend(vm, {
          configuration: $scope.configuration,
          directories: [],
          init: function () {
            if (vm.configuration.hotLinkingProtectedDirectories === null) {
              vm.configuration.hotLinkingProtectedDirectories = [];
            }

            shieldMediaProtectionResource.getDirectories().then(function (response) {
              vm.directories = response.data;
              vm.loading = false;
            });
          },
          toggleSelectedDirectory: function (directory) {
            var index = vm.configuration.hotLinkingProtectedDirectories.indexOf(directory);

            if (index === -1) {
              vm.configuration.hotLinkingProtectedDirectories.push(directory);
            } else {
              vm.configuration.hotLinkingProtectedDirectories.splice(index, 1);
            }
          }
        });
      }
    ]
  );