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
        ['$scope', '$routeParams', '$location', '$timeout', 'notificationsService', 'localizationService', 'ShieldResource', 'listViewHelper', 'navigationService',
        function ($scope, $routeParams, $location, $timeout, notificationsService, localizationService, shieldResource, listViewHelper, navigationService) {

            var vm = this;
            angular.extend(vm, {
                type: null,
                id: $routeParams.id,
                name: '',
                loading: true,
                saveButtonState: 'init',
                environments: [],
                environment: null,
                apps: [],
                app: null,
                configuration: null,
                path: null,
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
                                vm.configuration = response.data.configuration;
                                vm.tabs[0].label = 'Configuration'
                                vm.tabs[0].view = 'App';
                                vm.journal.columns[1].show = false;
                                vm.journal.columns[2].show = false;
                                vm.path = '-1,0,' + vm.environment.id + ',' + vm.id;
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
}(window));
