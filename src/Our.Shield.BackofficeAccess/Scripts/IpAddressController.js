/**
* @ngdoc controller
* @name Shield.Properties.IpAddress
* @function
*
* @description
* Controller to handle the custom IP Address Property Editor
*/
angular.module('umbraco').controller('Shield.Properties.IpAddress',
    ['$scope', '$routeParams', 'notificationsService', 'localizationService', 'ShieldResource',
    function ($scope, $routeParams, notificationsService, localizationService, resource) {

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
            },
            isValid: function (ip, edit) {
                ip.valid = true;
                ip.errorMsg = '';
                ip.errorState = null;

                if (ip.ipAddress === '') {
                    ip.valid = false;
                    ip.errorMsg = localizationService.localize('Shield.BackofficeAccess.ErrorMessages_IpRequired');
                    return false;
                }

                //Check if IPv4 with optional cidr
                var pattern = /^(?=\d+\.\d+\.\d+\.\d+($|\/))(([1-9]?\d|1\d\d|2[0-4]\d|25[0-5])\.?){4}(\/([0-9]|[1-2][0-9]|3[0-2]))?$/g;
                var valid = pattern.test(ip.ipAddress);

                if (!valid) {
                    //Check if IPv6 with optional cidr
                    pattern = /^s*((([0-9A-Fa-f]{1,4}:){7}([0-9A-Fa-f]{1,4}|:))|(([0-9A-Fa-f]{1,4}:){6}(:[0-9A-Fa-f]{1,4}|((25[0-5]|2[0-4]d|1dd|[1-9]?d)(.(25[0-5]|2[0-4]d|1dd|[1-9]?d)){3})|:))|(([0-9A-Fa-f]{1,4}:){5}(((:[0-9A-Fa-f]{1,4}){1,2})|:((25[0-5]|2[0-4]d|1dd|[1-9]?d)(.(25[0-5]|2[0-4]d|1dd|[1-9]?d)){3})|:))|(([0-9A-Fa-f]{1,4}:){4}(((:[0-9A-Fa-f]{1,4}){1,3})|((:[0-9A-Fa-f]{1,4})?:((25[0-5]|2[0-4]d|1dd|[1-9]?d)(.(25[0-5]|2[0-4]d|1dd|[1-9]?d)){3}))|:))|(([0-9A-Fa-f]{1,4}:){3}(((:[0-9A-Fa-f]{1,4}){1,4})|((:[0-9A-Fa-f]{1,4}){0,2}:((25[0-5]|2[0-4]d|1dd|[1-9]?d)(.(25[0-5]|2[0-4]d|1dd|[1-9]?d)){3}))|:))|(([0-9A-Fa-f]{1,4}:){2}(((:[0-9A-Fa-f]{1,4}){1,5})|((:[0-9A-Fa-f]{1,4}){0,3}:((25[0-5]|2[0-4]d|1dd|[1-9]?d)(.(25[0-5]|2[0-4]d|1dd|[1-9]?d)){3}))|:))|(([0-9A-Fa-f]{1,4}:){1}(((:[0-9A-Fa-f]{1,4}){1,6})|((:[0-9A-Fa-f]{1,4}){0,4}:((25[0-5]|2[0-4]d|1dd|[1-9]?d)(.(25[0-5]|2[0-4]d|1dd|[1-9]?d)){3}))|:))|(:(((:[0-9A-Fa-f]{1,4}){1,7})|((:[0-9A-Fa-f]{1,4}){0,5}:((25[0-5]|2[0-4]d|1dd|[1-9]?d)(.(25[0-5]|2[0-4]d|1dd|[1-9]?d)){3}))|:)))(%.+)?s*(\/([0-9]|[1-9][0-9]|1[0-1][0-9]|12[0-8]))?$/g;
                    valid = pattern.test(ip.ipAddress)

                    if (!valid) {
                        ip.valid = false;
                        ip.errorMsg = localizationService.localize('Shield.BackofficeAccess.ErrorMessages_IpInvalid');
                        return false;
                    }
                }

                var index = edit ? 1 : 0;

                if (vm.configuration.ipAddresses.filter((x) => x.ipAddress === ip.ipAddress)[index] !== undefined) {
                    ip.valid = false;
                    ip.errorMsg = localizationService.localize('Shield.BackofficeAccess.ErrorMessages_IpDuplicate');
                    return false;
                }

                return true;
            }
        });
    }]
);