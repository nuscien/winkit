/// <reference path="../src/localWebApp.d.ts" />
/** File browser demo - WinKit */
declare namespace FileBrowserDemo {
}
declare namespace FileBrowserDemo {
    const settings: {
        iconPath: string;
    };
    class Viewer {
        private _inner;
        onrender: ((file: localWebApp.FileInfoContract) => void);
        constructor(dom: HTMLElement, header: HTMLHeadElement);
        element(): HTMLElement;
        renderFolder(path: string): void | localWebApp.HandlerResponse<localWebApp.FileListResponseContract>;
        renderDrives(): void;
        renderTextFile(file: localWebApp.FileInfoContract): void;
        renderImageFile(file: localWebApp.FileInfoContract, type: string): void;
        private renderCustomizedFile;
    }
    function renderItem(item: localWebApp.FileInfoContract, dom: HTMLElement, callback?: ((file: localWebApp.FileInfoContract) => void)): "file" | "dir";
    function renderItems(col: localWebApp.FileInfoContract[], dom: HTMLElement, callback?: ((file: localWebApp.FileInfoContract) => void)): number;
}
