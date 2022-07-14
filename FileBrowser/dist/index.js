/** File browser demo - WinKit */
/// <reference path="./localWebApp.d.ts" />
/// <reference path="./common.ts" />
var FileBrowserDemo;
(function (FileBrowserDemo) {
    FileBrowserDemo.settings = {
        iconPath: "./images/icons/"
    };
    function renderItem(item, dom, callback) {
        if (!item)
            return null;
        if (!dom)
            dom = document.createElement("div");
        dom.innerHTML = "";
        if (typeof callback === "function")
            dom.addEventListener("click", function (ev) {
                callback(item);
            });
        if (item.type === "dir") {
            dom.className = "link-tile-folder";
            dom.appendChild(createImageElement(FileBrowserDemo.settings.iconPath + "Folder.png", item.name));
            dom.appendChild(createSpanElement(item.name, { title: true }));
            return "dir";
        }
        dom.className = "link-long-file";
        dom.appendChild(createImageElement(FileBrowserDemo.settings.iconPath + getIconName(item.name) + ".png", item.name));
        dom.appendChild(createSpanElement(item.name, { title: true }));
        if (item.modified) {
            var modified = new Date(item.modified);
            dom.appendChild(createSpanElement(modified.toLocaleString()));
        }
        dom.appendChild(createSpanElement(getFileLengthStr(item.length)));
        return "file";
    }
    FileBrowserDemo.renderItem = renderItem;
    function renderItems(col, dom, callback) {
        if (!col)
            return 0;
        if (!dom)
            dom = document.createElement("div");
        dom.innerHTML = "";
        var i = 0;
        col.forEach(function (file) {
            var ele = document.createElement("a");
            if (!renderItem(file, ele, callback))
                return;
            dom.appendChild(ele);
            i++;
        });
        return i;
    }
    FileBrowserDemo.renderItems = renderItems;
    function renderFolder(path, dom) {
        var promise = localWebApp.files.list(path);
        promise.then(function (r) {
            if (!r.data)
                return;
            renderDetails(r.data.info, r.data.dirs, r.data.files, r.data.parent, dom, function (item) {
                if (!item)
                    return null;
                if (item == r.data.parent) {
                    var pathArr = path.split('\\');
                    if (pathArr.length < 2)
                        return null;
                    path = pathArr.slice(0, pathArr.length - (pathArr[pathArr.length - 1] ? 1 : 2)).join('\\');
                    renderFolder(path, dom);
                    return "dir";
                }
                if (item.type !== "dir") {
                    return item.type;
                }
                path += "\\" + item.name;
                renderFolder(path, dom);
                return "dir";
            });
        });
        return promise;
    }
    FileBrowserDemo.renderFolder = renderFolder;
    function renderDetails(info, dirs, files, parent, dom, callback) {
        if (!dom)
            dom = document.createElement("div");
        dom.innerHTML = "";
        var section = document.createElement("section");
        if (info) {
            if (parent && parent != info) {
                var back = document.createElement("a");
                back.className = "x-file-back";
                back.innerHTML = "&lt;";
                if (callback)
                    back.addEventListener("click", function (ev) {
                        callback(parent);
                    });
                section.appendChild(back);
            }
            section.appendChild(createSpanElement(info.path || info.name, { styleRef: "x-file-name" }));
            dom.appendChild(section);
            section = document.createElement("section");
        }
        section = document.createElement("section");
        var i = renderItems(dirs, section, callback);
        if (i > 0) {
            dom.appendChild(createElement("h2", "Sub-folders"));
            dom.appendChild(section);
        }
        section = document.createElement("section");
        i = renderItems(files, section, callback);
        if (i > 0) {
            dom.appendChild(createElement("h2", "Files"));
            dom.appendChild(section);
        }
    }
    function getIconName(ext) {
        var i = ext.lastIndexOf('.');
        if (i >= 0)
            ext = ext.substring(i + 1);
        if (!ext)
            return "File";
        switch (ext.toLowerCase()) {
            case "jpg":
            case "jpe":
            case "png":
            case "webp":
            case "bmp":
            case "svg":
            case "tif":
            case "tiff":
                return "File_Image";
            case "txt":
            case "log":
            case "diff":
            case "patch":
                return "File_Text";
            case "json":
            case "config":
            case "xml":
            case "yml":
                return "File_Json";
            case "exe":
            case "msix":
            case "msi":
            case "appx":
                return "File_Exe";
            case "dll":
            case "bin":
            case "so":
            case "lib":
                return "File_Dll";
            case "zip":
            case "gz":
            case "br":
            case "rar":
            case "tar":
            case "gzip":
                return "File_Dll";
            default:
                return "File";
        }
    }
    function createImageElement(src, alt, options) {
        var ele = document.createElement("img");
        if (alt)
            ele.alt = alt;
        ele.src = src;
        fillOptions(ele, options);
        return ele;
    }
    function createSpanElement(text, options) {
        var ele = document.createElement("span");
        ele.innerText = text;
        if (options && options.title === true)
            options.title = text;
        fillOptions(ele, options);
        return ele;
    }
    function createElement(tagName, text, options) {
        var ele = document.createElement(tagName);
        ele.innerText = text;
        fillOptions(ele, options);
        return ele;
    }
    function fillOptions(dom, options) {
        if (!options)
            return;
        if (options.styleRef)
            dom.className = options.styleRef;
        if (options.title)
            dom.title = options.title;
    }
    function getFileLengthStr(length) {
        if (length < 1000)
            return length < 0 ? "N/A" : (length.toString() + "B");
        if (length < 1024000) {
            length /= 1024;
            return length.toFixed(1) + "KB";
        }
        if (length / 1048576000) {
            length /= 1048576;
            return length.toFixed(1) + "MB";
        }
        if (length < 1073741824000) {
            length /= 1073741824;
            return length.toFixed(1) + "GB";
        }
        length /= 1099511627776;
        return length.toFixed(1) + "TB";
    }
})(FileBrowserDemo || (FileBrowserDemo = {}));
//# sourceMappingURL=index.js.map