angular
  .module('umbraco')
  .controller('Shield.Controllers.Dashboard',
    [
      '$scope',
      'dashboardResource',
      'localizationService',
      function ($scope,
        dashboardResource,
        localizationService) {

        var dashboardCtrl = this;

        angular.extend(dashboardCtrl, {
          loading: true,
          name: '',
          tabs: [],
          init: function () {
            localizationService
              .localize('sections_Shield')
              .then(function (name) {
                dashboardCtrl.name = name;
              });

            dashboardResource
              .getDashboard('Shield')
              .then(function (tabs) {
                dashboardCtrl.tabs = tabs;
                // set first tab to active
                if (dashboardCtrl.tabs && dashboardCtrl.tabs.length > 0) {
                  dashboardCtrl.tabs[0].active = true;
                }
                dashboardCtrl.loading = false;
              });
          },
          changeTab: function (tab) {
            dashboardCtrl.tabs.forEach(function (tab) {
              tab.active = false;
            });

            tab.active = true;
          }
        });
      }
    ]);

angular
  .module('umbraco')
  .controller('Shield.Controllers.EnvironmentsDashboard',
    [
      '$scope',
      '$location',
      'shieldResource',
      function ($scope,
        $location,
        shieldResource) {
        var vm = this;
        angular.extend(vm, {
          loading: true,
          environments: [],
          init: function () {
            shieldResource
              .getEnvironments()
              .then(function (response) {
                vm.environments = response.environments;
                vm.loading = false;
              });
          },
          addEnvironment: function () {
            $location.path('/settings/shield/CreateEnvironment');
          },
          clickEnvironment: function (key) {
            $location.path('/settings/shield/environment/' + key);
          },
          sortEnvironments: function () {

          }
        });
      }
    ]
  );

angular
  .module('umbraco')
  .controller('Shield.Controllers.CreateEnvironment',
    [
      '$scope',
      'localizationService',
      'notificationsService',
      'shieldResource',
      function ($scope,
        localizationService,
        notificationsService,
        shieldResource) {
        var vm = this;
        angular.extend(vm,
          {
            loading: true,
            navigation: [
              {
                name: 'Settings',
                alias: 'settings',
                icon: 'icon-settings',
                view: '/App_Plugins/Shield/BackOffice/Views/EditEnvironment.html?version=2.0.0',
                active: true
              }
            ],
            button: {
              label: 'Create',
              labelKey: 'actions_create',
              state: 'init'
            },
            environment: {
              icon: '',
              name: '',
              domains: [],
              enabled: false,
              continueProcessing: false
            },
            init: function () {
              shieldResource
                .getEnvironments()
                .then(function (response) {
                  //  TODO: determine from response.environments the correct sort order for this environment
                  vm.environment.sortOrder = 20;
                  vm.loading = false;
                });
            },
            save: function ($form) {
              vm.button.state = 'busy';
              $scope.$broadcast('formSubmitting', { scope: $scope, action: 'publish' });

              if ($form.$invalid) {
                //validation error, don't save
                vm.button.state = 'error';
                angular.element(event.target).addClass('show-validation');

                localizationService.localize('Shield.General_CreateEnvironmentInvalid').then(function (value) {
                  notificationsService.error(value);
                });
                return;
              }

              $form.$setPristine();

              shieldResource
                .upsertEnvironment(vm.environment)
                .then(function (response) {
                  localizationService
                    .localize('Shield.General_CreateEnvironment' + (response.errorCode === 0 ? 'Success' : 'Error'))
                    .then(function (value) {
                      vm.button.state = 'init';
                      response.errorCode === 0
                        ? notificationsService.success(value)
                        : notificationsService.error(value);
                    });
                });
            }
          });
      }
    ]);

angular
  .module('umbraco')
  .controller('Shield.Controllers.ViewEnvironment',
    [
      '$scope',
      '$routeParams',
      'localizationService',
      'notificationsService',
      'shieldResource',
      function ($scope,
        $routeParams,
        localizationService,
        notificationsService,
        shieldResource) {
        var vm = this;
        angular.extend(vm,
          {
            environmentKey: $routeParams.id,
            loading: true,
            environment: { domains: [] },
            button: {
              label: 'Save',
              labelKey: 'buttons_save',
              state: 'init'
            },
            navigation: [
              {
                name: 'Apps',
                alias: 'apps',
                icon: 'icon-thumbnail-list',
                view: '/App_Plugins/Shield/BackOffice/Views/AppListing.html?version=2.0.0',
                active: true
              },
              {
                name: 'Settings',
                alias: 'settings',
                icon: 'icon-settings',
                view: '/App_Plugins/Shield/BackOffice/Views/EditEnvironment.html?version=2.0.0',
                active: false
              }
            ],
            init: function () {
              shieldResource
                .getEnvironment(vm.environmentKey)
                .then(function (environmentResponse) {
                  vm.environment = environmentResponse.environment;
                  vm.loading = false;
                });
            },
            save: function ($form) {
              vm.button.state = 'busy';
              $scope.$broadcast('formSubmitting', { scope: $scope, action: 'publish' });

              if ($form.$invalid) {
                //validation error, don't save
                vm.button.state = 'error';
                angular.element(event.target).addClass('show-validation');

                localizationService.localize('Shield.General_EditEnvironmentInvalid').then(function (value) {
                  notificationsService.error(value);
                });
                return;
              }

              $form.$setPristine();

              shieldResource
                .upsertEnvironment(vm.environment)
                .then(function (response) {
                  localizationService
                    .localize('Shield.General_EditEnvironment' + (response.errorCode === 0 ? 'Success' : 'Error'))
                    .then(function (value) {
                      vm.button.state = 'init';
                      response.errorCode === 0
                        ? notificationsService.success(value)
                        : notificationsService.error(value);
                    });
                });
            }
          });
      }
    ]);

angular
  .module('umbraco')
  .controller('Shield.Controllers.EnvironmentAppsListing',
    [
      '$scope',
      '$routeParams',
      '$location',
      'shieldResource',
      function ($scope,
        $routeParams,
        $location,
        shieldResource) {
        var vm = this;
        angular.extend(vm,
          {
            environmentKey: $routeParams.id,
            editing: $routeParams.edit === 'true',
            loading: true,
            apps: [],
            
            init: function () {
              shieldResource
                .getEnvironmentApps(vm.environmentKey)
                .then(function (appsResponse) {
                  vm.apps = appsResponse.apps;
                  vm.loading = false;
                });
            },
            appClick: function (key) {
              $location.path('/settings/shield/app/' + key);
            },
            getAppNameKey: function (appId) {
              return 'Shield.' + appId + '_Name'
            },
            getAppDescriptionKey: function (appId) {
              return 'Shield.' + appId + '_Description'
            }
          });
      }
    ]);

angular
  .module('umbraco')
  .controller('Shield.Controllers.AppEdit',
    [
      '$scope',
      '$routeParams',
      'notificationsService',
      'localizationService',
      'shieldResource',
      function ($scope,
        $routeParams,
        notificationsService,
        localizationService,
        shieldResource) {
        var vm = this;
        angular.extend(vm,
          {
            appKey: $routeParams.id,
            state: {
              loading: true
            },
            button: {
              label: 'Update',
              labelKey: 'general_update',
              state: 'init'
            },
            app: null,
            config: null,
            tabs: [],
            environmentKey: '',
            name: '',
            description: '',
            init: function () {
              shieldResource
                .getApp(vm.appKey)
                .then(function (response) {
                  vm.app = response.app;
                  vm.config = response.configuration;
                  vm.tabs = response.tabs;
                  vm.environmentKey = response.environmentKey;

                  localizationService
                    .localize('Shield.' + vm.app.id + '_Name')
                    .then(function (name) {
                      vm.name = name;

                      localizationService
                        .localize('Shield.' + vm.app.id + '_Description')
                        .then(function (description) {
                          vm.description = description;
                          vm.state.loading = false;
                        });
                    });
                });
            },
            save: function () {
              vm.button.state = 'busy';
              $scope.$broadcast('formSubmitting', { scope: $scope, action: 'publish' });
              if ($scope.appForm.$invalid) {
                angular.element(event.target).addClass('show-validation');
                localizationService.localize('Shield.General_SaveConfigurationInvalid').then(function (value) {
                  notificationsService.error(value);
                });
                vm.button.state = 'error';
                return;
              }

              $scope.appForm.$setPristine();

              shieldResource
                .postConfiguration(vm.app.id, vm.appKey, vm.config, vm.environmentKey)
                .then(function (response) {
                  if (response.errorCode === 0) {
                    localizationService.localize('Shield.General_SaveConfigurationSuccess').then(function (value) {
                      notificationsService.success(value);
                    });

                    vm.button.state = 'init';
                  } else {
                    localizationService.localize('Shield.General_SaveConfigurationError').then(function (value) {
                      notificationsService.error(value);
                    });
                    vm.button.state = 'error';
                  }
                });
            }
          });
      }
    ]);

angular
  .module('umbraco')
  .controller('Shield.Editors.Overview.Create',
    [
      '$scope',
      '$location',
      'navigationService',
      function ($scope,
        $location,
        navigationService) {
        var vm = this;

        angular.extend(vm, {
          create: function () {
            navigationService.hideDialog();
            $location.path('/settings/shield/CreateEnvironment/');
          }
        });
      }
    ]);

angular
  .module('umbraco')
  .controller('Shield.Editors.Overview.Delete',
    [
      '$scope',
      '$route',
      '$location',
      'treeService',
      'navigationService',
      'localizationService',
      'notificationsService',
      'shieldResource',
      function ($scope,
        $route,
        $location,
        treeService,
        navigationService,
        localizationService,
        notificationsService,
        shieldResource) {
        var vm = this;

        angular.extend(vm, {
          busy: false,
          currentNode: $scope.currentNode,
          performDelete: function () {
            if (vm.busy) {
              return;
            }

            vm.currentNode.loading = true;
            vm.busy = true;

            shieldResource
              .deleteEnvironment(vm.currentNode.id)
              .then(function (response) {
                if (response.successful === true && response.errorCode === 0) {
                  localizationService.localize('Shield.General_DeleteEnvironmentSuccess')
                    .then(function (value) {
                      notificationsService.success(value);
                      vm.currentNode.loading = false;
                      treeService.removeNode(vm.currentNode);
                      $location.path("/settings/shield/Dashboard");
                    });
                  navigationService.hideMenu();
                } else {
                  localizationService.localize('Shield.General_DeleteEnvironmentError')
                    .then(function (value) {
                      notificationsService.error(value);
                    });
                }
              });
          },
          cancel: function () {
            navigationService.hideDialog();
          }
        });
      }
    ]);

angular
  .module('umbraco')
  .controller('Shield.Editors.Overview.Sort',
    [
      '$scope',
      'localizationService',
      'notificationsService',
      'shieldResource',
      function ($scope,
        localizationService,
        notificationsService,
        shieldResource) {
        var vm = this;

        angular.extend(vm, {
          loading: true,
          environments: [],
          button: {
            state: 'init'
          },
          init: function () {
            shieldResource.getEnvironments().then(function (response) {
              angular.forEach(response.environments, function (value) {
                if (value.key != '705b8967-070e-44c8-805d-57e0f46af779') {
                  vm.environments.push(value);
                }
              })
              vm.loading = false;
            });
          },
          save: function () {
            vm.loading = true;
            vm.button.state = 'busy';
            for (var i = 0; i < vm.environments.length; i++) {
              vm.environments[i].sortOrder = i;
            }

            shieldResource.setEnvironmentsSortOrder(vm.environments)
              .then(function (response) {
                if (response.errorCode !== 0) {
                  localizationService.localize('Shield.General_SortEnvironmentError')
                    .then(function (value) {
                      notificationsService.error(value);
                      vm.button.state = 'error';
                      vm.loading = false;
                    });
                }
                else {
                  vm.button.state = 'init';
                  vm.loading = false;
                }
              });
          }
        });
      }
    ]);

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
angular
  .module('umbraco.resources')
  .factory('shieldResourceHelper',
    [
      '$http',
      '$q',
      function ($http, $q) {
        return {
          delete: function (url) {
            if (!url) {
              throw Error('url is required');
            }

            var deferred = $q.defer();

            $http({
              method: 'DELETE',
              url: url,
              headers: {
                'Content-Type': 'application/json'
              }
            }).then(function (response) {
              return deferred.resolve(response.data);
            }, function (response) {
              console.log(response);

              return deferred.resolve(false);
            });

            return deferred.promise;
          },
          get: function (url, data) {
            if (!url) {
              throw Error('url is required');
            }

            var deferred = $q.defer();

            data = data || {};

            $http
              .get(url,
                {
                  params: data
                })
              .then(function (response) {
                return deferred.resolve(response.data);
              }, function (response) {
                console.log(response);

                return deferred.resolve(false);
              });

            return deferred.promise;
          },
          post: function (url, data) {
            if (!url) {
              throw Error('url is required');
            }

            if (!data) {
              throw Error('data is required');
            }

            var deferred = $q.defer();

            $http({
              method: 'POST',
              url: url,
              data: JSON.stringify(data),
              dataType: 'json',
              contentType: 'application/json'
            }).then(function (response) {
              return deferred.resolve(response.data);
            }, function (response) {
              console.log(response);

              return deferred.resolve(false);
            });

            return deferred.promise;
          }
        };
      }
    ]);

angular
  .module('umbraco.resources')
  .factory('shieldResource',
    [
      'shieldResourceHelper',
      function (shieldResourceHelper) {
        var apiRoot = 'backoffice/shield/ShieldApi/';

        return {
          deleteEnvironment: function (key) {
            return shieldResourceHelper.delete(apiRoot + 'DeleteEnvironment?key=' + key);
          },
          getApp: function (key) {
            return shieldResourceHelper.get(apiRoot + 'GetApp', { key: key });
          },
          getEnvironment: function (key) {
            return shieldResourceHelper.get(apiRoot + 'GetEnvironment', { key: key });
          },
          getEnvironments: function () {
            return shieldResourceHelper.get(apiRoot + 'GetEnvironments');
          },
          getEnvironmentApps: function (environmentKey) {
            return shieldResourceHelper.get(
              apiRoot + 'GetEnvironmentApps',
              {
                environmentKey: environmentKey
              });
          },
          getView: function (id) {
            return shieldResourceHelper.get(apiRoot + 'View', { id: id });
          },
          postConfiguration: function (appId, key, config, environmentKey) {
            return shieldResourceHelper.post(
              apiRoot + 'UpdateAppConfiguration',
              {
                appId: appId,
                key: key,
                environmentKey: environmentKey,
                configuration: config
              });
          },
          setEnvironmentsSortOrder: function (environments) {
            return shieldResourceHelper.post(apiRoot + 'SortEnvironments', { environments: environments });
          },
          upsertEnvironment: function (environment) {
            return shieldResourceHelper.post(apiRoot + 'UpsertEnvironment', environment);
          }
        };
      }
    ]
  );