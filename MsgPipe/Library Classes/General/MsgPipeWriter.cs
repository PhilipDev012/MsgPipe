using System.Windows;
using System.Windows.Threading;

namespace Philip.MsgPipeTestApp
{
    public delegate void OnIMPDeliveredD(PipelineArgs args);

    /// <summary>
    /// This is the primary class that registers and unregisters message handlers
    /// </summary>
    public class MsgPipeWriter
    {
        private IMPHandler IMP { get; set; }
        private Queue<PipelineArgs> QueuedPipline { get; set; } = new Queue<PipelineArgs>();
        private DispatcherTimer QueuedPiplineTimer { get; set; }

        /// <summary>
        /// Basic constructor that initializes a timer that sends messages via a separate task
        /// </summary>
        public MsgPipeWriter()
        {
            IMP = new IMPHandler();

            QueuedPiplineTimer = new DispatcherTimer(TimeSpan.FromMilliseconds(100),
                                                        DispatcherPriority.Send,
                                                        QueuedPiplineTimer_Elapsed,
                                                        Application.Current.Dispatcher)
            {
                IsEnabled = false
            };
        }

        /// <summary>
        /// Messages are sent via a separae task so as not to hold up the application
        /// </summary>
        private void QueuedPiplineTimer_Elapsed(Object? sender,
                                                EventArgs e)
        {
            QueuedPiplineTimer.IsEnabled = false;

            if (QueuedPipline.Count > 0)
            {
                Task task = Task.Factory.StartNew
                (
                    state =>
                    {
                        SendPiplineItem(QueuedPipline.Dequeue());
                    },
                    TaskCreationOptions.PreferFairness | TaskCreationOptions.AttachedToParent,
                    new CancellationTokenSource().Token
                )
                .ContinueWith((c) =>
                {
                    QueuedPiplineTimer.IsEnabled = QueuedPipline.Count > 0;
                });
            }
        }

        /// <summary>
        /// Checks to see if a list is either null or empty
        /// </summary>
        public static Boolean IsNullOrEmpty<T>(List<T> source)
        {
            return source == null || source.Count == 0;
        }

        /// <summary>
        /// Sends a pipeline message. If args.TargetItemId == null then the message is sent to all objects that have registered for that actual message type.
        /// </summary>
        public void SendPiplineItem(PipelineArgs args)
        {
            var targetItemIds = IMP.GetTargetReceiverIds();

            if (IsNullOrEmpty(targetItemIds))
                return;

            // Queue the message
            foreach (var id in targetItemIds)
            {
                args.TargetItemId = id;
                IMP.Enqueue(args);
            }
        }

        /// <summary>
        /// Registers a receiver delegate method along with the types of messages its interested in
        /// </summary>
        public void RegisterReceiver(OnIMPDeliveredD receiver,
                                        List<Type> receiverTypes,
                                        String? targetItemId)
        {
            IMP.RegisterReceiverP(receiver, receiverTypes, targetItemId);
        }

        /// <summary>
        /// Unregisters a specific receiver.  A class can have as many receivers as it likes, and this method will unregister just one of those items
        /// </summary>
        public void UnregisterReceiver(OnIMPDeliveredD receiver)
        {
            IMP.UnregisterReceiver(receiver);
        }

        /// <summary>
        /// Unregisters all receivers that belong to a class. A class can have as many receivers as it likes, and this method will unregister all of those items.
        /// </summary>
        public void UnregisterReceiver(Object receiverClassInstance)
        {
            IMP.UnregisterReceiver(receiverClassInstance);
        }

        /// <summary>
        /// Sends a pipeline message. If args.TargetItemId == null then the message is sent to all objects that have registered for that actual message type.
        /// </summary>
        public void Send(PipelineArgs args)
        {
            if (args.CanBuffer)
            {
                QueuedPipline.Enqueue(args);
                if (!QueuedPiplineTimer.IsEnabled)
                    QueuedPiplineTimer.IsEnabled = true;
            }

            else
                SendPiplineItem(args);
        }

        /// <summary>
        ///  Unregisters all receivers that relate to a specific report guid
        /// </summary>
        public void UnregisterReceivers(String targetItemId)
        {
            IMP.UnregisterReceivers(targetItemId);
        }
    }
}
