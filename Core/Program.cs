using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleApplication1
{
    class Program
    {
        static void Main(string[] args)
        {
            IPushMessageQueue messageQueue = IronMQ.getPushQueue("name");
            messageQueue.Subscribe((message) =>
            {
                // do stuff and run out of time
                message.ExtendReservation(20); // equiv to next
                message.OnNext(20); // keep alive. Q: how to nicely wrap this? something.delay(Time s), or just like a keyword, "Delay()"S. How to DSL in C#?
                message.OnCompleted(); // done
                message.Done();
                message.Throw(ReleaseException); // won't handle
                // lala
            });
            // TODO:
            // - Configuration handling in modern C# libraries...

            // Publish(content, time-out, delay, expires_in)
            messageQueue.Publish(String body, 20); // unicast / multicast semantics?
            messageQueue.Publish(new Message("Hello"); // optional time-out, where to store default: option, in the messageQueue -> can't put into the interface? 
            messageQueue.Publish(new Message("Hello"), 20);
            messageQueue.Publish(new Message { "Hello", 20 });

            IObservable x = IronMQ.getPullQueue("name", TimeSpan.FromSeconds(10));
            IEnumerableAsync y = IronMQ.getPullQueue("name");
        }
    }
}
