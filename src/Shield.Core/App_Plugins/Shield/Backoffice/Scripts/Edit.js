$(function (root) {
    "use strict";

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
                tabs: [
                    {
                        id:'0',
                        label: 'Environments',
                        active: true,
                        view: 'Environments'
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
                                break;

                            case 1:     //  Environment
                                vm.nameLocked = false;
                                vm.environments = response.data.environments;
                                vm.environment = response.data.environment;
                                vm.apps = response.data.apps;
                                vm.tabs[0].label = 'Domains';
                                vm.tabs[0].view = 'Domains';
                                vm.journal.columns[1].show = false;
                                vm.path = '-1,0,' + vm.id;
                                break;

                            case 2:     //  App
                                vm.nameLocked = true;
                                vm.environment = response.data.environment;
                                vm.app = response.data.app;
                                vm.appView = response.data.appAssests.view;
                                vm.configuration = response.data.configuration;
                                vm.tabs[0].label = 'Configuration'
                                vm.tabs[0].view = 'App';
                                vm.journal.columns[1].show = false;
                                vm.journal.columns[2].show = false;
                                vm.path = '-1,0,' + vm.environment.id + ',' + vm.id;

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
                    vm.saveButtonState = 'busy';

                    vm.saveButtonState = 'success';
                },
                editItem: function (item, index) {
                    $location.path('shield/Shield/edit/' + item.id);
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


}(window));
