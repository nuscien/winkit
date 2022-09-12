🌐 __English__ (en) | [简体中文 (zh-Hans)](../localwebapp/guige)

This spec is about the workflow the framework works. See [documentation](../localwebapp/) for usages.

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

The manifest `localwebapp.json` include following properties. The relative paths mentioned below are based on the root path of the package.

- `id`* (string) is the resource package identifier, like `id` property in `package.json`.
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

So overall, the directories may look like following, the example is about app1 version 1.0.0 produced by Contoso Company.

- `appdata\localwebapp` Root directory in appdata for all local web apps
  - `contoso_app1` For a local web app (named by its identifier formatted)
    - `data` Used like appdata for the local web app
    - `cache`
      - `settings.json` A JSON format file with installation info and others
      - `*.*` Other files for runtime and installation/update
    - `v1.0.0` App directory (named by version and prefix `v`)
      - `localwebapp.json` The manifest
      - `localwebapp.files.json` Signatures
      - `index.html` Homepage
      - `*.js`/`*.css`/`*.webp`/`*.svg`/`*.*` Script and resource files
  - `contoso_app2` Another local web app (if exists)
    - `data` & `cache` & `v1.0.0` Same as above

The host app should check appdata to determine if it has initialize. If no, initialize.

1. The host app extract it into a specific temp folder in its `cache` directory. Please note that the compress file contains files and sub-directories directly without an additional parent/container folder.
2. Verify manifest and files. The resource package identifier should be the specific one. And the files signatures should be correct. The host app should remove the package if it fails.
3. Move the temp package directory to the upper one and rename as prefix `v` and its version. Write the version value into `version` property in `settings.json` JSON format file in `cache` sub-directory.
4. Runs in appdata.

## Running

The host app always load the package from appdata.

1. Parse `settings.json` JSON format file in `cache` sub-directory to read `version` property to get the version of the local web app installed.
2. Parse the manifest `localwebapp.json` in the sub-directory named by `v` + version installed.
3. Verify the manifest and files signatures.
4. Load files introduced in `dataRes` property and `strRes` property in manifest if applied.
5. Create a web view (Microsoft Edge WebView2) and set up the virtual host of the app folder.
6. Navigate to the homepage and bind native extentions.

The default virtual host is in following format.

- `{sub-id}.{org-id}.localwebapp.localhost` if the resource package identifier contains an org identifier. That means, the resource package identifier contains a slash. The `{org-id}` is the org identifier (first part) without `@` prefix; the `{sub-id}` is the package identifier (second part). For example, `sample.contoso.localwebapp.localhost` for ID `@contoso/sample`, `contoso/sample` or `contoso/sample/xxx`.
- `{package-id}.localwebapp.localhost` if the resource package identifier is a single item without slash. The `{package-id}` is the resource package identifier. For example, `sample.localwebapp.localhost` for ID `sample`.

## Auto update

After the app is running, the host app should check update if available.

The update logic can be configured in JSON format and it includes following properties.

- `url` (string) is the update URL. If no such property, do not check update in standard mode.
- `params` (JSON object) additional dynamic parameters. The key will be used as query key appending to `url` and the value is one of `package-id` (resource package identifier), `package-version` (resource package version), `host-id` (host app identifier), `host-additional` (host app additional string value), `host-version` (host app version), `fx-kind` (framework kind), `fx-version` (framework version), `guid` (random GUID) strings.
- `prop` (string) is the optional property name of response. If set, it defines the update information is in which node of the response of web API.
- `settings` (JSON object) is the settings data used by customized update logic (not for standard mode).

The host app should build the update URL to get the update information of the app. The response of the web API will return a JSON format data. It may in the property of `prop` defined, or the root node or `data` property if that property is null or does not exist.

The information is a JSON object or a JSON array. If it is a JSON array with items in JSON object, find the right item which include `id` property with the value as same as the resource package, and the object is the information; otherwise, no update.

Then, check the `version` property (`latestVersion` property for fallback) to compare with current resource package version to determine if there is any update. The boolean value property `force` is used to set if this update is required to force to update without version comparing.

If update is available, download the new package by `url` property and `params` property. The package should be downloaded in the `cache` directory of the local web app in app data. Then follow the steps like OOBE to extract, verify and update.

Notify web app after completion. It will be the new one after restart automatically.

## Dev environment

Local web app in dev enviroment uses a project configuration file to manage its asset. The package are in a local path (a directory in local file system) to load. The host app should also enable the debug tools for this scenario.

The project config file `localwebapp.project.json` is in the sub-directory `localwebapp` or the root directory. So the host app should check both folders that if there is the project config file. If so, parse that file by JSON format. The folder with project config file is the env folder. The project config file contains following properties. All relative pathes referenced by the project config file are base on the env folder.

- `id` (string) is the identifier of the resource package. It will in property `package` if the property does not exist.
- `package`* (JSON object) is the settings and metadata of the local web app. It is as same as `localwebapp.json` but only it may have no propery `id` which is in root node.
- `ref` (JSON object) is the settings used by host app.
- `dev` (JSON object) is to config the parameters about debug mode.

The properties in property `ref` are following.

- `update` (JSON object) is the configuration of update method, including URL and parameters.
- `path` (string) is the relative/absolute path of app folder, if need customize. The default value for null or non-exist, is the directory which the host app is loading.
- `sign` (string) is the signature algorithm name.
- `key` (string) is the public key to verify the signature. The value can also be in `localwebapp.pem` file in the env folder, so the property can be null or non-existed.
- `output` (JSON array) is the output copied paths. Each item should be a JSON object with property `zip` for output compressed package file path and property `config` for project config file path.

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

Host app provide JS bridge to extend native APIs to webpage. All components are on `window.localWebApp` object in web view. Most of the method/function are invoked asynchronously so their return results are encapsulated by Promise/A.

It includes following extended APIs.

- `files` A set of native API to access directories, files and download list.
- `cryptography` A set of native API about symmetric algorithm and hash.
- `text` A set of functions to encode/decode text.
- `hostApp` A set of native API to get host theme, check update and control window.

It also includes following extended properties.

- `hostInfo` All the information about the environment and metadata.
- `dataRes` & `strRes` The binding data from the specific file contents configured in manifest.

Host app also support customized extends. It contains a JS function `getCommandHandler` to get the proxy of command handler native implementation. It passes an identifier and returns an object with methods to send request and get response. Native app developers need implement the interface and register the instance into the window or page to apply.

See [Type Script definition file](https://raw.githubusercontent.com/nuscien/winkit/main/FileBrowser/src/localWebApp.d.ts) for API details.
