﻿angular
  .module('umbraco.directives')
  .directive('shieldApp',
    [
      '$compile',
      '$templateCache',
      '$http',
      function ($compile,
        $templateCache,
        $http) {
        return {
          restrict: 'E',
          scope: {
            view: '=',
            configuration: '='
          },
          link: function (scope, element) {
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
      }
    ]
  );

angular
  .module('umbraco.directives')
  .directive('shieldConvertToNumber',
    [
      function () {
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
    ]
  );

angular
  .module('umbraco.directives')
  .directive('shieldAddToForm',
    [
      function () {
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
        };
      }
    ]
  );

angular
  .module('umbraco.directives')
  .directive('shieldIpaddressvalid',
    [
      function () {
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

              return modelValue;
            });
          }
        };
      }
    ]
  );

angular
  .module('umbraco.directives')
  .directive('shieldIpAccessControl',
    [
      function () {
        return {
          restrict: 'E',
          templateUrl: '/App_Plugins/Shield/Backoffice/Views/Directives/IpAccessControl.html',
          scope: {
            ipAccessControl: '='
          },
          controller: [
            '$scope',
            'localizationService',
            'editorService',
            function ($scope,
              localizationService,
              editorService) {
              angular.extend($scope, {
                openDialog: function ($index) {
                  var ipAccessRule;
                  if ($index === -1) {
                    ipAccessRule = {
                      fromIpAddress: '',
                      toIpAddress: '',
                      description: '',
                      ipAddressType: 0
                    };
                  } else {
                    ipAccessRule = angular.copy($scope.ipAccessControl.ipAccessRules[$index]);
                  }

                  editorService.open({
                    view: '../App_Plugins/Shield/Backoffice/Views/Dialogs/EditIpAccessRule.html',
                    size: 'small',
                    ipAccessRule: ipAccessRule,
                    submit: function () {
                      if ($index === -1) {
                        $scope.ipAccessControl.ipAccessRules.push(ipAccessRule);
                      } else {
                        $scope.ipAccessControl.ipAccessRules[$index] = ipAccessRule;
                      }

                      editorService.close();
                    },
                    close: function () {
                      editorService.close();
                    }
                  });
                },
                remove: function ($index) {
                  var ipAccessRule = $scope.ipAccessControl.ipAccessRules[$index];

                  if (ipAccessRule.value !== '') {
                    var msg = ipAccessRule.value;

                    if (ipAccessRule.description !== '') {
                      msg += ' - ' + ipAccessRule.description;
                    }

                    localizationService
                      .localize('Shield.Properties.IpAccessControl.Messages_ConfirmRemoveIp')
                      .then(function (warningMsg) {
                        if (confirm(warningMsg + msg)) {
                          $scope.ipAccessControl.ipAccessRules.splice($index, 1);
                        }
                      });
                  } else {
                    $scope.ipAccessControl.ipAccessRules.splice($index, 1);
                  }
                }
              });
            }
          ]
        };
      }
    ]
  );

angular
  .module('umbraco.directives')
  .directive('shieldTransferUrlControl',
    [
      function () {
        return {
          restrict: 'E',
          templateUrl: '/App_Plugins/Shield/Backoffice/Views/Directives/TransferUrlControl.html',
          scope: {
            transferUrlControl: '='
          },
          link: function (scope) {
            if (scope.transferUrlControl.url === null) {
              scope.transferUrlControl.url = { type: 0, value: '' }
            };

            switch (scope.transferUrlControl.url.type) {
              case 0:
                scope.transferUrlControl.url.urlValue = scope.transferUrlControl.url.value;
                break;

              case 1:
                scope.transferUrlControl.url.xpathValue = scope.transferUrlControl.url.value;
                break;

              case 2:
                scope.transferUrlControl.url.mntpValue = scope.transferUrlControl.url.value || '';
                break;
            }

            angular.extend(scope, {
              contentPickerProperty: {
                view: 'contentpicker',
                alias: 'contentPicker',
                currentNode: {
                  path: '-1'
                },
                config: {
                  multiPicker: '0',
                  entityType: 'Document',
                  startNode: {
                    query: '',
                    type: 'content',
                    id: '-1'
                  },
                  filter: '',
                  minNumber: 1,
                  maxNumber: 1
                },
                value: scope.transferUrlControl.url.mntpValue
              }
            });

            scope.$on('formSubmitting', function () {
              switch (scope.transferUrlControl.url.type) {
                case 0:
                  scope.transferUrlControl.url.value = scope.transferUrlControl.url.urlValue;
                  break;

                case 1:
                  scope.transferUrlControl.url.value = scope.transferUrlControl.url.xpathValue;
                  break;

                case 2:
                  scope.transferUrlControl.url.value = scope.contentPickerProperty.url.mntpValue;
                  break;
              }
            });
          }
        };
      }
    ]
  );