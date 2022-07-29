# Local Web App

Local web app is a new way to run your web app locally.

You can develop an web SPA as the same way you always do. Then package it and publish online. The host app will check update for your package automatically.

Your web app runs on localhost in Microsoft Edge WebView2. The environment of runtime and debug mode is like any other website runs on a browser.

## Runtime

The host app implementation is transparent to developers (except metadata and platform target).

The main window contains a Microsoft Edge WebView2 element to host a local webpage. The webpage imports the JavaScript files and style sheets embedded in the resource package.

## App model

The app is the combination of host app and resource package.

### Host app

The app is to run as native app on each supported platform, e.g., UWP, Win32, macOS. Developer need build different host app for each platform, but they do NOT need to implement because it is a part of the app solution. They may config the project settings (and store settings if apply) on these targets.

### Resource package

Includes the business logic and shell for the app. It is what the developers need to focus/work on. They develop its content so that the host app can load them to show to the end users.

The primary content is listed by a manifest. It contains the metadata of the app.

## Update

- The host app will send HTTPS request to the web API defined in package manifest to check update.
- The host app will download the new package automatically and send a message to the webpage to notify.
- The fallback effective time is on next start.

## Native extended capabilities

- All components is on `window.localWebApp` object.
- The return result from native is encapsulated by Promise/A.

See [Type Script definition file](https://raw.githubusercontent.com/nuscien/winkit/main/FileBrowser/src/localWebApp.d.ts) for API details.

## Engineering supports

The same development and debug experience with H5.
