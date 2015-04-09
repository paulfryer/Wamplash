/******************************************************************************
 *
 *  Copyright 2012-2014 Tavendo GmbH.
 *
 *                                Apache License
 *                          Version 2.0, January 2004
 *                       http://www.apache.org/licenses/
 *
 ******************************************************************************/

/* global document: false, console: false, ab: true, $: true, JustGage: false, getRandomInt: false */

"use strict";

var newWindowLink = null,
    currentSubscriptions = [],
    gauges = [],
    sliders = null,
    isReconnect = false;

function setupDemo() {

   sess.prefix("api", demoPrefix + ".gauges");

   if (isReconnect) {
      return;
   }
   isReconnect = true;

   newWindowLink = document.getElementById('secondInstance');

   var gaugeValues = [30, 20, 40, 60];
   // create and configure gauges
   //

   gauges.push(new JustGage({
      id: "g" + gauges.length,
      value: gaugeValues[gauges.length],
      min: 0,
      max: 100,
      title: "Big Fella",
      label: "pounds"
   }));

   gauges.push(new JustGage({
      id: "g" + gauges.length,
      value: gaugeValues[gauges.length],
      min: 0,
      max: 100,
      title: "Small Buddy",
      label: "oz"
   }));

   gauges.push(new JustGage({
      id: "g" + gauges.length,
      value: gaugeValues[gauges.length],
      min: 0,
      max: 100,
      title: "Tiny Lad",
      label: "oz"
   }));

   gauges.push(new JustGage({
      id: "g" + gauges.length,
      value: gaugeValues[gauges.length],
      min: 0,
      max: 100,
      title: "Little Pal",
      label: "oz"
   }));



   // auto-animate gauges
   //
   if (false) {
      setInterval(function () {
         for (var j = 0; j < gauges.length; ++j) {
            gauges[j].refresh(getRandomInt(0, 100));
         }
      }, 2500);
   }


   // instantiate sliders
   $("#s0").slider({
      value: gaugeValues[0],
      orientation: "horizontal",
      range: "min",
      animate: true
   });

   var i = 1;

   $("#eqSliders > span").each(function() {
      // read initial values from markup and remove that
      // var value = parseInt($(this).text(), 10);
      var k = i;

      $(this).empty().slider({
         value: gaugeValues[i],
         range: "min",
         animate: true,
         orientation: "vertical"
      });
      i += 1;
   });

   // store sliders in array
   // sliders = [$("#s0")[0], $("#s1")[0], $("#s2")[0], $("#s3")[0]];
}

function onChannelSwitch(oldChannelId, newChannelId) {
   console.log("onChannelSwitch");

   // unsubscribe
   if (oldChannelId) {
      currentSubscriptions.forEach( function (el, index, array) {
         el.unsubscribe().then(
            function() {
               console.log("unsubscribed");
            },
            function(error) {
               console.log("unsubscribe failed", error);
            }
         );
      });
   }

   // wire up gauges + sliders for PubSub events
   //

   for (var k = 0; k < gauges.length; ++k) {
      (function (p) {
         sess.subscribe("api:" + newChannelId + ".g" + p, function (args, kwargs, details) {
            console.log("refresh", p, args[0]);
            gauges[p].refresh(args[0]);
            $("#s" + p).slider({ value: args[0]});
         }).then(
            function(subscription) {
               console.log("subscribed ", "api:" + newChannelId, subscription);
               currentSubscriptions.push(subscription);
            },
            function(error) {
               console.log("unsubscribe failed ", error);
            }
         );
      })(k);
   }

   // update publish for the sliders
   $("#s0").slider({
      slide: function(event, ui) {
         sess.publish("api:" + newChannelId + ".g0", [ui.value], {}, {acknowledge: true, exclude_me: false}).then(
            function(publication) {
               console.log("gauges published ", publication);
            },
            function(error) {
               console.log("gauges publish failed ", error);
            }
         );
      }
   });


   var i = 1;
   $("#eqSliders > span").each(function() {
      // read initial values from markup and remove that
      // var value = parseInt($(this).text(), 10);
      var k = i;

      $(this).slider({

         slide: function(event, ui) {
            sess.publish("api:" + newChannelId + ".g" + k, [ui.value], {}, {acknowledge: true, exclude_me: false}).then(
               function(publication) {
                  console.log("gauges published ", publication);
               },
               function(error) {
                  console.log("gauges publish failed ", error);
               }
            );
         }
      });
      i += 1;
   });


   newWindowLink.setAttribute('href', window.location.pathname + '?channel=' + newChannelId);
}
