using System;
using System.IO;
using System.Text;
using RT.Util.ExtensionMethods;

namespace RT.Util.Collections
{
    /// <summary>
    ///     Encapsulates a binary value 128 bits long. See Remarks.</summary>
    /// <remarks>
    ///     This type is significantly more memory-efficient compared to <c>byte[16]</c> when only one copy of the value needs
    ///     to be stored. Specifically, the byte array uses four extra IntPtrs per instance, which, on a 64-bit platform,
    ///     amounts to a three-fold RAM usage increase. It can also be compared for equality using just two integer
    ///     comparisons in safe code.</remarks>
    public struct Bin128
    {
        /// <summary>Gets a 128-bit binary value consisting of all zeroes.</summary>
        public static Bin128 Zero = new Bin128();

        private ulong _a, _b;

        /// <summary>
        ///     Constructor. Initializes this value to contain the same data as the specified byte array.</summary>
        /// <param name="bytes">
        ///     Byte array containing the data.</param>
        /// <param name="offset">
        ///     The offset at which the data begins. The length is fixed at 16 bytes.</param>
        public Bin128(byte[] bytes, int offset = 0)
        {
            if (bytes == null || offset < 0 || offset + 16 > bytes.Length)
                throw new ArgumentException();
            _a = (ulong) (uint) (bytes[00] | (bytes[01] << 8) | (bytes[02] << 16) | (bytes[03] << 24)) | ((ulong) (uint) (bytes[04] | (bytes[05] << 8) | (bytes[06] << 16) | (bytes[07] << 24)) << 32);
            _b = (ulong) (uint) (bytes[08] | (bytes[09] << 8) | (bytes[10] << 16) | (bytes[11] << 24)) | ((ulong) (uint) (bytes[12] | (bytes[13] << 8) | (bytes[14] << 16) | (bytes[15] << 24)) << 32);
        }

        /// <summary>Compares the two values and returns true iff they are equal.</summary>
        public static bool operator ==(Bin128 v1, Bin128 v2) { return (v1._a == v2._a) && (v1._b == v2._b); }
        /// <summary>Compares the two values and returns true iff they are different.</summary>
        public static bool operator !=(Bin128 v1, Bin128 v2) { return (v1._a != v2._a) || (v1._b != v2._b); }
        /// <summary>Compares an object to this value and returns true iff it's a Bin128 value that's equal.</summary>
        public override bool Equals(object other) { return other is Bin128 && this == (Bin128) other; }
        /// <summary>Gets the hash code for this value.</summary>
        public override int GetHashCode() { return _a.GetHashCode() * 5779 + _b.GetHashCode(); }

        /// <summary>Returns the binary value as a byte array. This method is not very efficient.</summary>
        public byte[] ToArray()
        {
            var ms = new MemoryStream();
            Write(ms);
            return ms.ToArray();
        }

        /// <summary>Converts the binary value to a hexadecimal string representation.</summary>
        public override string ToString()
        {
            return ToArray().ToHex();
        }

        /// <summary>Writes the value to the specified stream as 16 bytes.</summary>
        public void Write(Stream stream)
        {
            Write(new BinaryWriter(stream));
        }

        /// <summary>Writes the value to the specified binary writer as 16 bytes.</summary>
        public void Write(BinaryWriter bw)
        {
            bw.Write(_a);
            bw.Write(_b);
        }

        /// <summary>Reads a 128-bit binary value from the specified stream.</summary>
        public static Bin128 Read(Stream stream)
        {
            return Read(new BinaryReader(stream));
        }

        /// <summary>Reads a 128-bit binary value from the specified binary reader.</summary>
        public static Bin128 Read(BinaryReader br)
        {
            var result = new Bin128();
            result._a = br.ReadUInt64();
            result._b = br.ReadUInt64();
            return result;
        }
    }

}
