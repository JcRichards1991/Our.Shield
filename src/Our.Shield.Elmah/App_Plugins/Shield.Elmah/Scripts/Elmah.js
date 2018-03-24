(function(root){
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
                },
                generateTestException: function () {
                    vm.loading = true;
                    shieldElmahResource.generateTestException().then(function() {
                        vm.getErrors();
                        vm.loading = false;
                    });
                }
            });
        }]
);
/**
    * @ngdoc resource
    * @name shieldElmahResource
    * @function
    *
    * @description
    * Api resource for the Elmah App
*/
angular.module('umbraco.resources').factory('shieldElmahResource',
    ['$http',
        function ($http) {
            var apiRoot = 'backoffice/Shield/ElmahApi/';

            return {
                getErrors: function (page, resultsPerPage) {
                    return $http.get(apiRoot + 'GetErrors?page=' + page + '&resultsPerPage=' + resultsPerPage);
                },
                generateTestException: function () {
                    return $http({
                        method: 'POST',
                        url: apiRoot + 'GenerateTestException'
                    });
                }
            };
        }
    ]
);
}(window));