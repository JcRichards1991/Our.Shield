/// <binding BeforeBuild='default' />
//*********** IMPORTS *****************
'use strict';

var gulp = require('gulp');
var sass = require('gulp-sass');
var rename = require('gulp-rename');
var gutil = require('gulp-util');
var concat = require("gulp-concat");
var uglify = require('gulp-uglify');
var insert = require('gulp-insert');

var src = 'src/Our.Shield'

var jsOutput = '/App_Plugins/Shield';
var jsFiles = [
    {
        subFiles: src + '.Core/UI/Scripts/*.js',
        output: src + '.Core' + jsOutput + '/Backoffice/Scripts',
        name: 'Shield.js',
        nameMin: 'Shield.min.js'
    },
    {
        subFiles: src + '.BackofficeAccess/Scripts/*.js',
        output: src + '.BackofficeAccess' + jsOutput + '.BackofficeAccess/Scripts',
        name: 'BackofficeAccess.js',
        nameMin: 'BackofficeAccess.min.js'
    }
];

var cssOutput = '/App_Plugins/Shield';
var scssFiles = [
    {
        files: src + '.Core/UI/Scss/*.scss',
        output: src + '.Core' + cssOutput + '/Backoffice/Css',
        name: 'Shield.css',
        nameMin: 'Shield.min.css'
    },
    {
        files: src + '.BackofficeAccess/Scss/*.scss',
        output: src + '.BackofficeAccess' + cssOutput + '.BackofficeAccess/Css',
        name: 'BackofficeAccess.css',
        nameMin: 'BackofficeAccess.min.css'
    }
];

gulp.task('default', ['buildjs', 'buildscss']);

gulp.task('buildjs', function () {
    for (var j in jsFiles) {
        buildscript(jsFiles[j]);
    }
});

gulp.task('buildscss', function () {
    for (var j in scssFiles) {
        buildscss(scssFiles[j]);
    }
});

function buildscript (jsData) {
    gulp.src(jsData.subFiles)
    .pipe(concat(jsData.name))
    .pipe(insert.wrap('(function(root){\n', '\n}(window));'))
    .pipe(gulp.dest(jsData.output));
}

function buildscss(sassData) {
    gulp.src(sassData.files)
    .pipe(sass())
    .pipe(rename(sassData.name))
	.pipe(gulp.dest(sassData.output))
}
