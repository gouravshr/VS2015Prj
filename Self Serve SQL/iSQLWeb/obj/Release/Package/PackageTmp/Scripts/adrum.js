;/* Version 33b1625c0c5786bed6f93d0a90be1e91 #2013-06-18_15-21-11 r614258064afca1f0cce7fdf7eb1f26f51e8604ef 1 */(function(){var e=!0,h=null,j=!1;
if(!(window.ADRUM||window["adrum-disable"]===e)){window["adrum-start-time"]=window["adrum-start-time"]||(new Date).getTime();var k="https:"===document.location.protocol;window.ADRUM={beaconUrlHttp:"http://col.eum-appdynamics.com",beaconUrlHttps:"https://col.eum-appdynamics.com",appKey:window["adrum-app-key"]||"AD-AAB-AAA-ASP",adrumExtUrl:(k?"https://de8of677fyt0b.cloudfront.net":"http://de8of677fyt0b.cloudfront.net")+"/adrum-ext.33b1625c0c5786bed6f93d0a90be1e91.js",geoResolverUrl:k?"":""};var s=window.ADRUM;
s.ra=function(n){return 0<=n.location.search.indexOf("ADRUM_debug=true")||0<=n.cookie.search(/(^|;)\s*ADRUM_debug=true/)};s.iDR=s.ra;s.h=s.ra(document);s.S=100;s.logMessages=[];for(var t=["debug","warn","info","error"],u=0;u<t.length;u++)(function(n){s[n]=function(r,d){s.h&&s.logMessages.push([n,Array.prototype.slice.call(arguments).join(" | ")])}})(t[u]);var x=0,y=s.error;s.error=function(n){y(n);2<=x||(n=s.beaconUrlHttps+"/eumcollector/error.gif?version=1&appKey="+s.appKey+"&msg="+encodeURIComponent(n.substring(0,
500)),(new Image).src=n,x++)};s.assert=function(n,r){n||s.error("Assert fail: "+r)};s.h&&(s.dumpLog=function(){for(var n="",r=0;r<s.logMessages.length;r++)var d=s.logMessages[r],n=n+("["+d[0]+"]\t"+d[1].replace(/<br\/>/g,"\n\t")+"\n");return n});s.info("M0");s.addEventListener=function(n,r,d){function f(){try{return d.apply(this,Array.prototype.slice.call(arguments))}catch(f){s.error("M2",r,n,f)}}s.h&&s.debug("M1",r,n);n.attachEvent?n.attachEvent("on"+r,f):n.addEventListener&&n.addEventListener(r,
f,j)};s.compareWindows=function(n,r){return n==r};s.la=function(){function n(n){s.debug("M4",n);var r=new Date;r.setTime(r.getTime()-1E3);document.cookie=n+"=;Expires="+r.toGMTString()}s.info("M3");for(var r=[],d=/^\s*ADRUM_/,f=/^\s*ADRUM=s=([\d]+)&r=(.*)\s*/,g=document.cookie.split(";"),l=0;l<g.length;l++){var m=g[l];if(0<=m.search(d)){var p=m.split("=");if(2==p.length){var q=p[0].split("_"),v;if(v=4==q.length)v=document.referrer,v=q[2]==(v?v.length:0);v&&(n(p[0]),r.push(p[1]),s.debug("M5",m))}}if(p=
f.exec(m))s.debug("M6",m),3===p.length?(s.startTimeCookie={startTime:Number(p[1]),startPage:p[2]},n("ADRUM")):s.error("M7")}s.cookieMetadataChunks=r};s.la();s.q=[];s.isDead=j;s.command=function(n,r){s.h&&s.debug("M8",n,Array.prototype.slice.call(arguments).slice(1).join(", "));s.isDead?s.debug("M9"):s.q.length>=s.S?(s.info("M10"),s.q=[],s.isDead=e):(s.q.push(Array.prototype.slice.call(arguments)),s.processQ&&s.processQ())};s.command("mark","firstbyte",window["adrum-start-time"]);s.n=function(n,r,
d,f){r=r||function(){};n=n||function(){};d=d||function(){};return function(){s.h&&s.debug("M11",f,Array.prototype.slice.call(arguments).join(", "));var g=Array.prototype.slice.call(arguments),l;try{l=r.apply(this,g)}catch(m){s.error("Failure in interceptor ["+f+"] entry: "+m)}s.assert(!l||"[object Array]"===Object.prototype.toString.call(l));var p=void 0;try{p=n.apply(this,l||g)}finally{try{d.apply(this,g)}catch(q){s.error("Failure in interceptor ["+f+"] exit: "+q)}}return p}};var z=j;s.listenForErrors=
function(){window.onerror=s.n(window.onerror,function(n,r,d){z||s.command("reportError",n,r,d);z=e},function(){z=j},"onerror");s.windowErrorHandler=window.onerror};s.info("M12");s.listenForErrors();s.addEventListener(window,"load",function(){setTimeout(function(){s.command("mark","onload",(new Date).getTime());s.aa=h;var n=window.performance||window.a||window.b||window.c;if(n&&n.timing)if(n=n.timing,n.navigationStart&&n.navigationStart<=n.loadEventEnd){var r={},d;for(d in n){var f=n[d];"number"===
typeof f&&(r[d]=f)}s.aa=r}else s.debug("M31");s.command("reportOnload");d=document.createElement("script");d.async=e;d.src=s.adrumExtUrl;(n=document.getElementsByTagName("script")[0])?(n.parentNode.insertBefore(d,n),s.info("M32")):s.info("M33")},0);s.info("M13")});var A=j,B=function(){A||(s.command("mark","onready",(new Date).getTime()),A=e)},C=function(){document.addEventListener?(document.removeEventListener("DOMContentLoaded",C,j),B()):"complete"===document.readyState&&(document.detachEvent("onreadystatechange",
C),B())};if(document.addEventListener)document.addEventListener("DOMContentLoaded",C,j),window.addEventListener("load",B,j);else{document.attachEvent("onreadystatechange",C);window.attachEvent("onload",B);var D=j;try{D=window.frameElement==h&&document.documentElement}catch(F){}D&&D.doScroll&&function G(){try{D.doScroll("left")}catch(r){setTimeout(G,10);return}B()}()}s.info("M14");if(window["adrum-xhr-disable"]===e)s.info("M15");else if(window.XMLHttpRequest){var H=window.XMLHttpRequest.prototype;
if(H)if(!H.open||!H.send)s.info("M18");else{s.info("M19");var I=function(n){var r=n.k;if(r){var d=(new Date).getTime();2==n.readyState?r.firstByteTime=r.firstByteTime||d:4==n.readyState&&(r.respAvailTime=r.respAvailTime||d,r.firstByteTime=r.firstByteTime||d)}},J=function(n,r){var d=n.getAllResponseHeaders(),f=n.status;s.command("reportXhr",r,d,f,400<=f?n.responseText:h)},K=function(n,r,d){return s.n(n,function(){I(this)},function(){var n=r.k;n&&4==r.readyState&&(n.respProcTime=(new Date).getTime(),
J(r,n),delete r.k)},d)};H.open=s.n(H.open,function(){this.k={url:2<=arguments.length?String(arguments[1]):"",sendTime:h,firstByteTime:h,respAvailTime:h,respProcTime:h}},h,"XHR.open");H.send=s.n(H.send,function(){var n=this,r=n.k;if(r){r.sendTime=(new Date).getTime();var d=r.url,f=document.createElement("a");f.href=d;d=document.location;":"===f.protocol&&""===f.hostname&&""===f.port||f.protocol===d.protocol&&f.hostname===d.hostname&&f.port===d.port?n.setRequestHeader("ADRUM","isAjax:true"):s.debug("M22",
document.location.href,r.url);var g=0,l=function(){g++;var r=n.onreadystatechange;if(r)n.onreadystatechange=K(r,n,"XHR.onReadyStateChange"),s.debug("M23",g);else if(5>g)setTimeout(l,0);else if(s.debug("M24"),n.k){var d=(new Date).getTime()+3E4,f=function(){I(n);var r=n.k;r&&(4==n.readyState?(r.respProcTime=(new Date).getTime(),s.debug("M20"),J(n,r),delete n.k):(new Date).getTime()<d?setTimeout(f,50):(delete n.k,s.debug("M21")))};f()}};l()}},h,"XHR.send");H.addEventListener&&H.removeEventListener?
(H.addEventListener=s.n(H.addEventListener,function(n,r){if(this.k){var d=Array.prototype.slice.call(arguments);r&&("load"===n||"error"===n)?(r.H||(r.H=K(r,this,"XHR.invokeEventListener")),d[1]=r.H,s.debug("M25")):s.debug("M26",n,r);return d}},h,"XHR.addEventListener"),H.removeEventListener=s.n(H.removeEventListener,function(n,r){if(this.k){var d=Array.prototype.slice.call(arguments);r.H?(d[1]=r.H,s.debug("M27")):s.debug("M28");return d}},h,"XHR.removeEventListener")):s.debug("M29");s.info("M30")}else s.info("M17")}else s.info("M16")};})();//@ sourceMappingURL=https://eumsm.eum-appdynamics.com/jsagent-sm/33b1625c0c5786bed6f93d0a90be1e91/adrum.js.map

