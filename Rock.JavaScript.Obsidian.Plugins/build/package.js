const fs = require("fs");
const path = require("path");
const process = require("process");
const glob = require("glob");
const archiver = require("archiver");
const semver = require("semver");

/**
 * @typedef PluginData
 *
 * @property {string} name The name of the plugin.
 * @property {string} version The version number of the plugin to build.
 * @property {string[]} include The patterns of files to be included.
 * @property {string[]} exclude The patterns of files to be excluded from the included patterns.
 * @property {string[]} delete The list of additional files to be deleted in this version.
 */

/**
 * @typedef PluginLockVersion
 *
 * @property {string} version The version number of the release.
 * @property {string[]} installedFiles[] The paths to files that were installed.
 * @property {string[]} deletedFiles[] The paths to files that were deleted.
 */

/**
 * @typedef PluginLockData
 *
 * @property {string} lockFileVersion The version of the lock file.
 * @property {PluginLockVersion[]} versions The versions that have been published.
 */

// Get the standard directories we will be using later.
const pluginSourceDirectory = path.join(__dirname, "..", "src");
const solutionDirectory = path.join(__dirname, "..", "..");
const rockWebDirectory = path.join(solutionDirectory, "RockWeb");

/**
 * Finds files that match the pattern.
 *
 * @param {string} pattern The pattern to use when searching for files.
 *
 * @returns An array of matching paths and filenames.
 */
async function globAsync(pattern) {
    return new Promise((resolve, reject) => {
        glob(pattern, (error, matches) => {
            if (error) {
                reject(error);
            }
            else {
                resolve(matches);
            }
        });
    });
}

/**
 * Process all the plugin files one at a time.
 *
 * @param {string[]} pluginFiles The array of paths and filenames to the plugin files.
 */
async function processPlugins(pluginFiles) {
    for (const filename of pluginFiles) {
        try {
            await processPlugin(filename);
        }
        catch (e) {
            console.error(e);
        }
    }
}

/**
 * Processes a single plugin definition.
 *
 * @param {string} pluginSourceFile The path to the plugin definition file.
 */
async function processPlugin(pluginSourceFile) {
    const contents = await fs.promises.readFile(pluginSourceFile);
    /** @type PluginData */
    const plugin = JSON.parse(contents);

    /** @type PluginLockData | undefined */
    let pluginLock;
    const lockFilePath = path.join(path.dirname(pluginSourceFile), "plugin-lock.json");

    console.log(`Processing plugin ${plugin.name}`);

    // Load any existing published data.
    if (fs.existsSync(lockFilePath)) {
        pluginLock = JSON.parse(await fs.promises.readFile(lockFilePath));

        if (pluginLock.lockFileVersion !== 1) {
            throw new Error("This tool only understands version 1 lock files.");
        }

        if (pluginLock.versions.some(v => semver.gt(v.version, plugin.version))) {
            throw new Error("Lock file contains a later release, unable to package plugin.");
        }

        // Remove the version we are going to publish if it is already there.
        pluginLock.versions = pluginLock.versions.filter(v => v.version !== plugin.version);
    }
    else {
        pluginLock = {
            lockFileVersion: 1,
            versions: []
        };
    }

    // Get all the files to be included or deleted.
    const includedFiles = await getIncludedFiles(plugin);
    const deletedFiles = await getDeletedFiles(plugin);
    const uninstallFiles = getInstalledFiles(pluginLock,
        includedFiles.map(f => path.relative(rockWebDirectory, f)),
        deletedFiles.map(f => path.relative(rockWebDirectory, f)));

    const pluginSlugName = plugin.name.toLowerCase().replace(/[^a-zA-Z0-0_-]/, "-");
    const pluginFilename = `${pluginSlugName}-${plugin.version}.plugin`;
    const archivePath = path.join(path.dirname(pluginSourceFile), pluginFilename);

    writePluginArchive(archivePath, includedFiles, deletedFiles, uninstallFiles);

    // Update the lock file.
    addCurrentReleaseToLock(pluginLock,
        plugin.version,
        includedFiles.map(f => path.relative(rockWebDirectory, f)),
        deletedFiles.map(f => path.relative(rockWebDirectory, f)));
    await fs.promises.writeFile(lockFilePath, JSON.stringify(pluginLock, undefined, 2));

    console.log(`Created ${path.relative(process.cwd(), archivePath)}`);
    console.log("");
}

/**
 * Gets the files that should be included in the archive.
 *
 * @param {PluginData} plugin The plugin details.
 *
 * @returns {Promise<string[]>} The files to be included in the archive.
 */
async function getIncludedFiles(plugin) {
    const includedFiles = [];

    // Include all specifies files.
    if (plugin.include && Array.isArray(plugin.include)) {
        for (const includePath of plugin.include) {
            const files = await globAsync(path.join(rockWebDirectory, includePath));

            if (files.length === 0) {
                console.error(`Unable to locate any files matching pattern '${includePath}'.`);
                process.exit(-1);
            }
            for (const file of files) {
                includedFiles.push(file);
            }
        }
    }

    // Remove any files from the list that are excluded.
    if (plugin.exclude && Array.isArray(plugin.exclude)) {
        for (const excludePath of plugin.exclude) {
            const files = await globAsync(path.join(rockWebDirectory, excludePath));
            for (const file of files) {

                while (includedFiles.indexOf(file) !== -1) {
                    includedFiles.splice(includedFiles.indexOf(file), 1);
                }
            }
        }
    }

    // Make sure we only have unique filenames.
    return [...new Set(includedFiles)];
}

/**
 * Gets the files that should be marked for deletion.
 *
 * @param {PluginData} plugin The plugin details.
 *
 * @returns {Promise<string[]>} The files to be marked for deletion.
 */
async function getDeletedFiles(plugin) {
    const deletedFiles = [];

    // Check any explicitely marked files.
    if (plugin.delete && Array.isArray(plugin.delete)) {
        for (const deletePath of plugin.delete) {
            deletedFiles.push(path.join(rockWebDirectory, deletePath));
        }
    }

    // Make sure we only have unique filenames.
    return [...new Set(deletedFiles)];
}

/**
 * Returns the relative filename paths of any files installed by plugins up to
 * but not including the version of `plugin`.
 *
 * @param {PluginLockData | undefined} pluginLock The lock data for the plugin.
 * @param {string[] | undefined} includedFiles Additional files to be included after the lock file is processed.
 * @param {string[] | undefined} deletedFiles Additional files to be deleted after the lock file is processed.
 *
 * @returns {string[]} An array of relative filename paths.
 */
function getInstalledFiles(pluginLock, includedFiles, deletedFiles) {
    const versions = [...(pluginLock ? pluginLock.versions : [])];

    // Sort it in order of version number, lowest to highest.
    versions.sort((a, b) => semver.compare(a.version, b.version));

    // Include the current version files.
    if (includedFiles || deletedFiles) {
        versions.push({
            installedFiles: includedFiles.map(f => f.replace(/\\/g, "/")),
            deletedFiles: deletedFiles.map(f => f.replace(/\\/g, "/"))
        });
    }

    const installedFiles = [];

    for (const version of versions) {
        // Check for files that were deleted in this version.
        if (version.deletedFiles) {
            for (const file of version.deletedFiles) {
                const idx = installedFiles.indexOf(file);

                if (idx !== -1) {
                    installedFiles.splice(idx, 1);
                }
            }
        }

        // Check for files that were installed in this version.
        if (version.installedFiles) {
            for (const file of version.installedFiles) {
                if (!installedFiles.includes(file)) {
                    installedFiles.push(file);
                }
            }
        }
    }

    return installedFiles;
}

/**
 * Writes the plugin archive.
 *
 * @param {string} archivePath The full path and filename to the archive file to be created.
 * @param {string[]} includedFiles The array of files to be included in the archive.
 * @param {string[]} deletedFiles The array of files to be marked for deletion.
 * @param {string[]} uninstallFiles The array of files that represent every file to be removed during an uninstall.
 */
function writePluginArchive(archivePath, includedFiles, deletedFiles, uninstallFiles) {
    const archiveOutput = fs.createWriteStream(archivePath);
    const archive = archiver("zip", {
        zlib: {
            level: 9
        },
        forceLocalTime: true
    });
    archive.pipe(archiveOutput);

    for (const filename of includedFiles) {
        const fileStream = fs.createReadStream(filename);
        const date = fs.statSync(filename).mtime;

        archive.append(fileStream, {
            name: path.join("content", path.relative(rockWebDirectory, filename)),
            date
        });
    }

    if (deletedFiles.length > 0) {
        // delete file list should be in path\\segment\\file.txt format, with
        // backslashes instead of forward slashes.
        const deleteFileListContent = deletedFiles.map(f => path.relative(rockWebDirectory, f).replace(/\//g, "\\")).join("\r\n");

        archive.append(deleteFileListContent, {
            name: "install\\deletefile.lst"
        });
    }

    if (uninstallFiles.length > 0) {
        // delete file list should be in path\\segment\\file.txt format, with
        // backslashes instead of forward slashes.
        const deleteFileListContent = uninstallFiles.map(f => f.replace(/\//g, "\\")).join("\r\n");

        archive.append(deleteFileListContent, {
            name: "uninstall\\deletefile.lst"
        });
    }

    archive.finalize();
}

/**
 * Adds a new release version to the lock file data.
 *
 * @param {PluginLockData} pluginLock The current lock data to be modified.
 * @param {string} version The version number of the release to be added.
 * @param {string[]} installedFiles The files that were installed with this release.
 * @param {string[]} deletedFiles The files that were removed with this release.
 */
function addCurrentReleaseToLock(pluginLock, version, installedFiles, deletedFiles) {
    if (!pluginLock.versions) {
        pluginLock.versions = [];
    }

    pluginLock.versions.push({
        version,
        installedFiles: installedFiles.map(f => f.replace(/\\/g, "/")),
        deletedFiles: deletedFiles.map(f => f.replace(/\\/g, "/"))
    });

    pluginLock.versions.sort((a, b) => semver.compare(a.version, b.version));
}

if (process.argv.length > 2) {
    processPlugin(process.argv[2]);
}
else {
    glob(path.join(pluginSourceDirectory, "**", "plugin.json"), (error, matches) => {
        if (error) {
            console.error(error);
        }
        else {
            processPlugins(matches);
        }
    });
}
