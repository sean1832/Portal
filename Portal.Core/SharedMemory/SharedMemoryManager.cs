using Portal.Core.Binary;
using System;
using System.IO;
using System.IO.MemoryMappedFiles;
using System.Threading;

namespace Portal.Core.SharedMemory
{
    public class SharedMemoryManager : IDisposable
    {
        private readonly string _name;
        private MemoryMappedFile _mmf;
        private Mutex _mutex;
        private const long DefaultCapacity = 1024 * 1024; // 1 MB default capacity
        private bool _disposed;

        public SharedMemoryManager(string name, bool createNotExist = false)
        {
            _name = name;
            _mutex = new Mutex(false, $"Global\\{_name}Mutex");
            if (createNotExist)
            {
                CreateOrOpenMemoryMappedFile(DefaultCapacity);
            }
            else
            {
                OpenMemoryMappedFile();
            }

        }

        private void CreateOrOpenMemoryMappedFile(long capacity)
        {
            _mutex.WaitOne();
            try
            {
                if (_mmf == null)
                {
                    _mmf = MemoryMappedFile.CreateOrOpen(_name, capacity, MemoryMappedFileAccess.ReadWrite);
                }
            }
            finally
            {
                _mutex.ReleaseMutex();
            }
        }

        private void OpenMemoryMappedFile()
        {
            _mutex.WaitOne();
            try
            {
                if (_mmf == null)
                {
                    _mmf = MemoryMappedFile.OpenExisting(_name, MemoryMappedFileRights.ReadWrite);
                }
            }
            catch (FileNotFoundException)
            {
                // Handle the case where the file does not exist
                _mmf = null;
                throw new FileNotFoundException($"Memory mapped file '{_name}' not found.");
            }
            finally
            {
                _mutex.ReleaseMutex();
            }
        }

        public void Write(byte[] data, long offset, int count)
        {
            EnsureCapacity(offset + count);
            _mutex.WaitOne();
            try
            {
                using (var accessor = _mmf.CreateViewAccessor(offset, count, MemoryMappedFileAccess.Write))
                {
                    accessor.WriteArray(0, data, 0, count);
                }
            }
            finally
            {
                _mutex.ReleaseMutex();
            }
        }

        public byte[] ReadRange(long offset, int count)
        {
            EnsureCapacity(offset + count);
            _mutex.WaitOne();
            try
            {
                byte[] buffer = new byte[count];
                using (var accessor = _mmf.CreateViewAccessor(offset, count, MemoryMappedFileAccess.Read))
                {
                    accessor.ReadArray(0, buffer, 0, count);
                }
                return buffer;
            }
            finally
            {
                _mutex.ReleaseMutex();
            }
        }

        public byte[] ReadPacket()
        {
            // validate signature
            byte[] signature = ReadRange(0, 2);
            Packet.ValidateMagicNumber(signature);

            int headerSize = PacketHeader.GetExpectedSize();
            byte[] headerBytes = ReadRange(2, headerSize); // skip signature
            PacketHeader header = Packet.DeserializeHeader(headerBytes);
            int dataLength = header.Size;
            if (dataLength > 0)
            {
                byte[] data = ReadRange(0, dataLength + headerSize + 2);
                return data;
            }
            return Array.Empty<byte>();
        }

        private void EnsureCapacity(long requiredSize)
        {
            if (requiredSize > DefaultCapacity)
            {
                // If more capacity is needed, create a new MMF with larger size
                CreateOrOpenMemoryMappedFile(requiredSize);
            }
        }

        public void DeleteMemoryMappedFile()
        {
            if (_mutex == null) return; // Already disposed

            _mutex.WaitOne();
            try
            {
                Dispose(true);
            }
            finally
            {
                _mutex.ReleaseMutex();
            }
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
                _mmf?.Dispose();
                _mmf = null;
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
