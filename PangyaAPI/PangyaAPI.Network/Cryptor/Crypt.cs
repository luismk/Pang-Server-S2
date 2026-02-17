using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace PangyaAPI.Network.Cryptor
{
    public class Crypt
    {
        public uint PrivateKey { get; private set; }
        public uint PublicKey { get; private set; }

        public Crypt(int parseKey, int lowKey)
        {
            int index = (parseKey << 8) | lowKey;
            PrivateKey = CryptoOracle.PRIVATE_KEY_TABLE[index];
            PublicKey = CryptoOracle.PUBLIC_KEY_TABLE[index];
        }

        public uint Encrypt(byte[] buffer)
        {
            if (buffer == null || buffer.Length == 0)
                throw new ArgumentException("Buffer inválido para criptografia.");

            buffer[0] = (byte)(PrivateKey & 0xFF);

            var plain = new byte[buffer.Length];
            Array.Copy(buffer, plain, buffer.Length);

            int limit = buffer.Length >= 4 ? 4 : buffer.Length;

            for (int i = 0; i < limit; i++)
                buffer[i] = (byte)((plain[i] ^ _8bitShift(PublicKey, i)) & 0xFF);

            for (int i = 4; i < buffer.Length; i++)
                buffer[i] = (byte)((buffer[i] ^ plain[i - 4]) & 0xFF);

            return PublicKey;
        }

        public uint Decrypt(byte[] buffer)
        {
            if (buffer == null || buffer.Length == 0)
                throw new ArgumentException("Buffer inválido para descriptografia.");

            int limit = buffer.Length >= 4 ? 4 : buffer.Length;

            for (int i = 0; i < limit; i++)
                buffer[i] = (byte)((buffer[i] ^ _8bitShift(PublicKey, i)) & 0xFF);

            for (int i = 4; i < buffer.Length; i++)
                buffer[i] = (byte)((buffer[i] ^ buffer[i - 4]) & 0xFF);

            return PrivateKey;
        } 

        private static int _8bitShift(uint bits, int shift)
        {
            shift *= 8;
            return (int)((bits >> shift) & 0xFF);
        }

    }
}
