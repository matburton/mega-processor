
using Assembler.Core.Output;

namespace Assembler.MegaProcessor.Output;

public static class IntelHex
{
    extension (IEnumerable<OutputLine> outputLines)
    {
        [Pure]
        public IEnumerable<string> ToIntelHex()
        {
            var lines = outputLines.SelectMany(l => l.Bytes ?? []).Chunk(32);

            ushort address = 0;

            foreach (var bytes in lines)
            {
                yield return $":{bytes.Length:X2}{address:X4}00"
                           + string.Concat(bytes.Select(b => $"{b:X2}"))
                           + $"{CheckSumFor(bytes, address):X2}";

                address += (ushort)bytes.Length;
            }

            yield return ":00000001FF";
        }
    }

    private static int CheckSumFor(IReadOnlyList<byte> bytes, ushort address)
    {
        var allBytesSum = bytes.Append((byte)bytes.Count)
                               .Concat(BitConverter.GetBytes(address))
                               .Sum(b => b);

        return LeastSignificantByte(-LeastSignificantByte(allBytesSum));
    }

    private static byte LeastSignificantByte(int value)
    {
        var bytes = BitConverter.GetBytes(value);

        if (!BitConverter.IsLittleEndian) Array.Reverse(bytes);

        return bytes[0];
    }
}