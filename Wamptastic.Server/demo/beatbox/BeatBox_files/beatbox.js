/******************************************************************************
 *
 *  Copyright 2012-2014 Tavendo GmbH.
 *
 *  Licensed under the Apache 2.0 license
 *  http://www.apache.org/licenses/LICENSE-2.0.html
 *
 ******************************************************************************/

"use strict";

var newWindowLink = null;

var currentSubscriptions = [];


// options checkboxes
var enable_audio = null;
var pub_trigger = null;
var direct_trigger = null;

var samples = [];
var buttons = [];

var isReconnect = false;

/**
 * For unclear reasons, Chrome will persistently fail to rewind the samples when
 * the media files are served from Flask/SocketServer.
 * When Flask is served from Twisted, this will fail on first load, but then
 * on refresh (F5) it succeeds. Needs further investigation.
 * With the sound files served from Apache, this does not happen ..
 */
var samplesBaseUri = 'snd/';


function loadSample(btn, file) {
   samples[btn] = document.createElement('audio');
   samples[btn].setAttribute('src', samplesBaseUri + file);
   samples[btn].load();
   samples[btn].volume = 1;
   samples[btn].loop = true;
   samples[btn].initialTime = 0;
}


function setupDemo() {

   console.log("setupDemo", sess.id, sess.isOpen);

   sess.prefix("api", demoPrefix + ".beatbox");

   if (isReconnect) {
      return;
   }
   isReconnect = true;

   newWindowLink = document.getElementById('secondInstance');

   // IE doesn't do WAV, FF doesn't do mp3s
   if(navigator.userAgent.indexOf("Trident") !== -1)  {
      console.log("IE detected - loading mp3s");
      loadSample(0, 'demo_beatbox_sample_a.mp3');
      loadSample(1, 'demo_beatbox_sample_b.mp3');
      loadSample(2, 'demo_beatbox_sample_c.mp3');
      loadSample(3, 'demo_beatbox_sample_d.mp3');
   } else {
      console.log("using WAV versions of samples");
      loadSample(0, 'demo_beatbox_sample_a.wav');
      loadSample(1, 'demo_beatbox_sample_b.wav');
      loadSample(2, 'demo_beatbox_sample_c.wav');
      loadSample(3, 'demo_beatbox_sample_d.wav');
   }

   // check if audio enabled via URL switch
   if ("audio" in setupInfoDictionary && setupInfoDictionary.audio === "off") {
      document.getElementById('enable_audio').checked = false;
   }

   enable_audio = document.getElementById('enable_audio');
   pub_trigger = document.getElementById('pub_trigger');
   direct_trigger = document.getElementById('direct_trigger');

   buttons[0] = { btn: document.getElementById('button-a'), pressed: false };
   setPadButtonHandlers(buttons[0].btn, 0);

   buttons[1] = { btn: document.getElementById('button-b'), pressed: false };
   setPadButtonHandlers(buttons[1].btn, 1);

   buttons[2] = { btn: document.getElementById('button-c'), pressed: false };
   setPadButtonHandlers(buttons[2].btn, 2);

   buttons[3] = { btn: document.getElementById('button-d'), pressed: false };
   setPadButtonHandlers(buttons[3].btn, 3);

   // for suppressing key-autorepeat events
   var keysPressed = {};

   window.onkeydown = function(e) {

      if (keysPressed[e.keyCode]) {
         return;
      } else {
         keysPressed[e.keyCode] = true;
      }

      switch (e.keyCode) {
         case 65:
            padButton(0, true);
            break;
         case 66:
            padButton(1, true);
            break;
         case 67:
            padButton(2, true);
            break;
         case 68:
            padButton(3, true);
            break;
      }
   };

   window.onkeyup = function(e) {

      keysPressed[e.keyCode] = false;

      switch (e.keyCode) {
         case 65:
            padButton(0, false);
            break;
         case 66:
            padButton(1, false);
            break;
         case 67:
            padButton(2, false);
            break;
         case 68:
            padButton(3, false);
            break;
      }
   };

   $("#helpButton").click(function() { $(".info_bar").toggle(); });
}

function onPadButtonDown(args, kwargs, details) {

   console.log("onPadButtonDown", args, kwargs, details);

   if (!buttons[kwargs.b].pressed) {

      if (enable_audio.checked) {
         console.log("playing a sample");
         // do NOT change order/content of the following 2 lines!
         samples[kwargs.b].currentTime = samples[kwargs.b].initialTime;
         samples[kwargs.b].play();
      }

      buttons[kwargs.b].pressed = true;
      buttons[kwargs.b].btn.style.background = "#d0b800";
   }
}


function onPadButtonUp(args, kwargs, details) {

   console.log("onPadButtonUp", args, kwargs, details);

   if (buttons[kwargs.b].pressed) {

      if (enable_audio.checked) {
         // do NOT change order/content of the following 2 lines!
         samples[kwargs.b].currentTime = samples[kwargs.b].initialTime;
         samples[kwargs.b].pause();
      }

      buttons[kwargs.b].pressed = false;
      buttons[kwargs.b].btn.style.background = "#666";
   }
}


function padButton(btn, down) {

   if (down) {
      if (direct_trigger.checked) {
         onPadButtonDown(null, { "b": btn, "t": 0 });
      }
      if (pub_trigger.checked) {
         sess.publish("api:" + controllerChannelId + ".pad_down", [], { "b": btn, "t": 0 }, { exclude_me: false, acknowledge: true }).then(
            function(publication) {
               console.log("published", publication);
            },
            function(error) {
               console.log("publication error");
            }
         );
      }
   } else {
      if (direct_trigger.checked) {
         onPadButtonUp(null, { "b": btn, "t": 0});
      }
      if (pub_trigger.checked) {
         sess.publish("api:" + controllerChannelId + ".pad_up", [], { "b": btn, "t": 0}, { exclude_me: false, acknowledge: true }).then(
            function(publication) {
               console.log("published", publication);
            },
            function(error) {
               console.log("publication error");
            }
         );
      }
   }
}


function setPadButtonHandlers(button, btn) {

   button.ontouchstart = function(evt) {
      padButton(btn, true);
      evt.preventDefault();
   };
   button.ontouchend = function(evt) {
      padButton(btn, false);
      evt.preventDefault();
   };

   button.onmousedown = function() {
      padButton(btn, true);
   };
   button.onmouseup = function() {
      padButton(btn, false);
   };
   // prevent buttons from getting stuck on mouseout, since mouseup no longer on button
   button.onmouseout = function() {
      padButton(btn, false);
   };



}


function onChannelSwitch(oldChannelId, newChannelId) {

   if (oldChannelId) {
      // check whether session for subscriptions still open
      // since this might be called on a reconnect
      if(currentSubscriptions[0].session.isOpen === true) {
         currentSubscriptions[0].unsubscribe();
         currentSubscriptions[1].unsubscribe();
      }
   }

   sess.subscribe("api:" + newChannelId + ".pad_down", onPadButtonDown).then(
      function(subscription) {
         console.log("subscribed pad_down", subscription);
         currentSubscriptions[0] = subscription;
      },
      function(error) {
         console.log("subscription error pad_down", error);
      }
   );
   sess.subscribe("api:" + newChannelId + ".pad_up", onPadButtonUp).then(
      function(subscription) {
         console.log("subscribed pad_up", subscription);
         currentSubscriptions[1] = subscription;
      },
      function(error) {
         console.log("subscription error pad_up", error);
      }
   );

   newWindowLink.setAttribute('href', window.location.pathname + '?channel=' + newChannelId + '&audio=off');
}
