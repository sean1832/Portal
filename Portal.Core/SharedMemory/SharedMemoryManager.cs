using System;
using System.IO.MemoryMappedFiles;

namespace Portal.Core.SharedMemory
{
    public class SharedMemoryManager: IDisposable
    {
        private bool _disposed;
        private readonly string _name;
        private MemoryMappedFile _mmf;
        public SharedMemoryManager(string name)
        {
            _name = name;
        }

        public void Write(byte[] data)

        {
            Write(data, 0, data.Length);
        }

        public void Write(byte[] data, int start, int end)
        {
            _mmf ??= MemoryMappedFile.CreateOrOpen(_name, end);
            using var accessor = _mmf.CreateViewAccessor();

            // write
            accessor.WriteArray(start, data, 0, end);
        }

        public byte[] ReadRange(int start, int end)
        {
            // Open the memory-mapped file
            using MemoryMappedFile mmf = MemoryMappedFile.OpenExisting(_name);
            // Create a view accessor to read data
            using MemoryMappedViewAccessor accessor = mmf.CreateViewAccessor();

            byte[] buffer = new byte[end]; // Buffer size should match the capacity
            accessor.ReadArray(start, buffer, 0, end);
            return buffer;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        public void Dispose(bool disposing)
        {
            if (_disposed) return;
            if (disposing)
            {
                // Dispose managed resources
                _mmf?.Dispose();
                _mmf = null;
            }
            // Dispose unmanaged resources
            _disposed = true;
        }

        ~SharedMemoryManager()
        {
            Dispose(false);
        }
    }
}
