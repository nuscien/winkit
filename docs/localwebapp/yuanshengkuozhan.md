[Local Web App](../localwebapp/shuoming)

# 原生扩展

宿主程序通过 JS 桥接模式，来向前端 Web 页面提供原生扩展能力。所有相关功能，都附加在 `window.localWebApp` 对象之上。大多数函数或方法以 Promise/A 异步返回值形式提供。

## 内置 API

通过内置扩展，可以拥有访问本地资源和其它能力的权限和方法。

- `files` 对本地目录和文件，以及下载列表的访问能力。
- `cryptography` 对称加密、RSA、哈希、签名、验证。
- `text` 对文本的编解码。
- `hostApp` 对宿主的访问，例如获取主题、检查前端资源包更新、控制窗口状态。

可查看其 [Type Script 定义文件](https://raw.githubusercontent.com/nuscien/winkit/main/FileBrowser/src/localWebApp.d.ts)来获取详情。

## 数据

可以设定预设的数据资源绑定。

- `hostInfo` 于环境和元数据有关的各类信息。
- `dataRes` & `strRes` 经前端资源清单预设绑定的 JSON 和文本资源。

## 自定义扩展

原生应用开发者可以实现自定义的命令处理程序，以对前端页面的原生访问能力或其它功能提供支持。其基于内部消息机制，来实现宿主程序和前端页面的通信。JS 部分可以通过扩展实现者注册的 ID，来获取该命令集合，该命令集合通过暴露访问方法，来实现与宿主程序的通信。这些方法是内置的，JS 开发也可以基于这些方法进行进一步封装，以提供更为友好的 API。

函数 `getCommandHandler` 可以获取原生程序开发者提供的 API 集合的 JS 层代理，其接受一个 ID 并返回包含发送命令和获取数据的一个实例。

JS 也可以通过 `hostApp.handlers` 方法遍历已注册的所有代理。
