using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reactive;

namespace Qx
{
    public interface IMessage : IObserver<Unit>
    {
    }

    public class Message : IMessage
    {
        public String body { get; private set; }

        public Message(String body)
        {
            this.body = body;
        }

        public void OnCompleted()
        {
            throw new NotImplementedException();
        }

        public void OnError(Exception error)
        {
            throw new NotImplementedException();
        }

        public void OnNext(Unit value)
        {
            throw new NotImplementedException();
        }
    }
}
