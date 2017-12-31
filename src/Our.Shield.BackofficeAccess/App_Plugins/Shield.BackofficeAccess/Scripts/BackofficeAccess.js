(function (root) {
    /**
    * @ngdoc controller
    * @name Shield.BackofficeAccess.Edit
    * @function
    *
    * @description
    * Edit Controller for the Backoffice Access Edit view
    */
    angular.module('umbraco').controller('Shield.Editors.BackofficeAccess.Edit',
        ['$scope',
        function ($scope) {

            var vm = this;

            angular.extend(vm, {
                configuration: $scope.$parent.configuration
            });
        }]
    );
}(window));