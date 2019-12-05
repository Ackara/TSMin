// Imports
const os = require("os");
const fs = require("fs");
const glob = require("glob");
const path = require("path");
const typescript = require("typescript");
const uglifyJs = require("uglify-js");
const sourceMapMerger = require("multi-stage-sourcemap").transfer;

function compileTs(sourceFiles, outputFile, config, bundle) {
    let out = { name: path.basename(outputFile) };
    //console.log("out: " + outputFile);

    let tsc = typescript.createProgram(sourceFiles, config.compilerOptions);
    let result = tsc.emit((bundle ? null : tsc.getSourceFile(outputFile)), function (filePath, content) {
        //console.log("emit: " + filePath);

        switch (path.extname(filePath)) {
            case ".map":
                out.sourceMap = (config.compilerOptions.sourceMap ? JSON.parse(content) : null);
                break;

            case ".js":
                if (config.minifierOptions) {
                    if (config.compilerOptions.sourceMap) {
                        config.minifierOptions.sourceMap = {
                            filename: path.basename(outputFile),
                            url: (path.basename(outputFile) + ".map")
                        };
                    }

                    var min = uglifyJs.minify(content, config.minifierOptions);
                    out.sourceMap = (min.map ? JSON.stringify(mergeSourceMaps(min.map, out.sourceMap, out.name), null, 2) : null);
                    out.code = min.code;
                }
                else {
                    out.code = content;
                    out.sourceMap = (out.sourceMap ? JSON.stringify(out.sourceMap, null, 2) : null);
                }

                createFile(outputFile, out.code);
                if (out.sourceMap) { createFile((outputFile + ".map"), out.sourceMap); }
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
                file: path.resolve(item.file.fileName),
                line: position.line,
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

function createFile(absoluePath, content, out = true) {
    fs.writeFile(absoluePath, content, function (ex) {
        if (ex) { console.error(ex.message); }
        if (out) { console.log(">>" + absoluePath); }
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

function convertToBoolean(value) {
    if (value) {
        switch (value) {
            case "true": true;
            case "false": false;
            default: return null;
        }
    }
    return null;
}

function clone(obj) {
    return JSON.parse(JSON.stringify(obj));
}

function readConfigurationFile() {
    let me = this;
    var minify = convertToBoolean(process.argv[3]);
    var generateSourceMaps = convertToBoolean(process.argv[4]);

    // Read and validate configuraiton file.

    var fullPath = process.argv[2];
    var config = (fullPath ? JSON.parse(fs.readFileSync(fullPath)) : {});

    // Assigning default values.

    if (!config.hasOwnProperty("compilerOptions")) { config.compilerOptions = {}; }
    if (!config.compilerOptions.hasOwnProperty("sourceMap")) { config.compilerOptions.sourceMap = true; }
    if (generateSourceMaps != null) { config.compilerOptions.sourceMap = generateSourceMaps; }
    config.compilerOptions.noEmitOnError = true;

    if (!config.hasOwnProperty("minifierOptions")) { config.minifierOptions = {}; }
    if (minify != null) { config.minifierOptions = (minify ? (config.minifierOptions ? config.compilerOptions : {}) : null); }

    if (!config.sourceFiles) { config.sourceFiles = [{ include: ["**/*.ts"] }]; }

    config.getFileList = function (batch) {
        var list = [];
        for (var i = 0; i < batch.include.length; i++) {
            var result = glob.sync(batch.include[i]);
            for (var y = 0; y < result.length; y++) {
                list.push(result[y]);
                console.log("<<" + path.resolve(result[y]));
            }
        }
        return list;
    }

    config.getOutputPath = function (sourceFile) {
        let folder = path.dirname(sourceFile);
        let baseName = path.basename(sourceFile, path.extname(sourceFile));
        return path.resolve(path.join(folder, (baseName + ".js")));
    }

    me.log = function () {
        console.log(config);
        console.log("====================");
        console.log("");
    }
    //me.log();

    return config;
}

// ========== Entry Point ========== //

var config = readConfigurationFile();
for (var x = 0; x < config.sourceFiles.length; x++) {
    var batch = config.sourceFiles[x];

    if (batch.outputFile) {
        var options = clone(config);
        options.compilerOptions.outFile = config.getOutputPath(path.normalize(batch.outputFile));

        compileTs(
            config.getFileList(batch),
            config.getOutputPath(options.compilerOptions.outFile),
            options,
            true
        );
    }
    else {
        var fileList = config.getFileList(batch);
        for (var y = 0; y < fileList.length; y++) {
            var sourceFile = fileList[y];
            compileTs(
                [sourceFile],
                config.getOutputPath(sourceFile),
                clone(config),
                false
            );
        }
    }
}