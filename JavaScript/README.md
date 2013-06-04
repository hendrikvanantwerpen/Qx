# QxJavaScript

Qx for JavaScript. Currently supports:
 * STOMP message brokers with `STOMPChannel`

## Semantics

_This describes current, not necessarily desired, semantics_

Currently the channel forwards everything to the queue. This means
that anybody sending onCompleted or onError will terminate all
listeners. This does not make much sense with a queue, which will
still exist and produce values if others write to it or people connect
to it again.

## Example

We assume an running RabbitMQ, set up with `rabbitmq-create-rx.py`.

```javascript
var channel = new STOMPChannel("http://localhost:15674/stomp", "/exchange/rx");
var Rx.Observable.return("Hello, world!").subscribe(channel.asObserver());
channel.asObservable().subscribe(function(message){
    console.log(message);
});
```
