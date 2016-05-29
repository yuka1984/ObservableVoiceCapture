using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VoiceRecorder.Dependency
{
    public interface IFile
    {
        /// <summary>
        /// Get File Stream
        /// </summary>
        /// <param name="fileName"></param>
        /// <param name="storageType"></param>
        /// <param name="fileMode"></param>
        /// <param name="fileAccess"></param>
        /// <param name="fileShare"></param>
        /// <returns></returns>
        Stream GetStream(string fileName, StorageType storageType, FileMode fileMode, FileAccess fileAccess, FileShare fileShare);

        /// <summary>
        /// Delete File
        /// </summary>
        /// <param name="fileName"></param>
        /// <param name="storageType"></param>
        void Delete(string fileName, StorageType storageType);

        /// <summary>
        /// Check Exists File
        /// </summary>
        /// <param name="fileName"></param>
        /// <param name="storageType"></param>
        /// <returns></returns>
        bool Exsists(string fileName, StorageType storageType);

        /// <summary>
        /// Get File list
        /// </summary>
        /// <param name="storageType"></param>
        /// <returns></returns>
        FileInfo[] GetFiles(StorageType storageType);

        /// <summary>
        /// Open wav File Other App
        /// </summary>
        /// <param name="fileName"></param>
        void OpenWavFile(string fileName);
    }

    public class FileInfo
    {
        public string Extension { get; set; }
        public string FullName { get; set; }
        public DateTime LastAccessTimeUtc { get; set; }
        public DateTime CreationTimeUtc { get; set; }
        public DateTime LastWriteTimeUtc { get; set; }
        public long Length { get; set; }
        public string Name { get; set; }

    }
    [System.Runtime.InteropServices.ComVisible(true)]
    public enum FileMode
    {
        // Creates a new file. An exception is raised if the file already exists.
        CreateNew = 1,

        // Creates a new file. If the file already exists, it is overwritten.
        Create = 2,

        // Opens an existing file. An exception is raised if the file does not exist.
        Open = 3,

        // Opens the file if it exists. Otherwise, creates a new file.
        OpenOrCreate = 4,

        // Opens an existing file. Once opened, the file is truncated so that its
        // size is zero bytes. The calling process must open the file with at least
        // WRITE access. An exception is raised if the file does not exist.
        Truncate = 5,

        // Opens the file if it exists and seeks to the end.  Otherwise, 
        // creates a new file.
        Append = 6,
    }

    public enum StorageType
    {
        Local, Shared
    }

    // You can have Read, Write or ReadWrite access.
    [Flags]
    [System.Runtime.InteropServices.ComVisible(true)]
    public enum FileAccess
    {
        // Specifies read access to the file. Data can be read from the file and
        // the file pointer can be moved. Combine with WRITE for read-write access.
        Read = 1,

        // Specifies write access to the file. Data can be written to the file and
        // the file pointer can be moved. Combine with READ for read-write access.
        Write = 2,

        // Specifies read and write access to the file. Data can be written to the
        // file and the file pointer can be moved. Data can also be read from the 
        // file.
        ReadWrite = 3,
    }
    // Contains constants for controlling file sharing options while
    // opening files.  You can specify what access other processes trying
    // to open the same file concurrently can have.
    //
    // Note these values currently match the values for FILE_SHARE_READ,
    // FILE_SHARE_WRITE, and FILE_SHARE_DELETE in winnt.h
    [Flags]
    [System.Runtime.InteropServices.ComVisible(true)]
    public enum FileShare
    {
        // No sharing. Any request to open the file (by this process or another
        // process) will fail until the file is closed.
        None = 0,

        // Allows subsequent opening of the file for reading. If this flag is not
        // specified, any request to open the file for reading (by this process or
        // another process) will fail until the file is closed.
        Read = 1,

        // Allows subsequent opening of the file for writing. If this flag is not
        // specified, any request to open the file for writing (by this process or
        // another process) will fail until the file is closed.
        Write = 2,

        // Allows subsequent opening of the file for writing or reading. If this flag
        // is not specified, any request to open the file for writing or reading (by
        // this process or another process) will fail until the file is closed.
        ReadWrite = 3,

        // Open the file, but allow someone else to delete the file.
        Delete = 4,

        // Whether the file handle should be inheritable by child processes.
        // Note this is not directly supported like this by Win32.
        Inheritable = 0x10,
    }
}
