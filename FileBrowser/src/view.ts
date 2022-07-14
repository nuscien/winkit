/// <reference path="./common.ts" />

namespace FileBrowserDemo {
    export const settings = {
        iconPath: "./images/icons/"
    };

    export class Viewer {
        private _inner = null as {
            dom: HTMLElement;
        };

        onrender: ((file: localWebApp.FileInfoContract) => void);

        constructor(dom: HTMLElement) {
            this._inner = {
                dom: dom || document.getElementById("div")
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
                renderDetails(r.data.info, r.data.dirs, r.data.files, r.data.parent, this._inner.dom, item => {
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
                            this.renderTextFile(path);
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

        renderTextFile(path: string) {
            localWebApp.files.get(path, { read: "text" }).then(r => {
                if (!r.data || !r.data.info) return;
                let dom = this._inner.dom;
                dom.innerHTML = "";
                let section = document.createElement("section");
                renderHeader(r.data.info.path || r.data.info.name, ev => {
                    this.renderFolder(r.data.parent.path);
                }, section);
                dom.appendChild(section);
                section = document.createElement("section");
                section.appendChild(createSpanElement(r.data.value));
                dom.appendChild(section);
                if (typeof this.onrender === "function") this.onrender(null);
            })
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
        if (!dom) dom = document.createElement("div");
        let backEle = document.createElement("a");
        backEle.className = "x-file-back";
        backEle.innerHTML = "&lt;";
        if (back) backEle.addEventListener("click", back);
        dom.appendChild(backEle);
        dom.appendChild(createSpanElement(title, { styleRef: "x-file-name" }));
    }

    function renderDetails(info: localWebApp.FileInfoContract, dirs: localWebApp.FileInfoContract[], files: localWebApp.FileInfoContract[], parent: localWebApp.FileInfoContract | null, dom: HTMLElement, callback: ((item: localWebApp.FileInfoContract) => string | null)) {
        if (!dom) dom = document.createElement("div");
        dom.innerHTML = "";
        let section = document.createElement("section");
        if (info) {
            if (parent == info) parent = null;
            renderHeader(info.path || info.name, callback ? ev => {
                callback(parent);
            } : null, section);
            dom.appendChild(section);
            section = document.createElement("section");
        }
        
        section = document.createElement("section");
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
            case "json":
            case "config":
            case "xml":
            case "yml":
                return "File_Json";
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