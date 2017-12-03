/**
   * @ngdoc directive
   * @name shield-app
   * @function
   *
   * @description
   * Custom angular directive for inserting the Shield Apps view's onto the page 
*/
angular.module('umbraco.directives').directive('shieldApp',
    ['$compile', '$templateCache', '$http',
    function ($compile, $templateCache, $http) {
        return {
            restrict: 'E',
            scope: {
                view: '=',
                configuration: '='
            },
            link: function (scope, element, attr) {
                if (scope.view) {
                    var template = $templateCache.get(scope.view);
                    if (template) {
                        element.html(template);
                        $compile(element.contents())(scope);
                    } else {
                        $http.get(scope.view).then(function (response) {
                            $templateCache.put(scope.view, response.data);
                            element.html(response.data);
                            $compile(element.contents())(scope);
                        });
                    }
                }
            }
        };
    }]
);

/**
   * @ngdoc directive
   * @name shield-convert-to-number
   * @function
   *
   * @description
   * Custom angular directive for converting string to number
*/
angular.module('umbraco.directives').directive('shieldConvertToNumber', function () {
        return {
            restrict: 'A',
            require: 'ngModel',
            link: function (scope, element, attrs, ngModel) {
                ngModel.$parsers.push(function (val) {
                    return parseInt(val, 10);
                });
                ngModel.$formatters.push(function (val) {
                    return '' + val;
                });
            }
        };
    }
);

/**
   * @ngdoc directive
   * @name shield-add-to-form
   * @function
   *
   * @description
   * Adds form input elements to the backoffice access form for validation
*/
angular.module('umbraco.directives').directive('shieldAddToForm', function () {
        return {
            restrict: 'A',
            require: ['ngModel', '^form'],
            link: function ($scope, $element, $attr, controllers) {
                var ngModel = controllers[0],
                    $form = controllers[1];

                $form.$removeControl(ngModel);
                ngModel.$name = $attr.name;
                $form.$addControl(ngModel);
            }
        }
    }
);

/**
   * @ngdoc directive
   * @name shield-ipaddressvalid
   * @function
   *
   * @description
   * Custom angular directive for validating an IP Address
   * as IPv4 or IPv6 with optional cidr
*/
angular.module('umbraco.directives').directive('shieldIpaddressvalid', function () {
    return {
        restrict: 'A',
        require: 'ngModel',
        link: function (scope, elm, attr, ctrl) {
            ctrl.$parsers.push(function (modelValue) {
                if (modelValue === '' || modelValue === undefined) {
                    ctrl.$setValidity('shieldIpaddressvalid', true);
                    return modelValue;
                }

                //Check if IPv4 & IPv6
                var pattern = /^(([0-9]|[1-9][0-9]|1[0-9]{2}|2[0-4][0-9]|25[0-5])\.){3}([0-9]|[1-9][0-9]|1[0-9]{2}|2[0-4][0-9]|25[0-5])$|^(([a-zA-Z]|[a-zA-Z][a-zA-Z0-9\-]*[a-zA-Z0-9])\.)*([A-Za-z]|[A-Za-z][A-Za-z0-9\-]*[A-Za-z0-9])$|^\s*((([0-9A-Fa-f]{1,4}:){7}([0-9A-Fa-f]{1,4}|:))|(([0-9A-Fa-f]{1,4}:){6}(:[0-9A-Fa-f]{1,4}|((25[0-5]|2[0-4]\d|1\d\d|[1-9]?\d)(\.(25[0-5]|2[0-4]\d|1\d\d|[1-9]?\d)){3})|:))|(([0-9A-Fa-f]{1,4}:){5}(((:[0-9A-Fa-f]{1,4}){1,2})|:((25[0-5]|2[0-4]\d|1\d\d|[1-9]?\d)(\.(25[0-5]|2[0-4]\d|1\d\d|[1-9]?\d)){3})|:))|(([0-9A-Fa-f]{1,4}:){4}(((:[0-9A-Fa-f]{1,4}){1,3})|((:[0-9A-Fa-f]{1,4})?:((25[0-5]|2[0-4]\d|1\d\d|[1-9]?\d)(\.(25[0-5]|2[0-4]\d|1\d\d|[1-9]?\d)){3}))|:))|(([0-9A-Fa-f]{1,4}:){3}(((:[0-9A-Fa-f]{1,4}){1,4})|((:[0-9A-Fa-f]{1,4}){0,2}:((25[0-5]|2[0-4]\d|1\d\d|[1-9]?\d)(\.(25[0-5]|2[0-4]\d|1\d\d|[1-9]?\d)){3}))|:))|(([0-9A-Fa-f]{1,4}:){2}(((:[0-9A-Fa-f]{1,4}){1,5})|((:[0-9A-Fa-f]{1,4}){0,3}:((25[0-5]|2[0-4]\d|1\d\d|[1-9]?\d)(\.(25[0-5]|2[0-4]\d|1\d\d|[1-9]?\d)){3}))|:))|(([0-9A-Fa-f]{1,4}:){1}(((:[0-9A-Fa-f]{1,4}){1,6})|((:[0-9A-Fa-f]{1,4}){0,4}:((25[0-5]|2[0-4]\d|1\d\d|[1-9]?\d)(\.(25[0-5]|2[0-4]\d|1\d\d|[1-9]?\d)){3}))|:))|(:(((:[0-9A-Fa-f]{1,4}){1,7})|((:[0-9A-Fa-f]{1,4}){0,5}:((25[0-5]|2[0-4]\d|1\d\d|[1-9]?\d)(\.(25[0-5]|2[0-4]\d|1\d\d|[1-9]?\d)){3}))|:)))(%.+)?\s*$/;

                ctrl.$setValidity('shieldIpaddressvalid', pattern.test(modelValue));

                return modelValue
            });
        }
    };
});

/**
   * @ngdoc directive
   * @name shield-ipaddressduplicate
   * @function
   *
   * @description
   * Checks to make sure an IP address isn't being added more than
   * once to the IP address White-List
*/
angular.module('umbraco.directives').directive('shieldIpaddressduplicate', function () {
    return {
        restrict: 'A',
        require: 'ngModel',
        link: function (scope, elm, attr, ctrl) {
            ctrl.$parsers.push(function (modelValue) {
                if (modelValue === '' || modelValue === undefined) {
                    ctrl.$setValidity('shieldIpaddressduplicate', true);
                    return modelValue;
                }

                var ipAddresses = angular.fromJson(attr.shieldIpaddressduplicate);

                if (ipAddresses.filter((x) => x.ipAddress === modelValue)[0] !== undefined) {
                    ctrl.$setValidity('shieldIpaddressduplicate', false);
                    return modelValue;
                }

                ctrl.$setValidity('shieldIpaddressduplicate', true);
                return modelValue
            })
        }
    };
});

/**
   * @ngdoc directive
   * @name shield-ip-access-control
   * @function
   *
   * @description
   * Custom directive for handling whether or not to add IP Address restrictions
*/
angular.module('umbraco.directives').directive('shieldIpAccessControl', function () {
    return {
        restrict: 'E',
        templateUrl: '/App_Plugins/Shield/Backoffice/Views/Directives/IpAccessControl.html',
        scope: {
            model: '='
        },
        controller: ['$scope', function ($scope) {

        }]
    };
});

/**
   * @ngdoc directive
   * @name shield-ip-access-control-ranges
   * @function
   *
   * @description
   * Custom directive for handling the selected Url type
*/
angular.module('umbraco.directives').directive('shieldIpAccessControlRanges', function () {
    return {
        restrict: 'E',
        templateUrl: '/App_Plugins/Shield/Backoffice/Views/Directives/IpAccessControlRanges.html',
        scope: {
            exceptions: '='
        },
        controller: ['$scope', 'localizationService', 'dialogService', function ($scope, localizationService, dialogService) {
            angular.extend($scope, {
                add: function () {
                    $scope.openDialog(-1);
                },
                edit: function ($index) {
                    $scope.openDialog($index)
                },
                openDialog: function ($index) {
                    var dialogData = null;
                    if ($index === -1) {
                        dialogData = {
                            fromIpAddress: '',
                            toIpAddress: '',
                            description: '',
                            ipAddressType: 0
                        };
                    } else {
                        dialogData = angular.copy($scope.exceptions[$index]);
                    }

                    dialogService.open({
                        template: '../App_Plugins/Shield/Backoffice/Views/Dialogs/EditIpException.html',
                        dialogData: dialogData,
                        callback: function (ipException) {
                            if ($index === -1) {
                                $scope.exceptions.push(ipException);
                            } else {
                                $scope.exceptions[$index] = ipException;
                            }
                        }
                    });
                },
                remove: function ($index) {
                    var exception = $scope.exceptions[$index];

                    if (exception.value !== '') {
                        var msg = exception.value;

                        if (exception.description !== '') {
                            msg += ' - ' + exception.description;
                        }

                        localizationService.localize('Shield.Properties.IpAccessControl.Messages_ConfirmRemoveIp').then(function (warningMsg) {
                            if (confirm(warningMsg + msg)) {
                                $scope.exceptions.splice($index, 1);
                            }
                        });
                    } else {
                        $scope.exceptions.splice($index, 1);
                    }
                }
            });
        }]
    };
});

/**
   * @ngdoc directive
   * @name shield-umbraco-url
   * @function
   *
   * @description
   * Custom directive for handling the selected Url type
*/
angular.module('umbraco.directives').directive('shieldUmbracoUrl', function () {
    return {
        restrict: 'E',
        templateUrl: '/App_Plugins/Shield/Backoffice/Views/Directives/UmbracoUrl.html',
        scope: {
            model: '='
        },
        link: function (scope, elm, attr) {
            angular.extend(scope.model, {

                contentPickerProperty: {
                    view: 'contentpicker',
                    alias: 'contentPicker',
                    config: {
                        multiPicker: '0',
                        entityType: 'Document',
                        startNode: {
                            query: '',
                            type: 'content',
                            id: -1
                        },
                        filter: '',
                        minNumber: 1,
                        maxNumber: 1
                    },
                    value: scope.model.value
                }
            });

            scope.$watch('model.contentPickerProperty.value', function (newVal, oldVal) {
                scope.model.value = newVal;
            });
        }
    };
});

/**
   * @ngdoc directive
   * @name shield-transfer-url
   * @function
   *
   * @description
   * Custom directive for handling the selected Url type
*/
angular.module('umbraco.directives').directive('shieldTransferUrl', function () {
    return {
        restrict: 'E',
        templateUrl: '/App_Plugins/Shield/Backoffice/Views/Directives/TransferUrl.html',
        scope: {
            model: '='
        }
    };
});