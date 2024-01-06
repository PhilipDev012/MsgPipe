
namespace Philip.MsgPipeTestApp
{
    public class Notify_CounterChanged_PArgs : Notify_PArgs
    {
        public Int32 CounterValue { get; private set; }

        public Notify_CounterChanged_PArgs(String? targetItemId, Int32 counterValue)
            : base(targetItemId)
        {
            CounterValue = counterValue;
        }
    }
}
