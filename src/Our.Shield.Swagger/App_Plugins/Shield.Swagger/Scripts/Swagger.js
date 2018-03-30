(function(root){
/**
    * @ngdoc controller
    * @name Shield.Editors.Swagger.Edit
    * @function
    *
    * @description
    * Edit Controller for the Swagger Edit view
    */
angular.module('umbraco').controller('Shield.Editors.Swagger.Edit',
    ['$scope', 'localizationService',
        function ($scope) {
            var vm = this;
            angular.extend(vm, {
                configuration: $scope.$parent.configuration
            });
        }]
);
}(window));