// Imports
const os = require("os");
const fs = require("fs");
const path = require("path");
const typescript = require("typescript");
const uglifyJs = require("uglify-js");
const sourceMapMerger = require("multi-stage-sourcemap").transfer;

function compileTs(args) {
    let sourceMapA = null;
    args.typescriptOptions.noEmitOnError = true;

    let tsc = typescript.createProgram(args.sourceFiles, args.typescriptOptions);
    let result = tsc.emit(null, function (filePath, content) {
        console.log("emit: " + filePath);

        switch (path.extname(filePath)) {
            case ".map":
                sourceMapA = JSON.parse(content);
                if (!args.minify) { createFile(filePath, content, true); }
                break;

            case ".js":
                if (args.minify) {
                    let relativePath = (path.basename(filePath));

                    if (args.shouldGenerateSourceMap) {
                        args.uglifyJsOptions.sourceMap = {
                            filename: relativePath,
                            url: (path.basename(filePath) + ".map")
                        };
                    }

                    let result = uglifyJs.minify(content, args.uglifyJsOptions);

                    if (result.map) {
                        let finalMap = mergeSourceMaps(result.map, sourceMapA, relativePath);
                        createFile((filePath + ".map"), JSON.stringify(finalMap, null, 2), true);
                    }

                    createFile(filePath, result.code, true);
                }
                else {
                    createFile(filePath, content, true);
                }
                break;
        }
    });

    let item = null;
    let duplicates = {}, key;
    let diagnostic = typescript.getPreEmitDiagnostics(tsc).concat(result.diagnostics);
    for (var i = 0; i < diagnostic.length; i++) {
        item = diagnostic[i];
        if (!item.file) { continue; }

        let position = item.file.getLineAndCharacterOfPosition(item.start);
        let message = typescript.flattenDiagnosticMessageText(item.messageText, os.EOL);

        key = (item.file.fileName + position.line + position.character);
        if (duplicates.hasOwnProperty(key) === false && path.extname(item.file.fileName) === ".ts") {
            duplicates[key] = true;

            console.error(JSON.stringify({
                message: message.replace(/\s/, " "),
                file: item.file.fileName,
                line: (position.line + 1),
                column: (position.character + 1),
                status: item.start,
                level: convertToInt(item.code)
            }));
        }
    }
}

function mergeSourceMaps(mapB, mapA, targetFile) {
    var mapC = sourceMapMerger({
        fromSourceMap: mapB,
        toSourceMap: mapA
    });

    mapC = JSON.parse(mapC.toString());
    mapC.file = targetFile;
    return mapC;
}

function createFile(absoluePath, content, out) {
    fs.writeFile(absoluePath, content, function (ex) {
        if (ex) { console.error(ex.message); }
        if (out) { console.log("-> " + absoluePath); }
    });
}

function convertToInt(value) {
    let category;
    switch (value) {
        case 1:
            category = 0; /* error */
            break;

        case 0:
            category = 1; /* warn */
            break;

        default:
            category = 2; /* info */
            break;
    }

    return category;
}

function mergeOptions(args) {
    var configFile = { ts: null, js: null };
    if (args.optionsFile) {
        configFile = JSON.parse(fs.readFileSync(args.optionsFile).toString());
    }

    if (configFile.hasOwnProperty("minify")) { args.minify = configFile.minify; }
    if (configFile.hasOwnProperty("outFile")) { args.outFile = configFile.outFile; }
    if (configFile.hasOwnProperty("generateSourceMaps")) { args.generateSourceMaps = configFile.generateSourceMaps; }
}

function CompilerOptions() {
    let me = this;
    let bool = /true/i;

    me.typescriptOptions = JSON.parse("{}");
    me.uglifyJsOptions = JSON.parse("{}");

    me.sourceFiles = process.argv[2].split(';');
    me.optionsFile = process.argv[3];
    me.outFile = process.argv[4];

    me.minify = bool.test(process.argv[5]);
    me.shouldGenerateSourceMap = me.typescriptOptions.sourceMap = bool.test(process.argv[6]);

    mergeOptions(me);

    me.getOutputPath = function () {
        let baseName = path.basename(me.sourceFile, path.extname(me.sourceFile));
        return path.join(me.outputDirectory, (baseName + ".css"));
    }

    me.getSourceMapPath = function () {
        let baseName = path.basename(me.sourceFile, path.extname(me.sourceFile));
        return path.join(me.sourceMapDirectory, (baseName + ".css.map"));
    }

    me.log = function () {
        console.log("src: " + me.sourceFiles);
        console.log("out: " + me.typescriptOptions.outFile);

        console.log("min: " + me.minify);
        console.log("map: " + me.shouldGenerateSourceMap);
        console.log("====================");
        console.log("");
    }
    me.log();
}

compileTs(new CompilerOptions());