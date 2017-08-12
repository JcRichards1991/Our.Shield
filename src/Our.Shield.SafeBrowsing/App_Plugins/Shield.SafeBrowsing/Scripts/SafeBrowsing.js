(function(root){
    /**
    * @ngdoc controller
    * @name Shield.Editors.SafeBrowsing.Edit
    * @function
    *
    * @description
    * Edit Controller for the Safe Browsing Edit view
    */
    angular.module('umbraco').controller('Shield.Editors.SafeBrowsing.Edit',
        ['$scope', 'localizationService',
        function ($scope, localizationService) {

            var vm = this;

            angular.extend(vm, {
                loading: true,
                init: function () {
                    vm.loading = false;
                }
            });
        }]
    );
}(window));