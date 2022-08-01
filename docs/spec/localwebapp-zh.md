本文档描述框架执行逻辑；如需了解使用方式，请查阅[说明文档](../localwebapp/)。

# Local Web App

宿主应用通常都会从其 AppData 中加载 LWA 资源包。

## 文件结构

资源包中包含前端文件和资源清单。

宿主程序需要先解析清单，然后完成初始化运行环境，随后通过导航至主页的方式，完成 LWA 的加载。默认情况下，主页为前端根目录的 `index.html`，但也可以在资源清单中进行配置修改。

出于安全考量，部分前端文件需要进行签名校验，具体来说，包括以下文件类型。

- `.html` & `.htm` （静态网页）
- `.js` & `.ts` （脚本）
- `.css` （样式）

签名的结果会被集中存放在前端根目录里的 `localwebapp.files.json` 文件中，且签名是针对每个文件分布单独进行的。改结果文件为 JSON 格式，包含以下字段。

- `files`* _JSON array_: 签名情况集合。（数组中每一项均为一个 JSON 对象。）
  - `src`* _string_：被签名文件的相对路径。
  - `sign`* _string_：签名哈希结果。

支持的签名算法包括以下这些。

- `RS512`（默认）
- `RS384`
- `RS256`（推荐）

资源清单 `localwebapp.json` 包含以下属性。其中的相对路径，均基于前端根目录。

- `id`* _string_：前端资源包 ID。如同 `package.json` 中的 `id` 属性。
- `title`* _string_：名称。
- `version`* _string_：前端版本号。采用 SemVer（x.y.z）格式。
- `icon`* _string_：图标相对路径。
- `description` _string_：简介。
- `publisher` _string_：发布商名称。
- `copyright`* _string_：版权信息。
- `website` _string_：产品的官网地址。
- `entry` _string_：首页相对地址（如果需要修改的话）。
- `dataRes` _JSON object_：用于绑定的 JSON 字典，其每一项 value 需要是一个 JSON 文件相对地址。
- `strRes` _JSON object_：用于绑定的文本字典，其每一项 value 需要是一个文本文件相对地址。
- `host` _JSON array_：如果设定，那么用于指定哪些宿主程序可以加载本前端包；如果设为空，或者无此属性，即无任何限定。
- `tags` _JSON array_：标签。（其中每一项需要是一个字符串。）
- `meta` _JSON object_：额外的信息。

## 首次运行

首次运行时，宿主程序需要确保前端资源包已被初始化好。

为了确保这一点，前端资源包应该在宿主程序编译之时，即以内容形态被打包进宿主，该前端资源包即为初始包，用于在首次启动时被引用，并被解压到 AppData 中对应位置。

虽然说，大多数情况下一个宿主程序只会有一个前端资源包，但我们依旧会在 AppData 中，为每个前端资源包都设定其拥有专属独立文件夹。前端资源会被放置于其中的一个子文件夹内，该子文件夹会以版本号和前缀 `v` 命名（例如 `v1.0.0`）。除此之外，该专属独立文件夹还会包含以下子文件夹。

- `data`：用于作为该 LWA 的 AppData。请注意，该文件夹并非宿主程序的 AppData，而是为方便业务层存储任何文件而模拟出的文件夹。
- `cache`：用于宿主程序为该 LWA 缓存一些临时或永久文件，以及存放设置项的地方。

所以，纵观全局，AppData 相关文件结构可能如下，以 Contoso 公司发布的 app1 的 1.0.0 版为例。

- `appdata\localwebapp` AppData 中存储 LWA 相关能力的根目录
  - `contoso_app1` 具体其中一个 LWA 的根目录（以其 ID 格式化后命名）
    - `data` LWA 因业务需要存放数据文件的专有根目录
    - `cache`
      - `settings.json` JSON 格式的设置文件以存储安装信息和其它设置信息
      - `*.*` 其它运行时或安装（包括更新）时所用文件
    - `v1.0.0` 应用所在目录（以前缀 `v` 加版本号命名）
      - `localwebapp.json` 前端资源清单文件
      - `localwebapp.files.json` 签名文件
      - `index.html` 主页
      - `*.js`/`*.css`/`*.webp`/`*.svg`/`*.*` 脚本和资源文件
  - `contoso_app2` 另一个 LWA（如果有）
    - `data` & `cache` & `v1.0.0` 同上

宿主程序在每次加载前端资源包时，需要先检查宿主程序的 AppData 相关目录，以确定是否需要进行初始化。如果没有对应的 LWA 专属文件夹，那么即意味着需要完成初始化，具体如下。

1. 宿主程序将内置初始包解压到该 LWA 的 `cache` 文件夹内的某一特定临时文件夹内。请注意，压缩包内，是直接包含内含文件和子文件夹的，并没有一个额外的父容器文件夹包括。
2. 校验资源清单和相关文件。校验资源清单指的是比对其内资源包 ID 是否符合预期；校验相关文件指的是，对签名文件进行签名校验。如果校验失败，则需立即做清理。
3. 将该临时文件夹移动到上一级，并将文件夹名重命名为前缀 `v` 加版本号。同时，将当前版本信息写入 `cache` 文件夹内的 `settings.json` 配置文件中的 `version` 属性中。
4. 随后即可从 AppData 中运行该 LWA。

## 运行

宿主程序通常从 AppData 中运行 LWA。

1. 解析 `cache` 文件夹中的 `settings.json` 文件的 `version` 属性，以获得当前版本。
2. 根据版本号拼出应用所在路径，并解析其中 `localwebapp.json` 资源清单文件。
3. 校验资源清单和相关文件签名。
4. 加载资源清单中 `dataRes` 属性和 `strRes` 属性中所列各文件。
5. 创建 Web View（即 Microsoft Edge WebView2）并将应用所在路径设定为 Virtual Host。
6. 导航至主页，并绑定原生扩展。

默认 Virtual Host 如下。

- `{sub-id}.{org-id}.localwebapp.localhost`：适用于前端资源包 ID 包含组织信息时，即该 ID 包含斜线。这其中，`{org-id}` 表示组织 ID，但不包含 `@` 前缀；`{sub-id}` 表示包 ID。例如，当 ID 为 `@contoso/sample`、`contoso/sample` 或 `contoso/sample/xxx` 时，其 Virtual Host 均为 `sample.contoso.localwebapp.localhost`。
- `{package-id}.localwebapp.localhost`：适用于前端资源包 ID 仅为单一包 ID，即不包含斜线时。其中，`{package-id}` 表示该 ID。例如，当 ID 为 `sample` 时，其 Virtual Host 为 `sample.localwebapp.localhost`。

## 自动更新

LWA 运行后，随即检查更新。

更新策略可配置为 JSON 格式，包含以下属性。

- `url` _string_：检查更新的服务 URL。如果未配置或为空，则表示不采用标准方式进行更新。
- `params` _JSON object_：额外的动态参数。如有配置，会被用于作为参数拼接至 `url` 中。其键名为 URL 参数名，其值为其中一项字符串，包括 `package-id`（前端包 ID）、`package-version`（前端包版本）、`host-id`（宿主 ID）、`host-additional`（宿主附加字符串型信息）、`host-version`（宿主版本）、`fx-kind`（框架类型）、`fx-version`（框架版本）、`guid`（随机 GUID）。
- `prop` _string_：返回值所含更新信息所在属性。如果包含此值，则接口返回值预期为 JSON 对象，且更新信息仅位于其中一特定字段内，该字段的名称即为此值；如果为空，或未设定，则预期返回结果本身即为该信息，或位于 `data` 属性内。
- `settings` _JSON object_：用于给自定义更新方式的额外参数信息。不适用于标准方式。

宿主程序根据上述 `url` 和 `params` 属性拼接完整的请求 URL，接口返回后，根据 `prop` 字段取到对应更新信息结果。

更新信息结果应该是 JSON 对象格式，但也有可能会是 JSON 数组。如果是数组，那么遍历该数组，如其中项为 JSON 对象，且含有属性 `id`，其值与前端包 ID 一致，那么该对象才是真正的更新信息结果。否则，当下无更新。

然后，检查更新信息结果里的 `version` 属性，如无，则检查 `latestVersion` 属性，其值为线上最新版本，用改版本与当前所装前端包版本比较，以确定是否有更新。不过，更新信息结果里如果有额外的 `force` 字段，则当该字段为 `true` 时，即意味着这是以此强制更新，因此可以不用进行前述比较，即认为有更新。

如果更新存在，则通过更新信息结果里的 `url` 属性所指向的下载地址，以及可能存在的标明动态参数的 `params` 属性，来进行新包的下载。需下载至该前端包对应的 `cache` 文件夹内。随后的流程，与首次运行的解压、校验、更新的流程一致。

完成后向 LWA 发送更新可用通知，并会随机制在下次启动时自动生效。

## 开发环境

开发环境下的 LWA 前端文件直接存储于本地文件系统中的一个文件夹内，并且会有一个项目配置文件对其进行描述。运行时，宿主程序应当开放调试环境。

项目配置文件名为 `localwebapp.project.json`，会位于前端项目目录的根目录中，或其 `localwebapp` 子目录中。因此，当宿主程序加载某一文件夹时，需在这两处去找寻并解析该项目配置文件。该项目配置文件为 JSON 结构，包含以下属性，其所在目录即为环境目录，以下相对路径均基于该环境目录。

- `id` _string_：前端资源包 ID。它也可能存在于 `package` 属性中，而非此。
- `package` _JSON object_：前端资源包的所有配置信息，和 `localwebapp.json` 里的内容一致，但其中可能不存在 `id` 属性，如果不存在，那么父节点必须有该属性。
- `ref` _JSON object_：用于供宿主加载 LWA 的额外信息。
- `dev` _JSON object_：调试环境加载时的相关信息。

其中，`ref` 属性中的子属性如下。

- `update` _JSON object_：升级配置项，包括升级地址和参数等信息。
- `path` _string_：如果应用所在目录并非加载的路径，那么可以通过此属性来指定实际位置，可以是相对路径或绝对路径。
- `sign` _string_：签名算法名称。
- `key` _string_：签名校验用的公钥。这个属性也可以为空或不存在，但需要在环境目录中有 `localwebapp.pem` 文件，其内包含该公钥。
- `output` _JSON array_：可以指定一批额外输出地址，其内每项需为 JSON 对象，内含 `zip` 属性，为需要输出的压缩包路径，以及 `config` 属性，为本项目配置文件的输出路径。

在环境目录中，还需包含一个 `localwebapp.private.pem` 文件，其内容为签名私钥，宿主程序需要以此创建对相关文件进行签名。

以下是具体的包生成步骤。

1. 解析 `localwebapp.project.json` 文件。
2. 通过 `ref.path` 属性找到应用所在目录；如果无此字段，或其为空，则为宿主加载目录。
3. 在应用所在目录中创建 `localwebapp.json` 文件，其内容即为 `package` 属性的内容，如其中没有 `id` 属性，则还需整合 `id` 属性。
4. 读取 `localwebapp.private.pem` 文件内容，并使用 `ref.sign` 属性所描述的签名算法，对相关文件进行签名，将签名结果按格式写入应用所在目录重点 `localwebapp.files.json` 中。
5. 压缩应用所在目录下的所有文件和子文件夹，存储为环境目录下 `localwebapp.zip` 文件。
6. 如有 `output` 属性设定，则按其描述将压缩包和项目配置文件复制到对应位置。

当需要加载一个开发包时，先完成上述生成过程，然后直接从应用所在目录进行加载，而不再通过 AppData 相关目录。其 `data` 文件夹和 `cache` 文件夹则需创建于环境目录中（这两个文件夹最好被添加至 `.gitignore` 中）。这样可以确保开发环境和生产环境的隔离。

## 原生扩展

- 所有组件都绑定在 `window.localWebApp` 对象上。
- 原生功能均返回 Promise/A 形态结果。

具体接口可参考 [Type Script 定义文件](https://raw.githubusercontent.com/nuscien/winkit/main/FileBrowser/src/localWebApp.d.ts)。
