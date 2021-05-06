﻿/****************************************************************************
 * NVorbis                                                                  *
 * Copyright (C) 2014, Andrew Ward <afward@gmail.com>                       *
 *                                                                          *
 * See COPYING for license terms (Ms-PL).                                   *
 *                                                                          *
 ***************************************************************************/

using System;

namespace AudioChord.Parsers
{
    internal class Packet : DataPacket
    {
        private readonly OggContainerReader _containerReader; // IntPtr.Size
        private readonly int _length; // 4
        private readonly long _offset; // 8
        private int _curOfs; // 4
        private Packet _mergedPacket; // IntPtr.Size

        internal Packet(OggContainerReader containerReader, long streamOffset, int length)
            : base(length)
        {
            _containerReader = containerReader;

            _offset = streamOffset;
            _length = length;
            _curOfs = 0;
        }

        internal Packet Next { get; set; }

        internal Packet Prev { get; set; }

        internal bool IsContinued
        {
            get => GetFlag(PacketFlags.User1);
            set => SetFlag(PacketFlags.User1, value);
        }

        internal bool IsContinuation
        {
            get => GetFlag(PacketFlags.User2);
            set => SetFlag(PacketFlags.User2, value);
        }

        internal void MergeWith(DataPacket continuation)
        {
            Packet? op = continuation as Packet;

            if (op == null) throw new ArgumentException("Incorrect packet type!");

            Length += continuation.Length;

            if (_mergedPacket == null)
                _mergedPacket = op;
            else
                _mergedPacket.MergeWith(continuation);

            // per the spec, a partial packet goes with the next page's granulepos.  we'll go ahead and assign it to the next page as well
            PageGranulePosition = continuation.PageGranulePosition;
            PageSequenceNumber = continuation.PageSequenceNumber;
        }

        internal void Reset()
        {
            _curOfs = 0;
            ResetBitReader();

            if (_mergedPacket != null) _mergedPacket.Reset();
        }

        protected override int ReadNextByte()
        {
            if (_curOfs == _length)
            {
                if (_mergedPacket == null) return -1;

                return _mergedPacket.ReadNextByte();
            }

            int b = _containerReader.PacketReadByte(_offset + _curOfs);
            if (b != -1) ++_curOfs;
            return b;
        }

        public override void Done()
        {
            if (_mergedPacket != null)
                _mergedPacket.Done();
            else
                _containerReader.PacketDiscardThrough(_offset + _length);
        }
    }
}