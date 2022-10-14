/** File browser demo - WinKit */
/// <reference path="./localWebApp.d.ts" />
/// <reference path="./common.ts" />
var FileBrowserDemo;
(function (FileBrowserDemo) {
    FileBrowserDemo.settings = {
        iconPath: "./images/icons/"
    };
    var Viewer = /** @class */ (function () {
        function Viewer(dom, header) {
            this._inner = null;
            this._inner = {
                dom: dom || document.getElementById("section"),
                header: header || document.getElementById("header")
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
                renderDetails(r.data.info, r.data.dirs, r.data.files, r.data.parent, _this._inner.dom, _this._inner.header, function (item) {
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
                            _this.renderTextFile(item);
                            break;
                        case "image":
                            {
                                var ext = item.name.toLowerCase();
                                var i = ext.lastIndexOf('.');
                                if (i > 0)
                                    ext = ext.substring(i + 1);
                                switch (ext) {
                                    case "jpg":
                                    case "jpeg":
                                        _this.renderImageFile(item, "jpg");
                                        break;
                                    case "png":
                                    case "apng":
                                    case "webp":
                                    case "gif":
                                        _this.renderImageFile(item, ext);
                                    default:
                                        localWebApp.files.open(path);
                                }
                                break;
                            }
                        case "document":
                        case "slide":
                        case "pdf":
                        case "psd":
                            localWebApp.files.open(path);
                            break;
                        case "link":
                            if (!item.link)
                                break;
                            _this.renderFolder(item.link);
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
        Viewer.prototype.renderTextFile = function (file) {
            this.renderCustomizedFile(file, "text", ["text", "json", "markdown", "html"], function (dom, file) {
                appendSection(dom, createSpanElement(file.value, { styleRef: "x-file-text-content" }));
            });
        };
        Viewer.prototype.renderImageFile = function (file, type) {
            this.renderCustomizedFile(file, "base64", null, function (dom, file) {
                var image = document.createElement("img");
                image.src = "data:image/" + type + ";base64," + file.value;
                image.className = "x-file-image-content";
                appendSection(dom, image);
            });
        };
        Viewer.prototype.renderCustomizedFile = function (file, read, list, callback) {
            var _this = this;
            localWebApp.files.get(file.path, { read: read, maxLength: 10000000000 }).then(function (r) {
                if (!r.data || !r.data.info)
                    return;
                var dom = _this._inner.dom;
                dom.innerHTML = "";
                var button = document.createElement("a");
                button.className = "link-button";
                button.appendChild(createImageElement(FileBrowserDemo.settings.iconPath + getIconName(file.name) + ".png", "Open"));
                button.appendChild(createSpanElement("Open"));
                button.addEventListener("click", function (ev) {
                    localWebApp.files.open(file.path);
                });
                appendSection(dom, button, { styleRef: "x-bar-actions" });
                renderHeader(r.data.info.path || r.data.info.name, function (ev) {
                    _this.renderFolder(r.data.parent.path);
                }, _this._inner.header);
                if (typeof callback === "function")
                    callback(dom, r.data);
                if (typeof _this.onrender === "function")
                    _this.onrender(null);
                if (!list)
                    return;
                for (var i in dom.children) {
                    var section = dom.children[i];
                    if (!section || !section.tagName || section.tagName.toLowerCase() !== "section")
                        continue;
                    section.classList.add("x-file-view-splitted");
                }
                var ul = document.createElement("ul");
                appendSection(dom, ul, { styleRef: "x-file-view-menu" });
                var currentItem = document.createElement("li");
                currentItem.innerText = r.data.info.name;
                currentItem.className = "x-state-selected";
                ul.appendChild(currentItem);
                localWebApp.files.list(r.data.parent.path).then(function (r2) {
                    if (!r2.data || !r2.data.files)
                        return;
                    var names = r2.data.files.map(function (ele) {
                        var type = getIconName(ele.name).replace("File_", "").toLowerCase();
                        return !ele.name || list.indexOf(type) < 0 ? null : ele;
                    }).filter(function (ele) { return ele; }).map(function (ele) {
                        var li = createElement("li", ele.name, { click: function (ev) {
                                _this.renderTextFile(ele);
                            } });
                        if (ele.name === file.name)
                            li.className = "x-state-selected";
                        return li;
                    });
                    if (names.length < 2)
                        return;
                    ul.innerHTML = "";
                    for (var li in names) {
                        ul.appendChild(names[li]);
                    }
                });
            }, function (ex) {
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
        // dom.addEventListener("contextmenu", ev => {
        //     localWebApp.cryptography.hash("sha256", item.path, { type: "file" }).then(r => {
        //         alert(r.data.value);
        //     })
        // });
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
            dom = document.createElement("header");
        else
            dom.innerHTML = "";
        var backEle = document.createElement("a");
        backEle.className = "x-file-back";
        backEle.innerHTML = "↑";
        if (back)
            backEle.addEventListener("click", back);
        dom.appendChild(backEle);
        dom.appendChild(createSpanElement(title, { styleRef: "x-file-name" }));
    }
    function renderDetails(info, dirs, files, parent, dom, header, callback) {
        if (!dom)
            dom = document.createElement("div");
        dom.innerHTML = "";
        if (info) {
            if (parent == info)
                parent = null;
            renderHeader(info.path || info.name, callback ? function (ev) {
                callback(parent);
            } : null, header);
        }
        var section = createElement("section", null, { styleRef: "x-file-view-splitted" });
        var i = renderItems(dirs, section, callback);
        if (i > 0) {
            dom.appendChild(createElement("h2", "Sub-folders (" + i + ")", { styleRef: "x-style-label x-file-view-splitted" }));
            dom.appendChild(section);
        }
        section = createElement("section", null, { styleRef: "x-file-view-splitted" });
        i = renderItems(files, section, callback);
        if (i > 0) {
            dom.appendChild(createElement("h2", "Files (" + i + ")", { styleRef: "x-style-label x-file-view-splitted" }));
            dom.appendChild(section);
        }
        if (dom.childNodes.length < 1)
            dom.appendChild(createElement("section", "Empty", { styleRef: "x-style-label x-file-view-splitted" }));
        var ul = document.createElement("ul");
        appendSection(dom, ul, { styleRef: "x-file-view-menu" });
        var currentItem = document.createElement("li");
        currentItem.innerText = info.name;
        currentItem.className = "x-state-selected";
        ul.appendChild(currentItem);
        localWebApp.files.list(parent.path).then(function (r2) {
            if (!r2.data || !r2.data.dirs)
                return;
            var names = r2.data.dirs.filter(function (ele) { return ele && ele.name; }).map(function (ele) {
                var li = createElement("li", ele.name, { click: function (ev) {
                        callback(ele);
                    } });
                if (ele.name === info.name)
                    li.className = "x-state-selected";
                return li;
            });
            if (names.length < 2)
                return;
            ul.innerHTML = "";
            for (var li in names) {
                ul.appendChild(names[li]);
            }
        });
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
            case "vsconfig":
            case "gitconfig":
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
            // case "lnk":
            //     return "Link";
            // case "url":
            //     return "Link_Url";
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
    function appendSection(container, child, options) {
        var section = document.createElement("section");
        if (!child) {
        }
        else if (child instanceof Array) {
            for (var i in child) {
                var c = child[i];
                if (c)
                    section.appendChild(c);
            }
        }
        else {
            section.appendChild(child);
        }
        container.appendChild(section);
        fillOptions(section, options);
        return section;
    }
    function fillOptions(dom, options) {
        if (!options)
            return;
        if (options.styleRef)
            dom.className = options.styleRef;
        if (options.title)
            dom.title = options.title;
        if (options.click)
            dom.addEventListener("click", options.click);
        if (options.contextmenu)
            dom.addEventListener("contextmenu", options.click);
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