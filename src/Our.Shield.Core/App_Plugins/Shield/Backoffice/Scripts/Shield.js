(function(root){
/**
    * @ngdoc resource
    * @name Shield.Editors.Edit
    * @function
    *
    * @description
    * Handles the main view for Shield
*/
angular.module('umbraco').controller('Shield.Editors.Edit', 
    ['$scope', '$routeParams', '$location', '$timeout', 'notificationsService', 'localizationService', 'listViewHelper', 'navigationService', 'assetsService', 'shieldResource',
    function ($scope, $routeParams, $location, $timeout, notificationsService, localizationService, listViewHelper, navigationService, assetsService, shieldResource) {

        var vm = this;

        angular.extend(vm, {
            type: null,
            id: $routeParams.id,
            editingEnvironment: $routeParams.create === true || $routeParams.create === 'true' ? true : false,
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
                    active: true,
                },
                {
                    id: '1',
                    label: 'Domains',
                    active: true,
                },
                {
                    id:'2',
                    label:'Journal',
                    active: false
                }
            ],
            button: {
                label: '',
                state: 'init'
            },
            init: function () {
                localizationService.localize('general_update').then(function (value) {
                    vm.button.label = value;
                });

                shieldResource.getView(vm.id).then(function (response) {
                    vm.name = response.data.name;
                    vm.description = response.data.description;
                    angular.extend(vm.journalListing, response.data.journalListing);

                    switch (vm.type = response.data.type) {
                        case 0:     //  Environments
                            vm.environments = response.data.environments;
                            vm.path = ['-1', vm.id];
                            vm.ancestors = [{ id: vm.id, name: vm.name }]
                            vm.tabs.splice(1, 1);

                            if (vm.editingEnvironment) {
                                vm.type = 1;
                                vm.environment = {
                                    name: '',
                                    icon: 'icon-firewall red',
                                    domains: [],
                                    continueProcessing: false,
                                    enabled: true,
                                    sortOrder: vm.environments.length
                                };
                                localizationService.localize('general_create').then(function (value) {
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
                            vm.ancestors = [{ id: 0, name: 'Environments' }, { id: vm.id, name: vm.name }]
                            vm.appListing.apps = response.data.apps;

                            if (vm.id === '1') {
                                vm.tabs.splice(1, 1);
                            }

                            break;

                        case 2:     //  App
                            vm.environment = response.data.environment;
                            vm.app = response.data.app;
                            vm.appView = response.data.appAssests.view;
                            vm.configuration = response.data.configuration;
                            vm.tabs[0].label = 'Configuration'
                            vm.tabs.splice(1, 1);
                            vm.journalListing.columns.splice(1, 2);
                            vm.journalListing.columns[1].cssClass = 'shield-table__name-large'
                            vm.path = ['-1', '0', vm.environment.id, vm.id];
                            vm.ancestors = [{ id: 0, name: 'Environments' }, { id: vm.environment.id, name: vm.environment.name }, { id: vm.id, name: vm.name }]

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
            editEnvironment: function () {
                vm.editingEnvironment = true;
            },
            save: function () {
                vm.button.state = 'busy';
                $scope.$broadcast("formSubmitting", { scope: $scope, action: 'publish' });
                if ($scope.shieldForm.$invalid) {
                    //validation error, don't save

                    angular.element(event.target).addClass('show-validation');

                    var errorMsgDictionaryItem = '';

                    switch (vm.type) {
                        case 1:
                            if (vm.id === 0) {
                                errorMsgDictionaryItem = 'InvalidCreateEnvironmentError';
                            } else {
                                errorMsgDictionaryItem = 'InvalidSaveEnvironmentError';
                            }
                            break;

                        case 2:
                            errorMsgDictionaryItem = 'InvalidSaveConfigurationError';
                            break;
                    }

                    localizationService.localize('Shield.General_' + errorMsgDictionaryItem).then(function (value) {
                        notificationsService.error(value);
                    });
                    vm.button.state = 'error';
                    return;
                }

                switch (vm.type) {
                    case 1:
                        shieldResource.postEnvironment(vm.environment).then(function (response) {
                            if (response.data === true || response.data === 'true') {
                                localizationService.localize('Shield.General_SaveEnvironmentSuccess').then(function (value) {
                                    notificationsService.success(value);
                                });
                                vm.button.state = 'init';
                                $scope.shieldForm.$setPristine();

                                $timeout(function () {
                                    navigationService.syncTree({ tree: 'Shield', path: ['-1', '0'], forceReload: true, activate: true });
                                    vm.editingEnvironment = false;
                                }, 500);
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

                    case 2:
                        shieldResource.postConfiguration(vm.id, vm.configuration).then(function (response) {
                            if (response.data === true || response.data === 'true') {
                                localizationService.localize('Shield.General_SaveConfigurationSuccess').then(function (value) {
                                    notificationsService.success(value);
                                });
                                navigationService.syncTree({ tree: 'Shield', path: vm.path, forceReload: true, activate: true });
                                vm.button.state = 'init';
                                $scope.shieldForm.$setPristine();
                                $location.path('shield/shield/edit/' + vm.id);
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
    * @name Shield.Editors.Overview.Delete
    * @function
    *
    * @description
    * Handles the delete panel overview view
*/
angular.module('umbraco').controller('Shield.Editors.Overview.Delete',
    ['$scope', 'treeService', 'editorState', 'navigationService', 'localizationService', 'notificationsService', 'shieldResource',
    function ($scope, treeService, editorState, navigationService, localizationService, notificationsService, shieldResource) {

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
                            $location.path("/shield/shield/edit/0");
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
    ['$scope', 'navigationService', 'localizationService', 'notificationsService', 'shieldResource',
    function ($scope, navigationService, localizationService, notificationsService, shieldResource) {

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
                        localizationService.localize('Shield.General_SortEnvironmentSuccess').then(function (value) {
                            notificationsService.success(value);
                            vm.sortingComplete = true;
                        });
                    } else {
                        localizationService.localize('Shield.General_SortEnvironmentError').then(function (value) {
                            notificationsService.error(value);
                        });
                    }
                    vm.sorting = false;
                });
            },
            close: function () {
                navigationService.syncTree({ tree: 'Shield', path: ['-1', '0'], forceReload: true, activate: true });
                navigationService.hideDialog();
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

/**
   * @ngdoc directive
   * @name shield-app
   * @function
   *
   * @description
   * Custom angular directive for inserting the Shield Apps view's onto the page 
*/
angular.module('umbraco.directives').directive('shieldApp',
    ['$compile', '$templateCache', '$http',
    function ($compile, $templateCache, $http) {
        return {
            restrict: 'E',
            scope: {
                view: '=',
                configuration: '='
            },
            link: function (scope, element, attr) {
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
    }]
);

/**
   * @ngdoc directive
   * @name shield-convert-to-number
   * @function
   *
   * @description
   * Custom angular directive for converting string to number
*/
angular.module('umbraco.directives').directive('shieldConvertToNumber',
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
);

/**
   * @ngdoc directive
   * @name shield-add-to-form
   * @function
   *
   * @description
   * Adds form input elements to the backoffice access form for validation
*/
angular.module('umbraco.directives').directive('shieldAddToForm', function () {
    return {
        restrict: 'A',
        require: 'ngModel',
        link: function ($scope, $element, $attr, ctrl) {
            var $form = $scope[$attr.shieldAddToForm];

            $form.$removeControl(ctrl);
            ctrl.$name = $attr.name;
            $scope.backofficeAccessForm.$addControl(ctrl);
        }
    }
});

/**
    * @ngdoc resource
    * @name UmbracoAccessResource
    * @function
    *
    * @description
    * Api resource for the Umbraco Access area
*/
angular.module('umbraco.resources').factory('shieldResource', ['$http', function ($http) {

    var apiRoot = 'backoffice/Shield/ShieldApi/';

    return {
        getView: function (id) {
            return $http.get(apiRoot + 'View?id=' + id);
        },
        postConfiguration: function (id, config) {
            return $http({
                method: 'POST',
                url: apiRoot + 'Configuration?id=' + id,
                data: JSON.stringify(config),
                dataType: 'json',
                headers: {
                    'Content-Type': 'application/json'
                },
            });
        },
        postEnvironment: function (environment) {
            return $http({
                method: 'POST',
                url: apiRoot + 'WriteEnvironment',
                data: JSON.stringify(environment),
                dataType: 'json',
                headers: {
                    'Content-Type': 'application/json'
                }
            });
        },
        deleteEnvironment: function (id) {
            return $http({
                method: 'POST',
                url: apiRoot + 'DeleteEnvironment?id=' + id,
                dataType: 'json',
                headers: {
                    'Content-Type': 'application/json'
                }
            });
        },
        getEnvironments: function () {
            return $http.get(apiRoot + 'GetEnvironments');
        },
        setEnvironmentsSortOrder: function (environments) {
            return $http({
                method: 'POST',
                url: apiRoot + 'SortEnvironments',
                data: JSON.stringify(environments),
                dataType: 'json',
                headers: {
                    'Content-Type': 'application/json'
                }
            });
        },
        getJournals: function (id, page) {
            return $http.get(apiRoot + 'Journals?id=' + id + '&page=' + page);
        },
        getAppIds: function () {
            return $http.get(apiRoot + 'AppIds');
        }
    };
}]);
}(window));