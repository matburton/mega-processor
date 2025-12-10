
using System.Reflection;
using System.Runtime.CompilerServices;

namespace Assembler.Core;

using static BindingFlags;

public static class Variables
{
    public static (T, int totalBytes) ByteSizesToOffsets<T>(T byteSizes)
        where T : class
    {
        ArgumentNullException.ThrowIfNull(byteSizes);

        var offset = 0;

        var offsets = (T)ByteSizesToOffsets(byteSizes, ref offset);

        return (offsets, offset);
    }

    public static (T[], int totalBytes) ByteSizesToOffsets<T>(T[] byteSizes)
    {
        ArgumentNullException.ThrowIfNull(byteSizes);

        var offset = 0;

        // ReSharper disable once CoVariantArrayConversion
        var offsets = (T[])ByteSizesToOffsets(byteSizes, ref offset);

        return (offsets, offset);
    }

    public static OrderedDictionary<int, string> OffsetAddresses
        (object offsets,
         int address = 0,
         [CallerArgumentExpression(nameof(offsets))]
            string? offsetsProse = null)
    {
        var offsetsToPaths = new Dictionary<int, string>();

        PopulateOffsetsToPaths
            (offsetsToPaths, offsets, offsetsProse ?? string.Empty);

        return new (offsetsToPaths.ToDictionary(p => p.Key + address,
                                                p => p.Value));
    }

    private static object ByteSizesToOffsets(object? byteSizes, ref int offset)
    {
        if (byteSizes is null)
        {
            throw new ArgumentException("Value within was null",
                                        nameof(byteSizes));
        }

        var type = byteSizes.GetType();

        const BindingFlags bindingFlags = Instance | NonPublic | Public;

        var offsets = type.GetMethod("MemberwiseClone", bindingFlags)
                         !.Invoke(byteSizes, [])!;

        if (offsets is Array array)
        {
            for (var index = 0; index < array.Length; ++index)
            {
                array.SetValue
                    (ByteSizesToOffsets(array.GetValue(index), ref offset),
                     index);
            }

            return offsets;
        }

        if (type.GetFields(bindingFlags) is not [.., _] fields)
        {
            throw new ArgumentException("Value within was either not an int"
                                        + " or not an object with fields",
                                        nameof(byteSizes));
        }

        foreach (var field in fields)
        {
            var fieldValue = field.GetValue(byteSizes);

            if (fieldValue is int byteSize)
            {
                if (byteSize < 1)
                {
                    throw new ArgumentException
                        ("Value within was less than 1", nameof(byteSizes));
                }

                field.SetValue(offsets, offset);

                offset += byteSize;
            }
            else
            {
                field.SetValue(offsets,
                               ByteSizesToOffsets(fieldValue, ref offset));
            }
        }

        return offsets;
    }

    private static void PopulateOffsetsToPaths
        (IDictionary<int, string> offsetsToPaths, object? offsets, string path)
    {
        if (offsets is null)
        {
            throw new ArgumentException("Value within was null",
                                        nameof(offsets));
        }

        if (offsets is int offset)
        {
            offsetsToPaths.Add(offset, path);

            return;
        }

        if (offsets is Array array)
        {
            for (var index = 0; index < array.Length; ++index)
            {
                PopulateOffsetsToPaths
                    (offsetsToPaths, array.GetValue(index), $"{path}[{index}]");
            }

            return;
        }

        if (path is not []) path += '.';

        var type = offsets.GetType();

        const BindingFlags bindingFlags = Instance | Public;

        var properties = type.GetProperties(bindingFlags)
                             .Select(p => (p.Name, p.GetValue(offsets)));

        var fields = type.GetFields(bindingFlags)
                         .Select(f => (f.Name, f.GetValue(offsets)));

        foreach (var (name, value) in properties.Concat(fields))
        {
            PopulateOffsetsToPaths(offsetsToPaths, value, $"{path}{name}");
        }
    }
}