$(function (root) {
    "use strict";

    /**
     * @ngdoc resource
     * @name Environments
     * @function
     *
     * @description
     * Handles environment page
    */
    angular.module('umbraco').controller('Shield.Editors.Settings as vm', 
        ['$scope', '$routeParams', 'notificationsService', 'localizationService', 'ShieldResource', 
        function ($scope, $routeParams, notificationsService, localizationService, sheildResource) {

            vm = this;



        }]
    );
}(window));
