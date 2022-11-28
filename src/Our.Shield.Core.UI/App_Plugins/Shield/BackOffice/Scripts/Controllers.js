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
