[Local Web App](../localwebapp)

# 原生扩展

宿主程序通过 JS 桥接模式，来向前端 Web 页面提供原生扩展能力。所有相关功能，都附加在 `window.localWebApp` 对象之上。大多数函数或方法以 Promise/A 异步返回值形式提供。

## 内置 API

通过内置扩展，可以拥有访问本地资源和其它能力的权限和方法。

- `files` 对本地目录和文件，以及下载列表的访问能力。
- `cryptography` 对称加密、哈希、签名、验证。
- `text` 对文本的编解码。
- `hostApp` 对宿主的访问，例如获取主题、检查前端资源包更新、控制窗口状态。

可查看其 [Type Script 定义文件](https://raw.githubusercontent.com/nuscien/winkit/main/FileBrowser/src/localWebApp.d.ts)来获取详情。

## 数据

可以设定预设的数据资源绑定。

- `hostInfo` 于环境和元数据有关的各类信息。
- `dataRes` & `strRes` 经前端资源清单预设绑定的 JSON 和文本资源。

## 自定义扩展

The function `getCommandHandler` is to get the proxy of command handler native implementation. It passes an identifier and returns an object with methods to send request and get response.

JS also can enumerate the information of all command handler by `hostApp.handlers` method.
