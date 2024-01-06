//#define DEBUG_ONLY
using System.Collections;
using System.Reflection;

namespace Philip.MsgPipeTestApp
{
    // This class does all the hard work by registering message handlers, preparing messages and then sending them out to the relevant handlers

    internal class IMPHandler
    {
        private class IMPReceiver
        {
            internal OnIMPDeliveredD? ReceiverMethod { get; set; }
            internal List<Type>? ReceiverTypes { get; set; }
            internal String? TargetItemId { get; set; }
        }

        private Queue MessageQueue { get; set; }
        private readonly List<IMPReceiver> IMPReceivers = new List<IMPReceiver>();

        /// <summary>
        /// Constructor called by the owning application
        /// </summary>
        public IMPHandler()
        {
            MessageQueue = new Queue();
        }

        /// <summary>
        /// For debug purposes only - outputs information about the current set of registered messages
        /// </summary>
        private void OutputReceiversInfo()
        {
#if DEBUG_ONLY
            var receivers = IMPReceivers.ToList();

            Debug.WriteLine("~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~");

            Console.WriteLine("=> Num. IMPReceivers is {0}", receivers.Count);

            foreach (IMPReceiver receiver in receivers)
            {
                if (receiver.ReceiverMethod == null)
                    continue;

                Debug.WriteLine("\tTarget Class         : " + receiver.ReceiverMethod.Target?.GetType().FullName);
                Debug.WriteLine("\tTargetItemId         : " + receiver.TargetItemId);
                Debug.Write("\tAccepted Pipline Msgs: ");

                if (receiver.ReceiverTypes == null)
                    continue;

                foreach (var receiverType in receiver.ReceiverTypes)
                    Debug.Write(receiverType.Name + ", ");
                Debug.WriteLine("");
                Debug.WriteLine("");
            }

            Debug.WriteLine("---------------------------------------------------------------------------------------");
#endif
        }

        /// <summary>
        /// Sends a message to whatever handlers are registered
        /// </summary>
        private void InvokeSend(IMPReceiver receiver,
                                PipelineArgs args)
        {
            receiver.ReceiverMethod?.Invoke(args);
        }

        /// <summary>
        /// Gets ready to send a message to all handlers that are registered for this specific message
        /// </summary>
        private void DeliverMessage(PipelineArgs args)
        {
            // Go through each of the receiver and send the message back to them
            foreach (IMPReceiver receiver in IMPReceivers.ToList())     //: IMPORTANT::::  The .ToList() prevents "Collection was modified; enumeration operation may not execute." (https://stackoverflow.com/questions/604831/collection-was-modified-enumeration-operation-may-not-execute)
            {
                if (receiver.ReceiverTypes == null)
                    continue;

                Boolean send = receiver.ReceiverTypes.Contains(args.GetType());

                if (send)
                {
                    // If a receiver accepts any messages
                    if (receiver.TargetItemId == null)
                        InvokeSend(receiver, args);

                    // If a receiver accepts messages aimed at a specific report
                    else if (receiver.TargetItemId == args.TargetItemId)
                        InvokeSend(receiver, args);
                }
            }
        }

        /// <summary>
        /// Gets the next message to be sent
        /// </summary>
        private void PrepareNextMessage()
        {
            // Get the next item
            PipelineArgs? args = Dequeue();

            // ... if there is one
            if (args != null)
            {
                DeliverMessage(args); // Go through each of the receiver and send the message back to them

                // Call again to empty the queue
                PrepareNextMessage();
            }
        }

        /// <summary>
        /// Checks to make sure that a message handler/receiver is valid
        /// </summary>
        private IMPReceiver? IsValidReceiver(MethodInfo method)
        {
            String? methodFullName = method.ReflectedType?.FullName?.ToLower();

            IMPReceiver? receiver = null;

            IMPReceivers.ToList().ForEach((rcv) =>
            {
                if (rcv.ReceiverMethod?.Target?.GetType().FullName?.ToLower() == methodFullName)
                {
                    receiver = rcv;
                    return;
                }

            });

            return receiver;
        }

        /// <summary>
        /// Gets a list of all unique target receivers, just to make sure we don't send duplicates
        /// </summary>
        internal List<String?> GetTargetReceiverIds()
        {
            return IMPReceivers.Select(c => c.TargetItemId).Distinct().ToList();
        }

        /// <summary>
        /// Puts a message handler into our queue
        /// </summary>
        internal void Enqueue(PipelineArgs args)
        {
            MessageQueue.Enqueue(args);

            PrepareNextMessage();
        }

        /// <summary>
        /// Removes a message handler from the queue
        /// </summary>
        internal PipelineArgs? Dequeue()
        {
            if (MessageQueue.Count != 0)
                return MessageQueue.Dequeue() as PipelineArgs;

            return null;
        }

        /// <summary>
        /// Registers a message handler/receiver
        /// </summary>
        internal void RegisterReceiverP(OnIMPDeliveredD receiver,
                                        List<Type> receiverTypes,
                                        String? targetItemId)
        {
            // If we have been told to register a receiver that has no filters (receiverTypes) then there is no
            // point in having it - this check is here because at development time I had this scenario, so this check is just
            // to make sure we don't register rubbish.
            if (receiverTypes == null || receiverTypes.Count == 0)
                return;

            Boolean isFound = false;

            IMPReceivers.ToList().ForEach((rcv) =>
            {
                if (rcv.ReceiverMethod?.GetMethodInfo().DeclaringType?.FullName + "." + (rcv.ReceiverMethod?.GetMethodInfo().Name) == receiver.GetMethodInfo().DeclaringType?.FullName + "." + (receiver.GetMethodInfo().Name))
                {
                    isFound = true;
                    return;
                }
            });

            if (!isFound)
                IMPReceivers.Add(new IMPReceiver() { ReceiverMethod = receiver, ReceiverTypes = receiverTypes, TargetItemId = targetItemId });
        }

        /// <summary>
        /// Unregisteres a message handler/receiver
        /// </summary>
        internal void UnregisterReceiver(OnIMPDeliveredD? receiverMethod)
        {
            IMPReceivers.ToList().ForEach((rcv) =>
            {
                if (rcv.ReceiverMethod == receiverMethod)
                {
                    IMPReceivers.Remove(rcv);

                    return;
                }
            });
        }

        /// <summary>
        /// Unregisteres a message handler/receiver
        /// </summary>
        internal void UnregisterReceiver(Object receiverClassInstance)
        {
            Type type = receiverClassInstance.GetType();

            foreach (var method in type.GetMethods(BindingFlags.Instance | BindingFlags.NonPublic))
            {
                var paramInfo = method.GetParameters();

                foreach (var param in paramInfo)
                {
                    if (param.ParameterType == typeof(PipelineArgs) && method.ReturnType == typeof(void))
                    {
                        IMPReceiver? receiver = IsValidReceiver(method);
                        if (receiver != null)
                            UnregisterReceiver(receiver.ReceiverMethod);
                    }
                }
            }
        }

        /// <summary>
        /// Unregisters all message handlers/receivers
        /// </summary>
        internal void UnregisterReceivers(String targetItemId)
        {
            IMPReceivers.ToList().ForEach((rcv) =>
            {
                if (rcv.TargetItemId == targetItemId || targetItemId == null)
                    IMPReceivers.Remove(rcv);
            });
        }
    }
}
