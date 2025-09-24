namespace CGK.Descriptor.Service
{
    public interface IDescriptorFileLoader
    {
        public T LoadDescriptorFromString<T>(string content);
        public T LoadDescriptor<T>(string path);

        public DescriptorCollection<T> LoadDescriptorCollection<T>(string filePath, string rootElementName,
            string collectionElementName);
    }
}