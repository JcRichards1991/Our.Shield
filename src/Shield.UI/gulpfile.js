/// <binding BeforeBuild='buildJS' />
//*********** IMPORTS *****************
var gulp = require('gulp');
var sass = require('gulp-ruby-sass');
var gutil = require('gulp-util');
var rename = require("gulp-rename");
var concat = require("gulp-concat");
var uglify = require('gulp-uglify');
var insert = require('gulp-insert');

var jsFiles = [
    'UmbracoAccess/Js/**.js',
    'PropertyEditors/Js/**.js'
];

var jsOutput = 'Scripts/';


gulp.task('default', ['buildJS']);

gulp.task('buildJS', function () {
    gulp.src(jsFiles)
    .pipe(concat('Shield.js'))
    .pipe(insert.wrap('(function(root){ \n', '\n}(window));'))
    .pipe(gulp.dest(jsOutput))
    .pipe(uglify({ outSourceMap: false }))
    .pipe(rename('Shield.min.js'))
    .pipe(gulp.dest(jsOutput));
});
