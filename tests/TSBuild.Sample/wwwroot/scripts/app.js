var TSMin;!function(t){function e(){}e.toMoney=function(t){return"$".concat(t)},t.Convert=e}(TSMin=TSMin||{}),function(t){n.prototype.then=function(t){return this.thenCallback=t,this},n.prototype.catch=function(t){return this.catchCallback=t,this},n.prototype.finally=function(t){this.finalCallback=t},n.prototype.pass=function(t){this.thenCallback&&this.thenCallback(t||!0),this.finalCallback&&this.finalCallback()},n.prototype.reject=function(t){this.catchCallback&&this.catchCallback(t),this.finalCallback&&this.finalCallback()};var e=n;function n(t){t(this.pass.bind(this),this.reject.bind(this))}function s(){}t.ServerPromise=e,s.sendHttpRequest=function(o,i,c){return new e(function(e,n){var s="application/json; charset=utf-8",t=((c=c||{}).hasOwnProperty("contentType")||(c.contentType="object"==typeof c.body?s:null),new XMLHttpRequest);if(t.open(o,i,!0),c.contentType&&t.setRequestHeader("Content-Type",c.contentType),c.headers)for(var a=0;a<c.headers.length;a++)t.setRequestHeader(c.headers[a].name,c.headers[a].value);t.onreadystatechange=function(){var t;4===this.readyState&&(t=this.getResponseHeader("Content-Type")===s?JSON.parse(this.responseText):this.responseText,200<=this.status&&this.status<=299?e(t):n({status:this.status,url:this.responseURL,data:t,message:"".concat(o," ").concat(this.responseURL,": ").concat(this.status," ").concat("object"!=typeof t?this.responseText:"").trim()}))},t.onerror=function(){n({message:"Network Error"})},c.contentType===s?t.send(JSON.stringify(c.body)):t.send(c.body)})},t.Server=s}(TSMin=TSMin||{});
//# sourceMappingURL=app.js.map