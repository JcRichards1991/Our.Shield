/**
    * @ngdoc resource
    * @name Environments
    * @function
    *
    * @description
    * Handles environment page
*/
angular.module('umbraco').controller('Shield.Editors.Edit', 
    ['$scope', '$routeParams', '$location', '$timeout', 'notificationsService', 'localizationService', 'listViewHelper', 'navigationService', 'assetsService', 'shieldResource',
    function ($scope, $routeParams, $location, $timeout, notificationsService, localizationService, listViewHelper, navigationService, assetsService, shieldResource) {

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
            app: null,
            appView: null,
            configuration: null,
            path: null,
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
                    angular.extend(vm.journalListing, response.data.journalListing)

                    switch (vm.type = response.data.type) {
                        case 0:     //  Environments
                            vm.nameLocked = true;
                            vm.environments = response.data.environments;
                            vm.path = '-1,0';
                            vm.ancestors = [{ id: vm.id, name: vm.name }]
                            break;

                        case 1:     //  Environment
                            //vm.nameLocked = false;
                            vm.environments = response.data.environments;
                            vm.environment = response.data.environment;
                            vm.appListing.apps = response.data.apps;

                            vm.tabs[0].label = 'Apps';
                            vm.nameLocked = true;

                            vm.journalListing.columns.splice(1, 1);
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
                            vm.journalListing.columns.splice(1, 2);
                            vm.journalListing.columns[1].cssClass = 'shield-table__name-large'
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
            editItemById: function (id, index) {
                $location.path('shield/shield/edit/' + id);
            },
            editItem: function (item, index) {
                vm.editItemById(item.id);
            },
            appListing: {
                apps: [],
                selectItem: function (item) {
                    if (item.selected)
                        item.selected = false;
                    else
                        item.selected = true;
                }
            },
            journalListing: {
                columns: [
                    {
                        id: 0,
                        name: 'Date',
                        alias: 'datestamp',
                        allowSorting: true,
                        show: true,
                        cssClass: 'shield-table__name-small'
                    },
                    {
                        id: 1,
                        name: 'Environment',
                        alias: 'environmentId',
                        allowSorting: false,
                        show: true,
                        cssClass: ''
                    },
                    {
                        id: 2,
                        name: 'App',
                        alias: 'appId',
                        allowSorting: false,
                        show: true,
                        cssClass: 'shield-table__name'
                    },
                    {
                        id: 3,
                        name: 'Message',
                        alias: 'message',
                        allowSorting: false,
                        show: true,
                        cssClass: ''
                    },
                ],
                items: null,
                selection: [],
                totalPages: 0,
                pageNumber: 0,
                nextPage: function (page) {
                },
                previousPage: function (page) {
                },
                gotoPage: function (page) {
                },
                sort: function (id) {
                    //$scope.options.orderBySystemField = isSystem;
                    //listViewHelper.setSorting(field, allow, $scope.options);
                    //$scope.getContent($scope.contentId);
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
angular.module('umbraco').controller('Shield.Dashboards.Overview',
    ['$scope', 'localizationService', 'shieldResource',
    function ($scope, localizationService, shieldResource) {

        var vm = this;

        angular.extend(vm, {
            loading: true,
            appIds: [],
            appOverviews: [],
            init: function () {
                shieldResource.getAppIds().then(function (response) {
                    vm.appIds = response.data;

                    angular.forEach(vm.appIds, function (appId, index) {
                        vm.appOverviews.push('/App_Plugins/Shield.' + appId + '/Views/Overview.html?version=1.0.0-pre-alpha')
                    });

                    vm.loading = false;
                });
            }
        });
    }]
);
