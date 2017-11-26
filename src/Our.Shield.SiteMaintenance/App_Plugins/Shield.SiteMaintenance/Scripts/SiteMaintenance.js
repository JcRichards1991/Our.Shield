(function(root){
    /**
    * @ngdoc controller
    * @name Shield.Editors.SiteMaintenance.Edit
    * @function
    *
    * @description
    * Edit Controller for the Site Maintenance Edit view
    */
    angular.module('umbraco').controller('Shield.Editors.SiteMaintenance.Edit',
        ['$scope', 'localizationService',
        function ($scope, localizationService) {

            var vm = this;

            angular.extend(vm, {
                configuration: $scope.$parent.configuration
            });
        }]
    );
}(window));