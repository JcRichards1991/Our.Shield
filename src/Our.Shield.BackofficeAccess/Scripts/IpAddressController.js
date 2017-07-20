/**
* @ngdoc controller
* @name Shield.Properties.IpAddress
* @function
*
* @description
* Controller to handle the custom IP Address Property Editor
*/
angular.module('umbraco').controller('Shield.Properties.IpAddress',
    ['$scope', 'localizationService',
    function ($scope, localizationService) {

        var vm = this;

        angular.extend(vm, {
            configuration: $scope.configuration,
            init: function () {
                if (vm.configuration.ipAddresses.length === 0) {
                    vm.configuration.ipAddresses.push({
                        ipAddress: '',
                        description: ''
                    });
                }
            },
            add: function () {
                vm.configuration.ipAddresses.push({
                    ipAddress: '',
                    description: ''
                });
            },
            remove: function ($index) {
                var ip = vm.configuration.ipAddresses[$index];

                localizationService.localize('Shield.BackofficeAccess.AlertMessages_ConfirmRemoveIp').then(function (warningMsg) {
                    if (confirm(warningMsg + ip.ipAddress + ' - ' + ip.description)) {
                        vm.configuration.ipAddresses.splice($index, 1);
                    }
                });
            }
        });
    }]
);