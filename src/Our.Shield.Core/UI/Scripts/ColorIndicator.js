app.run(['shieldResource', function (shieldResource) {
    var vm = this;

    angular.extend(vm, {
        faviconId: 'favicon',
        defaultIndicatorColor: '#df7f48',
        run: function () {

            if (document.getElementById(vm.faviconId) || typeof (Path2D) === "undefined") {
                return;
            }

            shieldResource.getEnvironments().then(function (response) {
                var environments = response.data || [],
                    indicatorColor = vm.defaultIndicatorColor;

                if (environments.length !== 0) {
                    var environment = environments.filter((e) => e.domains.filter((d) => d.name === location.origin)[0] !== undefined)[0];

                    if (environment !== null && environment !== undefined && environment.enable) {
                        indicatorColor = environment.colorIndicator;
                    }
                }

                var link = document.createElement('link');
                link.id = vm.faviconId;
                link.type = 'image/x-icon';
                link.rel = 'shortcut icon';
                link.href = vm.drawImage(indicatorColor);

                var style = document.createElement("style");
                style.type = "text/css";
                style.innerText = "ul.sections li.avatar, ul.sections li.avatar:hover { border-left: 8px " + indicatorColor + " solid; }  ul.sections li.current, ul.sections li:hover { border-left: 4px " + indicatorColor + " solid; }";

                var head = document.getElementsByTagName('head')[0];
                head.appendChild(link);
                head.appendChild(style);
            });
        },
        drawImage: function (indicatorColor) {
            var canvas = document.createElement("canvas");
            canvas.width = 64;
            canvas.height = 64;
            var control = canvas.getContext('2d');
            control.fillStyle = indicatorColor;
            var path = new Path2D('M0,32C0,14.3,14.3,0,32,0c17.7,0,32,14.3,32,32c0,17.7-14.3,32-32,32C14.3,64,0,49.7,0,32z M31.3,42.9 c-3.1,0-5.6-0.2-7.4-0.7c-2-0.5-3.3-1.6-4-3.2c-0.7-1.7-1.1-4.2-1.1-7.7c0-1.9,0.1-3.7,0.3-5.4c0.2-1.8,0.4-3.2,0.6-4.3l0.2-1.1   c0,0,0-0.1,0-0.1c0-0.3-0.2-0.6-0.5-0.6L15.4,19c0,0-0.1,0-0.1,0c-0.3,0-0.6,0.2-0.6,0.5c-0.1,0.3-0.1,0.5-0.2,1.1  c-0.2,1.2-0.5,2.4-0.7,4.1c-0.2,1.7-0.4,3.6-0.5,5.7c0,0-0.1,0.5,0,4c0.1,3.5,0.7,6.3,1.8,8.4c1.1,2.1,3,3.6,5.6,4.5    c2.6,0.9,6.3,1.4,11,1.3h0.6c4.7,0,8.4-0.4,11-1.3c2.6-0.9,4.5-2.4,5.6-4.5c1.1-2.1,1.7-4.9,1.8-8.4c0.1-3.5,0-4,0-4    c-0.1-2.1-0.2-4-0.5-5.7c-0.2-1.7-0.5-2.9-0.7-4.1c-0.1-0.6-0.2-0.8-0.2-1.1C49.2,19.2,49,19,48.7,19c0,0-0.1,0-0.1,0l-4.1,0.6  c-0.3,0.1-0.5,0.3-0.5,0.6c0,0,0,0.1,0,0.1l0.2,1.1c0.2,1.1,0.4,2.6,0.6,4.3c0.2,1.8,0.3,3.6,0.3,5.4c0,3.5-0.3,6-1.1,7.7   c-0.7,1.7-2.1,2.8-4,3.2c-1.8,0.5-4.2,0.7-7.4,0.7H31.3z');
            control.fill(path);
            return canvas.toDataURL();
        }
    });

    vm.run()
}]);