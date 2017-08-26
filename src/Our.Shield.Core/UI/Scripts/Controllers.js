﻿/**
    * @ngdoc resource
    * @name Shield.Editors.Edit
    * @function
    *
    * @description
    * Handles the main view for Shield
*/
angular.module('umbraco').controller('Shield.Editors.Edit', 
    ['$scope', '$routeParams', '$location', '$timeout', '$route', '$window', 'notificationsService', 'localizationService', 'listViewHelper', 'navigationService', 'assetsService', 'treeService', 'shieldResource',
    function ($scope, $routeParams, $location, $timeout, $route, $window, notificationsService, localizationService, listViewHelper, navigationService, assetsService, treeService, shieldResource) {

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
                    label: 'Apps',
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
                    vm.environments = response.data.environments;

                    if (vm.id === '0') {
                        vm.type = 0;
                        vm.environment = {
                            name: '',
                            icon: 'icon-firewall red',
                            domains: [{ id: 0, name: '', umbracoDomainId: null }],
                            continueProcessing: false,
                            enable: true,
                            sortOrder: (vm.environments.filter((x) => x.id === 1)[0]).sortOrder
                        };
                        vm.button.labelKey = 'general_create';
                        localizationService.localize(vm.button.labelKey).then(function (value) {
                            vm.button.label = value;
                            vm.loading = false;
                        });
                        return;
                    }
                    vm.name = response.data.name;
                    vm.description = response.data.description;
                    vm.environment = response.data.environment;

                    angular.extend(vm.journalListing, response.data.journalListing);

                    switch (vm.type = response.data.type) {
                        case 0:     //  Environment
                            vm.path = ['-1' , '' + vm.id];
                            vm.ancestors = [{ id: vm.id, name: vm.name }];

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

                        case 1:     //  App
                            vm.path = ['-1', '' + vm.environment.id, '' + vm.id];
                            vm.ancestors = [{ id: vm.environment.id, name: vm.environment.name }, { id: vm.id, name: vm.name }];

                            vm.app = response.data.app;
                            vm.appView = response.data.appAssests.view;
                            vm.configuration = response.data.configuration;

                            vm.tabs[0].label = 'Configuration';
                            vm.tabs.splice(1, 1);

                            angular.forEach(response.data.appAssests.stylesheets, function (item, index) {
                                assetsService.loadCss(item);
                            });

                            angular.forEach(response.data.appAssests.scripts, function (item, index) {
                                assetsService.loadJs(item);
                            });

                            break;
                    }

                    $timeout(function () {
                        navigationService.syncTree({ tree: 'shield', path: vm.path, forceReload: false, activate: true });
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
                        case 0:     //  Environment
                            if (vm.id === 0) {
                                errorMsgDictionaryItem = 'CreateEnvironmentInvalid';
                            } else {
                                errorMsgDictionaryItem = 'SaveEnvironmentInvalid';
                            }
                            break;

                        case 1:     //  App
                            errorMsgDictionaryItem = 'SaveConfigurationInvalid';
                            break;
                    }

                    localizationService.localize('Shield.General_' + errorMsgDictionaryItem).then(function (value) {
                        notificationsService.error(value);
                    });
                    vm.button.state = 'error';
                    return;
                }
                var colorIndicatorChanged = vm.type === 0 && $scope.shieldForm.colorIndicator.$dirty;
                $scope.shieldForm.$setPristine();

                switch (vm.type) {

                    case 0:     //  Environment
                        shieldResource.postEnvironment(vm.environment).then(function (response) {
                            if (response.data === true || response.data === 'true') {
                                var saveMsgDictionaryItem = 'SaveEnvironmentSuccess';

                                if (vm.id === '0') {
                                    saveMsgDictionaryItem = 'CreateEnvironmentSuccess';
                                }

                                localizationService.localize('Shield.General_' + saveMsgDictionaryItem).then(function (value) {
                                    notificationsService.success(value);
                                });

                                if (colorIndicatorChanged && vm.environment.domains.filter((x) => x.name === $window.location.origin)[0] !== undefined) {
                                    shield.colorIndicator.run(shieldResource);
                                }

                                vm.cancelEditing();

                                if (vm.id === '0') {
                                    var path = ['-1', '-21'];
                                    navigationService.syncTree({ tree: "shield", path: path, forceReload: true, activate: true });
                                    $location.path('/shield');
                                } else {
                                    $route.reload();
                                }

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

                    case 1:     //  App
                        shieldResource.postConfiguration(vm.id, vm.configuration).then(function (response) {
                            if (response.data === true || response.data === 'true') {
                                localizationService.localize('Shield.General_SaveConfigurationSuccess').then(function (value) {
                                    notificationsService.success(value);
                                });

                                $scope.shieldForm.$setPristine();
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
                items: [],
                totalPages: 1,
                pageNumber: 1,
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
                        name: 'App',
                        alias: 'app',
                        allowSorting: false,
                        show: true
                    },
                    {
                        id: 2,
                        name: 'Message',
                        alias: 'message',
                        allowSorting: false,
                        show: true
                    },
                ],
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
                    vm.loading = true;

                    shieldResource.getJournals(vm.id, vm.journalListing.pageNumber, vm.journalListing.options.orderBy, vm.journalListing.options.orderDirection).then(function (response) {
                        vm.journalListing.items = response.data.items;
                        vm.journalListing.totalPages = response.data.totalPages;

                        vm.loading = false;
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
    * @name Shield.Dashboards.Environments
    * @function
    *
    * @description
    * Handles environments dashboard view
*/
angular.module('umbraco').controller('Shield.Dashboards.Environments',
    ['$scope', '$location', 'shieldResource',
    function ($scope, $location, shieldResource) {

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
            },
        });
    }]
);

/**
    * @ngdoc resource
    * @name Shield.Dashboards.Journals
    * @function
    *
    * @description
    * Handles journals dashboard view
*/
angular.module('umbraco').controller('Shield.Dashboards.Journals',
    ['$scope', '$location', 'listViewHelper', 'shieldResource',
    function ($scope, $location, listViewHelper, shieldResource) {

        var vm = this;

        angular.extend(vm, {
            id: '0',
            loading: true,
            init: function () {
                shieldResource.getJournals(vm.id, vm.journalListing.pageNumber, vm.journalListing.options.orderBy, vm.journalListing.options.orderDirection).then(function (response) {
                    vm.journalListing.items = response.data.items;
                    vm.journalListing.totalPages = response.data.totalPages;

                    vm.loading = false;
                });
            },
            editItem: function (item) {
                $location.path('/shield/shield/edit/' + item.id);
            },
            journalListing: {
                items: [],
                totalPages: 1,
                pageNumber: 1,
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
                    },
                ],
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
                    if (allow) {
                        vm.journalListing.options.orderBySystemField = false;
                        listViewHelper.setSorting(field, allow, vm.journalListing.options);
                        vm.journalListing.getJournalListing();
                    }
                },
                getJournalListing: function () {
                    vm.loading = true;

                    shieldResource.getJournals(vm.id, vm.journalListing.pageNumber, vm.journalListing.options.orderBy, vm.journalListing.options.orderDirection).then(function (response) {
                        vm.journalListing.items = response.data.items;
                        vm.journalListing.totalPages = response.data.totalPages;

                        vm.loading = false;
                    });
                }
            }
        });
    }]
);