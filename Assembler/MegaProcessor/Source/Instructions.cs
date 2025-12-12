
namespace Assembler.MegaProcessor;

using Exceptions;

using static Register;

public static class Instructions
{
    extension (Assembly assembly)
    {
        /// <summary>Cycles:1. Bytes:1. Sets bits 8-15 to equal bit 7</summary>
        ///
        /// <remarks>Sets:[N,Z]. Clears:[V,C]</remarks>
        ///
        [Pure]
        public Assembly SignExtend(Register register) =>
            assembly.AddOp(1, $"stx {register.Label}",
                           register.Bits << 2 | register.Bits);

        /// <summary>Cycles:1. Bytes:1</summary>
        ///
        /// <remarks>Sets:[N,Z]. Clears:[V,C]</remarks>
        ///
        [Pure]
        public Assembly CopyTo(Register to, Register from)
        {
            ThrowIfSame(to, from);

            return assembly.AddOp(1, $"move {to.Label}, {from.Label}",
                                  from.Bits << 2 | to.Bits);
        }

        /// <summary>Cycles:1. Bytes:1</summary>
        ///
        /// <remarks>Sets:[N,Z]. Clears:[V,C]</remarks>
        ///
        [Pure]
        public Assembly Test(Register register) =>
            assembly.AddOp(1, $"test {register.Label}",
                           0x10 | register.Bits << 2 | register.Bits);

        /// <summary>Cycles:1. Bytes:1. Binary AND</summary>
        ///
        /// <remarks>Sets:[N,Z]. Clears:[V,C]</remarks>
        ///
        [Pure]
        public Assembly AndTo(Register to, Register other)
        {
            ThrowIfSame(to, other);

            return assembly.AddOp(1, $"and {to.Label}, {other.Label}",
                                  0x10 | other.Bits << 2 | to.Bits);
        }

        /// <summary>Cycles:1. Bytes:1. Binary XOR</summary>
        ///
        /// <remarks>Sets:[N,Z]. Clears:[V,C]</remarks>
        ///
        [Pure]
        public Assembly ExclusiveOrTo(Register to, Register other) =>
            assembly.AddOp(1, $"xor {to.Label}, {other.Label}",
                           0x20 | other.Bits << 2 | to.Bits);

        /// <summary>Cycles:1. Bytes:1. Sets all bits to 0</summary>
        ///
        /// <remarks>Sets:[N,Z]. Clears:[V,C]</remarks>
        ///
        [Pure]
        public Assembly Clear(Register register) =>
            assembly.AddOp(1, $"clr {register.Label}",
                           0x20 | register.Bits << 2 | register.Bits);

        /// <summary>Cycles:1. Bytes:1. Inverts each bit</summary>
        ///
        /// <remarks>Sets:[N,Z]. Clears:[V,C]</remarks>
        ///
        public Assembly Invert(Register register) =>
            assembly.AddOp(1, $"inv {register.Label}",
                           0x30 | register.Bits << 2 | register.Bits);

        /// <summary>Cycles:1. Bytes:1. Binary OR</summary>
        ///
        /// <remarks>Sets:[N,Z]. Clears:[V,C]</remarks>
        ///
        [Pure]
        public Assembly OrTo(Register to, Register other)
        {
            ThrowIfSame(to, other);

            return assembly.AddOp(1, $"or {to.Label}, {other.Label}",
                                  0x30 | other.Bits << 2 | to.Bits);
        }

        /// <summary>Cycles:1. Bytes:1</summary>
        ///
        /// <remarks>Sets:[N,Z,V,X,C]</remarks>
        ///
        [Pure]
        public Assembly AddTo(Register to, Register other) =>
            assembly.AddOp(1, $"add {to.Label}, {other.Label}",
                           0x40 | other.Bits << 2 | to.Bits);

        /// <summary>Cycles:1. Bytes:1</summary>
        ///
        /// <remarks>Sets:[N,Z,V,X,C]</remarks>
        ///
        [Pure]
        public Assembly AddConst(Register register, int constant)
        {
            if (constant is 0) return assembly; // Be kind

            var constantBits = constant switch
            {
                2 => 0b00, 1 => 0b01, -2 => 0b10, -1 => 0b11,
                _ => throw new InvalidInstructionException
                        ($"{nameof(AddConst)} constant must be -2, -1, 1, or 2")
            };

            var comment = constant switch
                {  1 => $"inc {register.Label}",
                  -1 => $"dec {register.Label}",
                   _ => $"addq {register.Label}, #{constant}" };

            return assembly.AddOp
                (1, comment, 0x50 | constantBits << 2 | register.Bits);
        }

        /// <summary>Cycles:1. Bytes:1</summary>
        ///
        /// <remarks>Sets:[N,Z,V,X,C]</remarks>
        ///
        [Pure]
        public Assembly SubFrom(Register to, Register other)
        {
            ThrowIfSame(to, other);

            return assembly.AddOp(1, $"sub {to.Label}, {other.Label}",
                                  0x60 | other.Bits << 2 | to.Bits);
        }

        /// <summary>Cycles:1. Bytes:1</summary>
        ///
        /// <remarks>Sets:[N,Z,V,X,C]</remarks>
        ///
        [Pure]
        public Assembly Negate(Register register) =>
            assembly.AddOp(1, $"neg {register.Label}",
                           0x60 | register.Bits << 2 | register.Bits);

        /// <summary>Cycles:1. Bytes:1</summary>
        ///
        /// <remarks>Sets:[N,Z,V,X,C]</remarks>
        ///
        [Pure]
        public Assembly Absolute(Register register) =>
            assembly.AddOp(1, $"abs {register.Label}",
                           0x70 | register.Bits << 2 | register.Bits);

        /// <summary>Cycles:1. Bytes:1</summary>
        ///
        /// <remarks>Sets:[N,Z,V,X,C] according to
        ///          <paramref name="registerA" />
        ///          - <paramref name="registerB" /></remarks>
        [Pure]
        public Assembly Compare(Register registerA, Register registerB)
        {
            ThrowIfSame(registerA, registerB);

            return assembly.AddOp(1, $"cmp {registerA.Label}, {registerB.Label}",
                                  0x70 | registerB.Bits << 2 | registerA.Bits);
        }

        private Assembly IndirectCopy(IndexRegister indexRegister,
                                      bool load,
                                      int cycles,
                                      Size size,
                                      DataRegister dataRegister,
                                      bool bumpIndex = false)
        {
            var indexComment =
                $"({indexRegister.Label}{(bumpIndex ? "++" : string.Empty)})";

            var comment = load switch
            {
                true => $"ld.{size.Label} {dataRegister.Label}, {indexComment}",
                _    => $"st.{size.Label} {indexComment}, {dataRegister.Label}"
            };

            var op = (bumpIndex ? 0x90 : 0x80)
                   | (load ? 0 : 0b1000)
                   | size.Bits << 2
                   | indexRegister.IndexBits << 1
                   | dataRegister.DataBits;

            return assembly.AddOp(cycles, comment, op);
        }

        /// <summary>Cycles:2. Bytes:1</summary>
        ///
        /// <remarks>Sets:[N,Z]. Clears:[V,C].
        ///          <paramref name="bumpIndex"/> causes the value in
        ///          <paramref name="index"/> to be incremented by 1</remarks>
        [Pure]
        public Assembly CopyByteFromIndex
            (IndexRegister index, DataRegister to, bool bumpIndex = false) =>
                assembly.IndirectCopy
                    (index, load: true, 2, Size.Byte, to, bumpIndex);

        /// <summary>Cycles:3. Bytes:1</summary>
        ///
        /// <remarks>Sets:[N,Z]. Clears:[V,C].
        ///          <paramref name="bumpIndex"/> causes the value in
        ///          <paramref name="index"/> to be incremented by 2</remarks>
        [Pure]
        public Assembly CopyWordFromIndex
            (IndexRegister index, DataRegister to, bool bumpIndex = false) =>
                assembly.IndirectCopy
                    (index, load: true, 3, Size.Word, to, bumpIndex);

        /// <summary>Cycles:2. Bytes:1</summary>
        ///
        /// <remarks>Sets:[N,Z]. Clears:[V,C].
        ///          <paramref name="bumpIndex"/> causes the value in
        ///          <paramref name="index"/> to be incremented by 1</remarks>
        [Pure]
        public Assembly CopyByteToIndex
            (IndexRegister index, DataRegister data, bool bumpIndex = false) =>
                assembly.IndirectCopy
                    (index, load: false, 2, Size.Byte, data, bumpIndex);

        /// <summary>Cycles:3. Bytes:1</summary>
        ///
        /// <remarks>Sets:[N,Z]. Clears:[V,C].
        ///          <paramref name="bumpIndex"/> causes the value in
        ///          <paramref name="index"/> to be incremented by 2</remarks>
        [Pure]
        public Assembly CopyWordToIndex
            (IndexRegister index, DataRegister data, bool bumpIndex = false) =>
                assembly.IndirectCopy
                    (index, load: false, 3, Size.Word, data, bumpIndex);

        private Assembly StackCopy(bool load,
                                   int cycles,
                                   Size size,
                                   int offset,
                                   Register register,
                                   string? offsetProse)
        {
            offsetProse ??= $"{offset}";

            if (offset is < 0 or > byte.MaxValue)
            {
                throw new InvalidInstructionException
                    ($"Stack offset {offset} is not in valid range [0, 255]");
            }

            var comment = load switch
            {
                true => $"ld.{size.Label} {register.Label}, (sp + {offsetProse})",
                _    => $"st.{size.Label} (sp + {offsetProse}), {register.Label})"
            };

            var op = 0xA0 | (load ? 0 : 0b1000) | size.Bits << 2 | register.Bits;

            return assembly.AddOp(cycles, comment, op, [offset]);
        }

        /// <summary>Cycles:3. Bytes:2</summary>
        ///
        /// <remarks>Sets:[N,Z]. Clears:[V,C]</remarks>
        ///
        [Pure]
        public Assembly CopyByteFromStack
            (int offset,
             Register to,
             [CallerArgumentExpression(nameof(offset))]
                string? offsetProse = null)
        {
            return assembly.StackCopy
                (load: true, 3, Size.Byte, offset, to, offsetProse);
        }

        /// <summary>Cycles:4. Bytes:2</summary>
        ///
        /// <remarks>Sets:[N,Z]. Clears:[V,C]</remarks>
        ///
        [Pure]
        public Assembly CopyWordFromStack
            (int offset,
             Register to,
             [CallerArgumentExpression(nameof(offset))]
                string? offsetProse = null)
        {
            return assembly.StackCopy
                (load: true, 4, Size.Word, offset, to, offsetProse);
        }

        /// <summary>Cycles:3. Bytes:2</summary>
        ///
        /// <remarks>Sets:[N,Z]. Clears:[V,C]</remarks>
        ///
        [Pure]
        public Assembly CopyByteToStack
            (int offset,
             Register register,
             [CallerArgumentExpression(nameof(offset))]
                string? offsetProse = null)
        {
            return assembly.StackCopy
                (load: false, 3, Size.Byte, offset, register, offsetProse);
        }

        /// <summary>Cycles:4. Bytes:2</summary>
        ///
        /// <remarks>Sets:[N,Z]. Clears:[V,C]</remarks>
        ///
        [Pure]
        public Assembly CopyWordToStack
            (int offset,
             Register register,
             [CallerArgumentExpression(nameof(offset))]
                string? offsetProse = null)
        {
            return assembly.StackCopy
                (load: false, 4, Size.Word, offset, register, offsetProse);
        }

        private Assembly AbsoluteCopy(bool load,
                                      int cycles,
                                      Size size,
                                      Register register,
                                      Calculation calculation,
                                      string calculationProse)
        {
            var comment = load switch
            {
                true => $"ld.{size.Label} {register.Label}, {calculationProse}",
                _    => $"st.{size.Label} {calculationProse}, {register.Label}"
            };

            return assembly.AddOp
                (cycles,
                 comment,
                 0xB0 | (load ? 0 : 0b1000) | size.Bits << 2 | register.Bits,
                 [ToAddressFragment(calculation, calculationProse)]);
        }

        /// <summary>Cycles:4. Bytes:3</summary>
        ///
        /// <remarks>Sets:[N,Z]. Clears:[V,C]</remarks>
        ///
        [Pure]
        public Assembly CopyByteFrom
            (Calculation calculation,
             Register to,
             [CallerArgumentExpression(nameof(calculation))]
                string calculationProse = MissingProse)
        {
            return assembly.AbsoluteCopy
                (load: true, 4, Size.Byte, to, calculation, calculationProse);
        }

        /// <summary>Cycles:5. Bytes:3</summary>
        ///
        /// <remarks>Sets:[N,Z]. Clears:[V,C]</remarks>
        ///
        [Pure]
        public Assembly CopyWordFrom
            (Calculation calculation,
             Register to,
             [CallerArgumentExpression(nameof(calculation))]
                string calculationProse = MissingProse)
        {
            return assembly.AbsoluteCopy
                (load: true, 5, Size.Word, to, calculation, calculationProse);
        }

        /// <summary>Cycles:4. Bytes:3</summary>
        ///
        /// <remarks>Sets:[N,Z]. Clears:[V,C]</remarks>
        ///
        [Pure]
        public Assembly CopyByteTo
            (Calculation calculation,
             Register register,
             [CallerArgumentExpression(nameof(calculation))]
                string calculationProse = MissingProse)
        {
            return assembly.AbsoluteCopy(load: false,
                                         cycles: 4,
                                         Size.Byte,
                                         register,
                                         calculation,
                                         calculationProse);
        }

        /// <summary>Cycles:4. Bytes:3</summary>
        ///
        /// <remarks>Sets:[N,Z]. Clears:[V,C]</remarks>
        ///
        [Pure]
        public Assembly CopyWordTo
            (Calculation calculation,
             Register register,
             [CallerArgumentExpression(nameof(calculation))]
                string calculationProse = MissingProse)
        {
            return assembly.AbsoluteCopy(load: false,
                                         cycles: 4,
                                         Size.Word,
                                         register,
                                         calculation,
                                         calculationProse);
        }

        private Assembly StackCopy(bool push, int cycles, Register? register) =>
            assembly.AddOp
                (cycles, $"{(push ? "push" : "pop")} {register?.Label ?? "ps"}",
                 0xC0 | (push ? 0b1000 : 0) | (register?.Bits ?? 0b100));

        /// <summary>Cycles:3. Bytes:1</summary>
        ///
        /// <remarks>Sets:[N,Z]. Clears:[V,C]</remarks>
        ///
        [Pure]
        public Assembly PushToStack(Register register) =>
            assembly.StackCopy(push: true, 3, register);

        /// <summary>Cycles:3. Bytes:1</summary>
        ///
        /// <remarks>Sets:[N,Z]. Clears:[V,C]</remarks>
        ///
        [Pure]
        public Assembly PopFromStack(Register register) =>
            assembly.StackCopy(push: true, 3, register);

        /// <summary>Cycles:2. Bytes:1</summary>
        ///
        /// <remarks>Sets:[N,Z]. Clears:[V,C]</remarks>
        ///
        [Pure]
        public Assembly StatusPushToStack(Register register) =>
            assembly.StackCopy(push: true, 2, null);

        /// <summary>Cycles:2. Bytes:1</summary>
        ///
        /// <remarks>Sets:[N,Z]. Clears:[V,C]</remarks>
        ///
        [Pure]
        public Assembly StatusPopFromStack(Register register) =>
            assembly.StackCopy(push: true, 2, null);

        /// <summary>1 byte per cycle</summary>
        ///
        /// <remarks>Sets:[]</remarks>
        ///
        [Pure]
        public Assembly NoOp(int cycles = 1)
        {
            if (cycles < 0)
            {
                throw new InvalidInstructionException
                    ($"Invalid {nameof(NoOp)} {nameof(cycles)} {cycles}");
            }

            var bytes = Enumerable.Repeat<byte>(0xFF, cycles);

            var comment = $"{CyclesPrefix(cycles)}nop";

            if (cycles > 1) comment+= "s";

            return assembly.AddLines
                ([new ([new BytesFragment(bytes)], comment)]);
        }

        /// <summary>Cycles:4. Bytes:1</summary>
        ///
        /// <remarks>Sets:[]</remarks>
        ///
        [Pure]
        public Assembly ReturnFromRoutine() =>
            assembly.AddOp(4, "ret", 0xC0 | 0b110);

        /// <summary>Cycles:5. Bytes:1</summary>
        ///
        /// <remarks>Sets:[I,N,Z,V,X,C]</remarks>
        ///
        [Pure]
        public Assembly ReturnFromInterrupt() =>
            assembly.AddOp(5, "reti", 0xC0 | 0b111);

        /// <summary>Cycles:6. Bytes:1</summary>
        ///
        /// <remarks>Sets:[]. Clears:[I]</remarks>
        ///
        [Pure]
        public Assembly CallInterruptHandler() =>
            assembly.AddOp(6, "trap", 0xC0 | 0b1101);

        /// <summary>Cycles:4. Bytes:1</summary>
        ///
        /// <remarks>Sets:[]</remarks>
        ///
        [Pure]
        public Assembly CallRoutineR0() =>
            assembly.AddOp(4, "jsr (r0)", 0xC0 | 0b1110);

        /// <summary>Cycles:6. Bytes:3</summary>
        ///
        /// <remarks>Sets:[]</remarks>
        ///
        [Pure]
        public Assembly CallRoutine
            (Calculation calculation,
             [CallerArgumentExpression(nameof(calculation))]
                string calculationProse = MissingProse)
        {
            return assembly.AddOp
                (6, $"jsr {calculationProse}",
                 0xC0 | 0b1111,
                 [ToAddressFragment(calculation, calculationProse)]);
        }

        private Assembly SetValue(Register register,
                                  int cycles,
                                  Size size,
                                  string calculationProse,
                                  Calculation calculation)
        {
            var valueFragment = new ReferenceFragment
                (size.ByteCount,
                 r => ToLittleEndianBytes
                            (calculation.Calculate(r))[.. size.ByteCount]);

            return assembly.AddOp
                (cycles,
                 $"ld.{size.Label} {register.Label}, #{calculationProse}",
                 0xD0 | size.Bits << 2 | register.Bits,
                 [valueFragment]);
        }

        /// <summary>Cycles:2. Bytes:2</summary>
        ///
        /// <remarks>Sets:[N,Z]. Clears:[V,C]</remarks>
        ///
        [Pure]
        public Assembly SetByteValue
            (Register register,
             Calculation calculation,
             bool force = false,
             [CallerArgumentExpression(nameof(calculation))]
                string calculationProse = MissingProse)
        {
            return assembly.SetValue
                (register,
                 cycles: 2,
                 Size.Byte,
                 calculationProse,
                 new (r => calculation.Calculate(r) switch
            {
                0 when !force =>
                    throw new InvalidReferenceException
                        ($"'{calculationProse}' is 0, so use {nameof(Clear)}"),

                < 0 or > byte.MaxValue => throw new InvalidReferenceException
                    ($"'{calculationProse}' value not within 8-bit range"),

                var v => v
            }));
        }

        /// <summary>Cycles:3. Bytes:3</summary>
        ///
        /// <remarks>Sets:[N,Z]. Clears:[V,C]</remarks>
        ///
        [Pure]
        public Assembly SetWordValue
            (Register register,
             Calculation calculation,
             bool force = false,
             [CallerArgumentExpression(nameof(calculation))]
                string calculationProse = MissingProse)
        {
            return assembly.SetValue
                (register,
                 cycles: 3,
                 Size.Word,
                 calculationProse,
                 new (r => calculation.Calculate(r) switch
            {
                < short.MinValue or > ushort.MaxValue =>
                    throw new InvalidReferenceException
                        ($"'{calculationProse}' value not within 16-bit range"),

                >= 0 and <= byte.MaxValue when !force =>
                    throw new InvalidReferenceException
                        ($"'{calculationProse}' can be a byte rather than a word"),

                var v => v
            }));
        }

        // TODO: Shift instructions - mark as obsolete
        //       Shift that can rotate and extended rotate?

        /// <summary>Cycles:3. Bytes:2</summary>
        ///
        /// <remarks>Sets:[N]</remarks>
        ///
        [Pure]
        public Assembly BitHit
            (Register target, int bitIndex, BitChange? bitChange = null)
        {
            bitChange ??= BitChange.None;

            if (bitIndex is < 0 or > 15)
            {
                throw new InvalidInstructionException
                    ($"Bit index {bitIndex} is invalid");
            }

            return assembly.AddOp(3, $"{bitChange.Label} #{bitIndex}",
                                  0xD0 | 0b1100 | target.Bits,
                                  [bitChange.Bits | bitIndex]);
        }

        /// <summary>Cycles:3. Bytes:2</summary>
        ///
        /// <remarks>Sets:[N]</remarks>
        ///
        [Pure]
        public Assembly BitHit
            (Register target, Register bitIndex, BitChange? bitChange = null)
        {
            bitChange ??= BitChange.None;

            return assembly.AddOp(3, $"{bitChange.Label} #{bitIndex}",
                                  0xD0 | 0b1100 | target.Bits,
                                  [bitChange.Bits | 0b10_0000 | bitIndex.Bits]);
        }

        /// <summary>Cycles:3 if <paramref name="condition"/> was met,
        ///          otherwise 2. Bytes:2</summary>
        ///
        /// <remarks>Sets:[]</remarks>
        ///
        [Pure]
        public Assembly GoToIf
            (Condition condition,
             Calculation calculation,
             [CallerArgumentExpression(nameof(calculation))]
                string calculationProse = MissingProse)
        {
            var op = 0xE0 | condition.Bits;

            var fragment = new ReferenceFragment(byteCount: 1, r =>
            {
                var offset = calculation.Calculate(r) - r.CurrentLineAddress - 2;

                if (offset is < sbyte.MinValue or > sbyte.MaxValue)
                {
                    throw new InvalidReferenceException
                        ($"{nameof(GoToIf)} target '{calculationProse}'"
                         + $" was {offset} bytes away but must"
                         + " be within a signed 8-bit range");
                }

                return ToLittleEndianBytes(offset)[.. 1];
            });

            var comment = $"2-3 b{condition.Label} {calculationProse}";

            return assembly.AddLines
                ([new ([new BytesFragment([(byte)op]), fragment], comment)]);
        }

        /// <summary>Cycles:2. Bytes:1</summary>
        ///
        /// <remarks>Sets:[]</remarks>
        ///
        public Assembly StackToR0() => assembly.AddOp(2, "mv r0, sp", 0xF0);

        /// <summary>Cycles:2. Bytes:1</summary>
        ///
        /// <remarks>Sets:[]</remarks>
        ///
        public Assembly StackFromR0() => assembly.AddOp(2, "mv sp, r0", 0xF1);

        /// <summary>Cycles:2. Bytes:1</summary>
        ///
        /// <remarks>Sets:[]</remarks>
        ///
        public Assembly GoToR0() => assembly.AddOp(2, "jmp (r0)", 0xF2);

        /// <summary>Cycles:4. Bytes:3</summary>
        ///
        /// <remarks>Sets:[]</remarks>
        ///
        [Pure]
        public Assembly GoTo
            (Calculation calculation,
             bool forceAbsolute = false,
             [CallerArgumentExpression(nameof(calculation))]
                string calculationProse = MissingProse)
        {
            var usedCalculation = calculation;

            if (!forceAbsolute)
            {
                usedCalculation = new (r =>
                {
                    var offset =
                        calculation.Calculate(r) - r.CurrentLineAddress - 2;

                    if (offset is >= sbyte.MinValue and <= sbyte.MaxValue)
                    {
                        throw new InvalidReferenceException
                            ($"{nameof(GoTo)} target '{calculationProse}'"
                             + $" was {offset} bytes away. Use {nameof(GoToIf)}"
                             + " instead with an always met condition or specify"
                             + $" {nameof(forceAbsolute)} to ignore this hint");
                    }

                    return calculation.Calculate(r);
                });
            }

            return assembly.AddOp
                (4, $"jmp {calculationProse}",
                 0xF3,
                 [ToAddressFragment(usedCalculation, calculationProse)]);
        }

        /// <summary>Cycles:2. Bytes:2</summary>
        ///
        /// <remarks>Sets:[I,N,Z,V,X,C]</remarks>
        ///
        [Pure]
        public Assembly StatusAndWith(int value)
        {
            if (value is < 0 or > 0xF)
            {
                throw new InvalidInstructionException
                    ($"Status '{value}' out of 4-bit range");
            }

            return assembly.AddOp(2, $"and ps, #{value}", 0xF4, [value]);
        }

        /// <summary>Cycles:2. Bytes:2</summary>
        ///
        /// <remarks>Sets:[I,N,Z,V,X,C]</remarks>
        ///
        [Pure]
        public Assembly StatusOrWith(int value)
        {
            if (value is < 0 or > 0xF)
            {
                throw new InvalidInstructionException
                    ($"Status '{value}' out of 4-bit range");
            }

            return assembly.AddOp(2, $"ori ps, #{value}", 0xF5, [value]);
        }

        /// <summary>Cycles:2. Bytes:2</summary>
        ///
        /// <remarks>Sets:[]</remarks>
        ///
        [Pure]
        public Assembly StackAdd(int offset)
        {
            if (offset is < sbyte.MinValue or > sbyte.MaxValue)
            {
                throw new InvalidInstructionException
                    ($"Offset '{offset}' out of signed 8-bit range");
            }

            return assembly.AddOp(2, $"add.b sp, #{offset}", 0xF6, [offset]);
        }

        // TODO: Misc instructions - mark some as obsolete

        private Assembly AddOp
            (int cycles, string comment, int op, int[]? bytes = null)
        {
            var fragment = new BytesFragment
                ([(byte)op, ..bytes?.Select(b => (byte)b) ?? []]);

            return assembly.AddLines
                ([new ([fragment], $"{CyclesPrefix(cycles)}{comment}")]);
        }

        private Assembly AddOp
            (int cycles, string comment, int op, Fragment[] fragments)
        {
            return assembly.AddLines
                ([new ([new BytesFragment([(byte)op]), ..fragments],
                                          $"{CyclesPrefix(cycles)}{comment}")]);
        }
    }

    private static string CyclesPrefix(int cycles) => $" {cycles}  ";

    private static void ThrowIfSame
        (Register a, Register b, [CallerMemberName] string caller = "Unknown")
    {
        if (a == b)
        {
            throw new InvalidInstructionException
                ($"{caller} cannot use the same register twice");
        }
    }

    private static byte[] ToLittleEndianBytes(int value)
    {
        var bytes = BitConverter.GetBytes(value);

        if (!BitConverter.IsLittleEndian) Array.Reverse(bytes);

        return bytes;
    }

    private static byte[]? AddressToBytes(int address) =>
        address is < 0 or > ushort.MaxValue
            ? null
            : ToLittleEndianBytes(address)[.. 2];

    private static ReferenceFragment ToAddressFragment
        (Calculation calculation, string calculationProse) =>
            new (byteCount: 2,
                 r => AddressToBytes(calculation.Calculate(r))
                   ?? throw new InvalidReferenceException
                            ($"'{calculationProse}' value not in 16-bit range"));

    private const string MissingProse = "???";
}