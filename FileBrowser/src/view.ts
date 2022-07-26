/// <reference path="./common.ts" />

namespace FileBrowserDemo {
    export const settings = {
        iconPath: "./images/icons/"
    };

    export class Viewer {
        private _inner = null as {
            dom: HTMLElement;
            header: HTMLElement;
        };

        onrender: ((file: localWebApp.FileInfoContract) => void);

        constructor(dom: HTMLElement, header: HTMLHeadElement) {
            this._inner = {
                dom: dom || document.getElementById("section"),
                header: header || document.getElementById("header")
            };
        }

        element() {
            return this._inner.dom;
        }

        renderFolder(path: string) {
            if (!path) return this.renderDrives();
            let promise = localWebApp.files.list(path);
            promise.then(r => {
                if (!r.data) return;
                renderDetails(r.data.info, r.data.dirs, r.data.files, r.data.parent, this._inner.dom, this._inner.header, item => {
                    if (!item) {
                        this.renderDrives();
                        if (typeof this.onrender === "function") this.onrender(item);
                        return "drive";
                    }
    
                    if (item.path) {
                        path = item.path;
                    } else {
                        if (item == r.data.parent) {
                            let pathArr = path.split('\\');
                            if (pathArr.length < 2) return null;
                            path = pathArr.slice(0, pathArr.length - (pathArr[pathArr.length - 1] ? 1 : 2)).join('\\');
                            this.renderFolder(path);
                            return "dir";
                        }
    
                        path += "\\" + item.name;
                    }
    
                    if (item.type === "dir") {
                        this.renderFolder(path);
                        if (typeof this.onrender === "function") this.onrender(item);
                        return "dir";
                    }
    
                    let type = getIconName(item.name);
                    if (!type) return item.type;
                    switch (type.toLowerCase().replace("file_", "")) {
                        case "text":
                        case "json":
                        case "markdown":
                            this.renderTextFile(item);
                            break;
                        case "image":
                            {
                                let ext = item.name.toLowerCase();
                                let i = ext.lastIndexOf('.');
                                if (i > 0) ext = ext.substring(i + 1);
                                switch (ext) {
                                    case "jpg":
                                    case "jpeg":
                                        this.renderImageFile(item, "jpg");
                                        break;
                                    case "png":
                                    case "apng":
                                    case "webp":
                                    case "gif":
                                        this.renderImageFile(item, ext);
                                    default:
                                        localWebApp.files.open(path);
                                }

                                break;
                            }
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
        }

        renderDrives() {
            localWebApp.files.listDrives().then(r => {
                if (!r.data || !r.data.drives) return;
                let dom = this._inner.dom;
                dom.innerHTML = "";
                let arr = r.data.drives.filter(drive => {
                    if (!drive || typeof drive.type !== "string" || drive.type.toLowerCase() !== "fixed") return false;
                    return true;
                });
                if (arr.length > 0) dom.appendChild(createElement("h2", "Local drives (" + arr.length + ")", { styleRef: "x-style-label" }));
                arr.forEach(drive => {
                    let driveEle = document.createElement("a");
                    renderDrive(drive, driveEle, ev => {
                        this.renderFolder(drive.name);
                    });                
                    dom.appendChild(driveEle);
                });
                arr = r.data.drives.filter(drive => {
                    if (!drive || (typeof drive.type === "string" && drive.type.toLowerCase() === "fixed")) return false;
                    return true;
                });
                if (arr.length > 0) dom.appendChild(createElement("h2", "Other drives (" + arr.length + ")", { styleRef: "x-style-label" }));
                arr.forEach(drive => {
                    let driveEle = document.createElement("a");
                    renderDrive(drive, driveEle, ev => {
                        this.renderFolder(drive.name);
                    });                
                    dom.appendChild(driveEle);
                });
                if (typeof this.onrender === "function") this.onrender(null);
            });
        }

        renderTextFile(file: localWebApp.FileInfoContract) {
            this.renderCustomizedFile(file, "text", ["text", "json"], (dom, file) => {
                appendSection(dom, createSpanElement(file.value, { styleRef: "x-file-text-content" }));
            });
        }

        renderImageFile(file: localWebApp.FileInfoContract, type: string) {
            this.renderCustomizedFile(file, "base64", null, (dom, file) => {
                var image = document.createElement("img");
                image.src = "data:image/" + type + ";base64," + file.value;
                image.className = "x-file-image-content";
                appendSection(dom, image);
            });
        }

        private renderCustomizedFile(file: localWebApp.FileInfoContract, read: string, list: string[], callback: ((dom: HTMLElement, file: localWebApp.FileGetResponseContract) => void)) {
            localWebApp.files.get(file.path, { read: read as any, maxLength: 10000000000 }).then(r => {
                if (!r.data || !r.data.info) return;
                let dom = this._inner.dom;
                dom.innerHTML = "";
                let button = document.createElement("a");
                button.className = "link-button";
                button.appendChild(createImageElement(settings.iconPath + getIconName(file.name) + ".png", "Open"));
                button.appendChild(createSpanElement("Open"));
                button.addEventListener("click", ev => {
                    localWebApp.files.open(file.path);
                });
                appendSection(dom, button, { styleRef: "x-bar-actions" });
                renderHeader(r.data.info.path || r.data.info.name, ev => {
                    this.renderFolder(r.data.parent.path);
                }, this._inner.header);
                if (typeof callback === "function") callback(dom, r.data);
                if (typeof this.onrender === "function") this.onrender(null);
                if (!list) return;
                for (let i in dom.children) {
                    let section = dom.children[i];
                    if (!section || !section.tagName || section.tagName.toLowerCase() !== "section") continue;
                    section.classList.add("x-file-view-splitted");
                }

                let ul = document.createElement("ul");
                appendSection(dom, ul, { styleRef: "x-file-view-menu" });
                let currentItem = document.createElement("li");
                currentItem.innerText = r.data.info.name;
                currentItem.className = "x-state-selected";
                ul.appendChild(currentItem);
                localWebApp.files.list(r.data.parent.path).then(r2 => {
                    if (!r2.data || !r2.data.files) return;
                    let names = r2.data.files.map(ele => {
                        let type = getIconName(ele.name).replace("File_", "").toLowerCase();
                        return !ele.name || list.indexOf(type) < 0 ? null : ele;
                    }).filter(ele => ele).map(ele => {
                        let li = document.createElement("li");
                        li.innerText = ele.name;
                        li.addEventListener("click", ev => {
                            this.renderTextFile(ele);
                        });
                        if (ele.name === file.name) li.className = "x-state-selected";
                        return li;
                    });
                    if (names.length < 2) return;
                    ul.innerHTML = "";
                    for (let li in names) {
                        ul.appendChild(names[li]);
                    }
                });
            }, ex => {
                
            });
        }
    }

    export function renderItem(item: localWebApp.FileInfoContract, dom: HTMLElement, callback?: ((file: localWebApp.FileInfoContract) => void)) {
        if (!item) return null;
        if (!dom) dom = document.createElement("div");
        dom.innerHTML = "";
        if (typeof callback === "function") dom.addEventListener("click", ev => {
            callback(item);
        })
        if (item.type === "dir") {
            dom.className = "link-tile-folder";
            dom.appendChild(createImageElement(settings.iconPath + "Folder.png", item.name));
            dom.appendChild(createSpanElement(item.name, { title: true }));
            return "dir";
        }

        dom.className = "link-long-file";
        dom.appendChild(createImageElement(settings.iconPath + getIconName(item.name) + ".png", item.name));
        dom.appendChild(createSpanElement(item.name, { title: true }));
        if (item.modified) {
            let modified = new Date(item.modified);
            dom.appendChild(createSpanElement(modified.toLocaleString()));
        }

        dom.appendChild(createSpanElement(getFileLengthStr(item.length)));
        return "file";
    }

    export function renderItems(col: localWebApp.FileInfoContract[], dom: HTMLElement, callback?: ((file: localWebApp.FileInfoContract) => void)) {
        if (!col) return 0;
        if (!dom) dom = document.createElement("div");
        dom.innerHTML = "";
        let i = 0;
        col.forEach(file => {
            let ele = document.createElement("a");
            if (!renderItem(file, ele, callback)) return;
            dom.appendChild(ele);
            i++;
        });

        return i;
    }

    function renderDrive(drive: localWebApp.DriveInfoContract, dom: HTMLElement, callback: ((data: localWebApp.DriveInfoContract) => void)) {
        if (!drive) return;
        if (!dom) dom = document.createElement("div");
        dom.innerHTML = "";
        let driveEle = document.createElement("a");
        driveEle.className = "link-long-file";
        driveEle.appendChild(createImageElement(settings.iconPath + "Folder_Drive.png", drive.name));
        let name = drive.name;
        if (drive.label) name = drive.label + " (" + name.replace('\\', '') + ")";
        driveEle.appendChild(createSpanElement(name, { title: true }));
        driveEle.appendChild(createSpanElement(drive.type));
        if (drive.freespace) {
            let space = getFileLengthStr(drive.freespace.available || drive.freespace.total) + "/" + getFileLengthStr(drive.length);
            driveEle.appendChild(createSpanElement(space));
        }
        
        if (typeof callback === "function") driveEle.addEventListener("click", ev => {
            callback(drive);
        });
        dom.appendChild(driveEle);
    }

    function renderHeader(title: string, back: ((this: HTMLAnchorElement, ev: MouseEvent) => void) | null, dom: HTMLElement) {
        if (!dom) dom = document.createElement("header");
        else dom.innerHTML = "";
        let backEle = document.createElement("a");
        backEle.className = "x-file-back";
        backEle.innerHTML = "&lt;";
        if (back) backEle.addEventListener("click", back);
        dom.appendChild(backEle);
        dom.appendChild(createSpanElement(title, { styleRef: "x-file-name" }));
    }

    function renderDetails(info: localWebApp.FileInfoContract, dirs: localWebApp.FileInfoContract[], files: localWebApp.FileInfoContract[], parent: localWebApp.FileInfoContract | null, dom: HTMLElement, header: HTMLElement, callback: ((item: localWebApp.FileInfoContract) => string | null)) {
        if (!dom) dom = document.createElement("div");
        dom.innerHTML = "";
        if (info) {
            if (parent == info) parent = null;
            renderHeader(info.path || info.name, callback ? ev => {
                callback(parent);
            } : null, header);
        }
        
        let section = document.createElement("section");
        let i = renderItems(dirs, section, callback);
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

    function getIconName(ext: string) {
        let i = ext.lastIndexOf('.');
        if (i >= 0) ext = ext.substring(i + 1);
        if (!ext) return "File";
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
                return "File_Font"
            default:
                return "File";
        }
    }

    function createImageElement(src: string, alt?: string, options?: any) {
        let ele = document.createElement("img");
        if (alt) ele.alt = alt;
        ele.src = src;
        fillOptions(ele, options);
        return ele;
    }

    function createSpanElement(text: string, options?: any) {
        let ele = document.createElement("span");
        ele.innerText = text;
        if (options && options.title === true) options.title = text;
        fillOptions(ele, options);
        return ele;
    }

    function createElement(tagName: string, text: string, options?: any) {
        let ele = document.createElement(tagName);
        ele.innerText = text;
        fillOptions(ele, options);
        return ele;
    }

    function appendSection(container: HTMLElement, child: HTMLElement | HTMLElement[], options?: any) {
        let section = document.createElement("section");
        if (!child) {
        } else if (child instanceof Array) {
            for (let i in child) {
                let c = child[i];
                if (c) section.appendChild(c);
            }
        } else {
            section.appendChild(child);
        }

        container.appendChild(section);
        fillOptions(section, options);
        return section;
    }

    function fillOptions(dom: HTMLElement, options: any) {
        if (!options) return;
        if (options.styleRef) dom.className = options.styleRef;
        if (options.title) dom.title = options.title;
    }

    function getFileLengthStr(length: number) {
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
}