using System.Diagnostics;
using System.Windows;

namespace Philip.MsgPipeTestApp
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        /// <summary>
        /// MainWindow constructor
        /// </summary>
        public MainWindow()
        {
            InitializeComponent();

            // Start here!
            OnRegisterMsgPipeReceivers();

            // Do some work to show how to send and receive messages
            new Counter().Run();
        }

        /// <summary>
        /// Called when a message has been sent.  Handle the messages here
        /// </summary>
        /// <param name="args">This is the app-specific message that has been sent</param>
        private void OnIMPDelivered(PipelineArgs args)
        {
            // Do something to handle the app-specific notifications
            if (args is Notify_CounterStarted_PArgs)
            {
                Notify_CounterStarted_PArgs? pargs = args as Notify_CounterStarted_PArgs;
                Debug.WriteLine("Starting Counter...");
                return;
            }

            if (args is Notify_CounterChanged_PArgs)
            {
                Notify_CounterChanged_PArgs? pargs = args as Notify_CounterChanged_PArgs;
                Debug.WriteLine("     Counter Value = " + pargs?.CounterValue);
                return;
            }

            if (args is Notify_CounterFinished_PArgs)
            {
                Notify_CounterFinished_PArgs? pargs = args as Notify_CounterFinished_PArgs;
                Debug.WriteLine("     Final Counter Value = " + pargs?.FinalValue);
                return;
            }
        }

        /// <summary>
        /// Register all the messages you want to handle
        /// </summary>
        private void OnRegisterMsgPipeReceivers()
        {
            MessagingController.AppCtrl_MsgPipe.RegisterReceiver(OnIMPDelivered,
                [
                    typeof(Notify_CounterStarted_PArgs),
                    typeof(Notify_CounterChanged_PArgs),
                    typeof(Notify_CounterFinished_PArgs),
                ],
            null);
        }
    }
}