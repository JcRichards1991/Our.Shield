/**
 * @ngdoc controller
 * @name MediaProtection.EditController
 * @function
 *
 * @description
 * Edit Controller for the Media Protection Edit view
 */
angular.module('umbraco').controller('Shield.Editors.MediaProtection.EditController', ['$scope', 'notificationsService', 'localizationService', 'ShieldResource', function ($scope, notificationsService, localizationService, resource) {
    $scope.loading = 0;
    $scope.error = null;

    $scope.init = function () {

    }
}]);