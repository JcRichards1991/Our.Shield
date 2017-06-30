$(function (root) {
    /**
     * @ngdoc controller
     * @name UmbracoAccess.EditController
     * @function
     *
     * @description
     * Edit Controller for the Umbraco Access Edit view
     */
    angular.module('umbraco').controller('Shield.Editors.UmbracoAccess.EditController',
        ['$scope', '$routeParams', 'notificationsService', 'localizationService', 'ShieldResource',
        function ($scope, $routeParams, notificationsService, localizationService, resource) {

            var vm = this;
            angular.extend(vm, {
                loading: true,
                configuration: $scope.configuration,
                contentPickerProperty: null,
                init: function () {
                    vm.contentPickerProperty = {
                        label: localizationService.localize('Shield.UmbracoAccess.Properties_UnauthorisedUrlLabel'),
                        description: localizationService.localize('Shield.UmbracoAccess.Properties_UnauthorisedUrlContentPickerDescription'),
                        view: 'contentpicker',
                        alias: 'unauthorisedUrlContentPicker',
                        config: {
                            multiPicker: "0",
                            entityType: "Document",
                            startNode: {
                                query: "",
                                type: "content",
                                id: -1
                            },
                            filter: "",
                            minNumber: 0,
                            maxNumber: 1
                        },
                        value: vm.configuration.unauthorisedUrlContentPicker
                    }

                    angular.forEach(vm.configuration.ipAddresses, function (ip, index) {
                        vm.ipAddressProperty.value.push({
                            ipAddress: ip.ipAddress,
                            description: ip.description,
                            editMode: false,
                            valid: true,
                            errorMsg: '',
                            errorState: null
                        });
                    });

                    vm.loading = false;
                },
                ipAddressProperty: {
                    value: [],
                    newIp: {
                        ipAddress: '',
                        description: '',
                        valid: true,
                        errorMsg: '',
                        errorState: null
                    },
                    addIp: function () {
                        if (!vm.ipAddressProperty.isValidIpAddress(vm.ipAddressProperty.newIp, false)) {
                            return false;
                        }

                        vm.ipAddressProperty.value.push({
                            ipAddress: vm.ipAddressProperty.newIp.ipAddress,
                            description: vm.ipAddressProperty.newIp.description,
                            editMode: false
                        });

                        vm.ipAddressProperty.newIp.ipAddress = '';
                        vm.ipAddressProperty.newIp.description = '';
                    },
                    editIp: function (ip, update) {
                        var curEditIp = vm.ipAddressProperty.value.filter((ip) => ip.editMode === true)[0];

                        if (curEditIp && !update) {
                            return false;
                        }

                        if (!update) {
                            ip.editMode = true;
                        } else {
                            if (!vm.ipAddressProperty.isValidIpAddress(curEditIp, true)) {
                                return false;
                            }

                            curEditIp.editMode = false;
                        }
                    },
                    removeIp: function (ip) {
                        localizationService.localize('Shield.UmbracoAccess.AlertMessages_ConfirmRemoveIp').then(function (warningMsg) {
                            if (confirm(warningMsg + ip.ipAddress + ' - ' + ip.description)) {
                                var index = vm.ipAddressProperty.value.indexOf(ip);

                                if (index !== -1) {
                                    vm.ipAddressProperty.value.splice(index, 1);
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
                            ip.errorState = 'Required';
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
                                ip.errorState = "Invalid";
                                return false;
                            }
                        }

                        var index = edit ? 1 : 0;

                            ip.valid = false;
                            ip.errorMsg = localizationService.localize('Shield.UmbracoAccess.ErrorMessages_IpDuplicate');
                            ip.errorState = "Duplicate";
                            return false;
                        }

                        return true;
                    }
                }
            });
        }]
    );
}(window));