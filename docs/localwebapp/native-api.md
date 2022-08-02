[Local Web App](../localwebapp)

# Native extended capabilities

Host app provide JS bridge to extend native APIs to webpage. All components are on `window.localWebApp` object. Most of the method/function are invoked asynchronously so their return results are encapsulated by Promise/A.

See [Type Script definition file](https://raw.githubusercontent.com/nuscien/winkit/main/FileBrowser/src/localWebApp.d.ts) for API details. Following are the summary.

## Native APIs

It includes following extended APIs.

- `files` A set of native API to access directories, files and download list.
- `cryptography` A set of native API about symmetric algorithm and hash.
- `text` A set of functions to encode/decode text.
- `hostApp` A set of native API to get host theme, check update and control window.

## Data

It also includes following extended properties.

- `hostInfo` All the information about the environment and metadata.
- `dataRes` & `strRes` The binding data from the specific file contents configured in manifest.

## Customized extensions

The function `getCommandHandler` is to get the proxy of command handler native implementation. It passes an identifier and returns an object with methods to send request and get response.

JS also can enumerate the information of all command handler by `hostApp.handlers` method.
