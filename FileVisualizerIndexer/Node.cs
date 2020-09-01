using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace FileVisualizerIndexer
{
    class Node
    {
        private readonly long _offset;
        private readonly int _valueLength;
        private readonly string _value;
        private readonly long _offsetInDataFile;
        private readonly long _nextOffset;

        private Node(long offset, int valueLength, string value, long offsetInDataFile, long nextOffset)
        {
            _offset = offset;
            _valueLength = valueLength;
            _value = value;
            _offsetInDataFile = offsetInDataFile;
            _nextOffset = nextOffset;
        }

        public long Offset
        {
            get { return _offset; }
        }

        public string Value
        {
            get { return _value; }
        }

        public long OffsetInDataFile
        {
            get { return _offsetInDataFile; }
        }

        public long NextOffset
        {
            get { return _nextOffset; }
        }

        public static Node Read(Stream indexStream, Encoding encoding)
        {
            long offset = indexStream.Position;
            int valueLength = indexStream.ReadInt32();
            string value = indexStream.ReadString(valueLength, encoding);
            long offsetInDataFile = indexStream.ReadInt64();
            long nextOffset = indexStream.ReadInt64();

            return new Node(offset, valueLength, value, offsetInDataFile, nextOffset);
        }

        public static Node Create(Stream indexStream, string value, long offsetInDataFile, Encoding encoding)
        {
            long offset = indexStream.Position;
            var valueBuffer = encoding.GetBytes(value);
            var node = new Node(offset, valueBuffer.Length, value, offsetInDataFile, 0);
            return node;
        }

        public void Write(Stream indexStream, Encoding encoding)
        {
            var valueBuffer = encoding.GetBytes(_value);
            indexStream.WriteInt32(valueBuffer.Length);
            indexStream.Write(valueBuffer, 0, valueBuffer.Length);
            indexStream.WriteInt64(_offsetInDataFile);
            indexStream.WriteInt64(_nextOffset);
        }

        public Node SetNext(long nextOffset)
        {
            return new Node(_offset, _valueLength, _value, _offsetInDataFile, nextOffset);
        }
    }
}
