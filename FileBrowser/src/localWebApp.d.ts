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
         * Gets the command handler identifier.
         */
        id(): string;

        /**
         * Sends request.
         * @param cmd The command name.
         * @param data The input data.
         * @param context The optional context data which will be returned in response for reference.
         * @param info The additional information.
         * @param ref The additional reference object.
         */
        call(cmd: string, data: any, context?: HandlerInfoContract | undefined | null, info?: HandlerInfoContract | undefined | null, ref?: any): void;

        /**
         * Sends request to get result.
         * @param cmd The command name.
         * @param data The input data.
         * @param context The optional context data which will be returned in response for reference.
         * @param info The additional information.
         * @param ref The additional reference object.
         */
        request<T>(cmd: string, data: any, context?: HandlerInfoContract | undefined | null, info?: HandlerInfoContract | undefined | null, ref?: any): Promise<T>;
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

    interface BaseHandlerRequestOptionsContract {

        /**
         * The context from request. It will transfer to response.
         */
        context?: HandlerInfoContract;

        /**
         * Additional reference container. The caller will fill the properties after occur.
         */
        ref?: HandlerProcessingReferenceContract;
    }

    interface HandlerProcessingReferenceContract {

        /**
         * The operation trace identifier.
         */
        trace?: string | undefined;

        /**
         * The response data.
         */
        response?: HandlerResponseContract<any> | undefined;
    }

    interface FileInfoContract {

        /**
         * The file name with extension. 
         */
        name: string;

        /**
         * The last modification date time. 
         */
        modified: string;

        /**
         * The creation date time.
         */
        created: string;

        /**
         * The file system attributes.
         */
        attr: string;

        /**
         * The file system type.
         */
        type?: "file" | "dir" | null;

        /**
         * The file size.
         */
        length?: number | null;

        /**
         * The link of the target path if available.
         */
        link?: string | null;

        /**
         * The path of the file.
         */
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

        /**
         * true if the default download list dialog is shown; otherwise, false.
         */
        dialog: boolean;

        /**
         * The maximum count requested.
         */
        max: number;

        /**
         * The download item collection
         */
        list: {
            uri: string;
            file: string;
            state: "InProgress" | "Interrupted" | "Completed" | string;
            received: number;
            length: number;
            interrupt: string | null;
            mime: string | null;
        }[];

        /**
         * The enumerated date time.
         */
        enumerated: string;
    }

    interface WindowStateInfoContract {

        /**
         * The window width.
         */
        width: number;

        /**
         * The window height.
         */
        height: number;

        /**
         * The top distance to screen (y).
         */
        top: number;

        /**
         * The left distance to screen (x).
         */
        left: number;

        /**
         * The window state.
         */
        state: "Maximized" | "Restored" | "Minimized" | "Fullscreen" | "Compact";

        /**
         * The window title.
         */
        title: string;
    }

    type FilePath = string | {

        /**
         * The type of parent directory.
         */
        parent?: null | undefined | "absolute" | "parent" | "asset" | "appdata" | "doc";

        /**
         * The path.
         */
        path: string | null;
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
    function getHandler(id: string): CommandHandlerContract;

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
        list(dir: string, options?: ({
            q?: string;
            showHidden?: boolean | null;
        } & BaseHandlerRequestOptionsContract) | string | undefined | null): HandlerResponse<FileListResponseContract>;

        /**
         * Lists drives.
         * @param options The options.
         */
        listDrives(options?: ({

            /**
             * true if only return fixed drive; otherwise, false.
             */
            fixed?: boolean | null;
        } & BaseHandlerRequestOptionsContract) | string | undefined | null): HandlerResponse<{
            drives: DriveInfoContract[]
        }>;

        /**
         * Gets the file information.
         * @param path The path of the file to get information.
         * @param options The options.
         */
        get(path: FilePath, options?: {

            /**
             * The type of content to read and parse.
             */
            read?: boolean | "none" | "text" | "json" | "base64";

            /**
             * The maximum file size to read. If the file is larger than this length, it will skip to read content.
             */
            maxLength?: number;
        } & BaseHandlerRequestOptionsContract): HandlerResponse<FileGetResponseContract>;

        /**
         * Writes a file.
         * @param path The path of the file to write.
         * @param value The content to write.
         * @param options The options.
         */
        write(path: FilePath, value: { property: string } | any[] | string | null, options?: {
        } & BaseHandlerRequestOptionsContract): HandlerResponse<{}>;

        /**
         * Moves a file or a directory.
         * @param path The source path of the file to move.
         * @param dest The destination path.
         * @param options The options.
         */
        move(path: FilePath, dest: FilePath, options?: ({

            /**
             * true if override the destination file if exists; otherwise, false.
             */
            override?: boolean;

            /**
             * true if the path is a directory path, that means it will move a directory but not a file; otherwise, false.
             */
            dir?: boolean;
        } & BaseHandlerRequestOptionsContract) | boolean): HandlerResponse<{}>;

        /**
         * Copies a file or a directory.
         * @param path The source path of the file to copy.
         * @param dest The destination path.
         * @param options The options.
         */
        copy(path: FilePath, dest: FilePath, options?: ({

            /**
             * true if override the destination file if exists; otherwise, false.
             */
            override?: boolean;

            /**
             * true if the path is a directory path, that means it will copy a directory but not a file; otherwise, false.
             */
            dir?: boolean;
        } & BaseHandlerRequestOptionsContract) | boolean): HandlerResponse<{}>;

        /**
         * Deletes a file or a directory.
         * @param path The source path of the file to delete.
         * @param options The options.
         */
        delete(path: FilePath, options?: {

            /**
             * true if the path is a directory path, that means it will delete a directory but not a file; otherwise, false.
             */
            dir?: boolean;
        } & BaseHandlerRequestOptionsContract): HandlerResponse<{}>;

        /**
         * Makes a directory.
         * @param path The source path of the file to move.
         * @param options The options.
         */
        md(path: FilePath, options?: {
        } & BaseHandlerRequestOptionsContract): HandlerResponse<{
            existed: boolean;
            info: FileInfoContract;
        }>;

        /**
         * Opens a file by the default app; or opens the folder; or runs an app.
         * @param path The path of the file or folder to open.
         * @param options The options.
         */
        open(path: FilePath, options?: {

            /**
             * The command arguments.
             */
            args?: string | null;

            /**
             * The type of path to open.
             */
            type?: "file" | "dir" | "url" | "exe" | null;
        } & BaseHandlerRequestOptionsContract): HandlerResponse<{}>;

        /**
         * Compresses a folder content.
         * @param path The directory path to compress.
         * @param dest The destination path of the zip file.
         * @param options The options.
         */
        zip(path: FilePath, dest: FilePath, options?: {

            /**
             * true if override the destination file if exists; otherwise, false.
             */
            override?: boolean;

            /**
             * true if include the directory itself; otherwise, false. 
             */
            folder?: boolean;
        } & BaseHandlerRequestOptionsContract): HandlerResponse<{}>;

        /**
         * Extracts a zip file.
         * @param path The directory path to compress.
         * @param dest The destination path of the zip file.
         * @param options The options.
         */
        unzip(path: FilePath, dest: FilePath, options?: {

            /**
             * true if override the destination file if exists; otherwise, false.
             */
            override?: boolean;
        } & BaseHandlerRequestOptionsContract): HandlerResponse<{}>;

        /**
         * Lists the download items.
         * @param options The options.
         */
        listDownload(options?: boolean | number | {

            /**
             * true if display the default download list dialog; otherwise, false.
             */
            open?: boolean;

            /**
             * The maximum count of item returned.
             */
            max?: number;
        } & BaseHandlerRequestOptionsContract): HandlerResponse<DownloadListContract>;
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
        encrypt(alg: "aes" | "3des" | "rc2" | "des", value: string, key: string, iv: string, options?: {
        } & BaseHandlerRequestOptionsContract): HandlerResponse<{
            value: string;
            algorithm: string;
            encode: "base64";
        }>;

        /**
         * Decrypts by symmetric algorithm.
         * @param alg The algorithm name.
         * @param value The value to decrypt.
         * @param key The key.
         * @param iv The IV.
         * @param options The options.
         */
        decrypt(alg: "aes" | "3des" | "rc2" | "des", value: string, key: string, iv: string, options?: {
        } & BaseHandlerRequestOptionsContract): HandlerResponse<{
            value: string;
            algorithm: string;
            encode: "base64";
        }>;

        /**
         * Creates an RSA parameter.
         * @param options The options.
         */
        rsaCreate(options?: {

            /**
             * The RSA PEM.
             */
            pem?: string;
        } & BaseHandlerRequestOptionsContract): HandlerResponse<{
            publicPem: string;
            privatePem: string;
            publicParams: string;
            privateParams: string;
            modulus: string;
            exponent: string;
            includePrivate: boolean;
        }>;

        /**
         * Encrypts by RSA algorithm.
         * @param options The options.
         * @param value The value to encrypt.
         * @param pem The private key.
         */
        rsaEncrypt(value: string, pem: string, options?: {

            /**
             * The source type of value.
             */
            type?: "text" | "base64" | null;
        } & BaseHandlerRequestOptionsContract): HandlerResponse<{
            value: string;
            encode: "base64";
        }>;

        /**
         * Decrypts by RSA algorithm.
         * @param options The options.
         * @param value The value to decrypt.
         * @param pem The private key.
         */
        rsaDecrypt(value: string, pem: string, options?: {

            /**
             * The source type of value.
             */
            type?: "text" | "base64" | null;
        } & BaseHandlerRequestOptionsContract): HandlerResponse<{
            value: string;
            encode: "base64";
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

            /**
             * The source type of value.
             */
            type?: "text" | "file" | "base64" | null;
        } & BaseHandlerRequestOptionsContract): HandlerResponse<{
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

            /**
             * The source type of value.
             */
            type?: "text" | "file" | "base64" | null;
        } & BaseHandlerRequestOptionsContract): HandlerResponse<{
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
        hash(alg: "sha256" | "sha384" | "sha512" | "md5", value: string, options?: {

            /**
             * The test result.
             */
            test?: string;

            /**
             * The source type of value.
             */
            type?: "text" | "file" | "base64" | null;
        } & BaseHandlerRequestOptionsContract): HandlerResponse<{
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

    /**
     * The host app API set.
     * */
    const hostApp: {

        /**
         * Gets theme information.
         * @param options The options.
         */
        theme(options?: {
        } & BaseHandlerRequestOptionsContract): HandlerResponse<{
            brightness: "dark" | "light";
        }>;

        /**
         * Checks update.
         * @param options The options.
         */
        checkUpdate(options?: {

            /**
             * true if check update now; false if return current state; or the specific update service URL.
             */
            check?: boolean | string | null,
        } & BaseHandlerRequestOptionsContract): HandlerResponse<{
            version: string;
            has: boolean;
        }>;

        /**
         * Gets or sets the window state.
         * @param value The optional state to set.
         */
        window(value?: WindowStates | ({

            /**
             * The window state.
             */
            state?: WindowStates;

            /**
             * The window width.
             */
            width?: number;

            /**
             * The window height.
             */
            height?: number;

            /**
             * The top distance to screen (y).
             */
            top?: number;

            /**
             * The left distance to screen (x).
             */
            left?: number;

            /**
             * true if focus the window by programming; otherwise, false.
             */
            focus?: boolean;

            /**
             * true if the pixel data is based on physical; otherwise, false.
             */
            physical?: boolean;
        } & BaseHandlerRequestOptionsContract) | null): HandlerResponse<WindowStateInfoContract>;

        /**
         * Lists information of all command handler.
         * @param options The options.
         */
        handlers(options?: {
        } & BaseHandlerRequestOptionsContract): HandlerResponse<{
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
