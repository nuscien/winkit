/** File browser demo - WinKit */
/// <reference path="./localWebApp.d.ts" />
/// <reference path="./common.ts" />
var FileBrowserDemo;
(function (FileBrowserDemo) {
    FileBrowserDemo.settings = {
        iconPath: "./images/icons/"
    };
    var Viewer = /** @class */ (function () {
        function Viewer(dom) {
            this._inner = null;
            this._inner = {
                dom: dom || document.getElementById("div")
            };
        }
        Viewer.prototype.element = function () {
            return this._inner.dom;
        };
        Viewer.prototype.renderFolder = function (path) {
            var _this = this;
            if (!path)
                return this.renderDrives();
            var promise = localWebApp.files.list(path);
            promise.then(function (r) {
                if (!r.data)
                    return;
                renderDetails(r.data.info, r.data.dirs, r.data.files, r.data.parent, _this._inner.dom, function (item) {
                    if (!item) {
                        _this.renderDrives();
                        if (typeof _this.onrender === "function")
                            _this.onrender(item);
                        return "drive";
                    }
                    if (item.path) {
                        path = item.path;
                    }
                    else {
                        if (item == r.data.parent) {
                            var pathArr = path.split('\\');
                            if (pathArr.length < 2)
                                return null;
                            path = pathArr.slice(0, pathArr.length - (pathArr[pathArr.length - 1] ? 1 : 2)).join('\\');
                            _this.renderFolder(path);
                            return "dir";
                        }
                        path += "\\" + item.name;
                    }
                    if (item.type === "dir") {
                        _this.renderFolder(path);
                        if (typeof _this.onrender === "function")
                            _this.onrender(item);
                        return "dir";
                    }
                    var type = getIconName(item.name);
                    if (!type)
                        return item.type;
                    switch (type.toLowerCase().replace("file_", "")) {
                        case "text":
                        case "json":
                        case "markdown":
                            _this.renderTextFile(path);
                            break;
                        case "image":
                        case "doc":
                        case "slide":
                        case "pdf":
                            localWebApp.files.open(path);
                            break;
                    }
                    return item.type;
                });
            });
            return promise;
        };
        Viewer.prototype.renderDrives = function () {
            var _this = this;
            localWebApp.files.listDrives().then(function (r) {
                if (!r.data || !r.data.drives)
                    return;
                var dom = _this._inner.dom;
                dom.innerHTML = "";
                var arr = r.data.drives.filter(function (drive) {
                    if (!drive || typeof drive.type !== "string" || drive.type.toLowerCase() !== "fixed")
                        return false;
                    return true;
                });
                if (arr.length > 0)
                    dom.appendChild(createElement("h2", "Local drives (" + arr.length + ")", { styleRef: "x-style-label" }));
                arr.forEach(function (drive) {
                    var driveEle = document.createElement("a");
                    renderDrive(drive, driveEle, function (ev) {
                        _this.renderFolder(drive.name);
                    });
                    dom.appendChild(driveEle);
                });
                arr = r.data.drives.filter(function (drive) {
                    if (!drive || (typeof drive.type === "string" && drive.type.toLowerCase() === "fixed"))
                        return false;
                    return true;
                });
                if (arr.length > 0)
                    dom.appendChild(createElement("h2", "Other drives (" + arr.length + ")", { styleRef: "x-style-label" }));
                arr.forEach(function (drive) {
                    var driveEle = document.createElement("a");
                    renderDrive(drive, driveEle, function (ev) {
                        _this.renderFolder(drive.name);
                    });
                    dom.appendChild(driveEle);
                });
                if (typeof _this.onrender === "function")
                    _this.onrender(null);
            });
        };
        Viewer.prototype.renderTextFile = function (path) {
            var _this = this;
            localWebApp.files.get(path, { read: "text" }).then(function (r) {
                if (!r.data || !r.data.info)
                    return;
                var dom = _this._inner.dom;
                dom.innerHTML = "";
                var section = document.createElement("section");
                renderHeader(r.data.info.path || r.data.info.name, function (ev) {
                    _this.renderFolder(r.data.parent.path);
                }, section);
                dom.appendChild(section);
                section = document.createElement("section");
                section.appendChild(createSpanElement(r.data.value));
                dom.appendChild(section);
                if (typeof _this.onrender === "function")
                    _this.onrender(null);
            });
        };
        return Viewer;
    }());
    FileBrowserDemo.Viewer = Viewer;
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
    function renderDrive(drive, dom, callback) {
        if (!drive)
            return;
        if (!dom)
            dom = document.createElement("div");
        dom.innerHTML = "";
        var driveEle = document.createElement("a");
        driveEle.className = "link-long-file";
        driveEle.appendChild(createImageElement(FileBrowserDemo.settings.iconPath + "Folder_Drive.png", drive.name));
        var name = drive.name;
        if (drive.label)
            name = drive.label + " (" + name.replace('\\', '') + ")";
        driveEle.appendChild(createSpanElement(name, { title: true }));
        driveEle.appendChild(createSpanElement(drive.type));
        if (drive.freespace) {
            var space = getFileLengthStr(drive.freespace.available || drive.freespace.total) + "/" + getFileLengthStr(drive.length);
            driveEle.appendChild(createSpanElement(space));
        }
        if (typeof callback === "function")
            driveEle.addEventListener("click", function (ev) {
                callback(drive);
            });
        dom.appendChild(driveEle);
    }
    function renderHeader(title, back, dom) {
        if (!dom)
            dom = document.createElement("div");
        var backEle = document.createElement("a");
        backEle.className = "x-file-back";
        backEle.innerHTML = "&lt;";
        if (back)
            backEle.addEventListener("click", back);
        dom.appendChild(backEle);
        dom.appendChild(createSpanElement(title, { styleRef: "x-file-name" }));
    }
    function renderDetails(info, dirs, files, parent, dom, callback) {
        if (!dom)
            dom = document.createElement("div");
        dom.innerHTML = "";
        var section = document.createElement("section");
        if (info) {
            if (parent == info)
                parent = null;
            renderHeader(info.path || info.name, callback ? function (ev) {
                callback(parent);
            } : null, section);
            dom.appendChild(section);
            section = document.createElement("section");
        }
        section = document.createElement("section");
        var i = renderItems(dirs, section, callback);
        if (i > 0) {
            dom.appendChild(createElement("h2", "Sub-folders (" + i + ")", { styleRef: "x-style-label" }));
            dom.appendChild(section);
        }
        section = document.createElement("section");
        i = renderItems(files, section, callback);
        if (i > 0) {
            dom.appendChild(createElement("h2", "Files (" + i + ")", { styleRef: "x-style-label" }));
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
            case "mp4":
            case "wmv":
            case "webv":
            case "avi":
            case "mpg":
            case "mpeg":
            case "mov":
            case "av1":
                return "File_Movie";
            case "mp3":
            case "wma":
            case "weba":
            case "wav":
            case "ogg":
            case "flac":
            case "snd":
            case "mid":
                return "File_Music";
            case "json":
            case "config":
            case "xml":
            case "yml":
            case "ini":
            case "dtd":
            case "css":
            case "js":
            case "ts":
            case "vbs":
                return "File_Json";
            case "html":
            case "htm":
            case "hta":
            case "shtml":
                return "File_Html";
            case "md":
                return "File_Markdown";
            case "exe":
            case "msix":
            case "msi":
            case "appx":
                return "File_Exe";
            case "dll":
            case "bin":
            case "so":
            case "lib":
            case "msc":
            case "jar":
                return "File_Dll";
            case "zip":
            case "gz":
            case "br":
            case "rar":
            case "tar":
            case "gzip":
                return "File_Zip";
            case "docx":
            case "dotx":
            case "doc":
            case "xlsx":
            case "xlmx":
            case "xls":
            case "123":
            case "one":
            case "rtf":
                return "File_Document";
            case "pptx":
            case "potx":
            case "ppt":
                return "File_Slide";
            case "pdf":
            case "xps":
                return "File_Pdf";
            case "psd":
                return "File_Psd";
            case "font":
            case "fon":
            case "ttf":
            case "otf":
                return "File_Font";
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
        if (length < 1048576000) {
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