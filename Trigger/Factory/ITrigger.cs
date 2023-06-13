using Client.Core.Settings;
using Client.Core.Trigger.Descriptor;
using UniRx;

namespace Client.Core.Trigger.Factory
{
    public interface ITrigger
    {
        Subject<Unit> OnResult { get; }
        void Init(GameContext gameContext, TriggerDescriptor triggerDescriptor);

        string GetFormattedProgress();

        string[] GetParamsToSave();

        float GetProgressInPercent();
        void RestoreState(string[] snapshotItemTriggerParams);
    }
}