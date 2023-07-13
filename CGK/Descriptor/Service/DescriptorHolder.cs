using System;
using System.Collections.Generic;
using System.Linq;

namespace CGK.Descriptor.Service
{
    public class DescriptorHolder
    {
        private readonly List<object> _createdDescriptor = new List<object>();

        public void Register<T>(T newDescriptor)
        {
            T loadedDescriptor = GetDescriptor<T>();
            if (loadedDescriptor != null)
            {
                throw new ArgumentException("Descriptor already registered");
            }

            _createdDescriptor.Add(newDescriptor);
        }

        public void OverrideDescriptor<T>(T newDescriptor)
        {
            if (newDescriptor == null)
            {
                return;
            }

            _createdDescriptor.RemoveAll(t => t is T);
            _createdDescriptor.Add(newDescriptor);
        }

        public T GetDescriptor<T>()
        {
            object descriptor = _createdDescriptor.FirstOrDefault((s) => s is T);
            if (descriptor == null)
            {
                return default;
            }

            return (T)descriptor;
        }

        public List<object> GetAllDescriptors<T>()
        {
            List<object> descriptor = _createdDescriptor.Where((s) => s is T).ToList();
            return descriptor.Count == 0 ? default : descriptor;
        }
    }
}