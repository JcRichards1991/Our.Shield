
/**
 * @ngdoc controller
 * @name UmbracoAccess.EditController
 * @function
 *
 * @description
 * Edit Controller for the Umbraco Access Edit view
 */
angular.module('umbraco').controller('Shield.Editors.UmbracoAccess.EditController', ['$scope', 'notificationsService', 'localizationService', 'userService', 'ShieldUmbracoAccessResource', function ($scope, notificationsService, localizationService, userService, resource) {
    $scope.loading = 0;
    $scope.error = null;

    $scope.init = function () {
        $scope.loading++;

        resource.GetConfiguration().then(function success(response) {
            if (response.data) {
                $scope.configuration = response.data;
            } else {
                notificationsService.error(localizationService.localize('Shield.UmbracoAccess.ErrorMessages_GetConfiguration'));
                $scope.configuration = {
                    backendAccessUrl: '~/umbraco',
                    redirectRewrite: 0,
                    unauthorisedUrlType: 0,
                    ipAddresses: []
                };
            }

            $scope.properties = [{
                label: localizationService.localize('Shield.UmbracoAccess.Properties_EnableLabel'),
                description: localizationService.localize('Shield.UmbracoAccess.Properties_EnableDescription'),
                view: 'boolean',
                value: $scope.configuration.enable,
                visible: true
            }, {
                label: localizationService.localize('Shield.UmbracoAccess.Properties_BackendAccessUrlLabel'),
                description: localizationService.localize('Shield.UmbracoAccess.Properties_BackendAccessUrlDescription'),
                view: 'textbox',
                alias: 'backOfficeAccessUrl',
                value: $scope.configuration.backendAccessUrl,
                visible: true
            },
            {
                label: localizationService.localize('Shield.UmbracoAccess.Properties_RedirectRewriteLabel'),
                description: localizationService.localize('Shield.UmbracoAccess.Properties_RedirectRewriteDescription'),
                view: 'dropdown',
                alias: 'redirectRewrite',
                config: {
                    items: [{
                        value: localizationService.localize('Shield.UmbracoAccess.Properties_RedirectRewriteRedirectText'),
                        id: 0
                    }, {
                        value: localizationService.localize('Shield.UmbracoAccess.Properties_RedirectRewriteRewriteText'),
                        id: 1
                    }]
                },
                value: $scope.configuration.redirectRewrite,
                visible: true
            },
            {
                label: localizationService.localize('Shield.UmbracoAccess.Properties_UnauthorisedUrlTypeLabel'),
                description: localizationService.localize('Shield.UmbracoAccess.Properties_UnauthorisedUrlTypeDescription'),
                view: 'dropdown',
                alias: 'unauthorisedUrlType',
                config: {
                    items: [{
                        value: localizationService.localize('Shield.UmbracoAccess.Properties_UnauthorisedUrlTypeUrlText'),
                        id: 0
                    },
                    {
                        value: localizationService.localize('Shield.UmbracoAccess.Properties_UnauthorisedUrlTypeXPathText'),
                        id: 1
                    },
                    {
                        value: localizationService.localize('Shield.UmbracoAccess.Properties_UnauthorisedUrlTypeContentPickerText'),
                        id: 2
                    }],
                    multiple: false
                },
                value: $scope.configuration.unauthorisedUrlType,
                visible: true
            },
            {
                label: localizationService.localize('Shield.UmbracoAccess.Properties_UnauthorisedUrlLabel'),
                description: localizationService.localize('Shield.UmbracoAccess.Properties_UnauthorisedUrlDescription'),
                view: 'textbox',
                alias: 'unauthorisedUrl',
                value: $scope.configuration.unauthorisedUrl,
                visible: $scope.configuration.unauthorisedUrlType === 0
            },
            {
                label: localizationService.localize('Shield.UmbracoAccess.Properties_UnauthorisedUrlLabel'),
                description: localizationService.localize('Shield.UmbracoAccess.Properties_UnauthorisedUrlXPathDescription'),
                view: 'textbox',
                alias: 'unauthorisedUrlXPath',
                value: $scope.configuration.unauthorisedUrlXPath,
                visible: $scope.configuration.unauthorisedUrlType === 1
            },
            {
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
                value: $scope.configuration.unauthorisedUrlContentPicker,
                visible: $scope.configuration.unauthorisedUrlType === 2
            },
            {
                label: localizationService.localize('Shield.UmbracoAccess.Properties_AllowedIPsLabel'),
                description: localizationService.localize('Shield.UmbracoAccess.Properties_AllowedIPsDescription'),
                view: '/App_Plugins/Shield.UmbracoAccess/PropertyEditors/allowedIpsPropertyEditorView.html',
                alias: 'allowedIPs',
                config: {
                    showIpv4: true
                },
                value: $scope.configuration.ipAddresses,
                visible: true
            }];

            $scope.unauthorisedUrlTypeProperty = $scope.properties.filter((property) => property.alias === 'unauthorisedUrlType')[0];

            $scope.$watch('unauthorisedUrlTypeProperty.value', function (newVal, oldVal) {
                var unauthorisedUrlProperty = $scope.properties.filter((property) => property.alias === 'unauthorisedUrl')[0],
                    unauthorisedUrlXPathProperty = $scope.properties.filter((property) => property.alias === 'unauthorisedUrlXPath')[0],
                    unauthorisedUrlContentPickerProperty = $scope.properties.filter((property) => property.alias === 'unauthorisedUrlContentPicker')[0];

                switch (newVal) {
                    case 0:
                        unauthorisedUrlProperty.visible = true;
                        unauthorisedUrlXPathProperty.visible = false;
                        unauthorisedUrlContentPickerProperty.visible = false;
                        break;

                    case 1:
                        unauthorisedUrlProperty.visible = false;
                        unauthorisedUrlXPathProperty.visible = true;
                        unauthorisedUrlContentPickerProperty.visible = false;
                        break;

                    case 2:
                        unauthorisedUrlProperty.visible = false;
                        unauthorisedUrlXPathProperty.visible = false;
                        unauthorisedUrlContentPickerProperty.visible = true;
                        break;
                }
            });

            $scope.loading--;
        });
    };

    $scope.submitUmbracoAccess = function () {
        $scope.loading++;

        angular.forEach($scope.properties, function (property, key) {
            switch (property.alias) {
                case 'backOfficeAccessUrl':
                    $scope.configuration.backendAccessUrl = property.value;
                    break;

                case 'redirectRewrite':
                    $scope.configuration.redirectRewrite = property.value;
                    break;

                case 'unauthorisedUrlType':
                    $scope.configuration.unauthorisedUrlType = property.value;
                    break;

                case 'unauthorisedUrl':
                    $scope.configuration.unauthorisedUrl = property.value;
                    break;

                case 'unauthorisedUrlXPath':
                    $scope.configuration.unauthorisedUrlXPath = property.value;
                    break;

                case 'unauthorisedUrlContentPicker':
                    $scope.configuration.unauthorisedUrlContentPicker = property.value;
                    break;

                case 'allowedIPs':
                    $scope.configuration.ipAddresses = property.value;
                    break;
            }
        });

        resource.PostConfiguration($scope.configuration, userService.getCurrentUser()).then(function (response) {
            if (response.data) {
                notificationsService.success(localizationService.localize('Shield.UmbracoAccess.SuccessMessages_Updated'));
            } else {
                notificationsService.error(localizationService.localize('Shield.UmbracoAccess.ErrorMessages_UpdateConfiguration'));
            }

            $scope.loading--;
        });
    };
}]);