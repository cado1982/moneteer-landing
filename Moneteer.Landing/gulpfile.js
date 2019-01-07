/// <binding AfterBuild='scripts, sass' />
var gulp = require("gulp");
var merge = require("gulp-sequence");
var sass = require("gulp-sass");
var concat = require("gulp-concat");
var plumber = require("gulp-plumber")
var notify = require("gulp-notify");

var deps = {
    "jquery": {
        "dist/*": ""
    },
    "@clr/ui": {
        "*": ""
    },
    "@clr/icons": {
        "clr-icons.css": "",
        "clr-icons.min.css": "",
        "clr-icons.min.js": ""
    },
    "@webcomponents/custom-elements": {
        "custom-elements.min.js": ""
    },
    "qrcodejs": {
        "qrcode.min.js": ""
    }
};

gulp.task("scripts", function () {
    var streams = [];

    for (var prop in deps) {
        console.log("Prepping Scripts for: " + prop);
        for (var itemProp in deps[prop]) {
            streams.push(gulp.src("node_modules/" + prop + "/" + itemProp)
                .pipe(gulp.dest("wwwroot/lib/" + prop + "/" + deps[prop][itemProp])));
        }
    }

    return merge(streams);
});

gulp.task("sass", function () {
    return gulp.src("Styles/**/*.scss")
        .pipe(plumber({
            errorHandler: function (err) {
                notify.onError({
                    title: "Gulp error in " + err.plugin,
                    message: err.toString()
                })(err);
                this.emit('end');
            }
        }))
        .pipe(sass())
        .pipe(concat("site.css"))
        .pipe(gulp.dest("wwwroot/css"));
});

gulp.task("sass:watch", function () {
    gulp.watch("Styles/**/*.scss", ['sass']);
});