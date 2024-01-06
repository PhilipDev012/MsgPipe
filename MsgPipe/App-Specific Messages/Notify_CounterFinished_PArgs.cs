
namespace Philip.MsgPipeTestApp
{
    public class Notify_CounterFinished_PArgs : Notify_PArgs
    {
        public Int32 FinalValue { get; private set; }

        public Notify_CounterFinished_PArgs(String? targetItemId, Int32 finalValue)
            : base(targetItemId)
        {
            FinalValue = finalValue;
        }
    }
}
