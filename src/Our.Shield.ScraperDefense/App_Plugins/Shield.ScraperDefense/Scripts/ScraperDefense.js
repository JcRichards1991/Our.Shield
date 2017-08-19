(function (root) {
    /**
     * @ngdoc controller
     * @name ScraperDefense.EditController
     * @function
     *
     * @description
     * Edit Controller for the Scraper Defense Edit view
     */
    angular.module('umbraco').controller('Shield.Editors.ScraperDefense.Edit',
        ['$scope', function ($scope) {
            var vm = this;
            angular.extend(vm, {
                loading: true,
                configuration: $scope.$parent.configuration,
                init: function () {
                    vm.loading = false;
                }
            });
        }]
    );
}(window));
