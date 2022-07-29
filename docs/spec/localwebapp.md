# Local Web App

Host app should load the resource package from appdata to show the local web app to end-users.

## File architecture

The package include content files (e.g. `.html`, `.js`, `.css`) and manifest.

The host app will parse the manifest to initialize the additional runtime environment to load the homepage to show. The default homepage is `index.html` in the root directory of the package but it can be modified in the manifest.

Some content files are need to sign for security. Following are their types.

- `.html` & `.htm` (static webpage)
- `.js` & `.ts` (script)
- `.css` (style sheet)

The signature result is in the file `localwebapp.files.json`. The file is in JSON format like following.

- `files`* (JSON array) is the collection of each file with its signature value. Each item is a JSON object.
  - `src`* (string) relative path of the file to sign.
  - `sign`* (string) The hash value of signature.

Only supports following signature algorithms.

- `RS512` (default)
- `RS384`
- `RS256`

The manifest include following properties. The relative paths mentioned below are based on the root path of the package.

- `id`* (string) is the resource package identifier.
- `title`* (string) is the display name.
- `version`* (string) is the version of the package identifier in `x.y.z` SemVer format.
- `icon`* (string) is the relative path of icon or square logo.
- `description` (string) is an introduction.
- `publisher` (string) is the publisher name.
- `copyright`* (string) is the copyright information.
- `website` (string) is the official website URL of this app.
- `entry` (string) is the relative path of the customized homepage.
- `dataRes` (JSON object) is a dictionary of the data resource used to import into the app during runtime. The value in the dictionary should be the relative path of JSON files in the package.
- `strRes` (JSON object) is a dictionary of the data resource used to import into the app during runtime. The value in the dictionary should be the relative path of plain text files in the package.
- `host` (JSON array) is an optional collection described which host app can load this resource package, if set; or no limitation, if the value is null, or the property does not exist.
- `tags` (JSON array) is the tags. Each of item should be a string.
- `meta` (JSON object) is the additional metadata.

## OOBE

On the first/initial launch, the host app need to setup the environment including extracts the package to run.

The package resource should be added as content in host app so it can load it immedicately to extract to appdata after installing.

Each package should has its own folder in appdata directory, although most of host app are designed has only one resource package. Its own folder contains the app itself as app folder in a sub-directory named by its version and prefix `v` (such as `v1.0.0`). And also contains following sub-directories.

- `data`, its own appdata used by local web app. Note that it is NOT the appdata of the host app.
- `cache`, cache and settings used by host app.

The host app should check appdata to determine if it has initialize. If no, initialize.

1. The host app extract it into a specific temp folder in its `cache` directory. Please note that the compress file contains files and sub-directories directly without an additional parent/container folder.
2. Verify manifest and files. The resource package identifier should be the specific one. And the files signatures should be correct. The host app should remove the package if it fails.
3. Move the temp package directory to the upper one and rename as prefix `v` and its version. Write the version value somewhere into `settings.json` in `cache` sub-directory.
4. Runs in appdata.

## Running

The host app always load the package from appdata.

1. Parse `settings.json` in `cache` sub-directory to get the version of the local web app installed.
2. Parse the manifest `localwebapp.json` in the sub-directory named by `v` + version installed.
3. Verify the manifest and files signatures.
4. Load files introduced in `dataRes` property and `strRes` property in manifest if applied.
5. Create a web view (Microsoft Edge WebView2) and set up the virtual host of the app folder.
6. Navigate to the homepage and bind native extentions.

## Dev environment

Local web app in dev enviroment uses a project configuration file to manage its asset. The package are in a local path (a directory in local file system) to load. The host app should also enable the debug tools for this scenario.

The project config file `localwebapp.project.json` is in the sub-directory `localwebapp` or the root directory. So the host app should check both folders that if there is the project config file. If so, parse that file by JSON format. The folder with project config file is the env folder. The project config file contains following properties. All relative pathes referenced by the project config file are base on the env folder.

- `id` (string) is the identifier of the resource package. It will in property `package` if the property does not exist.
- `package`* (JSON object) is the settings and metadata of the local web app. It is as same as `localwebapp.json` but only it may have no propery `id` which is in root node.
- `ref` (JSON object) is the settings used by host app.
  - `update` (JSON object) is the configuration of update method, including URL and parameters.
  - `path` (string) is the relative/absolute path of app folder, if need customize. The default value for null or non-exist, is the directory which the host app is loading.
  - `sign` (string) is the signature algorithm name.
  - `key` (string) is the public key to verify the signature. The value can also be in `localwebapp.pem` file in the env folder, so the property can be null or non-existed.
  - `output` (JSON array) is the output copied paths. Each item should be a JSON object with property `zip` for output compressed package file path and property `config` for project config file path.
- `dev` (JSON object) is to config the parameters about debug mode.

The env folder should also contains a `localwebapp.private.pem` with private key of file signature so the host app can sign the files.

Following is the way to build the package.

1. Parse `localwebapp.project.json`.
2. Find the app folder by `ref.path` property in the project config file; if it does not exist or its value is null, the app folder is the directory that the host app is loading.
3. Generate `localwebapp.json` in app folder. The content is from `package` property and `id` property in the project config file.
4. Sign the files with algorithm name described in `ref.sign` property in the project config file, and private key in `localwebapp.private.pem`. Generate `localwebapp.files.json` in app folder to save the signature inforamtion.
5. Compress the files and sub-directories in app folder to `localwebapp.zip` in env folder.
6. Copy the compressed file and project config file to the output paths if applied.

When load a dev package, the host app should package it firstly, and then load from app folder directory instead of appdata of host app. The `data` folder and `cache` folder should be made in env folder (and they should be ignored in git). This can make sure the environments of dev mode and prod mode are separated.

## Native extended capabilities

- All components is on `window.localWebApp` object.
- The return result from native is encapsulated by Promise/A.