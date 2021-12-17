namespace Core.Repository
{
    public interface IRepository<T> where T : class
    {
        public T Get();
        public void Save(T model);
        public void Clear();
    }
}