/**
 * @ngdoc controller
 * @name PropertyEditors.AllowedIpsController
 * @function
 *
 * @description
 * Handles the Umbraco Access area of the custom section
 */
angular.module('umbraco').controller('Shield.PropertyEditors.AllowedIpsController', ['$scope', function ($scope) {
    $scope.newIp = {
        ipAddress: '',
        description: '',
        valid: true,
        errorMsg: ''
    };

    angular.forEach($scope.model.value, function (ip, index) {
        ip.editMode = false;
    });

    $scope.addIp = function () {
        $scope.newIp.errorMsg = '',
        $scope.newIp.valid = true;

        if ($scope.newIp.ipAddress === '') {
            $scope.newIp.valid = false;
            $scope.newIp.errorMsg = 'IP Address is required';
            return false;
        }

        if ($scope.model.value.filter((ip) => ip.ipAddress === $scope.newIp.ipAddress)[0] !== undefined) {
            $scope.newIp.valid = false;
            $scope.newIp.errorMsg = 'IP Address has already been added';
            return false;
        }

        var pattern = /^(?=\d+\.\d+\.\d+\.\d+($|\/))(([1-9]?\d|1\d\d|2[0-4]\d|25[0-5])\.?){4}(\/([0-9]|[1-2][0-9]|3[0-2]))?$/g;
        var valid = pattern.test($scope.newIp.ipAddress);

        if(!valid) {
            pattern = /^s*((([0-9A-Fa-f]{1,4}:){7}([0-9A-Fa-f]{1,4}|:))|(([0-9A-Fa-f]{1,4}:){6}(:[0-9A-Fa-f]{1,4}|((25[0-5]|2[0-4]d|1dd|[1-9]?d)(.(25[0-5]|2[0-4]d|1dd|[1-9]?d)){3})|:))|(([0-9A-Fa-f]{1,4}:){5}(((:[0-9A-Fa-f]{1,4}){1,2})|:((25[0-5]|2[0-4]d|1dd|[1-9]?d)(.(25[0-5]|2[0-4]d|1dd|[1-9]?d)){3})|:))|(([0-9A-Fa-f]{1,4}:){4}(((:[0-9A-Fa-f]{1,4}){1,3})|((:[0-9A-Fa-f]{1,4})?:((25[0-5]|2[0-4]d|1dd|[1-9]?d)(.(25[0-5]|2[0-4]d|1dd|[1-9]?d)){3}))|:))|(([0-9A-Fa-f]{1,4}:){3}(((:[0-9A-Fa-f]{1,4}){1,4})|((:[0-9A-Fa-f]{1,4}){0,2}:((25[0-5]|2[0-4]d|1dd|[1-9]?d)(.(25[0-5]|2[0-4]d|1dd|[1-9]?d)){3}))|:))|(([0-9A-Fa-f]{1,4}:){2}(((:[0-9A-Fa-f]{1,4}){1,5})|((:[0-9A-Fa-f]{1,4}){0,3}:((25[0-5]|2[0-4]d|1dd|[1-9]?d)(.(25[0-5]|2[0-4]d|1dd|[1-9]?d)){3}))|:))|(([0-9A-Fa-f]{1,4}:){1}(((:[0-9A-Fa-f]{1,4}){1,6})|((:[0-9A-Fa-f]{1,4}){0,4}:((25[0-5]|2[0-4]d|1dd|[1-9]?d)(.(25[0-5]|2[0-4]d|1dd|[1-9]?d)){3}))|:))|(:(((:[0-9A-Fa-f]{1,4}){1,7})|((:[0-9A-Fa-f]{1,4}){0,5}:((25[0-5]|2[0-4]d|1dd|[1-9]?d)(.(25[0-5]|2[0-4]d|1dd|[1-9]?d)){3}))|:)))(%.+)?s*(\/([0-9]|[1-9][0-9]|1[0-1][0-9]|12[0-8]))?$/g;
            valid = pattern.test($scope.newIp.ipAddress)

            if (!valid) {
                $scope.newIp.valid = false;
                $scope.newIp.errorMsg = 'Invalid IP Address. Please enter a valid IPv4 cidr or IPv6 cidr address';
                return false;
            }
        }

        $scope.model.value.push({
            ipAddress: $scope.newIp.ipAddress,
            description: $scope.newIp.description,
            editMode: false
        });

        $scope.newIp.ipAddress = '';
        $scope.newIp.description = '';
    };

    $scope.editIp = function (ip, update) {
        var curEditIp = $scope.model.value.filter((ip) => ip.editMode === true)[0];

        if (curEditIp && !update) {
            return false;
        }

        if (!update) {
            ip.editMode = true;
        } else {
            ip.editMode = false;
        }
    };

    $scope.removeIp = function (ip) {
        var index = $scope.model.value.indexOf(ip);

        if (index !== -1) {
            $scope.model.value.splice(index, 1);
        }
    };
}]);