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
            loading: true,
            newIp: {
                ipAddress: '',
                description: '',
                valid: true,
                errorMsg: '',
            },
            value: [],
            init: function () {
                angular.forEach($scope.configuration.ipAddresses, function (ip, index) {
                    vm.value.push({
                        ipAddress: ip.ipAddress,
                        description: ip.description,
                        editMode: false,
                        valid: true,
                        errorMsg: '',
                    });
                });

                vm.loading = false;
            },
            addIp: function () {
                if (!vm.isValidIpAddress(vm.newIp, false)) {
                    return false;
                }

                vm.value.push({
                    ipAddress: vm.newIp.ipAddress,
                    description: vm.newIp.description,
                    editMode: false
                });

                vm.newIp.ipAddress = '';
                vm.newIp.description = '';
            },
            editIp: function (ip, update) {
                var curEditIp = vm.value.filter((ip) => ip.editMode === true)[0];

                if (curEditIp && !update) {
                    return false;
                }

                if (!update) {
                    ip.editMode = true;
                } else {
                    if (!vm.isValidIpAddress(curEditIp, true)) {
                        return false;
                    }

                    curEditIp.editMode = false;
                }
            },
            removeIp: function (ip) {
                localizationService.localize('Shield.UmbracoAccess.AlertMessages_ConfirmRemoveIp').then(function (warningMsg) {
                    if (confirm(warningMsg + ip.ipAddress + ' - ' + ip.description)) {
                        var index = vm.value.indexOf(ip);

                        if (index !== -1) {
                            vm.value.splice(index, 1);
                        }
                    }
                });
            },
            isValidIpAddress: function (ip, edit) {
                ip.valid = true;
                ip.errorMsg = '';
                ip.errorState = null;

                if (ip.ipAddress === '') {
                    ip.valid = false;
                    ip.errorMsg = localizationService.localize('Shield.UmbracoAccess.ErrorMessages_IpRequired');
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
                        ip.errorMsg = localizationService.localize('Shield.UmbracoAccess.ErrorMessages_IpInvalid');
                        return false;
                    }
                }

                var index = edit ? 1 : 0;

                if (vm.value.filter((x) => x.ipAddress === ip.ipAddress)[index] !== undefined) {
                    ip.valid = false;
                    ip.errorMsg = localizationService.localize('Shield.UmbracoAccess.ErrorMessages_IpDuplicate');
                    return false;
                }

                return true;
            }
        });
    }]
);