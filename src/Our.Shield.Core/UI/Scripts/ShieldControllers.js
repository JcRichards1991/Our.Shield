/**
    * @ngdoc resource
    * @name Environments
    * @function
    *
    * @description
    * Handles environment page
*/
angular.module('umbraco').controller('Shield.Editors.Edit', 
    ['$scope', '$routeParams', '$location', '$timeout', 'notificationsService', 'localizationService', 'ShieldResource', 'listViewHelper', 'navigationService', 'assetsService',
    function ($scope, $routeParams, $location, $timeout, notificationsService, localizationService, shieldResource, listViewHelper, navigationService, assetsService) {

        var vm = this;

        angular.extend(vm, {
            type: null,
            id: $routeParams.id,
            name: '',
            description: '',
            loading: true,
            saveButtonState: 'init',
            environments: [],
            environment: null,
            apps: [],
            app: null,
            configuration: null,
            path: null,
            appView: null,
            ancestors: null,
            tabs: [
                {
                    id:'0',
                    label: 'Environments',
                    active: true,
                },
                {
                    id:'1',
                    label:'Journal',
                    active: true
                }
            ],
            init: function () {
                shieldResource.getView(vm.id).then(function (response) {
                    vm.name = response.data.name;
                    vm.description = response.data.description;
                    switch (vm.type = response.data.type) {
                        case 0:     //  Environments
                            vm.nameLocked = true;
                            vm.environments = response.data.environments;
                            vm.environment = response.data.environment;
                            vm.path = '-1,0';
                            vm.ancestors = [{ id: vm.id, name: vm.name }]
                            break;

                        case 1:     //  Environment
                            vm.nameLocked = false;
                            vm.environments = response.data.environments;
                            vm.environment = response.data.environment;
                            vm.apps = response.data.apps;
                            vm.tabs[0].label = 'Domains';
                            vm.journal.columns[1].show = false;
                            vm.path = '-1,0,' + vm.id;
                            vm.ancestors = [{ id: 0, name: 'Environments' },{ id: vm.id, name: vm.name }]
                            break;

                        case 2:     //  App
                            vm.nameLocked = true;
                            vm.environment = response.data.environment;
                            vm.app = response.data.app;
                            vm.appView = response.data.appAssests.view;
                            vm.configuration = response.data.configuration;
                            vm.tabs[0].label = 'Configuration'
                            vm.journal.columns[1].show = false;
                            vm.journal.columns[2].show = false;
                            vm.path = '-1,0,' + vm.environment.id + ',' + vm.id;
                            vm.ancestors = [{ id: 0, name: 'Environments' }, {id: vm.environment.id, name: vm.environment.name}, { id: vm.id, name: vm.name }]

                            angular.forEach(response.data.appAssests.stylesheets, function (item, index) {
                                assetsService.loadCss(item);
                            });

                            angular.forEach(response.data.appAssests.scripts, function (item, index) {
                                assetsService.loadJs(item);
                            });

                            break;

                    }
                    $timeout(function () {
                        navigationService.syncTree({ tree: 'Shield', path: vm.path, forceReload: false, activate: true });
                        vm.loading = false;
                    });
                });
            },
            save: function () {

                switch (vm.type) {
                    case 1:     //  Environment
                        vm.saveButtonState = 'busy';

                        //  TODO: Write stuff

                        vm.saveButtonState = 'success';
                        break;

                    case 2:     //  App
                        vm.saveButtonState = 'busy';
                        shieldResource.postConfiguration(vm.id, vm.configuration).then(function (response) {
                            if (response.data) {
                                localizationService.localize("Shield.General_SaveSuccess").then(function (value) {
                                    notificationsService.success(value);
                                });
                                navigationService.syncTree({ tree: 'Shield', path: vm.path, forceReload: true, activate: true });
                                vm.saveButtonState = 'init';
                                $scope.shieldForm.$setPristine();
                            } else {
                                localizationService.localize("Shield.General_SaveError").then(function (value) {
                                    notificationsService.error(value);
                                });
                                vm.saveButtonState = 'error';
                            }
                        });
                        break;
                }

            },
            editItem: function (item, index) {
                $location.path('shield/shield/edit/' + item.id);
            },
            journal: {
                columns: [
                    {
                        id: 0,
                        name: 'Date',
                        allowSorting: true,
                        show: true
                    },
                    {
                        id: 1,
                        name: 'Environments',
                        allowSorting: false,
                        show: true
                    },
                    {
                        id: 2,
                        name: 'App',
                        allowSorting: false,
                        show: true
                    },
                    {
                        id: 3,
                        name: 'Message',
                        allowSorting: false,
                        show: true
                    },
                ],
                items: null,
                selections: [],
                totalPages: 0,
                pageNumber: 0,
                nextPage: function (page) {
                },
                previousPage: function (page) {
                },
                gotoPage: function (page) {
                },
                selectAllItems: function (event) {
                    listViewHelper.selectAllItems(vm.journal.items, vm.journal.selections, event);
                },
                isSelectedAllItems: function () {
                    return listViewHelper.isSelectedAll(vm.journal.items, vm.journal.selections);
                },
                sort: function (id) {
                    //$scope.options.orderBySystemField = isSystem;
                    //listViewHelper.setSorting(field, allow, $scope.options);
                    //$scope.getContent($scope.contentId);
                },
                selectItem: function (item, index, event) {
                    listViewHelper.selectHandler(item, index, vm.journal.items, vm.journal.selections, event);
                    event.stopPropagation();
                },
                clickItem: function (item, index) {
                    //$location.path('shield/Shield/edit/' + item.id);
                }
            }
        });
    }]
);

/**
    * @ngdoc resource
    * @name Environments
    * @function
    *
    * @description
    * Handles environment page
*/
angular.module('umbraco').controller('Shield.Dashboards.Editors.Settings',
    ['$scope', '$routeParams', 'notificationsService', 'localizationService', 'ShieldResource',
    function ($scope, $routeParams, notificationsService, localizationService, sheildResource) {

        var vm = this;

        angular.extend(vm, {
            loading: true,
            init: function () {

                vm.loading = false;
            }
        });
    }]
);
