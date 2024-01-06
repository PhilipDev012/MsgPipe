namespace Philip.MsgPipeTestApp
{
    public abstract class Notify_PArgs : PipelineArgs
    {
        protected Notify_PArgs(String? targetItemId)
            : base(targetItemId)
        {
        }
    }
}
