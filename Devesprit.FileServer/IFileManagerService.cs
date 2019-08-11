using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Threading.Tasks;

namespace Devesprit.FileServer
{
    [ServiceContract]
    public partial interface IFileManagerService
    {

        [OperationContract]
        Task<List<FileSystemEntries>> EnumerateDirectoryEntries(string path, string searchPattern, bool includeSubEntries, bool includeDownloadLink, TimeSpan downloadLinksExpire, int downloadCountLimit);

        [OperationContract]
        Task<List<FileSystemEntries>> EnumerateDirectories(string path, string searchPattern, bool includeSubEntries);

        [OperationContract]
        Task<List<FileSystemEntries>> EnumerateFiles(string path, string searchPattern, bool includeDownloadLink, TimeSpan downloadLinksExpire, int downloadCountLimit);

        [OperationContract]
        Task<FileSystemEntries> GetFileInfo(string path, bool includeDownloadLink, TimeSpan downloadLinksExpire, int downloadCountLimit);

        [OperationContract]
        Task<FileSystemEntries> GetDirectoryInfo(string path);

        [OperationContract]
        Task<bool> DeleteFile(string path);

        [OperationContract]
        Task<bool> SearchAndDeleteFile(string path, string fileName);

        [OperationContract]
        Task<bool> DeleteDirectory(string path);

        [OperationContract]
        Task<bool> CreateDirectory(string path);

        [OperationContract]
        Task<bool> CopyFile(string src, string dest, bool overWrite);

        [OperationContract]
        Task<bool> MoveFile(string src, string dest, bool overWrite);

        [OperationContract]
        Task<bool> CopyDirectory(string src, string dest, bool overWrite);

        [OperationContract]
        Task<bool> MoveDirectory(string src, string dest, bool overWrite);

        [OperationContract]
        Task<string> GenerateDownloadLink(string path, TimeSpan expireAfter, int downloadCountLimit);

        [OperationContract]
        Task<bool> CompressFile(string path, string saveTo, bool overwrite, string password, int compressionLevel, bool useBZip2);

        [OperationContract]
        Task<bool> CompressDirectory(string path, string saveTo, bool overwrite, string password, int compressionLevel, bool useBZip2);

        [OperationContract]
        Task<string> CalculateMd5Checksum(string path);
        
        [OperationContract]
        Task<string> UploadFileRequest(string fileName, string saveTo, bool overWrite);
    }


    [DataContract]
    public partial class FileSystemEntries
    {
        [DataMember]
        public string Name { get; set; }

        [DataMember]
        public string Path { get; set; }

        [DataMember]
        public DateTime CreateDateUtc { get; set; }

        [DataMember]
        public DateTime ModifiedDateUtc { get; set; }

        [DataMember]
        public FileSystemEntryType Type { get; set; }

        [DataMember]
        public string DisplaySize { get; set; }

        [DataMember]
        public long SizeInByte { get; set; }

        [DataMember]
        public List<FileSystemEntries> SubEntries { get; set; }

        [DataMember]
        public string DownloadLink { get; set; }
    }

    [DataContract]
    public enum FileSystemEntryType
    {
        [EnumMember]
        File,
        [EnumMember]
        Dir
    }
}
