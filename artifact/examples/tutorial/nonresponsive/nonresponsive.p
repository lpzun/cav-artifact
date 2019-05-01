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
      send client, WAIT;
      if ($)
        goto SendPrefix;
      else
        goto Flood; }}

  state Flood {
    entry {
      send client, PING;
      goto Flood; }}}

///////////////////////////////////////

machine Client {
  start state Init {
    defer PING;  // removed: WAIT
    on DONE goto Consume; }

  state Consume { ignore WAIT, PING; }}
