//Event declaration
///////////////////////////////
/// events sent out from CPU
///////////////////////////////
// event grant assert 1;
event ask_share assume 1; // at most 3 instances of ask_share may exist in *some* (?) input queue. This is only used in systematic testing using Zing. 
event ask_excl assume 1;
///////////////////////////////
/// events sent out from Client
///////////////////////////////
// event unit assert 1;
event req_share assert 3 : machine; // at most 3 instances of req_share may exist in *any* input queue. Payload type is machine
event req_excl assert 3 : machine;
// event need_invalidate assert 1;
event invalidate_ack assert 3;
///////////////////////////////
/// events sent out from host
///////////////////////////////
event invalidate assert 1;
event grant_excl assert 1;
event grant_share assert 1;
// event normal assert 1;
// event wait assert 1;
// event invalidate_sharers assert 1 : int;
// event sharer_id assert 3 : machine;

// host machine 
machine Main {
	var curr_client : machine;
	var clients : (machine); // a tuple with 1 element of type machine, to simulate 1 client
	var curr_cpu : machine;
	var sharer_list : seq[machine]; // a list of machines shared the same cache content, need to be monitored
	var is_curr_req_excl : bool;
	var is_excl_granted : bool;
	var i, s :int;
        var temp: machine;

	start state init {
		entry {
		      temp = new Client(this, false); 
		      clients.0 = temp;
		      curr_client = null;
		      curr_cpu = new CPU(clients);
		      assert(sizeof(sharer_list) == 0);
		      goto receiveState;
		}
	}
	
	state receiveState {
		defer invalidate_ack;
		entry {} // do nothing except handling events
		
		on req_share goto ShareRequest;
		on req_excl  goto ExclRequest;		
	}
	
	state ShareRequest {
		entry (payload: machine) {
			curr_client = payload;
			is_curr_req_excl = false;
			goto ProcessReq;
		}
	}
	
	state ExclRequest {
		entry (payload: machine) {
		        curr_client = payload;
			is_curr_req_excl = true;
			goto ProcessReq;
		}
	}
	
	state ProcessReq {
		entry {
			if (is_curr_req_excl || is_excl_granted)
			{
				// need to invalidate before giving access
				goto inv;
			}
			else
			{
				goto grantAccess;
                        }
		}
	}
	
	state inv {
		defer req_share, req_excl;
		entry {
			i = 0;
			s = sizeof(sharer_list);
			if (s == 0)
				goto grantAccess;
			while (i < s)
			{
				send sharer_list[i], invalidate; // invalidate machine i
				i = i + 1;
			}
		}
		on invalidate_ack do rec_ack;
	}
	
	fun rec_ack() {
		sharer_list -= 0; // ?? pop the first machine
		s = sizeof(sharer_list);
		if (s == 0)
			goto grantAccess;
	}
	
	state grantAccess {
		entry {
			if (is_curr_req_excl)
			{
				is_excl_granted = true;
				send curr_client, grant_excl;
			}
			else
			{
				send curr_client, grant_share;
			}
			sharer_list += (0, curr_client);
			goto receiveState;
		}
	}
}

// client Machine
machine Client {
	var host : machine;
	var pending : bool;
	start state init {
		entry (payload: (machine,bool)) {
		        host = payload.0; 
		        pending = payload.1;
			goto invalid;
		}
	}

	state invalid {
		entry { 
		}
		on ask_share goto asked_share;
		on ask_excl goto asked_excl;
		on invalidate goto invalidating;
		on grant_excl goto exclusive;
		on grant_share goto sharing;
	}
	
	state asked_share {
		entry{
			send host, req_share, this;
			pending = true;
			goto invalid_wait;
		}
	}
	
	state asked_excl {
		entry {
			send host, req_excl, this;
			pending = true;
			goto invalid_wait;
		}
	}
	
	state invalid_wait {
		defer ask_share, ask_excl;
		on invalidate goto invalidating;
		on grant_excl goto exclusive;
		on grant_share goto sharing;
	}
	
	state asked_ex2 {
		entry {
			send host, req_excl, this;
			pending = true;
			goto sharing_wait;
		}
	}
	
	state sharing {
		entry {
			pending = false;
		}
		on invalidate goto invalidating;
		on grant_share goto sharing;
		on grant_excl goto exclusive;
		on ask_share goto sharing;
		on ask_excl goto asked_ex2;
	}
	
	state sharing_wait {
		defer ask_share, ask_excl;
		entry {}
		on invalidate goto invalidating;
		on grant_share goto sharing_wait;
		on grant_excl goto exclusive;
		
	}
	
	state exclusive {
		ignore ask_share, ask_excl;
		entry {
			pending = false;
		}
		on invalidate goto invalidating;
		on grant_share goto sharing;
		on grant_excl goto exclusive;
	}
	
	state invalidating {
		entry {
			send host, invalidate_ack;
			if (pending)
			{
				goto invalid_wait;
			}
			else
				goto invalid;
		}
	}
}


// environment machine in the form of a CPU which makes request to the clients

machine CPU {
	var cache : (machine);

	start state init {
		entry (payload: (machine)) {
			cache = payload;
			goto makeReq;
		}
	}
	
	state makeReq {
		entry {
			if ($)
			    send cache.0, ask_share;
			else
			    send cache.0, ask_excl;
			goto makeReq;
		}
	}
}

