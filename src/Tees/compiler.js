// Imports
const os = require("os");
const fs = require("fs");
const path = require("path");
const typescript = require("typescript");
const uglifyJs = require("uglify-js");
const mergeSourceMap = require("multi-stage-sourcemap").transfer;

function compileTS(args) {
    let sourceFiles = args.sourceFile.split(';');
    let shouldBundle = (sourceFiles.length > 1);

    let options = {};
    options.noEmitOnError = true;

    if (shouldBundle) {
        options.allowJs = true;
        options.outFile = args.getOutputPath();
    }

    if (args.generateSourceMaps) {
        options.sourceMap = true;
        options.mapRoot = args.sourceMapDirectory;
    }

    console.log(JSON.stringify(options));

    let tsc = typescript.createProgram(sourceFiles, options);
    let srcFile = (shouldBundle ? null : tsc.getSourceFile(args.getOutputPath()));
    let results = tsc.emit(srcFile, function (outPath, content) {
        console.log(outPath);
    });

    console.log(results.emitSkipped);
}

function CompilerOptions() {
    let me = this;
    let bool = /true/i;

    me.sourceFile = process.argv[2];

    me.outputDirectory = process.argv[3];
    if (!me.outputDirectory) { me.outputDirectory = path.dirname(me.sourceFile); }

    me.sourceMapDirectory = process.argv[4];
    if (!me.sourceMapDirectory) { me.sourceMapDirectory = me.outputDirectory; }

    me.suffix = process.argv[5];
    me.minify = bool.test(process.argv[6]);
    me.generateSourceMaps = bool.test(process.argv[7]);

    me.getOutputPath = function (omitSuffix = false) {
        let baseName = path.basename(me.sourceFile, path.extname(me.sourceFile));
        return path.join(me.outputDirectory, (baseName + (omitSuffix ? "" : me.suffix) + ".js"));
    }

    me.getSourceMapPath = function (omitSuffix = false) {
        let baseName = path.basename(me.sourceFile, path.extname(me.sourceFile));
        return path.join(me.sourceMapDirectory, (baseName + (omitSuffix ? "" : me.suffix) + "js.map"));
    }

    me.log = function () {
        console.log("src: " + me.sourceFile);
        console.log("out: " + me.getOutputPath());
        console.log("dir: " + me.outputDirectory);
        console.log("smd: " + me.sourceMapDirectory);
        console.log("suf: " + me.suffix);

        console.log("min: " + me.minify);
        console.log("map: " + me.generateSourceMaps);
        console.log("====================");
        console.log("");
    }
    //me.log();
}

var args = new CompilerOptions();
compileTS(args);