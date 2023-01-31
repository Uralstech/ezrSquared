using ezrSquared.Values;
using ezrSquared.Helpers;
using ezrSquared.Errors;
using ezrSquared.General;
using static ezrSquared.Constants.constants;
using System.IO;
using System;

namespace ezrSquared.Libraries.IO
{
    public class @file : baseFunction
    {
        public @file() : base("<io <file>>") { }

        public override runtimeResult execute(item[] args)
        {
            context internalContext = base.generateContext();
            internalContext.symbolTable.set("exists", new predefined_function("file_exists", fileExists, new string[1] { "filepath" }));
            internalContext.symbolTable.set("create", new predefined_function("file_create", createFile, new string[1] { "filepath" }));
            internalContext.symbolTable.set("delete", new predefined_function("file_delete", deleteFile, new string[1] { "filepath" }));
            internalContext.symbolTable.set("read", new predefined_function("file_read", readFile, new string[1] { "filepath" }));
            internalContext.symbolTable.set("write", new predefined_function("file_write", writeFile, new string[3] { "contents", "filepath", "mode" }));
            internalContext.symbolTable.set("copy", new predefined_function("file_copy", copyFile, new string[2] { "from_path", "to_path" }));
            internalContext.symbolTable.set("move", new predefined_function("file_move", moveFile, new string[2] { "from_path", "to_path" }));

            return new runtimeResult().success(new @object(name, internalContext).setPosition(startPos, endPos).setContext(context));
        }

        private runtimeResult fileExists(context context, position[] positions)
        {
            runtimeResult result = new runtimeResult();

            item path = context.symbolTable.get("filepath");
            if (path is not @string && path is not character_list)
                return result.failure(new runtimeError(positions[0], positions[1], RT_TYPE, "Filepath must be a string or character_list", context));

            string filepath = (path is @string) ? ((@string)path).storedValue.ToString() : string.Join("", ((character_list)path).storedValue);
            return result.success(new boolean(File.Exists(filepath)));
        }

        private runtimeResult createFile(context context, position[] positions)
        {
            runtimeResult result = new runtimeResult();

            item path = context.symbolTable.get("filepath");
            if (path is not @string && path is not character_list)
                return result.failure(new runtimeError(positions[0], positions[1], RT_TYPE, "Filepath must be a string or character_list", context));

            string filepath = (path is @string) ? ((@string)path).storedValue.ToString() : string.Join("", ((character_list)path).storedValue);

            try
            {
                File.Create(filepath).Close();
            }
            catch (IOException exception)
            {
                return result.failure(new runtimeError(positions[0], positions[1], RT_IO, $"Failed to create file \"{filepath}\"\n{exception.Message}", context));
            }

            return result.success(new nothing());
        }

        private runtimeResult deleteFile(context context, position[] positions)
        {
            runtimeResult result = new runtimeResult();

            item path = context.symbolTable.get("filepath");
            if (path is not @string && path is not character_list)
                return result.failure(new runtimeError(positions[0], positions[1], RT_TYPE, "Filepath must be a string or character_list", context));

            string filepath = (path is @string) ? ((@string)path).storedValue.ToString() : string.Join("", ((character_list)path).storedValue);
            if (!File.Exists(filepath))
                return result.failure(new runtimeError(positions[0], positions[1], RT_IO, $"File \"{filepath}\" does not exist", context));

            try
            {
                File.Delete(filepath);
            }
            catch (IOException exception)
            {
                return result.failure(new runtimeError(positions[0], positions[1], RT_IO, $"Failed to delete file \"{filepath}\"\n{exception.Message}", context));
            }

            return result.success(new nothing());
        }

        private runtimeResult readFile(context context, position[] positions)
        {
            runtimeResult result = new runtimeResult();

            item path = context.symbolTable.get("filepath");
            if (path is not @string && path is not character_list)
                return result.failure(new runtimeError(positions[0], positions[1], RT_TYPE, "Filepath must be a string or character_list", context));

            string filepath = (path is @string) ? ((@string)path).storedValue.ToString() : string.Join("", ((character_list)path).storedValue);
            if (!File.Exists(filepath))
                return result.failure(new runtimeError(positions[0], positions[1], RT_IO, $"File \"{filepath}\" does not exist", context));

            string[] contents;
            try
            {
                contents = File.ReadAllLines(filepath);
            }
            catch (IOException exception)
            {
                return result.failure(new runtimeError(positions[0], positions[1], RT_IO, $"Failed to load file \"{filepath}\"\n{exception.Message}", context));
            }

            item[] contentsAsString = new item[contents.Length];
            for (int i = 0; i < contentsAsString.Length; i++)
                contentsAsString[i] = new @string(contents[i]).setPosition(positions[0], positions[1]).setContext(context);
            return result.success(new array(contentsAsString));
        }

        private runtimeResult writeFile(context context, position[] positions)
        {
            runtimeResult result = new runtimeResult();

            item content = context.symbolTable.get("contents");
            if (content is not @string && content is not character_list)
                return result.failure(new runtimeError(positions[0], positions[1], RT_TYPE, "Contents must be a string or character_list", context));
            item path = context.symbolTable.get("filepath");
            if (path is not @string && path is not character_list)
                return result.failure(new runtimeError(positions[0], positions[1], RT_TYPE, "Filepath must be a string or character_list", context));
            item mode = context.symbolTable.get("mode");
            if (mode is not @string && mode is not character_list)
                return result.failure(new runtimeError(positions[0], positions[1], RT_TYPE, "Mode must be a string or character_list", context));

            string content_ = (content is @string) ? ((@string)content).storedValue.ToString() : string.Join("", ((character_list)content).storedValue);
            string filepath = (path is @string) ? ((@string)path).storedValue.ToString() : string.Join("", ((character_list)path).storedValue);
            string mode_ = ((mode is @string) ? ((@string)mode).storedValue.ToString() : string.Join("", ((character_list)mode).storedValue)).ToLower();

            if (mode_ == "write")
            {
                try
                {
                    File.WriteAllText(filepath, content_);
                }
                catch (IOException exception)
                {
                    return result.failure(new runtimeError(positions[0], positions[1], RT_IO, $"Failed to write to file \"{filepath}\"\n{exception.Message}", context));
                }
            }
            else if (mode_ == "append")
            {
                try
                {
                    File.AppendAllText(filepath, content_);
                }
                catch (IOException exception)
                {
                    return result.failure(new runtimeError(positions[0], positions[1], RT_IO, $"Failed to append to file \"{filepath}\"\n{exception.Message}", context));
                }
            }
            else
                return result.failure(new runtimeError(positions[0], positions[1], RT_TYPE, "Mode must be string/character_list literal \"write\" or \"append\"", context));

            return result.success(new nothing());
        }

        private runtimeResult copyFile(context context, position[] positions)
        {
            runtimeResult result = new runtimeResult();

            item from = context.symbolTable.get("from_path");
            if (from is not @string && from is not character_list)
                return result.failure(new runtimeError(positions[0], positions[1], RT_TYPE, "From_path must be a string or character_list", context));
            item to = context.symbolTable.get("to_path");
            if (to is not @string && to is not character_list)
                return result.failure(new runtimeError(positions[0], positions[1], RT_TYPE, "To_path must be a string or character_list", context));

            string from_ = (from is @string) ? ((@string)from).storedValue.ToString() : string.Join("", ((character_list)from).storedValue);
            string to_ = (to is @string) ? ((@string)to).storedValue.ToString() : string.Join("", ((character_list)to).storedValue);

            if (!File.Exists(from_))
                return result.failure(new runtimeError(positions[0], positions[1], RT_IO, $"File \"{from_}\" does not exist", context));

            try
            {
                File.Copy(from_, to_);
            }
            catch (IOException exception)
            {
                return result.failure(new runtimeError(positions[0], positions[1], RT_IO, $"Failed to copy contents of file \"{from_}\" to \"{to_}\"\n{exception.Message}", context));
            }

            return result.success(new nothing());
        }

        private runtimeResult moveFile(context context, position[] positions)
        {
            runtimeResult result = new runtimeResult();

            item from = context.symbolTable.get("from_path");
            if (from is not @string && from is not character_list)
                return result.failure(new runtimeError(positions[0], positions[1], RT_TYPE, "From_path must be a string or character_list", context));
            item to = context.symbolTable.get("to_path");
            if (to is not @string && to is not character_list)
                return result.failure(new runtimeError(positions[0], positions[1], RT_TYPE, "To_path must be a string or character_list", context));

            string from_ = (from is @string) ? ((@string)from).storedValue.ToString() : string.Join("", ((character_list)from).storedValue);
            string to_ = (to is @string) ? ((@string)to).storedValue.ToString() : string.Join("", ((character_list)to).storedValue);

            if (!File.Exists(from_))
                return result.failure(new runtimeError(positions[0], positions[1], RT_IO, $"File \"{from_}\" does not exist", context));

            try
            {
                File.Move(from_, to_);
            }
            catch (IOException exception)
            {
                return result.failure(new runtimeError(positions[0], positions[1], RT_IO, $"Failed to move file \"{from_}\" to \"{to_}\"\n{exception.Message}", context));
            }

            return result.success(new nothing());
        }

        public override item copy() { return new @file().setPosition(startPos, endPos).setContext(context); }

        public override string ToString() { return $"<builtin library {name}>"; }
        public override bool ItemEquals(item obj, out error? error) { error = null; return obj is @file; }
    }

    public class folder : baseFunction
    {
        public folder() : base("<io <folder>>") { }

        public override runtimeResult execute(item[] args)
        {
            context internalContext = base.generateContext();
            internalContext.symbolTable.set("exists", new predefined_function("folder_exists", folderExists, new string[1] { "folderpath" }));
            internalContext.symbolTable.set("create", new predefined_function("folder_create", createFolder, new string[1] { "folderpath" }));
            internalContext.symbolTable.set("delete", new predefined_function("folder_delete", deleteFolder, new string[1] { "folderpath" }));
            internalContext.symbolTable.set("subfolders_in", new predefined_function("folder_subfolders_in", subFolders, new string[1] { "folderpath" }));
            internalContext.symbolTable.set("files_in", new predefined_function("folder_files_in", filesInFolder, new string[1] { "folderpath" }));
            internalContext.symbolTable.set("files_and_subfolders_in", new predefined_function("folder_files_and_subfolders_in", filesAndSubFolders, new string[1] { "folderpath" }));
            internalContext.symbolTable.set("parent_of", new predefined_function("folder_parent_of", folderParent, new string[1] { "folderpath" }));
            internalContext.symbolTable.set("root_of", new predefined_function("folder_root_of", folderRoot, new string[1] { "folderpath" }));
            internalContext.symbolTable.set("current", new predefined_function("folder_current", currentFolder, new string[0]));
            internalContext.symbolTable.set("set_current", new predefined_function("folder_set_current", setCurrentFolder, new string[1] { "folderpath" }));
            internalContext.symbolTable.set("move", new predefined_function("folder_move", moveFolder, new string[2] { "from_path", "to_path" }));

            return new runtimeResult().success(new @object(name, internalContext).setPosition(startPos, endPos).setContext(context));
        }

        private runtimeResult folderExists(context context, position[] positions)
        {
            runtimeResult result = new runtimeResult();

            item path = context.symbolTable.get("folderpath");
            if (path is not @string && path is not character_list)
                return result.failure(new runtimeError(positions[0], positions[1], RT_TYPE, "Folderpath must be a string or character_list", context));

            string filepath = (path is @string) ? ((@string)path).storedValue.ToString() : string.Join("", ((character_list)path).storedValue);
            return result.success(new boolean(Directory.Exists(filepath)));
        }

        private runtimeResult createFolder(context context, position[] positions)
        {
            runtimeResult result = new runtimeResult();

            item path = context.symbolTable.get("folderpath");
            if (path is not @string && path is not character_list)
                return result.failure(new runtimeError(positions[0], positions[1], RT_TYPE, "Folderpath must be a string or character_list", context));

            string filepath = (path is @string) ? ((@string)path).storedValue.ToString() : string.Join("", ((character_list)path).storedValue);

            try
            {
                Directory.CreateDirectory(filepath);
            }
            catch (IOException exception)
            {
                return result.failure(new runtimeError(positions[0], positions[1], RT_IO, $"Failed to create folder \"{filepath}\"\n{exception.Message}", context));
            }

            return result.success(new nothing());
        }

        private runtimeResult deleteFolder(context context, position[] positions)
        {
            runtimeResult result = new runtimeResult();

            item path = context.symbolTable.get("folderpath");
            if (path is not @string && path is not character_list)
                return result.failure(new runtimeError(positions[0], positions[1], RT_TYPE, "Folderpath must be a string or character_list", context));

            string filepath = (path is @string) ? ((@string)path).storedValue.ToString() : string.Join("", ((character_list)path).storedValue);
            if (!Directory.Exists(filepath))
                return result.failure(new runtimeError(positions[0], positions[1], RT_IO, $"Folder \"{filepath}\" does not exist", context));

            try
            {
                Directory.Delete(filepath);
            }
            catch (IOException exception)
            {
                return result.failure(new runtimeError(positions[0], positions[1], RT_IO, $"Failed to delete folder \"{filepath}\"\n{exception.Message}", context));
            }

            return result.success(new nothing());
        }

        private runtimeResult subFolders(context context, position[] positions)
        {
            runtimeResult result = new runtimeResult();

            item path = context.symbolTable.get("folderpath");
            if (path is not @string && path is not character_list)
                return result.failure(new runtimeError(positions[0], positions[1], RT_TYPE, "Folderpath must be a string or character_list", context));

            string filepath = (path is @string) ? ((@string)path).storedValue.ToString() : string.Join("", ((character_list)path).storedValue);
            if (!Directory.Exists(filepath))
                return result.failure(new runtimeError(positions[0], positions[1], RT_IO, $"Folder \"{filepath}\" does not exist", context));

            string[] subDirectories;
            try
            {
                subDirectories = Directory.GetDirectories(filepath);
            }
            catch (IOException exception)
            {
                return result.failure(new runtimeError(positions[0], positions[1], RT_IO, $"Failed to access subfolders in folder \"{filepath}\"\n{exception.Message}", context));
            }

            item[] subDirectoriesAsString = new item[subDirectories.Length];
            for (int i = 0; i < subDirectories.Length; i++)
                subDirectoriesAsString[i] = new @string(subDirectories[i]).setPosition(positions[0], positions[1]).setContext(context);
            return result.success(new array(subDirectoriesAsString));
        }
        
        private runtimeResult filesInFolder(context context, position[] positions)
        {
            runtimeResult result = new runtimeResult();

            item path = context.symbolTable.get("folderpath");
            if (path is not @string && path is not character_list)
                return result.failure(new runtimeError(positions[0], positions[1], RT_TYPE, "Folderpath must be a string or character_list", context));

            string filepath = (path is @string) ? ((@string)path).storedValue.ToString() : string.Join("", ((character_list)path).storedValue);
            if (!Directory.Exists(filepath))
                return result.failure(new runtimeError(positions[0], positions[1], RT_IO, $"Folder \"{filepath}\" does not exist", context));

            string[] subFiles;
            try
            {
                subFiles = Directory.GetFiles(filepath);
            }
            catch (IOException exception)
            {
                return result.failure(new runtimeError(positions[0], positions[1], RT_IO, $"Failed to access files in folder \"{filepath}\"\n{exception.Message}", context));
            }

            item[] subFilesAsString = new item[subFiles.Length];
            for (int i = 0; i < subFiles.Length; i++)
                subFilesAsString[i] = new @string(subFiles[i]).setPosition(positions[0], positions[1]).setContext(context);
            return result.success(new array(subFilesAsString));
        }

        private runtimeResult filesAndSubFolders(context context, position[] positions)
        {
            runtimeResult result = new runtimeResult();

            item path = context.symbolTable.get("folderpath");
            if (path is not @string && path is not character_list)
                return result.failure(new runtimeError(positions[0], positions[1], RT_TYPE, "Folderpath must be a string or character_list", context));

            string filepath = (path is @string) ? ((@string)path).storedValue.ToString() : string.Join("", ((character_list)path).storedValue);
            if (!Directory.Exists(filepath))
                return result.failure(new runtimeError(positions[0], positions[1], RT_IO, $"Folder \"{filepath}\" does not exist", context));

            string[] subFilesAndDirectories;
            try
            {
                subFilesAndDirectories = Directory.GetFileSystemEntries(filepath);
            }
            catch (IOException exception)
            {
                return result.failure(new runtimeError(positions[0], positions[1], RT_IO, $"Failed to access subfolders and files in folder \"{filepath}\"\n{exception.Message}", context));
            }

            item[] subFilesAndDirectoriesAsString = new item[subFilesAndDirectories.Length];
            for (int i = 0; i < subFilesAndDirectories.Length; i++)
                subFilesAndDirectoriesAsString[i] = new @string(subFilesAndDirectories[i]).setPosition(positions[0], positions[1]).setContext(context);
            return result.success(new array(subFilesAndDirectoriesAsString));
        }

        private runtimeResult folderParent(context context, position[] positions)
        {
            runtimeResult result = new runtimeResult();

            item path = context.symbolTable.get("folderpath");
            if (path is not @string && path is not character_list)
                return result.failure(new runtimeError(positions[0], positions[1], RT_TYPE, "Folderpath must be a string or character_list", context));

            string filepath = (path is @string) ? ((@string)path).storedValue.ToString() : string.Join("", ((character_list)path).storedValue);
            if (!Directory.Exists(filepath))
                return result.failure(new runtimeError(positions[0], positions[1], RT_IO, $"Folder \"{filepath}\" does not exist", context));

            DirectoryInfo? parentInfo;
            try
            {
                parentInfo = Directory.GetParent(filepath);
            }
            catch (IOException exception)
            {
                return result.failure(new runtimeError(positions[0], positions[1], RT_IO, $"Failed to access parent of folder \"{filepath}\"\n{exception.Message}", context));
            }

            return result.success((parentInfo != null) ? new @string(parentInfo.Name) : new nothing());
        }

        private runtimeResult folderRoot(context context, position[] positions)
        {
            runtimeResult result = new runtimeResult();

            item path = context.symbolTable.get("folderpath");
            if (path is not @string && path is not character_list)
                return result.failure(new runtimeError(positions[0], positions[1], RT_TYPE, "Folderpath must be a string or character_list", context));

            string filepath = (path is @string) ? ((@string)path).storedValue.ToString() : string.Join("", ((character_list)path).storedValue);
            if (!Directory.Exists(filepath))
                return result.failure(new runtimeError(positions[0], positions[1], RT_IO, $"Folder \"{filepath}\" does not exist", context));

            string root;
            try
            {
                root = Directory.GetDirectoryRoot(filepath);
            }
            catch (IOException exception)
            {
                return result.failure(new runtimeError(positions[0], positions[1], RT_IO, $"Failed to access root of folder \"{filepath}\"\n{exception.Message}", context));
            }

            return result.success(new @string(root));
        }

        private runtimeResult moveFolder(context context, position[] positions)
        {
            runtimeResult result = new runtimeResult();

            item from = context.symbolTable.get("from_path");
            if (from is not @string && from is not character_list)
                return result.failure(new runtimeError(positions[0], positions[1], RT_TYPE, "From_path must be a string or character_list", context));
            item to = context.symbolTable.get("to_path");
            if (to is not @string && to is not character_list)
                return result.failure(new runtimeError(positions[0], positions[1], RT_TYPE, "To_path must be a string or character_list", context));

            string from_ = (from is @string) ? ((@string)from).storedValue.ToString() : string.Join("", ((character_list)from).storedValue);
            string to_ = (to is @string) ? ((@string)to).storedValue.ToString() : string.Join("", ((character_list)to).storedValue);

            if (!Directory.Exists(from_))
                return result.failure(new runtimeError(positions[0], positions[1], RT_IO, $"Folder \"{from_}\" does not exist", context));

            try
            {
                Directory.Move(from_, to_);
            }
            catch (IOException exception)
            {
                return result.failure(new runtimeError(positions[0], positions[1], RT_IO, $"Failed to move folder \"{from_}\" to \"{to_}\"\n{exception.Message}", context));
            }

            return result.success(new nothing());
        }

        private runtimeResult currentFolder(context context, position[] positions)
        {
            runtimeResult result = new runtimeResult();

            string filepath;
            try
            {
                filepath = Directory.GetCurrentDirectory();
            }
            catch (IOException exception)
            {
                return result.failure(new runtimeError(positions[0], positions[1], RT_IO, $"Failed to access current working directory\n{exception.Message}", context));
            }

            return result.success(new @string(filepath));
        }

        private runtimeResult setCurrentFolder(context context, position[] positions)
        {
            runtimeResult result = new runtimeResult();

            item path = context.symbolTable.get("folderpath");
            if (path is not @string && path is not character_list)
                return result.failure(new runtimeError(positions[0], positions[1], RT_TYPE, "Folderpath must be a string or character_list", context));

            string filepath = (path is @string) ? ((@string)path).storedValue.ToString() : string.Join("", ((character_list)path).storedValue);
            if (!Directory.Exists(filepath))
                return result.failure(new runtimeError(positions[0], positions[1], RT_IO, $"Folder \"{filepath}\" does not exist", context));

            try
            {
                Directory.SetCurrentDirectory(filepath);
            }
            catch (IOException exception)
            {
                return result.failure(new runtimeError(positions[0], positions[1], RT_IO, $"Failed to set current working directory to \"{filepath}\"\n{exception.Message}", context));
            }

            return result.success(new nothing());
        }

        public override item copy() { return new folder().setPosition(startPos, endPos).setContext(context); }

        public override string ToString() { return $"<builtin library {name}>"; }
        public override bool ItemEquals(item obj, out error? error) { error = null; return obj is folder; }
    }

    public class path : baseFunction
    {
        public path() : base("<io <path>>") { }

        public override runtimeResult execute(item[] args)
        {
            context internalContext = base.generateContext();

            internalContext.symbolTable.set("directory_separator", new @string(Path.DirectorySeparatorChar.ToString()));
            internalContext.symbolTable.set("alternate_directory_separator", new @string(Path.AltDirectorySeparatorChar.ToString()));
            internalContext.symbolTable.set("invalid_filename_characters", new @string(string.Join("", Path.GetInvalidFileNameChars())));
            internalContext.symbolTable.set("invalid_path_characters", new @string(string.Join("", Path.GetInvalidPathChars())));
            internalContext.symbolTable.set("exists", new predefined_function("path_exists", pathExists, new string[1] { "path" }));
            internalContext.symbolTable.set("join", new predefined_function("path_join", joinPaths, new string[1] { "paths" }));
            internalContext.symbolTable.set("combine", new predefined_function("path_combine", combinePaths, new string[2] { "path_1", "path_2" }));
            internalContext.symbolTable.set("has_extension", new predefined_function("path_has_extension", pathHasExtension, new string[1] { "path" }));
            internalContext.symbolTable.set("get_extension", new predefined_function("path_get_extension", pathExtension, new string[1] { "path" }));
            internalContext.symbolTable.set("set_extension", new predefined_function("path_set_extension", setPathExtension, new string[2] { "path", "extension" }));
            internalContext.symbolTable.set("get_folder", new predefined_function("path_get_folder", folderNameOfPath, new string[1] { "path" }));
            internalContext.symbolTable.set("get_file", new predefined_function("path_get_file", fileNameOfPath, new string[1] { "path" }));
            internalContext.symbolTable.set("get_file_without_extension", new predefined_function("path_get_file_without_extension", fileNameOfPathWithoutExtension, new string[1] { "path" }));
            internalContext.symbolTable.set("get_whole", new predefined_function("path_get_whole", fullPath, new string[1] { "path" }));
            internalContext.symbolTable.set("get_root", new predefined_function("path_get_root", pathRoot, new string[1] { "path" }));
            internalContext.symbolTable.set("create_temp_file", new predefined_function("path_create_temp_file", createTempFilePath, new string[0]));
            internalContext.symbolTable.set("temp", new predefined_function("path_temp", tempPath, new string[0]));
            internalContext.symbolTable.set("relative_path", new predefined_function("path_relative_path", relativePath, new string[2] { "relative_to", "path" }));
            internalContext.symbolTable.set("ends_in_folder_seperator", new predefined_function("path_ends_in_folder_seperator", pathEndsInFolderSeperator, new string[1] { "path" }));
            internalContext.symbolTable.set("remove_last_folder_seperator", new predefined_function("path_remove_last_folder_seperator", removeLastFolderSeperatorOfPath, new string[1] { "path" }));

            return new runtimeResult().success(new @object(name, internalContext).setPosition(startPos, endPos).setContext(context));
        }

        private runtimeResult pathExists(context context, position[] positions)
        {
            runtimeResult result = new runtimeResult();

            item path = context.symbolTable.get("path");
            if (path is not @string && path is not character_list)
                return result.failure(new runtimeError(positions[0], positions[1], RT_TYPE, "Path must be a string or character_list", context));

            string filepath = (path is @string) ? ((@string)path).storedValue.ToString() : string.Join("", ((character_list)path).storedValue);
            return result.success(new boolean(Path.Exists(filepath)));
        }

        private runtimeResult joinPaths(context context, position[] positions)
        {
            runtimeResult result = new runtimeResult();

            item paths = context.symbolTable.get("paths");
            if (paths is not list && paths is not array)
                return result.failure(new runtimeError(positions[0], positions[1], RT_TYPE, "Paths must be a list or array", context));

            item[] paths_ = (paths is list) ? ((list)paths).storedValue.ToArray() : ((array)paths).storedValue;
            string[] filepaths = new string[paths_.Length];
            for (int i = 0; i < paths_.Length; i++)
            {
                if (paths_[i] is not @string && paths_[i] is not character_list)
                    return result.failure(new runtimeError(positions[0], positions[1], RT_TYPE, "All elements of paths must be strings or character_lists", context));
                filepaths[i] = (paths_[i] is @string) ? ((@string)paths_[i]).storedValue : string.Join("", ((character_list)paths_[i]).storedValue);
            }

            return result.success(new @string(Path.Join(filepaths)));
        }

        private runtimeResult combinePaths(context context, position[] positions)
        {
            runtimeResult result = new runtimeResult();

            item path1 = context.symbolTable.get("path_1");
            if (path1 is not @string && path1 is not character_list)
                return result.failure(new runtimeError(positions[0], positions[1], RT_TYPE, "Path_1 must be a string or character_list", context));
            item path2 = context.symbolTable.get("path_2");
            if (path2 is not @string && path2 is not character_list)
                return result.failure(new runtimeError(positions[0], positions[1], RT_TYPE, "Path_2 must be a string or character_list", context));

            string filepath1 = (path1 is @string) ? ((@string)path1).storedValue.ToString() : string.Join("", ((character_list)path1).storedValue);
            string filepath2 = (path2 is @string) ? ((@string)path2).storedValue.ToString() : string.Join("", ((character_list)path2).storedValue);
            return result.success(new @string(Path.Combine(filepath1, filepath2)));
        }

        private runtimeResult pathHasExtension(context context, position[] positions)
        {
            runtimeResult result = new runtimeResult();

            item path = context.symbolTable.get("path");
            if (path is not @string && path is not character_list)
                return result.failure(new runtimeError(positions[0], positions[1], RT_TYPE, "Path must be a string or character_list", context));

            string filepath = (path is @string) ? ((@string)path).storedValue.ToString() : string.Join("", ((character_list)path).storedValue);
            return result.success(new boolean(Path.HasExtension(filepath)));
        }

        private runtimeResult pathExtension(context context, position[] positions)
        {
            runtimeResult result = new runtimeResult();

            item path = context.symbolTable.get("path");
            if (path is not @string && path is not character_list)
                return result.failure(new runtimeError(positions[0], positions[1], RT_TYPE, "Path must be a string or character_list", context));

            string filepath = (path is @string) ? ((@string)path).storedValue.ToString() : string.Join("", ((character_list)path).storedValue);

            string? extension = Path.GetExtension(filepath);
            return result.success((extension != null) ? new @string(extension) : new nothing());
        }

        private runtimeResult setPathExtension(context context, position[] positions)
        {
            runtimeResult result = new runtimeResult();

            item path = context.symbolTable.get("path");
            if (path is not @string && path is not character_list)
                return result.failure(new runtimeError(positions[0], positions[1], RT_TYPE, "Path must be a string or character_list", context));
            item extension = context.symbolTable.get("extension");
            if (extension is not @string && extension is not character_list)
                return result.failure(new runtimeError(positions[0], positions[1], RT_TYPE, "Extension must be a string or character_list", context));

            string filepath = (path is @string) ? ((@string)path).storedValue.ToString() : string.Join("", ((character_list)path).storedValue);
            string extension_ = (extension is @string) ? ((@string)extension).storedValue.ToString() : string.Join("", ((character_list)extension).storedValue);

            string? newPath = Path.ChangeExtension(filepath, extension_);
            return result.success((newPath != null) ? new @string(newPath) : new nothing());
        }

        private runtimeResult folderNameOfPath(context context, position[] positions)
        {
            runtimeResult result = new runtimeResult();

            item path = context.symbolTable.get("path");
            if (path is not @string && path is not character_list)
                return result.failure(new runtimeError(positions[0], positions[1], RT_TYPE, "Path must be a string or character_list", context));

            string filepath = (path is @string) ? ((@string)path).storedValue.ToString() : string.Join("", ((character_list)path).storedValue);

            string? folder;
            try
            {
                folder = Path.GetDirectoryName(filepath);
            }
            catch (Exception exception)
            {
                return result.failure(new runtimeError(positions[0], positions[1], RT_IO, $"Could not access directory name of path \"{filepath}\"\n\n{exception.Message}", context));
            }

            return result.success((folder != null) ? new @string(folder) : new nothing());
        }

        private runtimeResult fileNameOfPath(context context, position[] positions)
        {
            runtimeResult result = new runtimeResult();

            item path = context.symbolTable.get("path");
            if (path is not @string && path is not character_list)
                return result.failure(new runtimeError(positions[0], positions[1], RT_TYPE, "Path must be a string or character_list", context));

            string filepath = (path is @string) ? ((@string)path).storedValue.ToString() : string.Join("", ((character_list)path).storedValue);

            string? filename;
            try
            {
                filename = Path.GetFileName(filepath);
            }
            catch (Exception exception)
            {
                return result.failure(new runtimeError(positions[0], positions[1], RT_IO, $"Could not access file name of path \"{filepath}\"\n\n{exception.Message}", context));
            }

            return result.success((filename != null) ? new @string(filename) : new nothing());
        }

        private runtimeResult fileNameOfPathWithoutExtension(context context, position[] positions)
        {
            runtimeResult result = new runtimeResult();

            item path = context.symbolTable.get("path");
            if (path is not @string && path is not character_list)
                return result.failure(new runtimeError(positions[0], positions[1], RT_TYPE, "Path must be a string or character_list", context));

            string filepath = (path is @string) ? ((@string)path).storedValue.ToString() : string.Join("", ((character_list)path).storedValue);

            string? filename;
            try
            {
                filename = Path.GetFileNameWithoutExtension(filepath);
            }
            catch (Exception exception)
            {
                return result.failure(new runtimeError(positions[0], positions[1], RT_IO, $"Could not access file name of path \"{filepath}\"\n\n{exception.Message}", context));
            }

            return result.success((filename != null) ? new @string(filename) : new nothing());
        }

        private runtimeResult fullPath(context context, position[] positions)
        {
            runtimeResult result = new runtimeResult();

            item path = context.symbolTable.get("path");
            if (path is not @string && path is not character_list)
                return result.failure(new runtimeError(positions[0], positions[1], RT_TYPE, "Path must be a string or character_list", context));

            string filepath = (path is @string) ? ((@string)path).storedValue.ToString() : string.Join("", ((character_list)path).storedValue);

            string filepath_;
            try
            {
                filepath_ = Path.GetFullPath(filepath);
            }
            catch (Exception exception)
            {
                return result.failure(new runtimeError(positions[0], positions[1], RT_IO, $"Could not access full path of path \"{filepath}\"\n\n{exception.Message}", context));
            }

            return result.success(new @string(filepath_));
        }

        private runtimeResult pathRoot(context context, position[] positions)
        {
            runtimeResult result = new runtimeResult();

            item path = context.symbolTable.get("path");
            if (path is not @string && path is not character_list)
                return result.failure(new runtimeError(positions[0], positions[1], RT_TYPE, "Path must be a string or character_list", context));

            string filepath = (path is @string) ? ((@string)path).storedValue.ToString() : string.Join("", ((character_list)path).storedValue);

            string? filepath_;
            try
            {
                filepath_ = Path.GetPathRoot(filepath);
            }
            catch (Exception exception)
            {
                return result.failure(new runtimeError(positions[0], positions[1], RT_IO, $"Could not access root of path \"{filepath}\"\n\n{exception.Message}", context));
            }

            return result.success((filepath_ != null) ? new @string(filepath_) : new nothing());
        }

        private runtimeResult createTempFilePath(context context, position[] positions)
        {
            runtimeResult result = new runtimeResult();

            string filepath;
            try
            {
                filepath = Path.GetTempFileName();
            }
            catch (Exception exception)
            {
                return result.failure(new runtimeError(positions[0], positions[1], RT_IO, $"Could not create new temporary file\n\n{exception.Message}", context));
            }

            return result.success(new @string(filepath));
        }

        private runtimeResult tempPath(context context, position[] positions)
        {
            runtimeResult result = new runtimeResult();

            string filepath;
            try
            {
                filepath = Path.GetTempPath();
            }
            catch (Exception exception)
            {
                return result.failure(new runtimeError(positions[0], positions[1], RT_IO, $"Could not access Temp path\n\n{exception.Message}", context));
            }

            return result.success(new @string(filepath));
        }

        private runtimeResult relativePath(context context, position[] positions)
        {
            runtimeResult result = new runtimeResult();

            item relativeTo = context.symbolTable.get("relative_to");
            if (relativeTo is not @string && relativeTo is not character_list)
                return result.failure(new runtimeError(positions[0], positions[1], RT_TYPE, "Relative_to must be a string or character_list", context));
            item path = context.symbolTable.get("path");
            if (path is not @string && path is not character_list)
                return result.failure(new runtimeError(positions[0], positions[1], RT_TYPE, "Path must be a string or character_list", context));

            string relativeTo_ = (relativeTo is @string) ? ((@string)relativeTo).storedValue.ToString() : string.Join("", ((character_list)relativeTo).storedValue);
            string path_ = (path is @string) ? ((@string)path).storedValue.ToString() : string.Join("", ((character_list)path).storedValue);

            return result.success(new @string(Path.GetRelativePath(relativeTo_, path_)));
        }

        private runtimeResult pathEndsInFolderSeperator(context context, position[] positions)
        {
            runtimeResult result = new runtimeResult();

            item path = context.symbolTable.get("path");
            if (path is not @string && path is not character_list)
                return result.failure(new runtimeError(positions[0], positions[1], RT_TYPE, "Path must be a string or character_list", context));

            string filepath = (path is @string) ? ((@string)path).storedValue.ToString() : string.Join("", ((character_list)path).storedValue);
            return result.success(new boolean(Path.EndsInDirectorySeparator(filepath)));
        }

        private runtimeResult removeLastFolderSeperatorOfPath(context context, position[] positions)
        {
            runtimeResult result = new runtimeResult();

            item path = context.symbolTable.get("path");
            if (path is not @string && path is not character_list)
                return result.failure(new runtimeError(positions[0], positions[1], RT_TYPE, "Path must be a string or character_list", context));

            string filepath = (path is @string) ? ((@string)path).storedValue.ToString() : string.Join("", ((character_list)path).storedValue);
            return result.success(new @string(Path.TrimEndingDirectorySeparator(filepath)));
        }

        public override item copy() { return new path().setPosition(startPos, endPos).setContext(context); }

        public override string ToString() { return $"<builtin library {name}>"; }
        public override bool ItemEquals(item obj, out error? error) { error = null; return obj is path; }
    }
}