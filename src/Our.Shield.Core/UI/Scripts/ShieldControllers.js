/**
    * @ngdoc resource
    * @name Shield.Editors.Edit
    * @function
    *
    * @description
    * Handles the main view for Shield
*/
angular.module('umbraco').controller('Shield.Editors.Edit', 
    ['$scope', '$routeParams', '$location', '$timeout', '$route', 'notificationsService', 'localizationService', 'listViewHelper', 'navigationService', 'assetsService', 'treeService', 'shieldResource',
    function ($scope, $routeParams, $location, $timeout, $route, notificationsService, localizationService, listViewHelper, navigationService, assetsService, treeService, shieldResource) {

        var vm = this;

        angular.extend(vm, {
            type: null,
            id: $routeParams.id,
            editingEnvironment: $routeParams.edit === 'true' ? true : false,
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
            tabs: [
                {
                    id:'0',
                    label: 'Environments',
                    active: true
                },
                {
                    id: '1',
                    label: 'Settings',
                    active: false
                },
                {
                    id:'2',
                    label:'Journal',
                    active: false
                }
            ],
            button: {
                label: 'Update',
                labelKey: 'general_update',
                state: 'init'
            },
            init: function () {
                shieldResource.getView(vm.id).then(function (response) {
                    vm.name = response.data.name;
                    vm.description = response.data.description;
                    angular.extend(vm.journalListing, response.data.journalListing);

                    switch (vm.type = response.data.type) {
                        case 0:     //  Environments
                            vm.environments = response.data.environments;
                            vm.path = ['-1', vm.id];
                            vm.ancestors = [{ id: vm.id, name: vm.name }];
                            vm.tabs.splice(1, 1);

                            if (vm.editingEnvironment) {
                                vm.type = 1;
                                vm.environment = {
                                    name: '',
                                    icon: 'icon-firewall red',
                                    domains: [],
                                    continueProcessing: false,
                                    enable: true,
                                    sortOrder: vm.environments.length
                                };
                                vm.button.labelKey = 'general_create';
                                localizationService.localize(vm.button.labelKey).then(function (value) {
                                    vm.button.label = value;
                                    vm.loading = false;
                                });
                                return;
                            }

                            break;

                        case 1:     //  Environment
                            vm.environments = response.data.environments;
                            vm.environment = response.data.environment;
                            vm.tabs[0].label = 'Apps';
                            vm.journalListing.columns.splice(1, 1);
                            vm.path = ['-1', '0' , vm.id];
                            vm.ancestors = [{ id: 0, name: 'Environments' }, { id: vm.id, name: vm.name }];
                            vm.appListing.apps = response.data.apps;

                            if (vm.id === '1' && vm.editingEnvironment) {
                                vm.cancelEditing();
                            }

                            if (vm.environment.domains.length === 0) {
                                vm.environment.domains.push({
                                    id: 0,
                                    name: '',
                                    umbracoDomainId: null,
                                    environmentId: vm.environment.id
                                });
                            }

                            break;

                        case 2:     //  App
                            vm.environment = response.data.environment;
                            vm.app = response.data.app;
                            vm.appView = response.data.appAssests.view;
                            vm.configuration = response.data.configuration;
                            vm.tabs[0].label = 'Configuration';
                            vm.tabs.splice(1, 1);
                            vm.journalListing.columns.splice(1, 2);
                            vm.journalListing.columns[1].cssClass = 'shield-table__name-large';
                            vm.path = ['-1', '0', vm.environment.id, vm.id];
                            vm.ancestors = [{ id: 0, name: 'Environments' }, { id: vm.environment.id, name: vm.environment.name }, { id: vm.id, name: vm.name }];

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
            editItem: function (item) {
                $location.path('/shield/shield/edit/' + item.id);
            },
            editEnvironment: function () {
                $location.search('edit', 'true');
            },
            cancelEditing: function () {
                $location.search('edit', 'false');
            },
            save: function () {
                vm.button.state = 'busy';
                $scope.$broadcast("formSubmitting", { scope: $scope, action: 'publish' });
                if ($scope.shieldForm.$invalid) {
                    //validation error, don't save

                    angular.element(event.target).addClass('show-validation');

                    var errorMsgDictionaryItem = '';

                    switch (vm.type) {
                        case 1:     //  Environment
                            if (vm.id === 0) {
                                errorMsgDictionaryItem = 'InvalidCreateEnvironmentError';
                            } else {
                                errorMsgDictionaryItem = 'InvalidSaveEnvironmentError';
                            }
                            break;

                        case 2:     //  App
                            errorMsgDictionaryItem = 'InvalidSaveConfigurationError';
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

                                if (vm.id === '0') {
                                    saveMsgDictionaryItem = 'CreateEnvironmentSuccess';
                                }

                                localizationService.localize('Shield.General_' + saveMsgDictionaryItem).then(function (value) {
                                    notificationsService.success(value);
                                });

                                navigationService.syncTree({ tree: "shield", path: vm.path, forceReload: true, activate: true }).then(function (syncArgs) {
                                    if (vm.id === '0') {
                                        var node = syncArgs.node.children[(syncArgs.node.children.length - 1)];
                                        vm.editItem(node);
                                        vm.editEnvironment();
                                    } else {
                                        vm.button.state = 'init';
                                        $route.reload();
                                    }
                                });
                            } else {
                                var errorMsgDictionaryItem = 'SaveEnvironmentError';

                                if (vm.id === '0') {
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
            },
            appListing: {
                options: {
                    orderBy: 'name',
                    orderDirection: 'desc'
                },
                apps: null
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
    * @name Shield.Editors.Overview.Create
    * @function
    *
    * @description
    * Handles the create panel overview view
*/
angular.module('umbraco').controller('Shield.Editors.Overview.Create',
    ['$scope', '$location', 'navigationService',
    function ($scope, $location, navigationService) {

        var vm = this;

        angular.extend(vm, {
            create: function () {
                navigationService.hideDialog();
                $location.path('/shield/shield/edit/0');
                $location.search('edit', 'true');
            }
        });
    }]
);

/**
    * @ngdoc resource
    * @name Shield.Editors.Overview.Delete
    * @function
    *
    * @description
    * Handles the delete panel overview view
*/
angular.module('umbraco').controller('Shield.Editors.Overview.Delete',
    ['$scope', '$routeParams', '$route', '$location', 'treeService', 'navigationService', 'localizationService', 'notificationsService', 'shieldResource',
    function ($scope, $routeParams, $route, $location, treeService, navigationService, localizationService, notificationsService, shieldResource) {

        var vm = this;

        angular.extend(vm, {
            busy: false,
            currentNode: $scope.currentNode,
            performDelete: function () {
                if (vm.busy) {
                    return false;
                }

                vm.currentNode.loading = true;
                vm.busy = true;

                shieldResource.deleteEnvironment(vm.currentNode.id).then(function (response) {
                    if (response.data === true || response.data === 'true') {
                        localizationService.localize('Shield.General_DeleteEnvironmentSuccess').then(function (value) {
                            notificationsService.success(value);
                            vm.currentNode.loading = false;
                            treeService.removeNode(vm.currentNode);

                            if ($routeParams.id !== '0') {
                                $location.path("/shield/shield/edit/0");
                            } else {
                                $route.reload();
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
    }]
);

/**
    * @ngdoc resource
    * @name Shield.Editors.Overview.Sort
    * @function
    *
    * @description
    * Handles the sort panel overview view
*/
angular.module('umbraco').controller('Shield.Editors.Overview.Sort',
    ['$scope', 'localizationService', 'notificationsService', 'shieldResource',
    function ($scope, localizationService, notificationsService, shieldResource) {

        var vm = this;

        angular.extend(vm, {
            loading: true,
            sorting: false,
            sortingComplete: false,
            environments: null,
            init: function () {
                shieldResource.getEnvironments().then(function (response) {
                    vm.environments = response.data;
                    vm.loading = false;
                });
            },
            save: function () {
                vm.sorting = true;

                for (var i = 0; i < vm.environments.length; i++) {
                    vm.environments[i].sortOrder = i;
                };

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
    }]
);

/**
    * @ngdoc resource
    * @name Shield.Dashboards.Overview
    * @function
    *
    * @description
    * Handles dashboard overview view
*/
angular.module('umbraco').controller('Shield.Dashboards.Overview',
    ['$scope', 'shieldResource',
    function ($scope, shieldResource) {

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
