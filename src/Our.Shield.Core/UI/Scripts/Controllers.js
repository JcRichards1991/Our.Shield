/**
    * @ngdoc resource
    * @name Shield.Editors.Edit
    * @function
    *
    * @description
    * Handles the main view for Shield
*/
angular.module('umbraco').controller('Shield.Editors.Edit', 
    [
        '$scope', '$routeParams', '$location', '$timeout', '$route', 'notificationsService',
        'localizationService', 'navigationService', 'shieldResource',
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
                        vm.description = response.data.description;

                        switch (vm.type = response.data.type) {
                            case 0:
                                if (vm.id === '0') {
                                    vm.environment = {
                                        name: '',
                                        icon: 'icon-firewall red',
                                        domains: [{ id: 0, name: '', umbracoDomainId: null }],
                                        continueProcessing: false,
                                        enable: true,
                                        sortOrder: vm.environments.length === 1 ? 0 : (vm.environments[vm.environments.length - 2].sortOrder + 1)
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
                                vm.environment = response.data.environment;
                                vm.name = response.data.name;
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

                                if (vm.id === '1' && vm.editingEnvironment) {
                                    vm.cancelEditing();
                                }

                                if (vm.id !== '1' && vm.environment.domains.length === 0) {
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
                                vm.name = response.data.name;
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

                    if (vm.id === '0') {
                        $location.path('/shield');
                    }
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

                                    if (vm.id === '0') {
                                        saveMsgDictionaryItem = 'CreateEnvironmentSuccess';
                                    }

                                    localizationService.localize('Shield.General_' + saveMsgDictionaryItem).then(function (value) {
                                        notificationsService.success(value);
                                    });

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

                        case 2:     //  App
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
    [
        '$scope', '$location', 'shieldResource',
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
        }
    ]
);

/**
    * @ngdoc resource
    * @name Shield.Dashboards.Journal
    * @function
    *
    * @description
    * Handles journals dashboard view
*/
angular.module('umbraco').controller('Shield.Dashboards.Journal',
    [
        '$scope', '$routeParams', '$location', 'listViewHelper', 'shieldResource',
        function($scope,
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

/**
    * @ngdoc resource
    * @name Shield.Dialogs.EditIpException
    * @function
    *
    * @description
    * Handles the Edit IP Exception dialog view
*/
angular.module('umbraco').controller('Shield.Editors.Dialogs.EditException',
    [
        '$scope', 'localizationService',
        function ($scope, localizationService) {
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

/**
    * @ngdoc resource
    * @name Shield.Editors.Overview.Create
    * @function
    *
    * @description
    * Handles the create panel overview view
*/
angular.module('umbraco').controller('Shield.Editors.Overview.Create',
    [
        '$scope', '$location', 'navigationService',
        function ($scope, $location, navigationService) {
            var vm = this;

            angular.extend(vm, {
                create: function () {
                    navigationService.hideDialog();
                    $location.path('/shield/shield/edit/0');
                    $location.search('edit', 'true');
                }
            });
        }
    ]
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
    [
        '$scope', '$routeParams', '$route', '$location', 'treeService', 'navigationService', 'localizationService', 'notificationsService', 'shieldResource',
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

/**
    * @ngdoc resource
    * @name Shield.Editors.Overview.Sort
    * @function
    *
    * @description
    * Handles the sort panel overview view
*/
angular.module('umbraco').controller('Shield.Editors.Overview.Sort',
    [
        '$scope', 'localizationService', 'notificationsService', 'shieldResource',
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
                        vm.environments.splice(vm.environments.length - 1, 1);
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
        }
    ]
);