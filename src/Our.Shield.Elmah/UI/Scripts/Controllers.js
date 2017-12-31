/**
    * @ngdoc controller
    * @name Shield.Editors.Elmah.Edit
    * @function
    *
    * @description
    * Edit Controller for the Elmah Edit view
    */
angular.module('umbraco').controller('Shield.Editors.Elmah.Edit',
    ['$scope', 'localizationService',
        function ($scope) {
            var vm = this;
            angular.extend(vm, {
                configuration: $scope.$parent.configuration
            });
        }]
);

/**
* @ngdoc controller
* @name Shield.Editors.Elmah.Reporting
* @function
*
* @description
* Edit Controller for the Elmah Reporting view
*/
angular.module('umbraco').controller('Shield.Editors.Elmah.Reporting',
    ['$scope', 'shieldElmahResource',
        function ($scope, shieldElmahResource) {
            var vm = this;
            angular.extend(vm, {
                loading: true,
                pageNumber: 1,
                resultsPerPage: 100,
                totalPages: 0,
                errors: [],
                selectedError: null,
                init: function() {
                    vm.getErrors();
                },
                showDetails: function(id) {
                    vm.loading = true;
                    shieldElmahResource.getError(id).then(function(response) {
                        vm.selectedError = response.data;
                        vm.loading = false;
                    });
                },
                backToList: function() {
                    vm.selectedError = null;
                },
                getErrors: function() {
                    vm.loading = true;

                    shieldElmahResource.getErrors(vm.pageNumber, vm.resultsPerPage).then(function(response) {
                        vm.errors = response.data.errors;
                        vm.totalPages = response.data.totalPages;
                        vm.loading = false;
                    });
                },
                prevPage: function () {
                    vm.pageNumber--;
                    vm.getErrors();
                },
                nextPage: function() {
                    vm.pageNumber++;
                    vm.getErrors();
                },
                gotoPage: function(page) {
                    vm.pageNumber = page;
                    vm.getErrors();
                }
            });
        }]
);