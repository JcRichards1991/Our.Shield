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
    ['$scope', 'shieldMediaProtectionResource',
    function ($scope, shieldMediaProtectionResource) {
        var vm = this;
        angular.extend(vm, {
            configuration: $scope.$parent.configuration,
            directories: [],
            init: function () {
                if (vm.configuration.hotLinkingProtectedDirectories === null) {
                    vm.configuration.hotLinkingProtectedDirectories = [];
                }

                shieldMediaProtectionResource.getDirectories().then(function (response) {
                    vm.directories = response.data;
                    vm.loading = false;
                });
            },
            toggleSelectedDirectory: function (directory) {
                var index = vm.configuration.hotLinkingProtectedDirectories.indexOf(directory);

                if (index === -1) {
                    vm.configuration.hotLinkingProtectedDirectories.push(directory);
                } else {
                    vm.configuration.hotLinkingProtectedDirectories.splice(index, 1);
                }
            }
        });
    }]
);
/**
    * @ngdoc resource
    * @name shieldMediaProtectionResource
    * @function
    *
    * @description
    * Api resource for the Media Protection App
*/
angular.module('umbraco.resources').factory('shieldMediaProtectionResource', ['$http', function ($http) {

    var apiRoot = 'backoffice/Shield/MediaProtectionApi/';

    return {
        getDirectories: function () {
            return $http.get(apiRoot + 'GetDirectories');
        }
    };
}]);
}(window));