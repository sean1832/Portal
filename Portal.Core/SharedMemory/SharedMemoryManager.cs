using System;
using System.IO;
using System.IO.MemoryMappedFiles;
using System.Threading;
using Newtonsoft.Json.Bson;

namespace Portal.Core.SharedMemory
{
    public class SharedMemoryManager : IDisposable
    {
        private readonly string _name;
        private FileStream _fileStream;
        private Mutex _mutex;
        private const long DefaultCapacity = 1024 * 1024; // 1 MB default capacity
        private bool _disposed;

        public SharedMemoryManager(string name)
        {
            _name = name;
            _mutex = new Mutex(false, $"Global\\{_name}Mutex");
        }

        private string FilePath => Path.Combine(Path.GetTempPath(), $"{_name}.mmf");

        private void EnsureFileCreated(long requiredSize)
        {
            _mutex.WaitOne();
            try
            {
                if (_fileStream == null)
                {
                    _fileStream = new FileStream(FilePath, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.ReadWrite);
                }

                if (_fileStream.Length < requiredSize)
                {
                    _fileStream.SetLength(requiredSize);
                }
            }
            finally
            {
                _mutex.ReleaseMutex();
            }
        }

        public void Write(byte[] data, long offset, int count)
        {
            EnsureFileCreated(offset + count);
            _mutex.WaitOne();
            try
            {
                _fileStream.Position = offset;
                _fileStream.Write(data, 0, count);
                _fileStream.Flush();
            }
            finally
            {
                _mutex.ReleaseMutex();
            }
        }

        public byte[] ReadRange(long offset, int count)
        {
            EnsureFileCreated(offset + count);
            _mutex.WaitOne();
            try
            {
                byte[] buffer = new byte[count];
                _fileStream.Position = offset;
                _fileStream.Read(buffer, 0, count);
                return buffer;
            }
            finally
            {
                _mutex.ReleaseMutex();
            }
        }

        public void DeleteFile()
        {
            if (_mutex == null) return; // Already disposed

            _mutex.WaitOne();
            try
            {
                if (_fileStream != null)
                {
                    _fileStream.Dispose();
                    _fileStream = null;
                }

                if (File.Exists(FilePath))
                {
                    File.Delete(FilePath);
                }
            }
            finally
            {
                _mutex.ReleaseMutex();
            }

            // Dispose after releasing the mutex
            Dispose(true);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (_disposed) return;
            if (disposing)
            {
                _fileStream?.Dispose();
                _fileStream = null;
                _mutex?.Dispose();
                _mutex = null;
            }
            _disposed = true;
        }

        ~SharedMemoryManager()
        {
            Dispose(false);
        }
    }

}
