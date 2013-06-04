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
