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
              vm.description = response.description;
              vm.environments = response.environments;

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
            shieldResource
              .getJournals(vm.id, vm.pageNumber, vm.options.orderBy, vm.options.orderDirection)
              .then(function (response) {
                vm.items = response.items;
                vm.totalPages = response.totalPages;
                vm.type = response.type;
                vm.loading = false;
              });
          }
        });
      }
    ]
  );