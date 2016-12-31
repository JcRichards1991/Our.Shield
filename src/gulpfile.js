/// <binding AfterBuild='buildJS' />
//*********** IMPORTS *****************
var gulp = require('gulp');
var sass = require('gulp-ruby-sass');
var gutil = require('gulp-util');
var rename = require("gulp-rename");
var map = require("map-stream");
var livereload = require("gulp-livereload");
var concat = require("gulp-concat");
var uglify = require('gulp-uglify');
var watch = require('gulp-watch');
var minify = require('gulp-minify');
var merge = require('merge-stream');

global.errorMessage = '';

var sassFiles = [
	//{
	//	watch: '_scss/Site/**/*.scss',
	//	sass: '_scss/Site/site.scss',
	//	output: './www/app/View/Themed/Site/webroot/css',
	//	name: 'site.css',
	//}
];

var jsBaseDir = 'Shield.UI/'
var jsFiles = [
    jsBaseDir + 'UmbracoAccess/Js/UmbracoAccess.js',
    jsBaseDir + 'MediaProtect/Js/MediaProtect.js'
];

var jsOutput = 'Shield.Umbraco.UI/App_Plugins/Shield/backoffice/js';
//END configuration


//gulp.task('watch', function () {
//    for (var i in sassFiles) {
//    	sassWatch(sassFiles[i]);
//    }
//});

//function sassWatch(sassData) {
//    gulp.src(sassData.watch)
//	.pipe(watch({ glob: sassData.watch, emitOnGlob: true }, function () {
//	    gulp.src(sassData.sass)
//		.pipe(sass(sassOptions))
//		.on('error', function (err) {
//		    gutil.log(err.message);
//		    gutil.beep();
//		    global.errorMessage = err.message + " ";
//		})
//		.pipe(checkErrors())
//		.pipe(rename(sassData.name))
//		.pipe(gulp.dest(sassData.output))
//		.pipe(livereload());
//	}));
//}

gulp.task('default', ['buildJS']);

gulp.task('buildJS', function () {
    gulp.src(jsFiles)
    .pipe(concat('Shield.js'))
    .pipe(gulp.dest(jsOutput))
    .pipe(uglify({ outSourceMap: false }))
    .pipe(rename('Shield.min.js'))
    .pipe(gulp.dest(jsOutput));
});

var sassOptions = {
    'style': 'compressed',
    'unixNewlines': true,
    'cacheLocation': '_scss/.sass_cache'
};

var checkErrors = function (obj) {
    function checkErrors(file, callback, errorMessage) {
        if (file.path.indexOf('.scss') != -1) {
            file.contents = new Buffer("\
					body * { white-space:pre; }\
					body * { display: none!important; }\
					body:before {\
						white-space:pre;\
						content: '"+ global.errorMessage.replace(/(\\)/gm, "/").replace(/(\r\n|\n|\r)/gm, "\\A") + "';\
					}\
					html{background:#ccf!important; }\
				");
        }
        callback(null, file);
    }
    return map(checkErrors);
};
