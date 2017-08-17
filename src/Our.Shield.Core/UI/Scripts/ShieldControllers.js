/**
    * @ngdoc resource
    * @name Environments
    * @function
    *
    * @description
    * Handles environment page
*/
angular.module('umbraco').controller('Shield.Editors.Edit', 
    ['$scope', '$routeParams', '$location', '$timeout', '$filter', 'notificationsService', 'localizationService', 'listViewHelper', 'navigationService', 'assetsService', 'shieldResource',
    function ($scope, $routeParams, $location, $timeout, $filter, notificationsService, localizationService, listViewHelper, navigationService, assetsService, shieldResource) {

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
                    angular.extend(vm.journalListing, response.data.journalListing);

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

                            vm.tabs[0].label = 'Apps';
                            vm.nameLocked = true;

                            vm.journalListing.columns.splice(1, 1);
                            vm.path = '-1,0,' + vm.id;
                            vm.ancestors = [{ id: 0, name: 'Environments' }, { id: vm.id, name: vm.name }]
                            vm.appListing.apps = response.data.apps;
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
                $scope.$broadcast("formSubmitting", { scope: $scope, action: 'publish' });
                if ($scope.shieldForm.$invalid) {
                    //validation error, don't save

                    angular.element(event.target).addClass('show-validation');

                    localizationService.localize('Shield.General_InvalidError').then(function (value) {
                        notificationsService.error(value);
                    });
                    vm.saveButtonState = 'error';
                    return;
                }

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
                                localizationService.localize('Shield.General_SaveSuccess').then(function (value) {
                                    notificationsService.success(value);
                                });
                                navigationService.syncTree({ tree: 'Shield', path: vm.path, forceReload: true, activate: true });
                                vm.saveButtonState = 'init';
                                $scope.shieldForm.$setPristine();
                            } else {
                                localizationService.localize('Shield.General_SaveError').then(function (value) {
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
            appListing: {
                options: {
                    orderBy: 'name',
                    orderDirection: 'desc'
                },
                apps: null,
                enable: function (item) {
                    if (item.enable)
                        item.enable = false;
                    else
                        item.enable = true;
                }
            },
            journalListing: {
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
                        show: true,
                        cssClass: 'shield-table__name-small'
                    },
                    {
                        id: 1,
                        name: 'Environment',
                        alias: 'environment',
                        allowSorting: false,
                        show: true,
                        cssClass: 'shield-table__name'
                    },
                    {
                        id: 2,
                        name: 'App',
                        alias: 'app',
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
                totalPages: 1,
                pageNumber: 1,
                nextPage: function (page) {
                    vm.journalListing.pageNumber = page;
                    vm.journalListing.getJournalListing();
                },
                previousPage: function (page) {
                    vm.journalListing.pageNumber = page;
                    vm.journalListing.getJournalListing();
                },
                gotoPage: function (page) {
                    vm.journalListing.pageNumber = page;
                    vm.journalListing.getJournalListing();
                },
                isSortDirection: function (col, direction) {
                    return false;
                    //return listViewHelper.setSortingDirection(col, direction, vm.journalListing.options);
                },
                sort: function (field, allow) {
                    if(allow) {
                        vm.journalListing.options.orderBySystemField = false;
                        listViewHelper.setSorting(field, allow, vm.journalListing.options);
                        //vm.journalListing.getJournalListing();
                    }
                },
                getJournalListing: function () {
                    shieldResource.getJournals(vm.id, vm.journalListing.pageNumber).then(function (response) {
                        vm.journalListing.items = response.data.items;
                        vm.journalListing.totalPages = response.data.totalPages;
                    });
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
                        vm.appOverviews.push('/App_Plugins/Shield.' + appId + '/Views/Overview.html?version=1.0.2')
                    });

                    vm.loading = false;
                });
            }
        });
    }]
);
