# Documentation of OS Design

## 1. Some thoughts from Peizun

1. Currently, the logic of observation sequences (OS) 
related funtionalities are not quite correct. 

First, current version is interactive and step-wise. 
In particular, we need to increment the queue bound 
interactively. 


## 2. Invariant Checking

### Syntax

inv   ::= minv+

minv  ::= machine ID: qinv+ vinv+ rinv+

qinv  ::= Q |= qexpr

qexpr ::= true 
       | false
	   | qterm
	   | uop qexpr
	   | qexpr bop qexpr

qterm  ::= head = e
       | tail = e
	   | next(e) = e1
	   | e orderop e
	   | cntterm

cntterm ::= count(e)
	     | const
		 | cntterm relop cntterm

uop   ::= ~ // negation

bop   ::= & // and
	   | #  // or

orderop ::= ->
         | <-

relop   ::= <
         | =
		 | >

e ::= name

const is int

rinv ::= R |= rexpr+ // TODO

vinv ::= V |= vexpr+ // TODO

### Semantics

Q |= qexpr: the list of events in the queue of current machine 
            satisfiies qexpr

head = e: the head of current queue points to event WAIT

~(tail = e): the event at the tail of current queue is NOT WAIT

next(e) = e1: determine whether the next event of e, if any, is e1

count(e) = const: the number of event e in current queue

e1 -> e2: e1 is before e2

e1 <- e2: e2 -> e1



Examples:
1. Q |= head = WAIT # head = PING: the event at the head of queue 
either WAIT OR PING

2. Q |= count(e1) < count(e2): the number of e1 < the number of e2





