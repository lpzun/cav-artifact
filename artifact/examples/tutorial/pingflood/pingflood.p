//enum Global { ZERO=0, X=3 } // for some reason, at least one enum element must be numbered 0

event PRIME;
event DONE;
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
        send client, PRIME;
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
    defer PRIME;
    on DONE goto Consume; 
  }

  state Consume { 
  ignore PRIME, PING; 
  }
}
