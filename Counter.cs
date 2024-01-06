namespace Philip.MsgPipeTestApp
{
    internal class Counter
    {
        /// <summary>
        /// Test method to show how to send an app-specific message
        /// </summary>
        internal void Run()
        {
            // Send "Notify_CounterStarted_PArgs" to say that the counter is about to start
            MessagingController.AppCtrl_MsgPipe.Send(new Notify_CounterStarted_PArgs(null));

            Int32 index = 0;

            for (; ; )
            {
                // Send "Notify_CounterChanged_PArgs" to say that the counter value has changed
                MessagingController.AppCtrl_MsgPipe.Send(new Notify_CounterChanged_PArgs(null, index));

                // Stop at 100
                if (++index == 100)
                    break;
            }

            // Send "Notify_CounterFinished_PArgs" to say that the counter has finished
            MessagingController.AppCtrl_MsgPipe.Send(new Notify_CounterFinished_PArgs(null, index));
        }
    }
}
