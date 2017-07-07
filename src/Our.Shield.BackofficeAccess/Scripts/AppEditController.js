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
                    minNumber: 0,
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