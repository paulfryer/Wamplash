var wsuri,
    demoRealm = "crossbardemo",
    demoPrefix = "io.crossbar.demo";

if (document.location.protocol === "file:") {
   wsuri =  "ws://127.0.0.1:8080/ws";
} else {
   var scheme = document.location.protocol === 'https:' ? 'wss://' : 'ws://';
   var port = document.location.port !== "" ? ':' + document.location.port : '';
   wsuri = scheme + document.location.hostname + port + "/ws";
}

