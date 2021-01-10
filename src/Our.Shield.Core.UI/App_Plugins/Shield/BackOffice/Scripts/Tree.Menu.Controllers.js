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