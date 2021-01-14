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
          editEnvironment: function (key) {
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
  .controller('Shield.Controllers.EnvironmentEdit',
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
                }, 1000);
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
