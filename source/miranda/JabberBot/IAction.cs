using System.Threading;

namespace miranda.JabberBot
{
    public interface IAction
    {
        string Perform(ActionType actionType, params object[] args);
        SynchronizationContext SyncContext { get; }
    }

    public enum ActionType
    {
        None = 0,
        s,
        Log,
        list,
        r,
        lf,
        GetPics
    }
}