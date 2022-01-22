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
  .controller('Shield.Controllers.JournalDashboard',
    [
      '$scope',
      '$routeParams',
      '$location',
      'listViewHelper',
      'shieldResource',
      function ($scope,
        $routeParams,
        $location,
        listViewHelper,
        shieldResource) {

        var vm = this;

        angular.extend(vm, {
          id: $routeParams.id,
          method: $routeParams.method,
          loading: true,
          items: [],
          totalPages: 1,
          pageNumber: 1,
          type: null,
          options: {
            orderBy: 'datestamp',
            orderDirection: 'desc'
          },
          columns: [
            {
              id: 0,
              name: 'Date',
              alias: 'datestamp',
              allowSorting: true,
              show: true
            },
            {
              id: 1,
              name: 'Environment',
              alias: 'environment',
              allowSorting: true,
              show: true
            },
            {
              id: 2,
              name: 'App',
              alias: 'app',
              allowSorting: false,
              show: true
            },
            {
              id: 3,
              name: 'Message',
              alias: 'message',
              allowSorting: false,
              show: true
            }
          ],
          init: function () {
            if ($routeParams.method === undefined) {
              vm.id = '';
              vm.method = 'Environments';
            }

            vm.getListing();
          },
          editEnvironment: function (key) {
            $location.path('/shield/shield/environment/' + key);
          },
          editApp: function (key) {
            $location.path('/shield/shield/app/' + key);
          },
          nextPage: function (page) {
            vm.pageNumber = page;
            vm.getListing();
          },
          previousPage: function (page) {
            vm.pageNumber = page;
            vm.getListing();
          },
          gotoPage: function (page) {
            vm.pageNumber = page;
            vm.getListing();
          },
          isSortDirection: function (col, direction) {
            return listViewHelper.setSortingDirection(col, direction, vm.options);
          },
          sort: function (field) {
            vm.options.orderBySystemField = false;
            listViewHelper.setSorting(field, allow, vm.options);
            vm.getListing();
          },
          getListing: function () {
            vm.loading = true;
            shieldResource
              .getJournals(vm.method, vm.id, vm.pageNumber, vm.options.orderBy, vm.options.orderDirection)
              .then(function (response) {
                vm.items = []; //response.items;
                vm.totalPages = 0; //response.totalPages;
                vm.loading = false;
              });
          }
        });
      }
    ]
  );

angular
  .module('umbraco')
  .controller('Shield.Controllers.EnvironmentCreate',
    [
      '$scope',
      '$location',
      'localizationService',
      'navigationService',
      'notificationsService',
      'shieldResource',
      function ($scope,
        $location,
        localizationService,
        navigationService,
        notificationsService,
        shieldResource) {
        var vm = this;
        angular.extend(vm,
          {
            button: {
              label: 'Create',
              labelKey: 'general_create',
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
            loading: true,
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
                  if (response.errorCode === 0) {
                    localizationService.localize('Shield.General_CreateEnvironmentSuccess').then(function (value) {
                      notificationsService.success(value);
                    });
                    navigationService.syncTree({ tree: "shield", path: ['-1', '-21'], forceReload: true, activate: true });
                    $location.path('/shield');
                  } else {
                    vm.button.state = 'error';
                    localizationService.localize('Shield.General_CreateEnvironmentError').then(function (value) {
                      notificationsService.error(value);
                    });
                  }
                });
            }
          });
      }
    ]);

angular
  .module('umbraco')
  .controller('Shield.Controllers.EnvironmentEdit',
    [
      '$scope',
      '$routeParams',
      '$location',
      'notificationsService',
      'localizationService',
      'navigationService',
      'shieldResource',
      function ($scope,
        $routeParams,
        $location,
        notificationsService,
        localizationService,
        navigationService,
        shieldResource) {
        var vm = this;
        angular.extend(vm,
          {
            environmentKey: $routeParams.id,
            editing: $routeParams.edit === 'true',
            loading: true,
            environment: null,
            path: [],
            ancestors: null,
            apps: [],
            //  TODO: Make tab labels localized
            tabs: [
              {
                id: 0,
                name: 'Apps',
                icon: 'icon-thumbnail-list',
                active: true,
                view: '/App_Plugins/Shield/BackOffice/Views/AppListing.html?version=2.0.0',
              },
              {
                id: 1,
                name: 'Settings',
                icon: 'icon-settings',
                active: false,
                view: '/App_Plugins/Shield/BackOffice/Views/EditEnvironment.html?version=2.0.0',
              },
              {
                id: 2,
                name: 'Journal',
                icon: 'icon-message',
                active: false,
                view: '/App_Plugins/Shield/BackOffice/Dashboards/Journal.html?version=2.0.0',
              }
            ],
            button: {
              label: 'Update',
              labelKey: 'general_update',
              state: 'init'
            },
            init: function () {
              shieldResource
                .getEnvironment(vm.environmentKey)
                .then(function (environmentResponse) {
                  vm.environment = environmentResponse.environment;

                  shieldResource
                    .getEnvironmentApps(vm.environmentKey)
                    .then(function (appsResponse) {
                      vm.apps = appsResponse.apps;

                      navigationService.syncTree({ tree: 'shield', path: vm.path, forceReload: true, activate: true });
                      vm.loading = false;
                    });
                });
            },
            save: function ($form) {
              vm.button.state = 'busy';
              $scope.$broadcast('formSubmitting', { scope: $scope, action: 'publish' });

              if ($form.$invalid) {
                //validation error, don't save
                vm.button.state = 'error';
                angular.element(event.target).addClass('show-validation');

                localizationService
                  .localize('Shield.General_SaveEnvironmentInvalid')
                  .then(function (value) {
                    notificationsService.error(value);
                  });
                return;
              }

              $form.$setPristine();

              shieldResource
                .upsertEnvironment(vm.environment)
                .then(function (response) {
                  if (response.errorCode === 0) {
                    localizationService
                      .localize('Shield.General_SaveEnvironmentSuccess')
                      .then(function (value) {
                        notificationsService.success(value);
                      });

                    navigationService.syncTree({ tree: "shield", path: vm.path, forceReload: true, activate: true });
                  } else {
                    vm.button.state = 'error';
                    localizationService
                      .localize('Shield.General_SaveEnvironmentError')
                      .then(function (value) {
                        notificationsService.error(value);
                      });
                  }
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
      '$timeout',
      '$route',
      'notificationsService',
      'localizationService',
      'navigationService',
      'shieldResource',
      function ($scope,
        $routeParams,
        $timeout,
        $route,
        notificationsService,
        localizationService,
        navigationService,
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
            name: '',
            description: '',
            init: function () {
              shieldResource
                .getApp(vm.appKey)
                .then(function (response) {
                  vm.app = response.app;
                  vm.config = response.configuration;
                  vm.tabs = response.tabs;

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
                .postConfiguration(vm.appKey, vm.config)
                .then(function (response) {
                  if (response === true || response === 'true') {
                    localizationService.localize('Shield.General_SaveConfigurationSuccess').then(function (value) {
                      notificationsService.success(value);
                    });

                    navigationService.syncTree({ tree: "shield", path: vm.path, forceReload: true, activate: true });
                    $route.reload();
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

                      if ($location.path() === '/shield') {
                        $route.reload();
                      } else {
                        $location.path("/shield");
                      }
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
          sorting: false,
          sortingComplete: false,
          environments: null,
          init: function () {
            shieldResource.getEnvironments().then(function (response) {
              vm.environments = response;
              vm.environments.splice(vm.environments.length - 1, 1);
              vm.loading = false;
            });
          },
          save: function () {
            if (vm.sorting) {
              return;
            }

            vm.sorting = true;

            for (var i = 0; i < vm.environments.length; i++) {
              vm.environments[i].sortOrder = i;
            }

            shieldResource.setEnvironmentsSortOrder(vm.environments)
              .then(function (response) {
                if (response === true || response === 'true') {
                  vm.sortingComplete = true;
                  vm.sorting = false;
                } else {
                  localizationService.localize('Shield.General_SortEnvironmentError')
                    .then(function (value) {
                      notificationsService.error(value);
                    });
                }
                vm.sorting = false;
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
              scope.transferUrlControl = { type: 0, value: '' }
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
              switch (scope.model.type) {
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
          getJournals: function (method, id, page, orderBy, orderByDirection) {
            return shieldResourceHelper.get(
              apiRoot + 'Journals',
              {
                method: method,
                id: id,
                page: page,
                orderBy: orderBy,
                orderByDirection: orderByDirection
              });
          },
          getView: function (id) {
            return shieldResourceHelper.get(apiRoot + 'View', { id: id });
          },
          postConfiguration: function (key, config) {
            return shieldResourceHelper.post(apiRoot + 'UpdateAppConfiguration?key=' + key, config);
          },
          setEnvironmentsSortOrder: function (environments) {
            return shieldResourceHelper.post(apiRoot + 'SortEnvironments', environments);
          },
          upsertEnvironment: function (environment) {
            return shieldResourceHelper.post(apiRoot + 'UpsertEnvironment', environment);
          }
        };
      }
    ]
  );