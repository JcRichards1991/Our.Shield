/**
 * @ngdoc controller
 * @name PropertyEditors.AllowedIpsController
 * @function
 *
 * @description
 * Handles the Umbraco Access area of the custom section
 */
angular.module('umbraco').controller('Shield.PropertyEditors.AllowedIpsController', ['$scope', 'localizationService', function ($scope, localizationService) {

    function IsValidIpAddress(ip, edit) {
        ip.valid = true;
        ip.errorMsg = '';
        ip.errorState = null;

        if (ip.ipAddress === '') {
            ip.valid = false;
            ip.errorMsg = localizationService.localize('Shield.UmbracoAccess.ErrorMessages_IpRequired');
            ip.errorState = 'Required';
            return false;
        }

        //Check if IPv4 with optional cidr
        var pattern = /^(?=\d+\.\d+\.\d+\.\d+($|\/))(([1-9]?\d|1\d\d|2[0-4]\d|25[0-5])\.?){4}(\/([0-9]|[1-2][0-9]|3[0-2]))?$/g;
        var valid = pattern.test(ip.ipAddress);

        if (!valid) {
            //Check if IPv6 with optional cidr
            pattern = /^s*((([0-9A-Fa-f]{1,4}:){7}([0-9A-Fa-f]{1,4}|:))|(([0-9A-Fa-f]{1,4}:){6}(:[0-9A-Fa-f]{1,4}|((25[0-5]|2[0-4]d|1dd|[1-9]?d)(.(25[0-5]|2[0-4]d|1dd|[1-9]?d)){3})|:))|(([0-9A-Fa-f]{1,4}:){5}(((:[0-9A-Fa-f]{1,4}){1,2})|:((25[0-5]|2[0-4]d|1dd|[1-9]?d)(.(25[0-5]|2[0-4]d|1dd|[1-9]?d)){3})|:))|(([0-9A-Fa-f]{1,4}:){4}(((:[0-9A-Fa-f]{1,4}){1,3})|((:[0-9A-Fa-f]{1,4})?:((25[0-5]|2[0-4]d|1dd|[1-9]?d)(.(25[0-5]|2[0-4]d|1dd|[1-9]?d)){3}))|:))|(([0-9A-Fa-f]{1,4}:){3}(((:[0-9A-Fa-f]{1,4}){1,4})|((:[0-9A-Fa-f]{1,4}){0,2}:((25[0-5]|2[0-4]d|1dd|[1-9]?d)(.(25[0-5]|2[0-4]d|1dd|[1-9]?d)){3}))|:))|(([0-9A-Fa-f]{1,4}:){2}(((:[0-9A-Fa-f]{1,4}){1,5})|((:[0-9A-Fa-f]{1,4}){0,3}:((25[0-5]|2[0-4]d|1dd|[1-9]?d)(.(25[0-5]|2[0-4]d|1dd|[1-9]?d)){3}))|:))|(([0-9A-Fa-f]{1,4}:){1}(((:[0-9A-Fa-f]{1,4}){1,6})|((:[0-9A-Fa-f]{1,4}){0,4}:((25[0-5]|2[0-4]d|1dd|[1-9]?d)(.(25[0-5]|2[0-4]d|1dd|[1-9]?d)){3}))|:))|(:(((:[0-9A-Fa-f]{1,4}){1,7})|((:[0-9A-Fa-f]{1,4}){0,5}:((25[0-5]|2[0-4]d|1dd|[1-9]?d)(.(25[0-5]|2[0-4]d|1dd|[1-9]?d)){3}))|:)))(%.+)?s*(\/([0-9]|[1-9][0-9]|1[0-1][0-9]|12[0-8]))?$/g;
            valid = pattern.test(ip.ipAddress)

            if (!valid) {
                ip.valid = false;
                ip.errorMsg = localizationService.localize('Shield.UmbracoAccess.ErrorMessages_IpInvalid');
                ip.errorState = "Invalid";
                return false;
            }
        }

        var index = edit ? 1 : 0;

        if ($scope.model.value.filter((x) => x.ipAddress === ip.ipAddress)[index] !== undefined) {
            ip.valid = false;
            ip.errorMsg = localizationService.localize('Shield.UmbracoAccess.ErrorMessages_IpDuplicate');
            ip.errorState = "Duplicate";
            return false;
        }

        return true;
    };

    $scope.newIp = {
        ipAddress: '',
        description: '',
        valid: true,
        errorMsg: '',
        errorState: null
    };

    angular.forEach($scope.model.value, function (ip, index) {
        ip.editMode = false;
        ip.valid = true;
        ip.errorMsg = '';
        ip.errorState = null;
    });

    $scope.addIp = function () {
        if (!IsValidIpAddress($scope.newIp, false)) {
            return false;
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
            if (!IsValidIpAddress(curEditIp, true)) {
                return false;
            }

            curEditIp.editMode = false;
        }
    };

    $scope.removeIp = function (ip) {
        if (confirm(localizationService.localize('Shield.UmbracoAccess.AlertMessages_ConfirmRemoveIp') + ip.ipAddress + ' - ' + ip.description)) {
            var index = $scope.model.value.indexOf(ip);

            if (index !== -1) {
                $scope.model.value.splice(index, 1);
            }
        }
    };
}]);