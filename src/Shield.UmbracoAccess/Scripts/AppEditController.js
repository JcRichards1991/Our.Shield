/**
* @ngdoc controller
* @name Shield.UmbracoAccess.Edit
* @function
*
* @description
* Edit Controller for the Umbraco Access Edit view
*/
angular.module('umbraco').controller('Shield.Editors.UmbracoAccess.Edit',
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

                vm.loading = false;
            }
        });
    }]
);