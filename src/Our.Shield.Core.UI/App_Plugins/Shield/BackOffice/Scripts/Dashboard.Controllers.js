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
          editEnvironment: function (environmentKey) {
            $location.path('/shield/shield/environment/' + environmentKey);
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