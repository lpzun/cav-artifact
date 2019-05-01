enum Global { ZERO=0, X=3 } // for some reason, at least one enum element must be numbered 0

event WAIT;
event DONE;
event PING;

machine Main {
  var client: machine;

  start state Init {
    entry {
      client = new Client();
      goto SendPrefix; }}

  state SendPrefix {
    entry {
      var i:int;
      i = 0;
      while (i < X) {
        send client, WAIT;
        i = i + 1; }
      send client, DONE;
      goto Flood; }}

  state Flood {
    entry {
      send client, PING;
      goto Flood; }}}

///////////////////////////////////////////////////////////////////////////

machine Client {
  var b: bool;
  start state Init {
    entry { b = false; }
    defer WAIT;
    on DONE goto Consume; }

  state Consume {
    entry { b = $; }
    ignore WAIT, PING; }}
