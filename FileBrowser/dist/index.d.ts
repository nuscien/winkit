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
        constructor(dom: HTMLElement);
        element(): HTMLElement;
        renderFolder(path: string): void | localWebApp.HandlerResponse<{
            info: localWebApp.FileInfoContract;
            dirs: localWebApp.FileInfoContract[];
            files: localWebApp.FileInfoContract[];
            parent?: localWebApp.FileInfoContract;
        }>;
        renderDrives(): void;
        renderTextFile(path: string): void;
    }
    function renderItem(item: localWebApp.FileInfoContract, dom: HTMLElement, callback?: ((file: localWebApp.FileInfoContract) => void)): "file" | "dir";
    function renderItems(col: localWebApp.FileInfoContract[], dom: HTMLElement, callback?: ((file: localWebApp.FileInfoContract) => void)): number;
}
