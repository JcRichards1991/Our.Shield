angular.module('shield', ['umbraco', 'shield.resources']);
angular.module('shield.resources', []);

angular.module('umbraco').directive("ngIsolateApp", function () {
    return {
        "scope": {},
        "restrict": "AEC",
        "compile": function (element, attrs) {
            var html = element.html();
            element.html('');
            return function (scope, element) {
                scope.$destroy();
                setTimeout(function () {
                    var newRoot = document.createElement("div");
                    newRoot.innerHTML = html;
                    angular.bootstrap(newRoot, [attrs["ngIsolateApp"]]);
                    element.append(newRoot);
                });
            }
        }
    }
});