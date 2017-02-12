/// <binding BeforeBuild='buildJS' />
//*********** IMPORTS *****************
var gulp = require('gulp');
var sass = require('gulp-ruby-sass');
var gutil = require('gulp-util');
var rename = require("gulp-rename");
var concat = require("gulp-concat");
var uglify = require('gulp-uglify');
var insert = require('gulp-insert');

var jsOutput = 'App_Plugin/Shield/Backoffice/';
var jsFiles = [
	{
        subFiles: jsOutput + 'UmbracoAccess/Js/*.js',
        output: jsOutput + 'UmbracoAccess/Js/',
	    name: 'UmbracoAccess.js',
	    nameMin: 'UmbracoAccess.min.js'
	}
];

gulp.task('default', ['buildJS']);

gulp.task('buildJS', function () {
    for (var j in jsFiles) {
        gulp.src(j.subFiles)
        .pipe(concat(j.name))
        .pipe(insert.wrap('(function(root){ \n', '\n}(window));'))
        .pipe(gulp.dest(j.output))
        .pipe(uglify({ outSourceMap: false }))
        .pipe(rename(j.nameMin))
        .pipe(gulp.dest(j.output));
    }
});
