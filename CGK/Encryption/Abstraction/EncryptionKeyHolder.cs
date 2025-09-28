using CGK.Encryption.Abstraction;

namespace RuntimeSecurity
{
    internal partial class EncryptionKeyHolder : IEncryptionKeyProvider
    {
        public static readonly byte[] Key;

        public byte[] GetKey() => Key;
    }
}