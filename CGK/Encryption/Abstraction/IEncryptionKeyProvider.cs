namespace CGK.Encryption.Abstraction
{
    public interface IEncryptionKeyProvider
    {
        byte[] GetKey();
    }
}