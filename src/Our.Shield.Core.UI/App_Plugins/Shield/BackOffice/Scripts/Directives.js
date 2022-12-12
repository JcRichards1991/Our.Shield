angular
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
                      ipAddressType: 0,
                      isNew: true
                    };
                  } else {
                    ipAccessRule = angular.copy($scope.ipAccessControl.ipAccessRules[$index]);
                    ipAccessRule.isNew = false;
                  }

                  editorService.open({
                    view: '../App_Plugins/Shield/Backoffice/Views/Dialogs/EditIpAccessRule.html',
                    size: 'small',
                    ipAccessRule: ipAccessRule,
                    submit: function ($form) {
                      if ($form.$invalid) {
                        //validation error, don't save
                        angular.element(event.target).addClass('show-validation');
                        return;
                      }

                      $form.$setPristine();

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
                editor: 'Umbraco.ContentPicker',
                view: '/umbraco/views/propertyeditors/contentpicker/contentpicker.html',
                alias: 'contentPicker',
                currentNode: {
                  path: '-1'
                },
                config: {
                  idType: 'udi',
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
                  scope.transferUrlControl.url.value = scope.contentPickerProperty.value;
                  break;
              }
            });
          }
        };
      }
    ]
  );