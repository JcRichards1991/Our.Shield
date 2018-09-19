angular
  .module('umbraco')
  .controller('Shield.Editors.Edit',
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

        angular.extend(vm, {
          type: null,
          id: $routeParams.id,
          editingEnvironment: $routeParams.edit === 'true' || $routeParams.id === '-100',
          name: '',
          description: '',
          loading: true,
          environments: [],
          environment: null,
          app: null,
          appView: null,
          configuration: null,
          path: [],
          ancestors: null,
          apps: [],
          tabs: [],
          button: {
            label: 'Update',
            labelKey: 'general_update',
            state: 'init'
          },
          init: function () {
            shieldResource.getView(vm.id).then(function (response) {
              vm.environments = response.data.environments;
              vm.environment = response.data.environment;
              vm.description = response.data.description;
              vm.name = response.data.name;

              switch (vm.type = response.data.type) {
                case 1:     //  Environment
                  if (vm.id === '-100') {
                    vm.environment.sortOrder = vm.environments.length === 1
                      ? 0
                      : vm.environments[vm.environments.length - 2].sortOrder + 1;

                    vm.button.labelKey = 'general_create';
                    localizationService.localize(vm.button.labelKey).then(function (value) {
                      vm.button.label = value;
                      vm.loading = false;
                    });
                  } else if (vm.id === '1' && vm.editingEnvironment) {
                    vm.cancelEditing();
                  } else {
                    vm.path = ['-1', vm.id];
                    vm.ancestors = [{ id: vm.id, name: vm.name }];
                    vm.apps = response.data.apps;

                    //  TODO: Make tab labels localized
                    vm.tabs = [
                      {
                        id: '0',
                        label: 'Apps',
                        view: '/App_Plugins/Shield/Backoffice/Views/Environment.html?version=1.0.6',
                        active: true
                      },
                      {
                        id: '1',
                        label: 'Settings',
                        view: '/App_Plugins/Shield/Backoffice/Views/EditEnvironment.html?version=1.0.6',
                        active: false
                      },
                      {
                        id: '2',
                        label: 'Journal',
                        view: '/App_Plugins/Shield/Backoffice/Dashboards/Journal.html?version=1.0.6',
                        active: false
                      }
                    ];
                  }

                  if (vm.id !== '1' && (vm.environment.domains === null || vm.environment.domains.length === 0)) {
                    vm.environment.domains = [
                      {
                        id: 0,
                        name: '',
                        umbracoDomainId: null,
                        environmentId: vm.environment.id
                      }
                    ];
                  }
                  break;

                case 2:     //  App
                  vm.path = ['-1', '' + vm.environment.id, vm.id];
                  vm.ancestors = [{ id: vm.environment.id, name: vm.environment.name }, { id: vm.id, name: vm.name }];

                  vm.app = response.data.app;
                  vm.appView = response.data.appView;
                  vm.configuration = response.data.configuration;
                  vm.tabs = response.data.tabs;
                  break;
              }

              $timeout(function () {
                navigationService.syncTree({ tree: 'shield', path: vm.path, forceReload: true, activate: true });
                vm.loading = false;
              });
            });
          },
          editItem: function (item) {
            $location.path('/shield/shield/edit/' + item.id);
          },
          editEnvironment: function () {
            $location.search('edit', 'true');
          },
          cancelEditing: function () {
            $location.search('edit', 'false');

            if (vm.id === '-100') {
              $location.path('/shield');
            }
          },
          save: function () {
            vm.button.state = 'busy';
            $scope.$broadcast('formSubmitting', { scope: $scope, action: 'publish' });
            if ($scope.shieldForm.$invalid) {
              //validation error, don't save

              angular.element(event.target).addClass('show-validation');

              var errorMsgDictionaryItem = '';

              switch (vm.type) {
                case 1:     //  Environment
                  if (vm.id === '-100') {
                    errorMsgDictionaryItem = 'CreateEnvironmentInvalid';
                  } else {
                    errorMsgDictionaryItem = 'SaveEnvironmentInvalid';
                  }
                  break;

                case 2:     //  App
                  errorMsgDictionaryItem = 'SaveConfigurationInvalid';
                  break;
              }

              localizationService.localize('Shield.General_' + errorMsgDictionaryItem).then(function (value) {
                notificationsService.error(value);
              });
              vm.button.state = 'error';
              return;
            }

            $scope.shieldForm.$setPristine();

            switch (vm.type) {

              case 1:     //  Environment
                shieldResource.postEnvironment(vm.environment).then(function (response) {
                  if (response.data === true || response.data === 'true') {
                    var saveMsgDictionaryItem = 'SaveEnvironmentSuccess';

                    if (vm.id === '-100') {
                      saveMsgDictionaryItem = 'CreateEnvironmentSuccess';
                    }

                    localizationService.localize('Shield.General_' + saveMsgDictionaryItem).then(function (value) {
                      notificationsService.success(value);
                    });

                    vm.cancelEditing();

                    if (vm.id === '-100') {
                      var path = ['-1', '-21'];
                      navigationService.syncTree({ tree: "shield", path: path, forceReload: true, activate: true });
                      $location.path('/shield');
                    } else {
                      $route.reload();
                    }

                  } else {
                    var errorMsgDictionaryItem = 'SaveEnvironmentError';

                    if (vm.id === '-100') {
                      errorMsgDictionaryItem = 'CreateEnvironmentError';
                    }

                    localizationService.localize('Shield.General_' + errorMsgDictionaryItem).then(function (value) {
                      notificationsService.error(value);
                    });
                    vm.button.state = 'error';
                  }
                });
                break;

              case 2:     //  App
                shieldResource.postConfiguration(vm.id, vm.configuration).then(function (response) {
                  if (response.data === true || response.data === 'true') {
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
                break;
            }
          }
        });
      }
    ]
  );