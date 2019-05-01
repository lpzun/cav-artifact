//enum Global { ZERO=0, X=3 } // for some reason, at least one enum element must be numbered 0

event WAIT;
event DONE assert 1;
event PING;

machine Main {
  var client: machine;

  start state Init {
    entry {
      client = new Client();
      goto SendPrefix; 
    }
  }

  state SendPrefix {
    entry {
      var i:int;
      i = 0;
      while (i < 3) {
        send client, WAIT;
        i = i + 1; 
      }
      send client, DONE;
      goto Flood;
    }
  }

  state Flood {
    entry {
      send client, PING;
      goto Flood; 
    }
  }
}

///////////////////////////////////////////////////////////////////////////

machine Client {
  start state Init {
    defer WAIT, PING;
    on DONE goto Consume; 
  }

  state Consume { 
  ignore WAIT, PING; 
  }
}
