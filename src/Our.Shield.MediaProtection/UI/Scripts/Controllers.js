/**
     * @ngdoc controller
     * @name MediaProtection.EditController
     * @function
     *
     * @description
     * Edit Controller for the Media Protection Edit view
     */
angular.module('umbraco').controller('Shield.Editors.MediaProtection.Edit',
    ['$scope', 'shieldMediaProtectionResource',
    function ($scope, shieldMediaProtectionResource) {
        var vm = this;
        angular.extend(vm, {
            loading: true,
            configuration: $scope.$parent.configuration,
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
            toggleSelectedDirectories: function (directory) {
                var index = vm.configuration.hotLinkingProtectedDirectories.indexOf(directory);

                if (index === -1) {
                    vm.configuration.hotLinkingProtectedDirectories.push(directory);
                } else {
                    vm.configuration.hotLinkingProtectedDirectories.splice(index, 1);
                }
            }
        });
    }]
);