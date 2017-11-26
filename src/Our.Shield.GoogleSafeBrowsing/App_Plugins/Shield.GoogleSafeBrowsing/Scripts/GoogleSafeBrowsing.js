(function(root){
    /**
    * @ngdoc controller
    * @name Shield.Editors.SafeLinking.Edit
    * @function
    *
    * @description
    * Edit Controller for the Safe Browsing Edit view
    */
    angular.module('umbraco').controller('Shield.Editors.SafeLinking.Edit',
        ['$scope', 'localizationService',
        function ($scope, localizationService) {

            var vm = this;

            angular.extend(vm, {
                configuration: $scope.$parent.configuration
            });
        }]
    );
}(window));