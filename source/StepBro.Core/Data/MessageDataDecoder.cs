using StepBro.Core.Api;
using StepBro.Core.Data;
using System;
using System.Collections.Generic;
using System.Text;

namespace StepBro
{
    [Public]
    public interface IMessageDataDecoder
    {
        public MessageDataDecoder Decoder { get; }
    }

    /// <summary>
    /// Signature for a method that decodes an optional data ID and some optional binary message data into separate descriptive text fields.
    /// </summary>
    /// <param name="id">A numeric ID representing the message to be decoded.</param>
    /// <param name="data">An array of binary message date to be decoded.</param>
    /// <param name="decoded">The target list of message data description text fields.</param>
    /// <returns>Whether the input ID and data matched any of the decoders known addresses/IDs.</returns>
    [Public]
    public delegate bool MessageDataDecoder(uint id, Byte[] data, List<Tuple<uint, string>> decoded);

    [Public]
    public class MessageDataDecoderProxy : IMessageDataDecoder, INameable
    {
        public string Name { get; set; } = "Decoder";

        public MessageDataDecoder Decoder { get; set; } = null;

        public bool Decode(uint id, byte[] data, List<Tuple<uint, string>> decoded)
        {
            if (this.Decoder == null) return false;
            else return this.Decoder(id, data, decoded);
        }
    }
}
