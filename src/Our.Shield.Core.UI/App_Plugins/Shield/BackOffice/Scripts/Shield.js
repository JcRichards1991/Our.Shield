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
            shieldResource.getEnvironments().then(function (response) {
              vm.environments = response;
              vm.loading = false;
            });
          },
          addEnvironment: function () {
            $location.path('/settings/shield/CreateEnvironment');
          },
          editEnvironment: function (environmentKey) {
            $location.path('/settings/shield/environment/' + environmentKey);
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
  .controller('Shield.Editors.Dialogs.EditException',
    [
      '$scope',
      function ($scope) {
        var vm = this;

        angular.extend(vm, {
          exception: $scope.dialogData,
          buttonKey: '',
          init: function () {
            vm.buttonKey = 'general_' + (vm.exception.fromIpAddress === '' ? 'add' : 'update');
          },
          close: $scope.close,
          submit: function () {
            $scope.submit(vm.exception);
          }
        });
      }
    ]
  );
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
            model: '='
          }
        };
      }
    ]
  );

angular
  .module('umbraco.directives')
  .directive('shieldIpAccessControlRanges',
    [
      function () {
        return {
          restrict: 'E',
          templateUrl: '/App_Plugins/Shield/Backoffice/Views/Directives/IpAccessControlRanges.html',
          scope: {
            exceptions: '='
          },
          controller: [
            '$scope',
            'localizationService',
            'dialogService',
            function ($scope,
              localizationService,
              dialogService) {
              angular.extend($scope, {
                add: function () {
                  $scope.openDialog(-1);
                },
                edit: function ($index) {
                  $scope.openDialog($index);
                },
                openDialog: function ($index) {
                  var dialogData;
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
            }
          ]
        };
      }
    ]
  );

angular
  .module('umbraco.directives')
  .directive('shieldTransferUrl',
    [
      function () {
        return {
          restrict: 'E',
          templateUrl: '/App_Plugins/Shield/Backoffice/Views/Directives/TransferUrl.html?version=1.1.0',
          scope: {
            model: '='
          }
        };
      }
    ]
  );

angular
  .module('umbraco.directives')
  .directive('shieldUmbracoUrl',
    [
      function () {
        return {
          restrict: 'E',
          templateUrl: '/App_Plugins/Shield/Backoffice/Views/Directives/UmbracoUrl.html',
          scope: {
            model: '='
          },
          link: function (scope) {
            if (scope.model === null)
              scope.model = { type: 0, value: '' };

            switch (scope.model.type) {
              case 0:
                scope.model.urlValue = scope.model.value;
                break;

              case 1:
                scope.model.xpathValue = scope.model.value;
                break;

              case 2:
                scope.model.mntpValue = scope.model.value || '';
                break;
            }

            angular.extend(scope.model, {
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
                value: scope.model.mntpValue
              }
            });

            scope.$on('formSubmitting', function () {
              switch (scope.model.type) {
              case 0:
                scope.model.value = scope.model.urlValue;
                break;
              case 1:
                scope.model.value = scope.model.xpathValue;
                break;
              case 2:
                scope.model.value = scope.model.contentPickerProperty.value;
                break;
              }
            });
          }
        };
      }
    ]
  );
angular
  .module('umbraco')
  .controller('shield.editors.environment.edit',
    [
      '$scope',
      '$routeParams',
      '$location',
      '$timeout',
      '$route',
      'notificationsService',
      'localizationService',
      'navigationService',
      'shieldResource',
      function ($scope,
        $routeParams,
        $location,
        $timeout,
        $route,
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
            environment: {},
            path: [],
            ancestors: null,
            apps: [],
            //  TODO: Make tab labels localized
            tabs: [
              {
                id: '0',
                label: 'Apps',
                view: '/App_Plugins/Shield/BackOffice/Views/AppListing.html?version=1.1.3',
                active: true
              },
              {
                id: '1',
                label: 'Settings',
                view: '/App_Plugins/Shield/BackOffice/Views/EditEnvironment.html?version=1.1.3',
                active: false
              },
              {
                id: '2',
                label: 'Journal',
                view: '/App_Plugins/Shield/BackOffice/Dashboards/Journal.html?version=1.1.3',
                active: false
              }
            ],
            button: {
              label: 'Update',
              labelKey: 'general_update',
              state: 'init'
            },
            init: function () {
              shieldResource.getEnvironment(vm.environmentKey).then(function (environment) {
                vm.environment = environment;

                if (vm.environment.id === 1 && vm.editing) {
                  vm.cancelEditing();
                } else {
                  vm.path = ['-1', vm.environment.key];
                  vm.ancestors = [{ id: vm.environment.key, name: vm.environment.name }];
                }

                if (vm.environment.id !== 1 && (vm.environment.domains === null || vm.environment.domains.length === 0)) {
                  vm.environment.domains = [
                    {
                      id: 0,
                      name: '',
                      umbracoDomainId: null,
                      environmentId: vm.environment.id
                    }
                  ];
                }

                $timeout(function () {
                  navigationService.syncTree({ tree: 'shield', path: vm.path, forceReload: true, activate: true });
                  vm.loading = false;
                },1000);
              });
            },
            editApp: function (appKey) {
              $location.path('/shield/shield/App/' + appKey);
            },
            edit: function () {
              $location.search('edit', 'true');
            },
            cancelEditing: function () {
              $location.search('edit');
            },
            save: function ($form) {
              if ($form.overlayForm) {
                return;
              }

              vm.button.state = 'busy';
              $scope.$broadcast('formSubmitting', { scope: $scope, action: 'publish' });

              if ($form.$invalid) {
                //validation error, don't save
                vm.button.state = 'error';
                angular.element(event.target).addClass('show-validation');

                localizationService.localize('Shield.General_SaveEnvironmentInvalid').then(function (value) {
                  notificationsService.error(value);
                });
                return;
              }

              $form.$setPristine();

              shieldResource.postEnvironment(vm.environment).then(function (response) {
                if (response === true || response === 'true') {
                  localizationService.localize('Shield.General_SaveEnvironmentSuccess').then(function (value) {
                    notificationsService.success(value);
                  });
                  
                  navigationService.syncTree({ tree: "shield", path: vm.path, forceReload: true, activate: true });
                  vm.cancelEditing();
                } else {
                  vm.button.state = 'error';
                  localizationService.localize('Shield.General_SaveEnvironmentError').then(function (value) {
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
  .controller('shield.editors.environment.create',
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
              domains: [{ id: 0, name: '', umbracoDomainId: null, environmentId: 0 }],
              enable: false,
              continueProcessing: false
            },
            loading: true,
            init: function () {
              vm.loading = false;
            },
            save: function ($form) {
              if ($form.overlayForm) {
                return;
              }

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

              shieldResource.postEnvironment(vm.environment).then(function (response) {
                if (response === true || response === 'true') {
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
  .controller('shield.editors.app.edit',
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
            loading: true,
            app: {},
            path: [],
            ancestors: [],
            button: {
              label: 'Update',
              labelKey: 'general_update',
              state: 'init'
            },
            init: function () {
              shieldResource.getApp(vm.appKey).then(function (app) {
                vm.app = app;
                vm.path = ['-1', vm.app.environment.key, vm.appKey];
                vm.ancestors = [{ id: vm.app.environment.key, name: vm.app.environment.name }, { id: vm.appKey, name: vm.app.name }];

                $timeout(function () {
                  navigationService.syncTree({ tree: 'shield', path: vm.path, forceReload: true, activate: true });
                  vm.loading = false;
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

              shieldResource.postConfiguration(vm.appKey, vm.app.configuration).then(function (response) {
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
  .module('umbraco.resources')
  .factory('shieldResourceHelper',
    [
      '$http',
      '$q',
      function ($http, $q) {
        return {
          delete: function (url) {
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
            var deferred = $q.defer();

            $http({
              method: 'POST',
              url: url,
              data: JSON.stringify(data),
              dataType: 'json',
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
            return shieldResourceHelper.get(
              apiRoot + 'GetApp',
              {
                key: key
              });
          },
          getEnvironment: function (key) {
            return shieldResourceHelper.get(
              apiRoot + 'GetEnvironment',
              {
                key: key
              });
          },
          getEnvironments: function () {
            return shieldResourceHelper.get(apiRoot + 'GetEnvironments');
          },
          getJournals: function (method, id, page, orderBy, orderByDirection) {
            return shieldResourceHelper.get(apiRoot + 'Journals',
              {
                method: method,
                id: id,
                page: page,
                orderBy: orderBy,
                orderByDirection: orderByDirection
              });
          },
          getView: function (id) {
            return shieldResourceHelper.get(apiRoot + 'View',
              {
                id: id
              });
          },
          postConfiguration: function (key, config) {
            return shieldResourceHelper.post(
              apiRoot + 'WriteConfiguration?key=' + key,
              config);
          },
          postEnvironment: function (environment) {
            return shieldResourceHelper.post(
              apiRoot + 'WriteEnvironment',
              environment);
          },
          setEnvironmentsSortOrder: function (environments) {
            return shieldResourceHelper.post(
              apiRoot + 'SortEnvironments',
              environments);
          }
        };
      }
    ]
  );
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
                if (response === true || response === 'true') {
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