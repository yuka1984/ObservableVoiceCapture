using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VoiceRecorder.Dependency
{
    public class MockFile : IFile
    {
        public Stream GetStream(string fileName, StorageType storageType, FileMode fileMode, FileAccess fileAccess, FileShare fileShare)
        {
            return new MemoryStream();
        }

        public void Delete(string fileName, StorageType storageType)
        {
            
        }

        public bool Exsists(string fileName, StorageType storageType)
        {
            return true;
        }

        public FileInfo[] GetFiles(StorageType storageType)
        {
            return Enumerable.Range(1, 10).Select(x => new FileInfo
            {
                CreationTimeUtc = DateTime.Now.AddSeconds(x).ToUniversalTime(),
                LastAccessTimeUtc = DateTime.MaxValue.ToUniversalTime(),
                LastWriteTimeUtc = DateTime.MinValue.ToUniversalTime(),
                Length = 1024,
                Extension = "wav",
                Name = $"TestName{x}",
                FullName = $"TestName{x}.wav"
            }).ToArray();
        }

        public void OpenWavFile(string fileName)
        {
            Debug.WriteLine($"Open File {fileName}");
        }
    }
}
