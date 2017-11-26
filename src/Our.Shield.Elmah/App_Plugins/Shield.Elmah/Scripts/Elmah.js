(function(root){
    /**
    * @ngdoc controller
    * @name Shield.Editors.FrontendAccess.Edit
    * @function
    *
    * @description
    * Edit Controller for the frontend access Edit view
    */
    angular.module('umbraco').controller('Shield.Editors.Elmah.Edit',
        ['$scope', 'localizationService',
        function ($scope, localizationService) {

            var vm = this;

            angular.extend(vm, {
                configuration: $scope.$parent.configuration
            });
        }]
    );

    /**
    * @ngdoc controller
    * @name Shield.Editors.FrontendAccess.Edit
    * @function
    *
    * @description
    * Edit Controller for the frontend access Edit view
    */
    angular.module('umbraco').controller('Shield.Editors.Elmah.Reporting',
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