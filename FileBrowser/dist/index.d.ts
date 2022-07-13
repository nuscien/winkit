/// <reference path="../src/localWebApp.d.ts" />
/** File browser demo - WinKit */
declare namespace FileBrowserDemo {
}
declare namespace FileBrowserDemo {
    const settings: {
        iconPath: string;
    };
    function renderItem(item: localWebApp.FileInfoContract, dom: HTMLElement, callback?: ((file: localWebApp.FileInfoContract) => void)): "file" | "dir";
    function renderItems(col: localWebApp.FileInfoContract[], dom: HTMLElement, callback?: ((file: localWebApp.FileInfoContract) => void)): number;
    function renderFolder(path: string, dom: HTMLElement): localWebApp.HandlerResponse<{
        info: localWebApp.FileInfoContract;
        dirs: localWebApp.FileInfoContract[];
        files: localWebApp.FileInfoContract[];
        parent?: localWebApp.FileInfoContract;
    }>;
}
