﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace ReadIndex
{
    public class TrackingTextReader : TextReader
    {
        private readonly TextReader _baseReader;
        private int _position;

        public TrackingTextReader(TextReader baseReader)
        {
            _baseReader = baseReader;
        }

        public override int Read()
        {
            _position++;
            return _baseReader.Read();
        }

        public override int Peek()
        {
            return _baseReader.Peek();
        }

        public int Position
        {
            get { return _position; }
        }
    }
}
