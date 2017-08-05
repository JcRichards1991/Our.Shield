/**
* @ngdoc controller
* @name Shield.BackofficeAccess.Edit
* @function
*
* @description
* Edit Controller for the Backoffice Access Edit view
*/
angular.module('umbraco').controller('Shield.Editors.BackofficeAccess.Edit',
    ['$scope', 'localizationService',
    function ($scope, localizationService) {

        var vm = this;

        angular.extend(vm, {
            loading: true,
            configuration: $scope.configuration,
            contentPickerProperty: {
                label: localizationService.localize('Shield.BackofficeAccess.Properties_UnauthorisedUrlLabel'),
                description: localizationService.localize('Shield.BackofficeAccess.Properties_UnauthorisedUrlContentPickerDescription'),
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
                    minNumber: 1,
                    maxNumber: 1
                }
            },
            init: function () {
                vm.contentPickerProperty.value = vm.configuration.unauthorisedUrlContentPicker;

                $scope.$watch('vm.contentPickerProperty.value', function (newVal, oldVal) {
                    vm.configuration.unauthorisedUrlContentPicker = newVal;
                });

                vm.loading = false;
            }
        });
    }]
);

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