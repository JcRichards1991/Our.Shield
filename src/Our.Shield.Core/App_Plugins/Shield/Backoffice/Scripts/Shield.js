(function(root){
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
                    vm.appListing.apps = response.data.apps;
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
            editItem: function (item, index) {
                $location.path('shield/shield/edit/' + item.id);
            },
            appListing: {
                options: {
                    orderBy: 'name',
                    orderDirection: 'desc'
                },
                apps: null,
                totalPages: 1,
                pageNumber: 1,
                nextPage: function (page) {
                    //TODO: get listing with desired sort on field for next page
                },
                previousPage: function (page) {
                    //TODO: get listing with desired sort on field for previous page
                },
                gotoPage: function (page) {
                    //TODO: get listing with desired sort on field for desired page
                },
                isSortDirection: function (col, direction) {
                    return false;

                    //TODO: uncomment when get listing functionality is completed
                    //return listViewHelper.setSortingDirection(col, direction, vm.journalListing.options);
                },
                sort: function (field, allow) {
                    if(allow) {
                        vm.journalListing.options.orderBySystemField = false;
                        listViewHelper.setSorting(field, allow, vm.journalListing.options);
                        
                        //TODO: get listing of journals with desired sort on field
                    }
                },
                getJournalListing: function (page) {
                    //TODO: call the API endpoint passing the corresponsing values
                    //and update the vm.journalListing.items array with that returned
                },
                enableItem: function (item) {
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
                    //TODO: get listing with desired sort on field for next page
                },
                previousPage: function (page) {
                    //TODO: get listing with desired sort on field for previous page
                },
                gotoPage: function (page) {
                    //TODO: get listing with desired sort on field for desired page
                },
                isSortDirection: function (col, direction) {
                    return false;

                    //TODO: uncomment when get listing functionality is completed
                    //return listViewHelper.setSortingDirection(col, direction, vm.journalListing.options);
                },
                sort: function (field, allow) {
                    if(allow) {
                        vm.journalListing.options.orderBySystemField = false;
                        listViewHelper.setSorting(field, allow, vm.journalListing.options);
                        
                        //TODO: get listing of journals with desired sort on field
                    }
                },
                getJournalListing: function (page) {
                    //TODO: call the API endpoint passing the corresponsing values
                    //and update the vm.journalListing.items array with that returned
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
        getJournals: function (environemntId, id, page, itemsPerPage) {
            return $http.get(apiRoot + 'Journals?environmentId=' + environemntId + '&id=' + id + '&page=' + page + '&itemsPerPage=' + itemsPerPage);
        },
        getAppIds: function () {
            return $http.get(apiRoot + 'AppIds');
        }
    };
}]);
}(window));