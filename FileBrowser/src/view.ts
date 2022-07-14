/// <reference path="./common.ts" />

namespace FileBrowserDemo {
    export const settings = {
        iconPath: "./images/icons/"
    };

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

    export function renderFolder(path: string, dom: HTMLElement) {
        let promise = localWebApp.files.list(path);
        promise.then(r => {
            if (!r.data) return;
            renderDetails(r.data.info, r.data.dirs, r.data.files, r.data.parent, dom, item => {
                if (!item) return null;
                if (item == r.data.parent) {
                    let pathArr = path.split('\\');
                    if (pathArr.length < 2) return null;
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

    function renderDetails(info: localWebApp.FileInfoContract, dirs: localWebApp.FileInfoContract[], files: localWebApp.FileInfoContract[], parent: localWebApp.FileInfoContract | null, dom: HTMLElement, callback: ((item: localWebApp.FileInfoContract) => string | null)) {
        if (!dom) dom = document.createElement("div");
        dom.innerHTML = "";
        let section = document.createElement("section");
        if (info) {
            if (parent && parent != info) {
                let back = document.createElement("a");
                back.className = "x-file-back";
                back.innerHTML = "&lt;";
                if (callback) back.addEventListener("click", ev => {
                    callback(parent);
                });
                section.appendChild(back);
            }

            section.appendChild(createSpanElement(info.path || info.name, { styleRef: "x-file-name" }));
            dom.appendChild(section);
            section = document.createElement("section");
        }
        
        section = document.createElement("section");
        let i = renderItems(dirs, section, callback);
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
}