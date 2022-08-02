# Local Web App

Local web app is a new way to run your web app locally.

You can develop an web SPA as the same way you always do. Then package it and publish online. The host app will check update for your package automatically.

Your web app runs on localhost in Microsoft Edge WebView2. The environment of runtime and debug mode is like any other website runs on a browser.

## App model

The app is the combination of host app and resource package.

### Host app

The host app is the container and the runtime of the business logic implementation. It is a native app running on each platform supported, e.g., UWP, Win32, macOS.

The host app implementation is transparent to developers. Developers need configurate the project settings (and store settings if applied) and do build for each platform target, but they do NOT need to implement them directly because they are a part of the app framework solution.

The host app creates and actives a main window with a Microsoft Edge WebView2 element to host a local webpage. The webpage imports the JavaScript files and style sheets embedded in the resource package. In most cases, developers need only to implement (or output) these web resources as same as front-end web development.

### Resource package

Resource package is all of front-end assets runs in the host app. It includes the business logic and UX shell for the app. It is what the developers need to focus/work on. They develop its content so that the host app can load them to show to the end users.

The resource package contains a manifest with the key information and metadata of the app.

The best practice about the development experience is to develop an SPA instead of tranditional mode. Developers do never consider the size of the script file because it is loaded from local without network transfer cost during runtime.

## Update

Often, app just need to update its resource package but not the whole host app. In the app framework solution, there is a built-in auto update method to check for each resource package. The native developers can also prevent this mechanism to enable their customized one.

### Default auto update

The host app enables auto update by default. It will check update on the local web app is running and then download the latest package if available.

The host app will send a notification message to webpage after download completed. And JS can also check the update information via native API at anytime. So JS can determine the rest actions if need.

The host app will load the newer version on the next startup.

The app maintainer may need host an online update service or join in a supported app store so that the host app can connect to check and download the update for the resource package. The host app will send a HTTPS request to the web API configured. It can include the identifier and version information in URL query as needed. The web API should return a JSON response with the latest version and download URL if the update is available.

### Customized update

Native app developers can disable the default auto update mechanism and implement the customized one. If so, the framework will not check update automatically and the new logic should handle the way and timing about it.

Following are some of appropriate scenario to customize the update logic.

- The host app is a open hub to host 1st-part or 3rd-party resource packages.
- The framework is a part of another project and is visible on-demand by its business.
- The update service is designed by a particular way that is not compatible with the default mode.

## Native extended capabilities

Host app provide JS bridge to extend native APIs to webpage. All components are on `window.localWebApp` object. Most of the method/function are invoked asynchronously so their return results are encapsulated by Promise/A.

### Built-in extensions and datas

It includes following extended APIs.

- File APIs to access directories, files and download list.
- Cryptography API about symmetric algorithm and hash.
- Functions to encode/decode text.
- Host APIs to get theme, check update and control window.

It also bind following data.

- All the information about the environment and metadata.
- The binding data loaded from the specific file contents. These are configured in manifest.

See [introduction](./native-api) or [Type Script definition file](https://raw.githubusercontent.com/nuscien/winkit/main/FileBrowser/src/localWebApp.d.ts) for API details.

### Customized extensions

Native app developers can implement the command handler for customized extensions. It is based on message communication between host app and webpage. JS can get the command handler by its identifier and it returns a proxy of the native implementation to send request and get response. It would be great if JS developers add a corresponding JS API based on the command handler to export them as a set of JS accessing friendly functions.

See [introduction](./command-handler) for native implementation details.

## Engineering supports

The same development and debug experience with H5.
