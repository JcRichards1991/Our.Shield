(function(root){
    /**
    * @ngdoc controller
    * @name Shield.Editors.SafeBrowsing.Edit
    * @function
    *
    * @description
    * Edit Controller for the Safe Browsing Edit view
    */
    angular.module('umbraco').controller('Shield.Editors.FrontendLocker.Edit',
        ['$scope', 'localizationService',
        function ($scope, localizationService) {

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