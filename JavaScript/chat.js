$(function(){

    var channel = new STOMPChannel("http://localhost:15674/stomp", "/exchange/rx");

    var input = $("#input");
    var output = $("#output");
    var line = input.keyupAsObservable()
        .selectMany(function (keyevent){
            if ( keyevent.which == 13 ) {
                var val = input.val();
                input.val("");
                return Rx.Observable.returnValue(val);
            } else if ( keyevent.which == 27 ) {
                input.val("");
            }
            return Rx.Observable.never();
        });
    line.subscribe(channel.asObserver());
    channel.asObservable().subscribe(function(line){
        output.html(output.html()+line+"<br/>");
    });
    
    function STOMPChannel(url,endpoint) {
        var ws = new SockJS(url);
        var client = Stomp.over(ws);
        // RabbitMQ-STOMP-Web doesn't support heartbeats over SockJS
        // It will lose the connection on the first heartbeat try
        client.heartbeat.outgoing = 0;
        client.heartbeat.incoming = 0;
        client.connect("guest", "guest", on_connect, on_error, "/");

        var queue = [];

        function ignore_call() {}
        function unguarded_call(f) {
            f();
        }
        var guarded_call = function(f) {
            queue.push(f);
        };
        
        function on_connect() {
            for ( var i = 0; i < queue.length; i++ ) {
                queue[i]();
            }
            delete queue;
            guarded_call = unguarded_call;
        }

        function on_error(error) {
            guarded_call = ignore_call;
            console.error(error);
        }

        this.asObserver = function() {
            return Rx.Observer.create(
                function(next){
                    guarded_call(function(){
                        client.send(endpoint, {}, "OnNext:"+next);
                    });
                }, function(error){
                    guarded_call(function(){
                        client.send(endpoint, {}, "OnError:"+error);
                    });
                }, function(){
                    guarded_call(function(){
                        client.send(endpoint, {}, "OnCompleted");
                    });
                });
        };

        this.asObservable = function() {
            return Rx.Observable.create(function(observer){
                var sid = null;
                guarded_call(function(){
                    sid = client.subscribe(endpoint, function(frame){
                        var protoMessage = frame.body;
                        if ( /^OnNext:/.test(protoMessage) ) {
                            observer.onNext(protoMessage.substring(7));
                        } else if ( /^OnError:/.test(protoMessage) ) {
                            observer.onError(protoMessage.substring(8));
                            dispose();
                        } else if ( /^OnCompleted/.test(protoMessage) ) {
                            observer.onCompleted();
                            dispose();
                        } else {
                            console.error("Unknown protocol message",protoMessage);
                        }
                    });
                });
                function dispose() {
                    guarded_call(function(){
                        if ( sid !== null ) {
                            client.unsubscribe(sid);
                            sid = null;
                        }
                    });
                };
                return dispose;
            });
        };

    }

});
