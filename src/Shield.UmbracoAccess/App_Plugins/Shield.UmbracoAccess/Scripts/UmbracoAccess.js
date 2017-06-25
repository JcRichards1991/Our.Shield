$(function (root) {
    /**
     * @ngdoc controller
     * @name UmbracoAccess.EditController
     * @function
     *
     * @description
     * Edit Controller for the Umbraco Access Edit view
     */
    angular.module('umbraco').controller('Shield.Editors.UmbracoAccess.EditController', ['$scope', '$routeParams', 'notificationsService', 'localizationService', 'ShieldResource', function ($scope, $routeParams, notificationsService, localizationService, resource) {
        function IsValidIpAddress(ip, edit) {
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

            if ($scope.IpAddressProperty.value.filter((x) => x.ipAddress === ip.ipAddress)[index] !== undefined) {
                ip.valid = false;
                ip.errorMsg = localizationService.localize('Shield.UmbracoAccess.ErrorMessages_IpDuplicate');
                ip.errorState = "Duplicate";
                return false;
            }

            return true;
        };

        $scope.loading = 0;
        $scope.error = null;

        $scope.IpAddressProperty = {
            value: [],
            newIp: {
                ipAddress: '',
                description: '',
                valid: true,
                errorMsg: '',
                errorState: null
            }
        }

        $scope.contentPickerProperty = {
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
            value: ''
        };

        $scope.init = function () {
            $scope.loading++;

            resource.GetConfiguration('UmbracoAccess').then(function success(response) {
                if (response.data && response.data.Data) {
                    $scope.configuration = response.data.Data;

                    $scope.contentPickerProperty.value = $scope.configuration.unauthorisedUrlContentPicker
                    angular.forEach($scope.configuration.ipAddresses, function (ip, index) {
                        $scope.IpAddressProperty.value.push({
                            ipAddress: ip.ipAddress,
                            description: ip.description,
                            editMode: false,
                            valid: true,
                            errorMsg: '',
                            errorState: null
                        });
                    });

                } else {
                    notificationsService.error(localizationService.localize('Shield.UmbracoAccess.ErrorMessages_GetConfiguration'));
                    $scope.configuration = {
                        ipAddresses: []
                    };
                }

                $scope.loading--;
            });
        };

        //$scope.$watch('configuration.unauthorisedUrlType', function (newVal, oldVal) {
        //    if (newVal === undefined)
        //        return;
        //    $scope.configuration.unauthorisedUrlType = parseInt(newVal);
        //});

        //$scope.$watch('configuration.enable', function (newVal, oldVal) {
        //    if (newVal === undefined)
        //        return;
        //    $scope.configuration.enable = newVal === true || newVal === 1 || newVal === "1" ? true : false;
        //});

        $scope.IpAddressProperty.addIp = function () {
            if (!IsValidIpAddress($scope.IpAddressProperty.newIp, false)) {
                return false;
            }

            $scope.IpAddressProperty.value.push({
                ipAddress: $scope.IpAddressProperty.newIp.ipAddress,
                description: $scope.IpAddressProperty.newIp.description,
                editMode: false
            });

            $scope.IpAddressProperty.newIp.ipAddress = '';
            $scope.IpAddressProperty.newIp.description = '';
        };

        $scope.IpAddressProperty.editIp = function (ip, update) {
            var curEditIp = $scope.IpAddressProperty.value.filter((ip) => ip.editMode === true)[0];

            if (curEditIp && !update) {
                return false;
            }

            if (!update) {
                ip.editMode = true;
            } else {
                if (!IsValidIpAddress(curEditIp, true)) {
                    return false;
                }

                curEditIp.editMode = false;
            }
        };

        $scope.IpAddressProperty.removeIp = function (ip) {
            localizationService.localize('Shield.UmbracoAccess.AlertMessages_ConfirmRemoveIp').then(function (warningMsg) {
                if (confirm(warningMsg + ip.ipAddress + ' - ' + ip.description)) {
                    var index = $scope.IpAddressProperty.value.indexOf(ip);

                    if (index !== -1) {
                        $scope.IpAddressProperty.value.splice(index, 1);
                    }
                }
            });
        };

        $scope.submitUmbracoAccess = function () {
            $scope.loading++;

            $scope.configuration.unauthorisedUrlContentPicker = $scope.contentPickerProperty.value;
            $scope.configuration.ipAddresses = $scope.IpAddressProperty.value;

            resource.PostConfiguration('UmbracoAccess', $scope.configuration).then(function (response) {
                if (response.data) {
                    notificationsService.success(localizationService.localize('Shield.UmbracoAccess.SuccessMessages_Updated'));
                } else {
                    notificationsService.error(localizationService.localize('Shield.UmbracoAccess.ErrorMessages_UpdateConfiguration'));
                }

                $scope.loading--;
            });
        };
    }]);
}(window));