/**
   * @ngdoc directive
   * @name shield-app
   * @function
   *
   * @description
   * Custom angular directive for inserting the Shield Apps view's onto the page 
*/
angular.module('umbraco.directives').directive('shieldApp',
    ['$compile', '$templateCache', '$http',
    function ($compile, $templateCache, $http) {
        return {
            restrict: 'E',
            scope: {
                view: '=',
                configuration: '='
            },
            link: function (scope, element, attr) {
                if (scope.view) {
                    var template = $templateCache.get(scope.view);
                    if (template) {
                        element.html(template);
                        $compile(element.contents())(scope);
                    } else {
                        $http.get(scope.view).then(function (response) {
                            $templateCache.put(scope.view, response.data);
                            element.html(response.data);
                            $compile(element.contents())(scope);
                        });
                    }
                }
            }
        };
    }]
);

/**
   * @ngdoc directive
   * @name shield-convert-to-number
   * @function
   *
   * @description
   * Custom angular directive for converting string to number
*/
angular.module('umbraco.directives').directive('shieldConvertToNumber',
    function () {
        return {
            restrict: 'A',
            require: 'ngModel',
            link: function (scope, element, attrs, ngModel) {
                ngModel.$parsers.push(function (val) {
                    return parseInt(val, 10);
                });
                ngModel.$formatters.push(function (val) {
                    return '' + val;
                });
            }
        };
    }
);