# Qx

Reactive Messaging

## Design

 * Two-way network channels are modeled as Subject<String>
 * A `RemoteSubject : Subject<String>` implements a protocol of three
   messages.
   * `OnNext:<message text>`
   * `OnError:<message text>`
   * `OnCompleted`
 * A `RabbitPubSubSubject(host,queueName) : Subject<String>` allows
   sending messages over RabbitMQ with a publish subscribe mechanism.

## Proposal

Remoting ``Rx`` is implemented using an interface ``HereComeDragons`` and
an extension method on ``IObservable``. They are designed in such a
way that the user has an Rx stream with messages and another to handle
network issues. The latter can be ignored of you don't want to recover.

We define two semantics, for communication through a message broker
and for point-to-point communication. Any implementation should
document which semantic it implements.

*The point to point part is particularly unfinished*

### Interface and extension method

The extension methods are provided by ``Qx``, the interface has to be
implemented for every communication method you want to support.

```csharp

// there should be some things here that look kind of dual to each
// other

interface Exposer<TState>
{
    TState SubscribeTo(IObservalbe<string>) {}
}

interface Consumer<TState>
{
    TState Subscribe(IObservalbe<string>) {}
}

class ExtensionMethods
{
    public IObservable<TState> Expose<TState>(this IObservable<string>,
                                              Exposer<TState>)
    {
        // create a connection with SubscribeTo
        // create ChannelState with possible reconnect behaviour
        // protocol logic lives here
    }

    public Pair<IObservable<string>,IObservable<TState>> Consume<TState>(this Consumer<TState>)
    // or
    public IObservable<string> Consume<TState>(this Consumer<TState>,
                                        IObservable<TState> => ())
    {
        // create a connection with Subscribe
        // create ChannelState with possible reconnect behaviour
        // protocol logic lives here
    }

    IObservable<string> ConsumeSimple<TState>()
    {
        return Consume(state => ())
    }
    public IDisposable ExposeSimple<TState>(this IObservable<string>, HereComeDragons<TState>)
    {
    }
}

// point-to-point connections probably need their own Listen method
// and an alternative to Consumer that creates new connection objects on
// an incoming connection.

```

We present a list of actor roles and the actions ``[A]`` and events
``[E]`` they have. In the semantics description below, we identify
which roles actors have and which events map to what actions.

IObservable
 * Expose [E] -- request to expose this stream to a channel

ToChannel
 * OnNext [E] -- next value from stream event
 * OnError [E] -- error on stream event
 * OnCompleted [E] -- stream completed event
 * SubscribeTo [E] -- asked to subscribe to a stream

ToNetwork
 * Connect [A] -- connect to remote host
 * Send [A] -- send message over connection
 * Close [A] -- close connection
 * Error [E] -- error happened

ToBroker
 * Connect [A] -- connect to broker
 * Send [A] -- send message to broker
 * Close [A] -- close connection to broker
 * Error [E] -- error occurred
 
FromChannel
 * OnNext [A] -- put next value in stream
 * OnError [A] -- put error on stream
 * OnCompleted [A] -- set stream as completed
 * Subscribe [E] -- someone subscribes to the stream, invoked by HereComeDragons.Consume
 * Dispose [E] -- someone stops listening
 * Consume [E] -- a channel create request

FromNetwork
 * Listen [A] -- listen to remote connections
 * Connect [E] -- new connection comes in
 * Receive [E] -- new message comes from network
 * Close [E] -- connection was closed
 * Error [E] -- error occurred

FromBroker
 * Connect [A] -- connect to broker
 * Receive [E] -- message received from broker
 * Close [A] -- close connection to broker
 * Error [E] -- an error occurred

ChannelState (= IObservable<TState>)
 * Subscribe [E] -- someone starts listening to channel state
 * OnNext [A] -- put the channel state in the stream
 * OnError [A] -- put a channel error in the stream
 * OnCompleted [A] -- signal the channel terminated peacefully

General notes:
 * Reconnection is only possible on ends where there is a connect
   action.
 * To keep things simple for the programmer:
   - Expose is one client connection
   - Consume is one client connection
   - There's no magic with every subscribe being a different thing or
     whatever, because then it's hard to map it on their knowledge of
     the middleware.
 * Failures are factored out from the value channels and exposed
   through the ChannelState. This way you can differentiate between
   the network aspects and the application level stream. For brokers
   this means you will never receive OnError or OnCompleted, because
   that doesn't exist in that world.
 * If no one is subscribed, nothing happens. So if you
   subscribe/dispose a lot but don't want to miss messages, you better
   stick a buffer between.
 * ChannelState manages it's subscribes in generations. If OnCompleted
   or OnError, the previous generation is finished and let go. The
   first of the next generation triggers a connect (although a connect
   always happen before the first generation). Some smart locks should
   make sure we don't attempt multiple connections.

Broker notes:
 * Consider broker as part of infra, so no QueueDeleted events or
   anything
 * We assume ``Rx`` behaviour, so all consumers on one connection get
   the same message. No multiplexing!
 * Physical connections can be shared, as long as it behaves the same
   as if two connections were used.

### Semantics for message broker

*Sending -- ToChannel-ToBroker*

| Event | Action |
|-------|--------|
| ToChannel.OnNext | ToBroker.Send |
| ToChannel.OnError | ToBroker.Close, ChannelState.OnCompleted |
| ToChannel.OnCompleted | ToBroker.Close, ChannelState.OnCompleted |
| ToChannel.Expose | ToBroker.Connect, ChannelState.OnNext |
| ToBroker.Error | ToBroker.Close, ChannelState.OnError |
| ChannelState.Subscribe | (last state unless error) ToBroker.Connect, ChannelState.OnNext, ToBroker.Send(pending) |

Reconnects are possible.

For optimization the channel could be closed if there are no message
for a certain time and reopened if a new message arrives.

*Receiving -- FromBroker-FromChannel*

| Event | Action |
|-------|--------|
| FromBroker.Receive | FromChannel.OnNext |
| FromBroker.Error | FromBroker.Close, ChannelState.OnError |
| FromChannel.Consume | FromBroker.Connect, ChannelState.OnNext |
| ChannelState.Subscribe | (last state unless error) FromBroker.Connect, ChannelState.OnNext |

Reconnects are possible.

For optimization the connection can be closed if there are no
listeners and reopened when someone subscribes.

### Semantics for point-to-point connections

*Sending -- ToChannel-ToNetwork*

| Event | Action |
|-------|--------|
| ToChannel.OnNext | ToNetwork.Send |
| ToChannel.OnError | ToNetwork.Send(error), ToNetwork.Close, ChannelState.OnCompleted |
| ToChannel.OnCompleted | ToNetwork.Close, ChannelState.OnCompleted |
| ToChannel.Expose | ToNetwork.Connect, ChannelState.OnNext |
| ToNetwork.Error | ToNetwork.Close, ChannelState.OnError |
| ChannelState.Subscribe | (last state unless error) ToNetwork.Connect, ChannelState.OnNext, ToNetwork.Send(pending) |

Reconnects are possible.

Optimizing disconnects and reconnects are not allowed for point-to-point connections.

*Receiving -- FromNetwork-FromChannel*

| Event | Action |
|-------|--------|
| FromNetwork.Connect | ?? |
| FromNetwork.Receive | FromChannel.OnNext |
| FromNetwork.Receive(error) | FromChannel.OnError, ChannelState.OnCompleted |
| FromNetwork.Error | FromNetwork.Close, ChannelState.OnError |
| FromNetwork.Close | FromChannel.OnCompleted, ChannelState.OnCompleted |
| ChannelState.Subscribe | (last state) |

Reconnects are *not* possible.

### Scenarios

```csharp

// write to queue
IObservable<string> someThing;
var channel = someThing.Expose(AMQP("url").in("endpoint"));

// subscribe gives one connection
// it either gives an error or completes
// on retry, if reconnection is possible, it repeats, otherwise it throws ReconnectNotPossible
// we cannot easily expose the disposable of the connection here, which equates to stop sending despite what the stream does
//     this can be simulated however with a switch before the expose. Also other scenarios like user connect/disconnect can be done this way.
channel.Retry().Subscribe( (connection) => {
    connection.bandwidth.Subscribe(...);
}, (error) => {
}, () => {
} ) // IDisposable disposes subscriptions and ultimately connection

```
