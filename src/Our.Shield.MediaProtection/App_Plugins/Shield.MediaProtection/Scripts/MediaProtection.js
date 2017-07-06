(function(root){ 
    /**
     * @ngdoc controller
     * @name MediaProtection.EditController
     * @function
     *
     * @description
     * Edit Controller for the Media Protection Edit view
     */
    angular.module('umbraco').controller('Shield.Editors.MediaProtection.Edit',
        ['$scope', 'notificationsService', 'localizationService', 'ShieldResource',
        function ($scope, notificationsService, localizationService, resource) {
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
