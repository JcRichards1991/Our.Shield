(function(root){ 

/**
 * @ngdoc controller
 * @name UmbracoAccess.EditController
 * @function
 *
 * @description
 * Edit Controller for the Umbraco Access Edit view
 */
angular.module('umbraco').controller('Shield.Editors.UmbracoAccess.EditController', ['$scope', 'notificationsService', 'ShieldUmbracoAccessResource', function ($scope, notificationsService, resource) {
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
                description: 'The allowed IPs that can access the Backend Office Access Url.',
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
    $scope.newIp = {
        ipAddress: '',
        description: '',
        valid: true,
        errorMsg: ''
    };

    angular.forEach($scope.model.value, function (ip, index) {
        ip.editMode = false;
    });

    $scope.addIp = function () {
        $scope.newIp.errorMsg = '',
        $scope.newIp.valid = true;

        if ($scope.newIp.ipAddress === '') {
            $scope.newIp.valid = false;
            $scope.newIp.errorMsg = 'IP Address is required';
            return false;
        }

        if ($scope.model.value.filter((ip) => ip.ipAddress === $scope.newIp.ipAddress)[0] !== undefined) {
            $scope.newIp.valid = false;
            $scope.newIp.errorMsg = 'IP Address has already been added';
            return false;
        }

        var pattern = /^(?=\d+\.\d+\.\d+\.\d+($|\/))(([1-9]?\d|1\d\d|2[0-4]\d|25[0-5])\.?){4}(\/([0-9]|[1-2][0-9]|3[0-2]))?$/g;
        var valid = pattern.test($scope.newIp.ipAddress);

        if(!valid) {
            pattern = /^s*((([0-9A-Fa-f]{1,4}:){7}([0-9A-Fa-f]{1,4}|:))|(([0-9A-Fa-f]{1,4}:){6}(:[0-9A-Fa-f]{1,4}|((25[0-5]|2[0-4]d|1dd|[1-9]?d)(.(25[0-5]|2[0-4]d|1dd|[1-9]?d)){3})|:))|(([0-9A-Fa-f]{1,4}:){5}(((:[0-9A-Fa-f]{1,4}){1,2})|:((25[0-5]|2[0-4]d|1dd|[1-9]?d)(.(25[0-5]|2[0-4]d|1dd|[1-9]?d)){3})|:))|(([0-9A-Fa-f]{1,4}:){4}(((:[0-9A-Fa-f]{1,4}){1,3})|((:[0-9A-Fa-f]{1,4})?:((25[0-5]|2[0-4]d|1dd|[1-9]?d)(.(25[0-5]|2[0-4]d|1dd|[1-9]?d)){3}))|:))|(([0-9A-Fa-f]{1,4}:){3}(((:[0-9A-Fa-f]{1,4}){1,4})|((:[0-9A-Fa-f]{1,4}){0,2}:((25[0-5]|2[0-4]d|1dd|[1-9]?d)(.(25[0-5]|2[0-4]d|1dd|[1-9]?d)){3}))|:))|(([0-9A-Fa-f]{1,4}:){2}(((:[0-9A-Fa-f]{1,4}){1,5})|((:[0-9A-Fa-f]{1,4}){0,3}:((25[0-5]|2[0-4]d|1dd|[1-9]?d)(.(25[0-5]|2[0-4]d|1dd|[1-9]?d)){3}))|:))|(([0-9A-Fa-f]{1,4}:){1}(((:[0-9A-Fa-f]{1,4}){1,6})|((:[0-9A-Fa-f]{1,4}){0,4}:((25[0-5]|2[0-4]d|1dd|[1-9]?d)(.(25[0-5]|2[0-4]d|1dd|[1-9]?d)){3}))|:))|(:(((:[0-9A-Fa-f]{1,4}){1,7})|((:[0-9A-Fa-f]{1,4}){0,5}:((25[0-5]|2[0-4]d|1dd|[1-9]?d)(.(25[0-5]|2[0-4]d|1dd|[1-9]?d)){3}))|:)))(%.+)?s*(\/([0-9]|[1-9][0-9]|1[0-1][0-9]|12[0-8]))?$/g;
            valid = pattern.test($scope.newIp.ipAddress)

            if (!valid) {
                $scope.newIp.valid = false;
                $scope.newIp.errorMsg = 'Invalid IP Address. Please enter a valid IPv4 cidr or IPv6 cidr address';
                return false;
            }
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
            ip.editMode = false;
        }
    };

    $scope.removeIp = function (ip) {
        var index = $scope.model.value.indexOf(ip);

        if (index !== -1) {
            $scope.model.value.splice(index, 1);
        }
    };
}]);
 }(window));