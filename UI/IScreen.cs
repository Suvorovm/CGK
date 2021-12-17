using Core.UI.Model;
using JetBrains.Annotations;

namespace Core.UI
{
    public interface IScreen
    {
        public void Configure([CanBeNull] IScreenModel screenModel);
    }
}