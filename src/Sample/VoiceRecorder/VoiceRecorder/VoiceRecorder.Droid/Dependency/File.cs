using System;
using System.IO;
using System.Linq;
using Android.Content;
using VoiceRecorder.Dependency;
using Xamarin.Forms;
using FileAccess = VoiceRecorder.Dependency.FileAccess;
using FileInfo = VoiceRecorder.Dependency.FileInfo;
using FileMode = VoiceRecorder.Dependency.FileMode;
using FileShare = VoiceRecorder.Dependency.FileShare;

namespace VoiceRecorder.Droid.Dependency
{
    public class File : IFile
    {
        private readonly string _localpath = Environment.GetFolderPath(Environment.SpecialFolder.Personal);

        private readonly string _sharedpathh =
            Path.Combine(Android.OS.Environment.GetExternalStoragePublicDirectory(Android.OS.Environment.DirectoryMusic).Path, "VoiceRecorder");

        public Stream GetStream(string fileName, StorageType storageType, FileMode fileMode, FileAccess fileAccess,
            FileShare fileShare)
        {
            return System.IO.File.Open(
                GetFilePath(fileName, storageType)
                , (System.IO.FileMode) (int) fileMode
                , (System.IO.FileAccess) (int) fileAccess
                , (System.IO.FileShare) (int) fileShare);
        }

        public void Delete(string fileName, StorageType storageType)
        {
            var path = GetFilePath(fileName, storageType);
            if (System.IO.File.Exists(path))
            {
                System.IO.File.Delete(GetFilePath(fileName, storageType));
            }           
        }

        public bool Exsists(string fileName, StorageType storageType)
            => System.IO.File.Exists(GetFilePath(fileName, storageType));

        public FileInfo[] GetFiles(StorageType storageType)
        {
            var path = storageType == StorageType.Local ? _localpath : _sharedpathh;
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
            var test = Directory.GetFiles(path);

            return Directory.GetFiles(path)
                .Select(x => new System.IO.FileInfo(x))
                .Select(x => new FileInfo
                {
                    CreationTimeUtc = x.CreationTimeUtc,
                    Extension = x.Extension,
                    FullName = x.FullName,
                    LastAccessTimeUtc = x.LastAccessTimeUtc,
                    LastWriteTimeUtc = x.LastWriteTimeUtc,
                    Length = x.Length,
                    Name = x.Name
                }).ToArray();
        }

        public void OpenWavFile(string fileName)
        {
            var file = new Java.IO.File(fileName);
            var intent = new Intent();
            intent.SetAction(Intent.ActionView);
            intent.SetDataAndType(Android.Net.Uri.FromFile(file), "audio/wav");
            intent.PutExtra(Intent.ExtraStream, Android.Net.Uri.FromFile(file));
            Forms.Context.StartActivity(intent);
        }

        private string GetFilePath(string fileName, StorageType storageType)
            => Path.Combine(storageType == StorageType.Local ? _localpath : _sharedpathh, fileName);
    }
}