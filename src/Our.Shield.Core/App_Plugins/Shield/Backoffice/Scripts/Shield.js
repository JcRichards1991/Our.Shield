angular
  .module('umbraco')
  .controller('Shield.Dashboards.Environments',
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
          Description: '',
          Environments: [],
          init: function () {
            shieldResource.getView('0').then(function (response) {
              vm.description = response.data.description;
              vm.environments = response.data.environments;

              vm.loading = false;
            });
          },
          editItem: function (item) {
            $location.path('/shield/shield/edit/' + item.id);
          }
        });
      }
    ]
  );

angular
  .module('umbraco')
  .controller('Shield.Dashboards.Journal',
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
            if (vm.id === undefined)
              vm.id = null;

            vm.getListing();
          },
          editItem: function (item) {
            $location.path('/shield/shield/edit/' + item.id);
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
            shieldResource.getJournals(vm.id, vm.pageNumber, vm.options.orderBy, vm.options.orderDirection).then(function (response) {
              vm.items = response.data.items;
              vm.totalPages = response.data.totalPages;
              vm.type = response.data.type;
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
  .directive('shieldIpddressvalid',
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

            scope.$watch('model.contentPickerProperty.value', function (newVal) {
              scope.model.value = newVal;
            });
          }
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
          templateUrl: '/App_Plugins/Shield/Backoffice/Views/Directives/TransferUrl.html?version=1.0.6',
          scope: {
            model: '='
          }
        };
      }
    ]
  );
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
angular
  .module('umbraco.resources')
  .factory('shieldResource',
    [
      '$http',
      '$q',
      function ($http, $q) {

        var apiRoot = 'backoffice/Shield/ShieldApi/';

        var get = function (url, data) {
          var deferred = $q.defer();

          data = data || {};

          $http
            .get(apiRoot + url,
              {
                params: data
              })
            .then(function (response) {
              return deferred.resolve(response.data);
            },
              function (response) {
                return deferred.resolve(response);
              });

          return deferred.promise;
        };

        var post = function (url, data) {
          var deferred = $q.defer();

          $http({
            method: 'POST',
            url: apiRoot + url,
            data: JSON.stringify(data),
            dataType: 'json',
            headers: {
              'Content-Type': 'application/json'
            }
          }).then(function (response) {
            return deferred.resolve(response.data);
          },
            function (response) {
              return deferred.resolve(response);
            });

          return deferred.promise;
        };

        return {
          deleteEnvironment: function (id) {
            return post('DeleteEnvironment', { id: id });
          },
          getEnvironments: function () {
            return get('GetEnvironments');
          },
          getJournals: function (id, page, orderBy, orderByDirection) {
            return get('Journals',
              {
                id: id,
                page: page,
                orderBy: orderBy,
                orderByDirection: orderByDirection
              });
          },
          getView: function (id) {
            return get('View',
              {
                id: id
              });
          },
          postConfiguration: function (id, config) {
            config.id = id;
            return post('WriteConfiguration', config);
          },
          postEnvironment: function (environment) {
            return post('WriteEnvironment', environment);
          },
          setEnvironmentsSortOrder: function (environments) {
            return post('SortEnvironments', environments);
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
            $location.path('/shield/shield/edit/-100');
          }
        });
      }
    ]
  );

angular
  .module('umbraco')
  .controller('Shield.Editors.Overview.Delete',
    [
      '$scope',
      '$routeParams',
      '$route',
      '$location',
      'treeService',
      'navigationService',
      'localizationService',
      'notificationsService',
      'shieldResource',
      function ($scope,
        $routeParams,
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

            shieldResource.deleteEnvironment(vm.currentNode.id).then(function (response) {
              if (response.data === true || response.data === 'true') {
                localizationService.localize('Shield.General_DeleteEnvironmentSuccess').then(function (value) {
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
                localizationService.localize('Shield.General_DeleteEnvironmentError').then(function (value) {
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
    ]
  );

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
              vm.environments = response.data;
              vm.environments.splice(vm.environments.length - 1, 1);
              vm.loading = false;
            });
          },
          save: function () {
            vm.sorting = true;

            for (var i = 0; i < vm.environments.length; i++) {
              vm.environments[i].sortOrder = i;
            }

            shieldResource.setEnvironmentsSortOrder(vm.environments).then(function (response) {
              if (response.data === true || response.data === 'true') {
                vm.sortingComplete = true;
              } else {
                localizationService.localize('Shield.General_SortEnvironmentError').then(function (value) {
                  notificationsService.error(value);
                });
              }
              vm.sorting = false;
            });
          }
        });
      }
    ]
  );