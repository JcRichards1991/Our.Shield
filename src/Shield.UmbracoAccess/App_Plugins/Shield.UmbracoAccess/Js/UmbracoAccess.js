(function(root){ 

/**
 * @ngdoc controller
 * @name UmbracoAccess.EditController
 * @function
 *
 * @description
 * Edit Controller for the Umbraco Access Edit view
 */
angular.module('umbraco').controller('Shield.Editors.UmbracoAccess.EditController', ['$scope', 'notificationsService', 'localizationService', 'ShieldUmbracoAccessResource', function ($scope, notificationsService, localizationService, resource) {
    $scope.loading = 0;
    $scope.error = null;

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
                label: localizationService.localize('Shield.UmbracoAccess.Properties_BackendAccessUrlLabel'),
                description: localizationService.localize('Shield.UmbracoAccess.Properties_BackendAccessUrlDescription'),
                view: 'textbox',
                alias: 'backOfficeAccessUrl',
                value: $scope.configuration.backendAccessUrl,
                visible: true
            },
            {
                label: localizationService.localize('Shield.UmbracoAccess.Properties_StatusCodeLabel'),
                description: localizationService.localize('Shield.UmbracoAccess.Properties_StatusCodeDescription'),
                view: 'integer',
                alias: 'statusCode',
                value: $scope.configuration.statusCode,
                visible: true
            },
            {
                label: localizationService.localize('Shield.UmbracoAccess.Properties_UnauthorisedUrlTypeLabel'),
                description: localizationService.localize('Shield.UmbracoAccess.Properties_UnauthorisedUrlTypeDescription'),
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
                label: localizationService.localize('Shield.UmbracoAccess.Properties_UnauthorisedUrlLabel'),
                description: localizationService.localize('Shield.UmbracoAccess.Properties_UnauthorisedUrlDescription'),
                view: 'textbox',
                alias: 'unauthorisedUrl',
                value: $scope.configuration.unauthorisedUrl,
                visible: $scope.configuration.unauthorisedUrlType === 0
            },
            {
                label: localizationService.localize('Shield.UmbracoAccess.Properties_UnauthorisedUrlXPathLabel'),
                description: localizationService.localize('Shield.UmbracoAccess.Properties_UnauthorisedUrlXPathDescription'),
                view: 'textbox',
                alias: 'unauthorisedUrlXPath',
                value: $scope.configuration.unauthorisedUrlXPath,
                visible: $scope.configuration.unauthorisedUrlType === 1
            },
            {
                label: localizationService.localize('Shield.UmbracoAccess.Properties_UnauthorisedUrlContentPickerLabel'),
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

/**
 * @ngdoc resource
 * @name UmbracoAccessResource
 * @function
 *
 * @description
 * Api resource for the Umbraco Access area
*/
angular.module('umbraco.resources').factory('ShieldUmbracoAccessResource', ['$http', function ($http) {
    var apiRoot = 'backoffice/Shield/UmbracoAccessApi/';

    return {
        PostConfiguration: function (data) {
            return $http.post(apiRoot + 'PostConfiguration', angular.toJson(data));
        },
        GetConfiguration: function () {
            return $http.get(apiRoot + 'GetConfiguration');
        }
    };
}]);
/**
 * @ngdoc controller
 * @name PropertyEditors.AllowedIpsController
 * @function
 *
 * @description
 * Handles the Umbraco Access area of the custom section
 */
angular.module('umbraco').controller('Shield.PropertyEditors.AllowedIpsController', ['$scope', function ($scope) {

    function IsValidIpAddress(ip, edit) {
        ip.valid = true;
        ip.errorMsg = '';
        ip.errorState = null;

        if (ip.ipAddress === '') {
            ip.valid = false;
            ip.errorMsg = 'IP Address is required';
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
                ip.errorMsg = 'Invalid IP Address. Please enter a valid IPv4 or IPv6 address';
                ip.errorState = "Invalid";
                return false;
            }
        }

        var index = edit ? 1 : 0;

        if ($scope.model.value.filter((x) => x.ipAddress === ip.ipAddress)[index] !== undefined) {
            ip.valid = false;
            ip.errorMsg = 'IP Address has already been added';
            ip.errorState = "Duplicate";
            return false;
        }

        return true;
    };

    $scope.newIp = {
        ipAddress: '',
        description: '',
        valid: true,
        errorMsg: '',
        errorState: null
    };

    angular.forEach($scope.model.value, function (ip, index) {
        ip.editMode = false;
        ip.valid = true;
        ip.errorMsg = '';
        ip.errorState = null;
    });

    $scope.addIp = function () {
        if (!IsValidIpAddress($scope.newIp, false)) {
            return false;
        }

        $scope.model.value.push({
            ipAddress: $scope.newIp.ipAddress,
            description: $scope.newIp.description,
            editMode: false
        });

        $scope.newIp.ipAddress = '';
        $scope.newIp.description = '';
    };

    $scope.editIp = function (ip, update) {
        var curEditIp = $scope.model.value.filter((ip) => ip.editMode === true)[0];

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

    $scope.removeIp = function (ip) {
        if (confirm('Are you sure you want to remove IP Address: ' + ip.ipAddress + ' - ' + ip.description )) {
            var index = $scope.model.value.indexOf(ip);

            if (index !== -1) {
                $scope.model.value.splice(index, 1);
            }
        }
    };
}]);
 }(window));