using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Qx
{
    /**
     * Message queue pushes messages to us.
     */
    public interface IMessageQueue : IObservable<Message>
    {
        void Publish(Message messages, double timeOut);
    }

    public interface IPushMessageQueue : IObservable<Message> {
        IDeliveryInfo Publish(Message message, double timeOut);
    }

    public interface IDeliveryInfo
    {
        async Boolean isDelivered();
        async void dispose();
    }

    // CompositeDisposable(d1,d2,...) : Disposable

    public class MessageQueue : IMessageQueue
    {
        private IKeyValueStore queue = new InMemoryKeyValueStore();

        public void push(IEnumerable<Message> messages)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<Message> pull()
        {
            throw new NotImplementedException();
        }

        public IDisposable Subscribe(IObserver<Message> observer)
        {
            throw new NotImplementedException();
        }
    }
}
