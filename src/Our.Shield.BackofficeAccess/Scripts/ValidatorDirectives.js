/**
   * @ngdoc directive
   * @name ipaddress-valid
   * @function
   *
   * @description
   * Custom angular directive for validating an IP Address
   * as IPv4 or IPv6 with optional cidr
*/
angular.module('umbraco.directives').directive('ipaddressvalid', function () {
    return {
        restrict: 'A',
        require: 'ngModel',
        link: function (scope, elm, attr, ctrl) {
            ctrl.$parsers.push(function (modelValue) {
                if (modelValue === '' || modelValue === undefined) {
                    ctrl.$setValidity('ipaddressvalid', true);
                    return modelValue;
                }

                //Check if IPv4 with optional cidr
                var pattern = /^(?=\d+\.\d+\.\d+\.\d+($|\/))(([1-9]?\d|1\d\d|2[0-4]\d|25[0-5])\.?){4}(\/([0-9]|[1-2][0-9]|3[0-2]))?$/g;

                if (pattern.test(modelValue)) {
                    ctrl.$setValidity('ipaddressvalid', true);
                } else {
                    //Check if IPv6 with optional cidr
                    pattern = /^s*((([0-9A-Fa-f]{1,4}:){7}([0-9A-Fa-f]{1,4}|:))|(([0-9A-Fa-f]{1,4}:){6}(:[0-9A-Fa-f]{1,4}|((25[0-5]|2[0-4]d|1dd|[1-9]?d)(.(25[0-5]|2[0-4]d|1dd|[1-9]?d)){3})|:))|(([0-9A-Fa-f]{1,4}:){5}(((:[0-9A-Fa-f]{1,4}){1,2})|:((25[0-5]|2[0-4]d|1dd|[1-9]?d)(.(25[0-5]|2[0-4]d|1dd|[1-9]?d)){3})|:))|(([0-9A-Fa-f]{1,4}:){4}(((:[0-9A-Fa-f]{1,4}){1,3})|((:[0-9A-Fa-f]{1,4})?:((25[0-5]|2[0-4]d|1dd|[1-9]?d)(.(25[0-5]|2[0-4]d|1dd|[1-9]?d)){3}))|:))|(([0-9A-Fa-f]{1,4}:){3}(((:[0-9A-Fa-f]{1,4}){1,4})|((:[0-9A-Fa-f]{1,4}){0,2}:((25[0-5]|2[0-4]d|1dd|[1-9]?d)(.(25[0-5]|2[0-4]d|1dd|[1-9]?d)){3}))|:))|(([0-9A-Fa-f]{1,4}:){2}(((:[0-9A-Fa-f]{1,4}){1,5})|((:[0-9A-Fa-f]{1,4}){0,3}:((25[0-5]|2[0-4]d|1dd|[1-9]?d)(.(25[0-5]|2[0-4]d|1dd|[1-9]?d)){3}))|:))|(([0-9A-Fa-f]{1,4}:){1}(((:[0-9A-Fa-f]{1,4}){1,6})|((:[0-9A-Fa-f]{1,4}){0,4}:((25[0-5]|2[0-4]d|1dd|[1-9]?d)(.(25[0-5]|2[0-4]d|1dd|[1-9]?d)){3}))|:))|(:(((:[0-9A-Fa-f]{1,4}){1,7})|((:[0-9A-Fa-f]{1,4}){0,5}:((25[0-5]|2[0-4]d|1dd|[1-9]?d)(.(25[0-5]|2[0-4]d|1dd|[1-9]?d)){3}))|:)))(%.+)?s*(\/([0-9]|[1-9][0-9]|1[0-1][0-9]|12[0-8]))?$/g;
                    if (pattern.test(modelValue)) {
                        ctrl.$setValidity('ipaddressvalid', true);
                    } else {
                        ctrl.$setValidity('ipaddressvalid', false);
                    }
                }

                return modelValue
            });
        }
    };
});

/**
   * @ngdoc directive
   * @name ipaddress-duplicate
   * @function
   *
   * @description
   * Checks to make sure an IP address isn't being added more than
   * once to the IP address White-List
*/
angular.module('umbraco.directives').directive('ipaddressduplicate', function () {
    return {
        restrict: 'A',
        require: 'ngModel',
        link: function (scope, elm, attr, ctrl) {
            ctrl.$parsers.push(function (modelValue) {
                if (modelValue === '' || modelValue === undefined) {
                    ctrl.$setValidity('ipaddressduplicate', true);
                    return modelValue;
                }

                var ipAddresses = angular.fromJson(attr.ipaddressduplicate);

                if (ipAddresses.filter((x) => x.ipAddress === modelValue)[0] !== undefined) {
                    ctrl.$setValidity('ipaddressduplicate', false);
                    return modelValue;
                }

                ctrl.$setValidity('ipaddressduplicate', true);
                return modelValue
            })
        }
    };
});