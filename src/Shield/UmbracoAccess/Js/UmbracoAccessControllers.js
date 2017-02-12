
/**
 * @ngdoc controller
 * @name UmbracoAccess.EditController
 * @function
 *
 * @description
 * Handles the Umbraco Access area of the custom section
 */
angular.module('umbraco').controller('Shield.Editors.UmbracoAccess.EditController', ['$scope', 'notificationsService', 'ShieldUmbracoAccessResource', function ($scope, notificationsService, resource) {
    $scope.loading = 0;
    $scope.error = null;
    $scope.newIp = {
        Ip: '',
    };

    $scope.init = function () {
        $scope.loading++;

        resource.GetConfiguration().then(function success(response) {
            if (response.data === null || angular.isUndefined(response.data)) {
                notificationsService.error("Something went wrong getting the configuration, the error has been logged");
                $scope.configuration = {
                    backendAccessUrl: '~/umbraco',
                    statusCode: '404',
                    unauthorisedUrlType: 0,
                    unauthorisedUrl: '/404',
                    unauthorisedUrlXPath: '',
                    unauthorisedUrlContentPicker: '',
                    ipAddresses: [],
                    isDirty: false
                };
            } else {
                $scope.configuration = response.data;
            }

            $scope.properties = [{
                label: 'Backend Office Access URL',
                description: 'The URL used to access the backend office (Umbraco)',
                view: 'textbox',
                alias: 'backOfficeAccessUrl',
                value: $scope.configuration.backendAccessUrl,
                visible: true
            },
            {
                label: 'Status Code',
                description: 'The Status code to display to the user when accessing the Backend Access URL from a disallowed IP address',
                view: 'integer',
                alias: 'statusCode',
                value: $scope.configuration.statusCode,
                visible: true
            },
            {
                label: 'Unauthorised Url Type',
                description: 'The type of selector for the Unauthorised Url',
                view: 'dropdown',
                alias: 'unauthorisedUrlType',
                config: {
                    items: [{
                        value: 'String',
                        id: 0
                    },
                    {
                        value: 'XPath',
                        id: 1
                    },
                    {
                        value: 'Content Picker',
                        id: 2
                    }],
                    multiple: false
                },
                value: $scope.configuration.unauthorisedUrlType,
                visible: true
            },
            {
                label: 'Unautorised Url',
                description: 'The URL to redirect the user to when from a disallowed IP Address',
                view: 'textbox',
                alias: 'unauthorisedUrl',
                value: $scope.configuration.unauthorisedUrl,
                visible: $scope.configuration.unauthorisedUrlType === 0
            },
            {
                label: 'Unautorised Url by XPath',
                description: 'The XPath to the content node to redirect the user to when from a disallowed IP Address',
                view: 'textbox',
                alias: 'unauthorisedUrlXPath',
                value: $scope.configuration.unauthorisedUrlXPath,
                visible: $scope.configuration.unauthorisedUrlType === 1
            },
            {
                label: 'Unauthorised Url by Content Picker',
                description: 'Select the content node to redirect the user to when from a disallowed IP Address',
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
                label: 'Allowed IPs',
                description: 'The allowed IPs that can access the Backend Office Access Url. Localost (127.0.0.1) is added by default.',
                view: '/App_Plugins/Shield/backoffice/PropertyEditors/Views/allowedIps.html',
                alias: 'allowedIPs',
                config: { },
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

                case 'statusCode':
                    $scope.configuration.statusCode = property.value;
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

        resource.PostConfiguration($scope.configuration).then(function (response) {
            if (response.data === 'null' || response.data === undefined || response.data === 'false') {
                notificationsService.error("Something went wrong, the error has been logged");
            } else {
                notificationsService.success("Successfully updated");

                $scope.defaultConfiguration = {
                    backendAccessUrl: $scope.configuration.backendAccessUrl
                }
                $scope.displayUmbracoAccessWarningMessage = false;
            }

            $scope.loading--;
        });
    };
}]);