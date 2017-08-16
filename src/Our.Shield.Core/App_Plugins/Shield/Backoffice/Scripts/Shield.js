(function(root){
app.run(['shieldResource', function (shieldResource) {
    var vm = this;

    angular.extend(vm, {
        faviconId: 'favicon',
        defaultIndicatorColor: '#df7f48',
        run: function () {

            if (document.getElementById(vm.faviconId) || typeof (Path2D) === "undefined") {
                return;
            }

            shieldResource.getEnvironments().then(function (response) {
                var environments = response.data || [],
                    indicatorColor = vm.defaultIndicatorColor;

                if (environments.length !== 0) {
                    var environment = environments.filter((e) => e.domains.filter((d) => d.name === location.origin)[0] !== undefined)[0];

                    if (environment !== null && environment !== undefined && environment.enable) {
                        indicatorColor = environment.colorIndicator;
                    }
                }

                var link = document.createElement('link');
                link.id = vm.faviconId;
                link.type = 'image/x-icon';
                link.rel = 'shortcut icon';
                link.href = vm.drawImage(indicatorColor);

                var style = document.createElement("style");
                style.type = "text/css";
                style.innerText = "ul.sections li.avatar, ul.sections li.avatar:hover { border-left: 8px " + indicatorColor + " solid; }  ul.sections li.current, ul.sections li:hover { border-left: 4px " + indicatorColor + " solid; }";

                var head = document.getElementsByTagName('head')[0];
                head.appendChild(link);
                head.appendChild(style);
            });
        },
        drawImage: function (indicatorColor) {
            var canvas = document.createElement("canvas");
            canvas.width = 64;
            canvas.height = 64;
            var control = canvas.getContext('2d');
            control.fillStyle = indicatorColor;
            var path = new Path2D('M0,32C0,14.3,14.3,0,32,0c17.7,0,32,14.3,32,32c0,17.7-14.3,32-32,32C14.3,64,0,49.7,0,32z M31.3,42.9 c-3.1,0-5.6-0.2-7.4-0.7c-2-0.5-3.3-1.6-4-3.2c-0.7-1.7-1.1-4.2-1.1-7.7c0-1.9,0.1-3.7,0.3-5.4c0.2-1.8,0.4-3.2,0.6-4.3l0.2-1.1   c0,0,0-0.1,0-0.1c0-0.3-0.2-0.6-0.5-0.6L15.4,19c0,0-0.1,0-0.1,0c-0.3,0-0.6,0.2-0.6,0.5c-0.1,0.3-0.1,0.5-0.2,1.1  c-0.2,1.2-0.5,2.4-0.7,4.1c-0.2,1.7-0.4,3.6-0.5,5.7c0,0-0.1,0.5,0,4c0.1,3.5,0.7,6.3,1.8,8.4c1.1,2.1,3,3.6,5.6,4.5    c2.6,0.9,6.3,1.4,11,1.3h0.6c4.7,0,8.4-0.4,11-1.3c2.6-0.9,4.5-2.4,5.6-4.5c1.1-2.1,1.7-4.9,1.8-8.4c0.1-3.5,0-4,0-4    c-0.1-2.1-0.2-4-0.5-5.7c-0.2-1.7-0.5-2.9-0.7-4.1c-0.1-0.6-0.2-0.8-0.2-1.1C49.2,19.2,49,19,48.7,19c0,0-0.1,0-0.1,0l-4.1,0.6  c-0.3,0.1-0.5,0.3-0.5,0.6c0,0,0,0.1,0,0.1l0.2,1.1c0.2,1.1,0.4,2.6,0.6,4.3c0.2,1.8,0.3,3.6,0.3,5.4c0,3.5-0.3,6-1.1,7.7   c-0.7,1.7-2.1,2.8-4,3.2c-1.8,0.5-4.2,0.7-7.4,0.7H31.3z');
            control.fill(path);
            return canvas.toDataURL();
        }
    });

    vm.run()
}]);
/**
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
                                    domains: [{ id: 0, name: '', umbracoDomainId: null}],
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
                var colorIndicatorChanged = vm.type === 1 && $scope.shieldForm.colorIndicator.$dirty;
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

                                if (vm.id === '0') {
                                    var path = vm.path;
                                    path.push('-20');

                                    navigationService.syncTree({ tree: "shield", path: path, forceReload: true, activate: true }).then(function (syncArgs) {
                                        vm.editItem(syncArgs.node);
                                    });

                                    vm.cancelEditing();

                                    if (colorIndicatorChanged && (vm.environment.domains.filter((x) => x.name === $window.location.origin)[0] !== null || vm.environment.domains.filter((x) => x.name === $window.location.origin)[0] !== undefined)) {
                                        $window.location.reload()
                                    } else {
                                        $route.reload();
                                    }
                                } else {
                                    if (colorIndicatorChanged && (vm.environment.domains.filter((x) => x.name === $window.location.origin)[0] !== null || vm.environment.domains.filter((x) => x.name === $window.location.origin)[0] !== undefined)) {
                                        $window.location.reload()
                                    } else {
                                        $route.reload();
                                    }
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
            $form.$addControl(ctrl);
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