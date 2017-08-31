(function(root){
var shield = {
    colorIndicator: {
        faviconId: 'favicon',
        defaultIndicatorColor: '#df7f48',
        styleId: 'shieldColorIndicatorStyle',
        run: function (shieldResource) {
            if (typeof (Path2D) === "undefined") {
                return;
            }

            shieldResource.getEnvironments().then(function (response) {
                var environments = response.data || [],
                    indicatorColor = shield.colorIndicator.defaultIndicatorColor;

                if (environments.length !== 0) {
                    var environment = environments.filter((e) => e.domains.filter((d) => d.name === location.origin)[0] !== undefined)[0];

                    if (environment !== undefined) {
                        indicatorColor = environment.colorIndicator;
                    }
                }


                if (document.getElementById(shield.colorIndicator.faviconId)) {
                    var link = document.getElementById(shield.colorIndicator.faviconId);
                    link.href = shield.colorIndicator.drawImage(indicatorColor);

                    var style = document.getElementById(shield.colorIndicator.styleId);
                    style.innerText = shield.colorIndicator.setStyle(indicatorColor);

                    return;
                }

                var link = document.createElement('link');
                link.id = shield.colorIndicator.faviconId;
                link.type = 'image/x-icon';
                link.rel = 'shortcut icon';
                link.href = shield.colorIndicator.drawImage(indicatorColor);

                var style = document.createElement("style");
                style.type = "text/css";
                style.id = shield.colorIndicator.styleId
                style.innerText = shield.colorIndicator.setStyle(indicatorColor);

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
        },
        setStyle: function (indicatorColor) {
            return "ul.sections li.avatar, ul.sections li.avatar:hover { border-left: 8px " + indicatorColor + " solid; }  ul.sections li.current, ul.sections li:hover { border-left: 4px " + indicatorColor + " solid; }"
        }
    }
};

//app.run(['shieldResource', function (shieldResource) {
//    shield.colorIndicator.run(shieldResource);
//}]);
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
                            vm.path = ['-1', vm.id];
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
                            vm.path = ['-1', '' + vm.environment.id, vm.id];
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
        require: ['ngModel', '^form'],
        link: function ($scope, $element, $attr, controllers) {
            var ngModel = controllers[0],
                $form = controllers[1];

            $form.$removeControl(ngModel);
            ngModel.$name = $attr.name;
            $form.$addControl(ngModel);
        }
    }
});

/**
   * @ngdoc directive
   * @name shield-ipaddressvalid
   * @function
   *
   * @description
   * Custom angular directive for validating an IP Address
   * as IPv4 or IPv6 with optional cidr
*/
angular.module('umbraco.directives').directive('shieldIpaddressvalid', function () {
    return {
        restrict: 'A',
        require: 'ngModel',
        link: function (scope, elm, attr, ctrl) {
            ctrl.$parsers.push(function (modelValue) {
                if (modelValue === '' || modelValue === undefined) {
                    ctrl.$setValidity('shieldIpaddressvalid', true);
                    return modelValue;
                }

                //Check if IPv4 & IPv6
                var pattern = /^(([0-9]|[1-9][0-9]|1[0-9]{2}|2[0-4][0-9]|25[0-5])\.){3}([0-9]|[1-9][0-9]|1[0-9]{2}|2[0-4][0-9]|25[0-5])$|^(([a-zA-Z]|[a-zA-Z][a-zA-Z0-9\-]*[a-zA-Z0-9])\.)*([A-Za-z]|[A-Za-z][A-Za-z0-9\-]*[A-Za-z0-9])$|^\s*((([0-9A-Fa-f]{1,4}:){7}([0-9A-Fa-f]{1,4}|:))|(([0-9A-Fa-f]{1,4}:){6}(:[0-9A-Fa-f]{1,4}|((25[0-5]|2[0-4]\d|1\d\d|[1-9]?\d)(\.(25[0-5]|2[0-4]\d|1\d\d|[1-9]?\d)){3})|:))|(([0-9A-Fa-f]{1,4}:){5}(((:[0-9A-Fa-f]{1,4}){1,2})|:((25[0-5]|2[0-4]\d|1\d\d|[1-9]?\d)(\.(25[0-5]|2[0-4]\d|1\d\d|[1-9]?\d)){3})|:))|(([0-9A-Fa-f]{1,4}:){4}(((:[0-9A-Fa-f]{1,4}){1,3})|((:[0-9A-Fa-f]{1,4})?:((25[0-5]|2[0-4]\d|1\d\d|[1-9]?\d)(\.(25[0-5]|2[0-4]\d|1\d\d|[1-9]?\d)){3}))|:))|(([0-9A-Fa-f]{1,4}:){3}(((:[0-9A-Fa-f]{1,4}){1,4})|((:[0-9A-Fa-f]{1,4}){0,2}:((25[0-5]|2[0-4]\d|1\d\d|[1-9]?\d)(\.(25[0-5]|2[0-4]\d|1\d\d|[1-9]?\d)){3}))|:))|(([0-9A-Fa-f]{1,4}:){2}(((:[0-9A-Fa-f]{1,4}){1,5})|((:[0-9A-Fa-f]{1,4}){0,3}:((25[0-5]|2[0-4]\d|1\d\d|[1-9]?\d)(\.(25[0-5]|2[0-4]\d|1\d\d|[1-9]?\d)){3}))|:))|(([0-9A-Fa-f]{1,4}:){1}(((:[0-9A-Fa-f]{1,4}){1,6})|((:[0-9A-Fa-f]{1,4}){0,4}:((25[0-5]|2[0-4]\d|1\d\d|[1-9]?\d)(\.(25[0-5]|2[0-4]\d|1\d\d|[1-9]?\d)){3}))|:))|(:(((:[0-9A-Fa-f]{1,4}){1,7})|((:[0-9A-Fa-f]{1,4}){0,5}:((25[0-5]|2[0-4]\d|1\d\d|[1-9]?\d)(\.(25[0-5]|2[0-4]\d|1\d\d|[1-9]?\d)){3}))|:)))(%.+)?\s*$/;

                ctrl.$setValidity('shieldIpaddressvalid', pattern.test(modelValue));

                return modelValue
            });
        }
    };
});

/**
   * @ngdoc directive
   * @name shield-ipaddressduplicate
   * @function
   *
   * @description
   * Checks to make sure an IP address isn't being added more than
   * once to the IP address White-List
*/
angular.module('umbraco.directives').directive('shieldIpaddressduplicate', function () {
    return {
        restrict: 'A',
        require: 'ngModel',
        link: function (scope, elm, attr, ctrl) {
            ctrl.$parsers.push(function (modelValue) {
                if (modelValue === '' || modelValue === undefined) {
                    ctrl.$setValidity('shieldIpaddressduplicate', true);
                    return modelValue;
                }

                var ipAddresses = angular.fromJson(attr.shieldIpaddressduplicate);

                if (ipAddresses.filter((x) => x.ipAddress === modelValue)[0] !== undefined) {
                    ctrl.$setValidity('shieldIpaddressduplicate', false);
                    return modelValue;
                }

                ctrl.$setValidity('shieldIpaddressduplicate', true);
                return modelValue
            })
        }
    };
});

/**
   * @ngdoc directive
   * @name shield-ip-addresses-access
   * @function
   *
   * @description
   * Custom directive for handling whether or not to add Ip Address restrictions
*/
angular.module('umbraco.directives').directive('shieldIpAddressesAccess', function () {
    return {
        restrict: 'E',
        templateUrl: '/App_Plugins/Shield/Backoffice/Views/Directives/IpAddresses-Access.html',
        scope: {
            ipAddressesAccess: '=',
            ipAddresses: '=',
        },
        controller: ['$scope', 'localizationService', function ($scope, localizationService) {
            $scope.remove = function ($index) {
                var ip = $scope.ipAddresses[$index],
                    msg = ip.ipAddress;

                if (ip.ipAddress !== '') {
                    if (ip.description !== '') {
                        msg += ' - ' + ip.description;
                    }

                    localizationService.localize('Shield.Properties.IpAddressAccess.Messages_ConfirmRemoveIp').then(function (warningMsg) {
                        if (confirm(warningMsg + msg)) {
                            $scope.ipAddresses.splice($index, 1);
                        }
                    });
                } else {
                    $scope.ipAddresses.splice($index, 1);
                }
            };

            if ($scope.ipAddresses.length === 0) {
                $scope.ipAddresses.push({
                    ipAddress: '',
                    description: ''
                });
            }
        }]
    };
});

/**
   * @ngdoc directive
   * @name shield-url-type
   * @function
   *
   * @description
   * Custom directive for handling the selected Url type
*/
angular.module('umbraco.directives').directive('shieldUrlType', function () {
    return {
        restrict: 'E',
        templateUrl: '/App_Plugins/Shield/Backoffice/Views/Directives/Url-Type.html',
        scope: {
            model: '='
        },
        link: function (scope, elm, attr) {
            if (scope.model === null) {
                scope.model = {
                    urlSelector: 0,
                    strUrl: '',
                    xpathUrl: '',
                    contentPickerUrl: ''
                }
            }

            scope.model.contentPickerProperty = {
                view: 'contentpicker',
                alias: 'contentPicker',
                config: {
                    multiPicker: '0',
                    entityType: 'Document',
                    startNode: {
                        query: '',
                        type: 'content',
                        id: -1
                    },
                    filter: '',
                    minNumber: 1,
                    maxNumber: 1
                },
                value: scope.model.contentPickerUrl
            };

            scope.$watch('model.contentPickerProperty.value', function (newVal, oldVal) {
                scope.model.contentPickerUrl = newVal;
            });
        }
    };
});
/**
    * @ngdoc resource
    * @name shieldResource
    * @function
    *
    * @description
    * Api resource for the Shield Framework
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
        getJournals: function (id, page, orderBy, orderByDirection) {
            return $http.get(apiRoot + 'Journals?id=' + id + '&page=' + page + "&orderBy=" + orderBy + "&orderByDirection=" + orderByDirection);
        }
    };
}]);
}(window));