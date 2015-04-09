/******************************************************************************
 *
 *  Copyright (C) 2012-2014 Tavendo GmbH.
 *
 *  Licensed under the Apache 2.0 license
 *  http://www.apache.org/licenses/LICENSE-2.0.html
 *
 ******************************************************************************/

/*
   Requires the following elements to be present in the HTML:
      - .statusline - displays the connection state
      - #controller-channel - to display the currently used channel
      - #controller-channel-switch - button to start switch to another channel
      - #controller-channel-cancel - button to cancel changing to another channel
*/
/*
   Checks for channel information encoded in the URL to automatically switch to that channel
*/

"use strict";

var wsuri;
var sess;
var windowUrl;
var isReconnect = false;

if (document.location.protocol === "file:") {
   wsuri =  "ws://127.0.0.1:8080/ws";
} else {
   var scheme = document.location.protocol === 'https:' ? 'wss://' : 'ws://';
   var port = document.location.port !== "" ? ':' + document.location.port : '';
   wsuri = scheme + document.location.hostname + port + "/ws";
}


var _idchars = "0123456789";
var _idlen = 6;
var _idpat = /^\d*$/;


function randomChannelId() {
   var id = "";
   for (var i = 0; i < _idlen; i += 1) {
      id += _idchars.charAt(Math.floor(Math.random() * _idchars.length));
   }
   return id;
};

function checkChannelId(id) {
   return id != null && id != "" && id.length == _idlen && _idpat.test(id);
}

function isValueChar(e) {

   var kc = e.keyCode;
   if ((kc > 8 && kc < 46 && kc !== 32) || (kc > 90 && kc < 94) || (kc > 111 && kc < 186) ) {
      return false;
   } else {
      return true;
   }
}

var controllerChannelId;
var controllerChannel = null;
var controllerChannelSwitch = null;
var controllerChannelCancel = null;


function switchChannel(newChannelId) {

   onChannelSwitch(controllerChannelId, newChannelId);

   controllerChannelId = newChannelId;
   controllerChannel.disabled = false;
   controllerChannelSwitch.disabled = true;
   controllerChannelCancel.disabled = true;
   controllerChannel.value = controllerChannelId;
}


function updateStatusline(status) {
   $(".statusline").text(status);
};

var connection = null;
function connect() {

   connection = new autobahn.Connection({
      url: wsuri,
      realm: demoRealm,
      max_retries: 30,
      initial_retry_delay: 2
      }
   );

   connection.onopen = function (session) {

      sess = session;

      controllerChannelId = null;

      setupDemo();

      updateStatusline("Connected to " + wsuri);

      // establish prefix to use for shorter URL notation
      // sess.prefix("api", channelBaseUri);

      if (checkChannelId(controllerChannel.value)) {
         switchChannel(controllerChannel.value);
      } else {
         switchChannel(randomChannelId());
      }

      if(typeof(afterAuth) !== "undefined" ) {
         afterAuth(); // only exists in colorpicker demo
      }



   };

   connection.onclose = function() {
      sess = null;
      console.log("connection closed ", arguments);
   }

   connection.open();
}

var setupInfoDictionary = {};

$(document).ready(function()
{
   updateStatusline("Not connected.");

   controllerChannelSwitch = document.getElementById('controller-channel-switch');
   controllerChannelCancel = document.getElementById('controller-channel-cancel');
   controllerChannel = document.getElementById('controller-channel');

   // select the current channel string on focus
   controllerChannel.onmouseup = function() { return false; };
   controllerChannel.onfocus = function(evt) {
         evt.target.select();
   };

   // check for additional demo setup data in the URL
   windowUrl = document.URL; // string

   // check if '?' fragment is present
   // then make dictionary of values here
   if (windowUrl.indexOf('?') !== -1) {
      var setupInfoRaw = windowUrl.split('?')[1];
      var setupInfoSeparated = setupInfoRaw.split('&');

      for (var i = 0; i < setupInfoSeparated.length; i++) {
         var pair = setupInfoSeparated[i].split('=');
         var key = pair[0];
         var value = pair[1];
         setupInfoDictionary[key] = value;
      }

   }
   if ("channel" in setupInfoDictionary) {
      controllerChannel.value = setupInfoDictionary.channel;
   }

   controllerChannel.onkeyup = function (e) {

      if (controllerChannel.value != controllerChannelId) {

         controllerChannelCancel.disabled = false;

         if (controllerChannel.value.length == _idlen && _idpat.test(controllerChannel.value)) {
            controllerChannelSwitch.disabled = false;
         } else {
            controllerChannelSwitch.disabled = true;
         }
      } else {
         controllerChannelCancel.disabled = true;
         controllerChannelSwitch.disabled = true;
      }
   };

   controllerChannelCancel.onclick = function () {
      controllerChannel.value = controllerChannelId;
      controllerChannelSwitch.disabled = true;
      controllerChannelCancel.disabled = true;
   }

   controllerChannelSwitch.onclick = function () {

      switchChannel(controllerChannel.value);
      controllerChannelSwitch.disabled = true;
      controllerChannelCancel.disabled = true;
   }

   // setupDemo();

   connect();

});
