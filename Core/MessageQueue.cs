using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Qx
{
    public interface IMessageQueue : IObservable<Message>
    {
        void push(IEnumerable<Message> messages);
        IEnumerable<Message> pull();
    }

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
