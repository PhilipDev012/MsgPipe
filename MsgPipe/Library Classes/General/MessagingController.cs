namespace Philip.MsgPipeTestApp
{
    public static class MessagingController
    {
        private static MsgPipeWriter? _AppCtrl_MsgPipe;

        public static MsgPipeWriter AppCtrl_MsgPipe { get { if (_AppCtrl_MsgPipe == null) _AppCtrl_MsgPipe = new MsgPipeWriter(); return _AppCtrl_MsgPipe; } set { } }
    }
}
