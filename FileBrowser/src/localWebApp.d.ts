declare namespace localWebApp {

    interface EventCallbackContract {

        /**
         * Processes.
         * @param ev
         */
        proc(ev: any): void;

        /**
         * Tests if the handler is not valid anymore.
         */
        invalid?: boolean | number | ((ev: any) => boolean) | null;

        /**
         * An optional value indicating whether need still keep the handler in the registration.
         */
        keep?: boolean | null;

        /**
         * Processes on remove from registration.
         */
        dispose?(): void;
    }

    interface HandlerInfoContract {
        [property: string]: any;
    }

    interface CommandHandlerContract {

        /**
         * Gets the handler identifier.
         */
        id(): string;

        /**
         * Sends request.
         * @param cmd The command name.
         * @param data The input data.
         * @param context The optional context data which will be returned in response for reference.
         * @param info The additional information.
         */
        call(cmd: string, data: any, context?: HandlerInfoContract | undefined | null, info?: HandlerInfoContract | undefined | null): void;

        /**
         * Sends request to get result.
         * @param cmd The command name.
         * @param data The input data.
         * @param context The optional context data which will be returned in response for reference.
         * @param info The additional information.
         */
        request<T>(cmd: string, data: any, context?: HandlerInfoContract | undefined | null, info?: HandlerInfoContract | undefined | null): Promise<T>;
    }

    interface HandlerResponseContract<T> {

        /**
         * The trace identifier for the operation.
         */
        trace: string;

        /**
         * The command name.
         */
        cmd: string;

        /**
         * The handler identifier.
         */
        handler: string | null;

        /**
         * The response data.
         */
        data?: T | null;

        /**
         * The additional information.
         */
        info: HandlerInfoContract;

        /**
         * The message.
         */
        message?: string | null;

        /**
         * A value indicating whether the response is error.
         */
        error: boolean;

        /**
         * The context from request.
         */
        context: HandlerInfoContract;

        /**
         * The timeline of processing.
         */
        timeline: {
            request: string;
            processing: string;
            processed: string;
        };
    }

    interface HandlerProcessingReferenceContract {
        trace?: string | undefined;
        response?: HandlerResponseContract<any> | undefined;
    }

    interface FileInfoContract {
        name: string;
        modified: string;
        created: string;
        attr: string;
        type?: "file" | "dir" | null;
        length?: number | null;
        link?: string | null;
        path?: string | null;
    }

    interface DriveInfoContract {
        name: string;
        ready: boolean;
        length: number;
        freespace: {
            total: number;
            available: number;
        };
        label: string | null;
        format: "NTFS" | "FAT32" | "exFAT" | string | null;
        type: "Unknown" | "NoRootDirectory" | "Removable" | "Fixed" | "Network" | "CDRom" | "Ram" | string;
        dir: FileInfoContract;
    }

    interface FileListResponseContract {
        info: FileInfoContract;
        dirs: FileInfoContract[];
        files: FileInfoContract[];
        parent?: FileInfoContract | null;
    }

    interface FileGetResponseContract {
        info: FileInfoContract;
        value?: any;
        valueType?: "text" | "json" | "jsonArray" | "base64" | null;
        parent?: FileInfoContract | null;
    }

    interface DownloadListContract {
        dialog: boolean;
        max: number;
        list: {
            uri: string;
            file: string;
            state: "InProgress" | "Interrupted" | "Completed" | string;
            received: number;
            length: number;
            interrupt: string | null;
            mime: string | null;
        }[];
        enumerated: string;
    }

    interface WindowStateInfoContract {
        width: number;
        height: number;
        top: number;
        left: number;
        state: "Maximized" | "Restored" | "Minimized" | "Fullscreen" | "Compact";
        title: string;
    }

    type EventCallback = EventCallbackContract | ((ev: any) => void);

    type HandlerResponse<T> = Promise<HandlerResponseContract<T>>;

    type ArchValue = "X86" | "X64" | "ARM" | "ARM64" | "WASM" | "S390x" | string | null;

    type WindowStates = "Maximize" | "maximize" | "MAXIMIZE" | "Restore" | "restore" | "RESTORE" | "Minimize" | "minimize" | "MINIMIZE" | "Fullscreen" | "fullscreen" | "FULLSCREEN" | "Enter Full Screen" | "enter full screen" | "ENTER FULL SCREEN" | "Exit Full screen" | "exit full screen" | "EXIT FULL SCREEN";

    /**
     * Adds an event handler on message.
     * @param type The message type.
     * @param callback The callback.
     * @param options The options.
     */
    function onMessage(type: string, callback: EventCallback): void;

    /**
     * Gets the command handler.
     * @param id The handler identifier.
     */
    function getCommandHandler(id: string): CommandHandlerContract;

    /**
     * Gets a value in the specific cookie item.
     * @param key The cookie key.
     */
    function getCookie(key: string): string;

    /**
     * Files utilities.
     */
    const files: {

        /**
         * Lists files and folders in a specific directory.
         * @param dir The path of the directory to list its children.
         * @param options The options.
         */
        list(dir: string, options?: {
            q?: string;
            showHidden?: boolean | null;
            context?: HandlerInfoContract;
            ref?: HandlerProcessingReferenceContract;
        } | string | undefined | null): HandlerResponse<FileListResponseContract>;

        /**
         * Lists drives.
         * @param options The options.
         */
         listDrives(options?: {
            fixed?: boolean | null;
            context?: HandlerInfoContract;
            ref?: HandlerProcessingReferenceContract;
        } | string | undefined | null): HandlerResponse<{
            drives: DriveInfoContract[]
        }>;

        /**
         * Gets the file information.
         * @param path The path of the file to get information.
         * @param options The options.
         */
        get(path: string, options?: {
            appData?: boolean;
            read?: boolean | "none" | "text" | "json" | "base64";
            maxLength?: number;
            context?: HandlerInfoContract;
            ref?: HandlerProcessingReferenceContract;
        }): HandlerResponse<FileGetResponseContract>;

        /**
         * Writes a file.
         * @param path The path of the file to write.
         * @param value The content to write.
         * @param options The options.
         */
        write(path: string, value: { property: string } | any[] | string | null, options?: {
            appData?: boolean;
            context?: HandlerInfoContract;
            ref?: HandlerProcessingReferenceContract;
        }): HandlerResponse<{}>;

        /**
         * Moves a file or a directory.
         * @param path The source path of the file to move.
         * @param dest The destination path.
         * @param options The options.
         */
        move(path: string, dest: string, options?: {
            override?: boolean;
            dir?: boolean;
            context?: HandlerInfoContract;
            ref?: HandlerProcessingReferenceContract;
        } | boolean): HandlerResponse<{}>;

        /**
         * Copies a file or a directory.
         * @param path The source path of the file to copy.
         * @param dest The destination path.
         * @param options The options.
         */
        copy(path: string, dest: string, options?: {
            override?: boolean;
            dir?: boolean;
            context?: HandlerInfoContract;
            ref?: HandlerProcessingReferenceContract;
        } | boolean): HandlerResponse<{}>;

        /**
         * Deletes a file or a directory.
         * @param path The source path of the file to delete.
         * @param options The options.
         */
        delete(path: string, options?: {
            dir?: boolean;
            context?: HandlerInfoContract;
            ref?: HandlerProcessingReferenceContract;
        } | boolean): HandlerResponse<{}>;

        /**
         * Makes a directory.
         * @param path The source path of the file to move.
         * @param options The options.
         */
        md(path: string, options?: {
            context?: HandlerInfoContract;
            ref?: HandlerProcessingReferenceContract;
        } | boolean): HandlerResponse<{
            existed: boolean;
            info: FileInfoContract;
        }>;

        /**
         * Opens a file by the default app; or opens the folder; or runs an app.
         * @param path The path of the file or folder to open.
         * @param options The options.
         */
        open(path: string, options?: {
            appData?: boolean;
            args?: string | null;
            type?: "file" | "dir" | "url" | "exe" | null;
            context?: HandlerInfoContract;
            ref?: HandlerProcessingReferenceContract;
        }): HandlerResponse<{}>;

        /**
         * Lists the download items.
         * @param options The options.
         */
        listDownload(options?: boolean | number | {
            open?: boolean;
            max?: number;
            context?: HandlerInfoContract;
            ref?: HandlerProcessingReferenceContract;
        }): HandlerResponse<DownloadListContract>;
    };

    /**
     * Cryptography utilities.
     */
    const cryptography: {

        /**
         * Encrypts by symmetric algorithm.
         * @param alg The algorithm name.
         * @param value The value to encrypt.
         * @param key The key.
         * @param iv The IV.
         * @param options The options.
         */
        encrypt(alg: "aes" | "3des", value: string, key: string, iv: string, options?: {
            context?: HandlerInfoContract;
            ref?: HandlerProcessingReferenceContract;
        }): HandlerResponse<{
            value: string;
            algorithm: string;
            encode: "hex";
        }>;

        /**
         * Decrypts by symmetric algorithm.
         * @param alg The algorithm name.
         * @param value The value to decrypt.
         * @param key The key.
         * @param iv The IV.
         * @param options The options.
         */
        decrypt(alg: "aes" | "3des", value: string, key: string, iv: string, options?: {
            context?: HandlerInfoContract;
            ref?: HandlerProcessingReferenceContract;
        }): HandlerResponse<{
            value: string;
            algorithm: string;
            encode: "hex";
        }>;

        /**
         * Verify the signature.
         * @param alg The algorithm name.
         * @param value The value to verify.
         * @param key The key.
         * @param test The signature to test.
         * @param options The options.
         */
        verify(alg: "rs256" | "rs384" | "rs512" | "hs256" | "hs384" | "hs512", value: string, key: string, test: string, options?: {
            type?: "text" | "file" | null;
            context?: HandlerInfoContract;
            ref?: HandlerProcessingReferenceContract;
        }): HandlerResponse<{
            verify: boolean;
            algorithm: string;
        }>;

        /**
         * Signs.
         * @param alg The algorithm name.
         * @param value The value to sign.
         * @param key The key.
         * @param options The options.
         */
        sign(alg: "rs256" | "rs384" | "rs512" | "hs256" | "hs384" | "hs512", value: string, key: string, options?: {
            type?: "text" | "file" | null;
            context?: HandlerInfoContract;
            ref?: HandlerProcessingReferenceContract;
        }): HandlerResponse<{
            sign: string;
            algorithm: string;
            encode: "base64url";
        }>;

        /**
         * Hashes a text of a file.
         * @param alg The algorithm name.
         * @param value The text content or the file path.
         * @param options The options.
         */
        hash(alg: "sha256" | "sha384" | "sha512", value: string, options?: {
            test?: string;
            type?: "text" | "file" | null;
            context?: HandlerInfoContract;
            ref?: HandlerProcessingReferenceContract;
        }): HandlerResponse<{
            value: string;
            algorithm: string;
            encode: "hex";
        }>;
    };

    /**
     * Text utilities.
     */
    const text: {

        /**
         * Encodes by Base64.
         * @param s The input text.
         * @param url true if encodes by Base64Url; otherwise, false.
         */
        encodeBase64(s: string, url?: boolean): string;

        /**
         * Decodes by Base64.
         * @param s The input text.
         * @param url true if decodes by Base64Url; otherwise, false.
         */
        decodeBase64(s: string, url?: boolean): string;

        /**
         * Encodes by URI component.
         * @param s The input text.
         * @param url true if encode by URI component parameter; otherwise, false.
         */
        encodeUri(s: string, parameter?: boolean): string;

        /**
         * Decodes by URI component.
         * @param s The input text.
         * @param url true if decode by URI component parameter; otherwise, false.
         */
        decodeUri(s: string, parameter?: boolean): string;
    };

    const hostApp: {

        /**
         * Gets theme information.
         * @param options The options.
         */
        theme(options?: {
            context?: HandlerInfoContract;
            ref?: HandlerProcessingReferenceContract;
        }): HandlerResponse<{
            brightness: "dark" | "light";
        }>;

        /**
         * Checks update.
         * @param options The options.
         */
        checkUpdate(options?: {
            context?: HandlerInfoContract;
            ref?: HandlerProcessingReferenceContract;
        }): HandlerResponse<{
            version: string;
            has: boolean;
        }>;

        /**
         * Gets or sets the window state.
         * @param value The optional state to set.
         */
        window(value?: WindowStates | {
            state?: WindowStates;
            width?: number;
            height?: number;
            top?: number;
            left?: number;
            focus?: boolean;
            physical?: boolean;
            context?: HandlerInfoContract;
            ref?: HandlerProcessingReferenceContract;
        } | null): HandlerResponse<WindowStateInfoContract>;

        /**
         * Lists information of all command handler.
         * @param options The options.
         */
        handlers(options?: {
            context?: HandlerInfoContract;
            ref?: HandlerProcessingReferenceContract;
        }): HandlerResponse<{
            collection: {
                id: string;
                description: string;
                version: string;
            }[];
        }>;
    };

    /**
     * Host information.
     */
    const hostInfo: {
        hostApp: {
            id: string;
            version: string;
            name: string;
            value: string;
            additional: string;
        };
        intro: {
            id: string;
            icon: string | null;
            description: string | null;
            url: string | null;
            copyright: string | null;
            publisher: string | null;
            version: string | null;
            tags: string[];
        };
        runtime: {
            kind: "wasdk" | "wpf" | string;
            version: string | null;
            netfx: string | null;
            id: string | null;
            webview2: string | null;
            arch: ArchValue;
        };
        os: {
            value: string | null;
            arch: ArchValue;
            version: string | null;
            platform: string | null;
        };
        cmdLine: {
            args: string[];
            systemAccount: string;
        };
        mkt: {
            value: string | null;
            name: string | null;
            rtl: boolean;
            timeZone: string | null;
            timeZoneDisplayName: string | null;
            baseOffset: string | null;
        };
        device: {
            form: "Unknown" | "Phone" | "Tablet" | "Desktop" | "Notebook" | "Convertible" | "Detachable" | "All-in-One" | "Stick PC" | "Puck" | "Surface Hub" | "Head-mounted display" | "Industry handheld" | "Industry tablet" | "Banking" | "Building automation" | "Digital signage" | "Gaming" | "Home automatio" | "Industrial automation" | "Kiosk" | "Maker board" | "Medical" | "Networking" | "Point of Service" | "Printing" | "Thin client" | "Toy" | "Vending" | "Industry other" | string;
            family: "Windows.Desktop" | "Windows.Mobile" | string | null;
            manufacturer: string | null;
            productName: string | null;
            productSku: string | null;
        };
    };

    /**
     * Resources of JSON data.
     */
    const dataRes: {
        [property: string]: any;
    };

    /**
     * Resources of text.
     */
    const strRes: {
        [property: string]: string;
    };
}
