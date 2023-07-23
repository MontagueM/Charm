using System;
using System.IO;
using System.Runtime.InteropServices;
using static RevorbStd.Native;

namespace RevorbStd
{
    public static class Revorb
    {
        /// <summary>
        /// Revorbs Ogg Vorbis data from the input stream.
        /// </summary>
        /// <param name="inputFile">The Ogg Vorbis physical input stream.</param>
        /// <returns>A <see cref="MemoryStream"/> containing revorbed data.</returns>
        public static MemoryStream Jiggle(Stream inputFile)
        {
            if (inputFile is MemoryStream
                && (inputFile as MemoryStream).TryGetBuffer(out var streamBuf))
            {
                // Use underlying stream buffer
                return Jiggle(streamBuf.Array);
            }
            else
            {
                // Read stream into array, preserve position
                var input = new byte[inputFile.Length];
                var cursor = inputFile.Position;
                inputFile.Position = 0;
                inputFile.Read(input, 0, input.Length);
                inputFile.Position = cursor;
                return Jiggle(input);
            }
        }

        /// <summary>
        /// Revorbs Ogg Vorbis data from the input buffer.
        /// </summary>
        /// <param name="input">The Ogg Vorbis physical stream data.</param>
        /// <returns>A <see cref="MemoryStream"/> containing revorbed data.</returns>
        public static MemoryStream Jiggle(byte[] input)
        {
            // Pin input
            var hInput = GCHandle.Alloc(input, GCHandleType.Pinned);
            var pInput = hInput.AddrOfPinnedObject();
            RevorbFile inputFile = new RevorbFile {
                Start = pInput,
                Cursor = pInput,
                Size = input.Length
            };

            // Create and pin output
            var output = new byte[input.Length + 4096]; // Arbitrary extra space
            var hOutput = GCHandle.Alloc(output, GCHandleType.Pinned);
            var pOutput = hOutput.AddrOfPinnedObject();
            RevorbFile outputFile = new RevorbFile
            {
                Start = pOutput,
                Cursor = pOutput,
                Size = output.Length
            };

            int result = revorb(ref inputFile, ref outputFile);

            // Unpin buffers
            hInput.Free();
            hOutput.Free();

            if (Enum.IsDefined(typeof(RevorbResult), result))
            {
                throw new Exception($"Revorb error {result} -- refer to RevorbStd.Native");
            }

            return new MemoryStream(output, 0, result);
        }
    }
}
