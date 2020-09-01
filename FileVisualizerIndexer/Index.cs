using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace FileVisualizerIndexer
{
    public class Index : IDisposable
    {
        private const long IndexFileHeaderSize = 1024;

        private readonly Stream _bucketStream;
        private readonly Stream _indexStream;
        private readonly Encoding _encoding;
        private readonly int _bucketCount;

        private Index(string fileName, Encoding encoding, int bucketCount, bool create)
        {
            _encoding = encoding;
            try
            {
                _indexStream = CreateIndexStream(fileName, create, ref bucketCount);
                _bucketStream = CreateBucketStream(fileName + ".bkt", create, bucketCount);
                _bucketCount = bucketCount;
            }
            catch
            {
                Dispose();
                throw;
            }
        }

        public static Index Open(string fileName, Encoding encoding)
        {
            return new Index(fileName, encoding, 0, false);
        }

        public IEnumerable<long> GetOffsetFrom(string value)
        {
            long bucketOffset = 8 * GetBucketForValue(value);
            _bucketStream.Seek(bucketOffset, SeekOrigin.Begin);
            long offsetInIndexFile = _bucketStream.ReadInt64();
            while (offsetInIndexFile >= IndexFileHeaderSize)
            {
                _indexStream.Seek(offsetInIndexFile, SeekOrigin.Begin);
                var node = Node.Read(_indexStream, _encoding);
                if (string.Equals(node.Value, value))
                    yield return node.OffsetInDataFile;
                offsetInIndexFile = node.NextOffset;
            }
        }

        private static Stream CreateIndexStream(string fileName, bool createFile, ref int bucketCount)
        {
            var mode = createFile ? FileMode.Create : FileMode.Open;
            var access = createFile ? FileAccess.ReadWrite : FileAccess.Read;
            FileStream stream = null;
            try
            {
                stream = new FileStream(fileName, mode, access, FileShare.Read);
                if (createFile)
                {
                    stream.SetLength(IndexFileHeaderSize);
                    stream.WriteInt32(bucketCount);
                }
                else
                {
                    if (stream.Length < IndexFileHeaderSize)
                        throw new InvalidOperationException("Invalid index file.");
                    bucketCount = stream.ReadInt32();
                }
                return stream;
            }
            catch
            {
                if (stream != null)
                    stream.Dispose();
                throw;
            }
        }

        private static Stream CreateBucketStream(string fileName, bool createFile, int bucketCount)
        {
            var mode = createFile ? FileMode.Create : FileMode.Open;
            var access = createFile ? FileAccess.ReadWrite : FileAccess.Read;
            FileStream stream = null;
            try
            {
                int bucketFileSize = bucketCount * 8;
                stream = new FileStream(fileName, mode, access, FileShare.Read);
                if (createFile)
                {
                    stream.SetLength(bucketFileSize);
                }
                else
                {
                    if (stream.Length != bucketFileSize)
                        throw new InvalidOperationException("Invalid bucket file.");
                }
                return stream;
            }
            catch
            {
                if (stream != null)
                    stream.Dispose();
                throw;
            }
        }

        public static string GetIndexFileName(string dataFileName)
        {
            string dir = Path.GetDirectoryName(dataFileName);
            string baseName = Path.GetFileNameWithoutExtension(dataFileName);
            string indexName = string.Format("{0}.idx", baseName);
            return Path.Combine(dir, indexName);
        }

        public static Index Create(string fileName, Encoding encoding, int bucketCount)
        {
            return new Index(fileName, encoding, bucketCount, true);
        }

        public void Add(string value, long offsetInDataFile)
        {
            long bucketOffset = 8 * GetBucketForValue(value);

            _bucketStream.Seek(bucketOffset, SeekOrigin.Begin);
            long offsetInIndexFile = _bucketStream.ReadInt64();

            Node prevNode = null;
            if (offsetInIndexFile >= IndexFileHeaderSize)
            {
                _indexStream.Seek(offsetInIndexFile, SeekOrigin.Begin);
                prevNode = Node.Read(_indexStream, _encoding);
                while (prevNode.NextOffset != 0)
                {
                    _indexStream.Seek(prevNode.NextOffset, SeekOrigin.Begin);
                    prevNode = Node.Read(_indexStream, _encoding);
                }
            }

            _indexStream.Seek(0, SeekOrigin.End);
            var newNode = Node.Create(_indexStream, value, offsetInDataFile, _encoding);
            newNode.Write(_indexStream, _encoding);
            if (prevNode != null)
            {
                prevNode = prevNode.SetNext(newNode.Offset);
                _indexStream.Seek(prevNode.Offset, SeekOrigin.Begin);
                prevNode.Write(_indexStream, _encoding);
            }
            if (offsetInIndexFile < IndexFileHeaderSize)
            {
                _bucketStream.Seek(bucketOffset, SeekOrigin.Begin);
                _bucketStream.WriteInt64(newNode.Offset);
            }
        }

        private long GetBucketForValue(string value)
        {
            int hashcode = value != null ? value.GetHashCode() : 0;
            long positiveHashcode = (long)hashcode - int.MinValue;
            long bucket = positiveHashcode % _bucketCount;
            return bucket;
        }


        private bool _disposed;
        public void Dispose()
        {
            if (_disposed)
                return;

            if (_bucketStream != null)
                _bucketStream.Dispose();
            if (_indexStream != null)
                _indexStream.Dispose();
            _disposed = true;
        }
    }
}
