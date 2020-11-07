﻿/****************************************************************************
 * NVorbis                                                                  *
 * Copyright (C) 2014, Andrew Ward <afward@gmail.com>                       *
 *                                                                          *
 * See COPYING for license terms (Ms-PL).                                   *
 *                                                                          *
 ***************************************************************************/

namespace AudioChord.Parsers
{
    internal class Crc
    {
        private const uint CRC32_POLY = 0x04c11db7;
        private static readonly uint[] crcTable = new uint[256];

        static Crc()
        {
            for (uint i = 0; i < 256; i++)
            {
                uint s = i << 24;
                for (int j = 0; j < 8; ++j) s = (s << 1) ^ (s >= 1U << 31 ? CRC32_POLY : 0);
                crcTable[i] = s;
            }
        }

        public Crc()
        {
            Reset();
        }

        public uint Value { get; private set; }

        public void Reset()
        {
            Value = 0U;
        }

        public void Update(int nextVal)
        {
            Value = (Value << 8) ^ crcTable[nextVal ^ (Value >> 24)];
        }

        public bool Test(uint checkCrc)
        {
            return Value == checkCrc;
        }
    }
}