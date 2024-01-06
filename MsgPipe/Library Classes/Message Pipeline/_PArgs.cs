namespace Philip.MsgPipeTestApp
{
    public abstract class PipelineArgs
    {
        public String? TargetItemId { get; internal set; }  // This is the report guid that the command should be sent to (if no specific target report then set to null)
        public Boolean CanBuffer { get; set; } = false;

        protected PipelineArgs(String? targetItemId)
        {
            TargetItemId = targetItemId;
        }
    }
}
