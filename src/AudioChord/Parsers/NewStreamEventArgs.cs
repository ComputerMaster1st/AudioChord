/****************************************************************************
 * NVorbis                                                                  *
 * Copyright (C) 2014, Andrew Ward <afward@gmail.com>                       *
 *                                                                          *
 * See COPYING for license terms (Ms-PL).                                   *
 *                                                                          *
 ***************************************************************************/

using System;

namespace AudioChord.Parsers
{
    /// <summary>
    ///     Event data for when a new logical stream is found in a container.
    /// </summary>
    internal class NewStreamEventArgs : EventArgs
    {
        /// <summary>
        ///     Creates a new instance of <see cref="NewStreamEventArgs" /> with the specified
        ///     <see cref="Parsers.IPacketProvider" />.
        /// </summary>
        /// <param name="packetProvider">An <see cref="Parsers.IPacketProvider" /> instance.</param>
        public NewStreamEventArgs(IPacketProvider packetProvider)
        {
            if (packetProvider == null) throw new ArgumentNullException("packetProvider");

            PacketProvider = packetProvider;
        }

        /// <summary>
        ///     Gets new the <see cref="Parsers.IPacketProvider" /> instance.
        /// </summary>
        public IPacketProvider PacketProvider { get; }

        /// <summary>
        ///     Gets or sets whether to ignore the logical stream associated with the packet provider.
        /// </summary>
        public bool IgnoreStream { get; set; }
    }
}