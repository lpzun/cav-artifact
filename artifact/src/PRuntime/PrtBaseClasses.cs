using System;
using System.Collections.Generic;
using System.Linq;
using System.Diagnostics;

namespace P.Runtime
{

    public abstract class PrtMachine
    {
        #region Fields
        public List<PrtEventValue> sends;
        public string renamedName;
        public bool isSafe;
        public int instanceNumber;
        public List<PrtValue> fields;
        protected PrtValue eventValue;
        protected PrtStateStack stateStack;
        protected PrtFunStack invertedFunStack;
        public PrtContinuation continuation;
        public PrtMachineStatus currentStatus;
        public PrtNextStatemachineOperation nextSMOperation;
        protected PrtStateExitReason stateExitReason;
        public PrtValue currentTrigger;
        public PrtValue currentPayload;
        public Tuple<string, string> currentTriggerSenderInfo;
        public PrtState destOfGoto;

        protected StateImpl stateImpl; //just a reference to stateImpl
        #endregion

        #region Constructor
        public PrtMachine()
        {
            this.instanceNumber = 0;
            this.fields = new List<PrtValue>();
            this.eventValue = PrtValue.@null;
            this.stateStack = new PrtStateStack();
            this.invertedFunStack = new PrtFunStack();
            this.continuation = new PrtContinuation();
            this.currentTrigger = PrtValue.@null;
            this.currentPayload = PrtValue.@null;
            this.currentStatus = PrtMachineStatus.Enabled;
            this.nextSMOperation = PrtNextStatemachineOperation.ExecuteFunctionOperation;
            this.stateExitReason = PrtStateExitReason.NotExit;
            this.sends = new List<PrtEventValue>();
            this.stateImpl = null;
            this.renamedName = null;
            this.isSafe = false;
        }
        #endregion

        public override int GetHashCode()
        {
            return Hashing.Hash(
                renamedName.GetHashCode(),
                isSafe.GetHashCode(),
                //instanceNumber.GetHashCode(),
                fields.Select(v => v.GetHashCode()).Hash(),
                eventValue.GetHashCode(),
                stateStack.GetHashCode(),
                invertedFunStack.GetHashCode(),
                continuation.GetHashCode(),
                currentStatus.GetHashCode(),
                nextSMOperation.GetHashCode(),
                stateExitReason.GetHashCode(),
                currentTrigger.GetHashCode(),
                currentPayload.GetHashCode(),
                destOfGoto == null ? Hashing.Hash() : destOfGoto.GetHashCode()
                );
        }

        public virtual void Resolve(StateImpl state)
        {
            fields.ForEach(v => v.Resolve(state));
            eventValue.Resolve(state);
            invertedFunStack.Resolve(state);
            continuation.Resolve(state);
            currentTrigger.Resolve(state);
            currentPayload.Resolve(state);
        }

        public string ToPrettyString(string indent = "")
        {
            string result = "";
            result += indent + "renamedName:      " + renamedName + "\n";
            result += indent + "isSafe:           " + isSafe.ToString() + "\n";
            result += indent + "instanceNumber:   " + instanceNumber.ToString() + "\n";
            result += indent + "fields:           " + (fields.Count == 0 ? "null" : fields.Select(v => v.ToString()).Aggregate((s1, s2) => s1 + "," + s2)) + "\n";
            result += indent + "eventValue:       " + eventValue.ToString() + "\n";
            result += indent + "stateStack:       " + stateStack.ToString() + "\n";
            result += indent + "invertedFunStack: " + invertedFunStack.ToString() + "\n";
            result += indent + "continuation:     " + continuation.ToString() + "\n";
            result += indent + "currentStatus:    " + currentStatus.ToString() + "\n";
            result += indent + "nextSMOperation:  " + nextSMOperation.ToString() + "\n";
            result += indent + "stateExitReason:  " + stateExitReason.ToString() + "\n";
            result += indent + "currentTrigger:   " + currentTrigger.ToString() + "\n";
            result += indent + "currentPayload:   " + currentPayload.ToString() + "\n";
            result += indent + "destOfGoto:       " + (destOfGoto == null ? "null" : destOfGoto.ToString()) + "\n";

            return result;
        }


        public virtual void DbgCompare(PrtMachine machine)
        {
            Debug.Assert(renamedName == machine.renamedName);
            Debug.Assert(isSafe == machine.isSafe);
            Debug.Assert(instanceNumber == machine.instanceNumber);
            Debug.Assert(fields.Count == machine.fields.Count);
            for (int i = 0; i < fields.Count; i++)
            {
                Debug.Assert(fields[i].GetHashCode() == machine.fields[i].GetHashCode());
            }
            Debug.Assert(eventValue.GetHashCode() == machine.eventValue.GetHashCode());
            Debug.Assert(stateStack.GetHashCode() == machine.stateStack.GetHashCode());
            Debug.Assert(invertedFunStack.GetHashCode() == machine.invertedFunStack.GetHashCode());
            Debug.Assert(continuation.GetHashCode() == machine.continuation.GetHashCode());
            Debug.Assert(currentStatus == machine.currentStatus);
            Debug.Assert(nextSMOperation == machine.nextSMOperation);
            Debug.Assert(stateExitReason == machine.stateExitReason);
            Debug.Assert(currentTrigger.GetHashCode() == machine.currentTrigger.GetHashCode());
            Debug.Assert(currentPayload.GetHashCode() == machine.currentPayload.GetHashCode());
            Debug.Assert((destOfGoto == null && machine.destOfGoto == null) ||
                (destOfGoto != null && machine.destOfGoto != null && destOfGoto.GetHashCode() == machine.destOfGoto.GetHashCode()));
        }

        public abstract string Name
        {
            get;
        }

        public abstract PrtState StartState
        {
            get;
        }

        public abstract void PrtEnqueueEvent(PrtValue e, PrtValue arg, PrtMachine source, PrtMachineValue target = null);

        public PrtState CurrentState
        {
            get
            {
                return stateStack.TopOfStack.state;
            }
        }

        public HashSet<PrtValue> CurrentActionSet
        {
            get
            {
                return stateStack.TopOfStack.actionSet;
            }
        }

        public HashSet<PrtValue> CurrentDeferredSet
        {
            get
            {
                return stateStack.TopOfStack.deferredSet;
            }
        }

        #region Prt Helper functions
        public PrtFun PrtFindActionHandler(PrtValue ev)
        {
            var tempStateStack = stateStack.Clone();
            while (tempStateStack.TopOfStack != null)
            {
                if (tempStateStack.TopOfStack.state.dos.ContainsKey(ev))
                {
                    return tempStateStack.TopOfStack.state.dos[ev];
                }
                else
                    tempStateStack.PopStackFrame();
            }
            Debug.Assert(false);
            return null;
        }

        public void PrtPushState(PrtState s)
        {
            stateStack.PushStackFrame(s);
        }

        public bool PrtPopState(bool isPopStatement)
        {
            Debug.Assert(stateStack.TopOfStack != null);
            //pop stack
            stateStack.PopStackFrame();
            if (stateStack.TopOfStack == null)
            {
                if (isPopStatement)
                {
                    throw new PrtInvalidPopStatement();
                }
                //TODO : Handle the spec machine case separately for the halt event
                else if (!eventValue.Equals(PrtValue.halt))
                {
                    throw new PrtUnhandledEventException(string.Format("{0} failed to handle event {1}", Name, eventValue.ToString()));
                }
                else
                {
                    if (this as PrtImplMachine != null)
                    {
                        stateImpl.TraceLine("<HaltLog> Machine {0}-{1} HALTED with {2} events in the queue", this.Name, this.instanceNumber, (((PrtImplMachine)this).eventQueue).Size());
                    }
                    else
                    {
                        //SpecMachine case:
                        //TODO: is it possible to send "halt" event to a spec machine?
                        stateImpl.TraceLine("<HaltLog> Machine {0}-{1} HALTED", this.Name, this.instanceNumber);
                    }
                    currentStatus = PrtMachineStatus.Halted;
                }
            }

            return currentStatus == PrtMachineStatus.Halted;
        }

        public void PrtChangeState(PrtState s)
        {
            Debug.Assert(stateStack.TopOfStack != null);
            stateStack.PopStackFrame();
            stateStack.PushStackFrame(s);
        }

        public PrtFunStackFrame PrtPopFunStackFrame()
        {
            return invertedFunStack.PopFun();
        }

        public void PrtPushFunStackFrame(PrtFun fun, List<PrtValue> locals)
        {
            if (!fun.IsAnonFun)
            {
                stateImpl.TraceLine("<FunctionLog> Machine {0}-{1} executing Function {2}", this.Name, this.instanceNumber, fun);
            }
            invertedFunStack.PushFun(fun, new List<PrtValue>(locals.Select(v => v.Clone()))); // TODO: temporary
        }

        public void PrtPushFunStackFrame(PrtFun fun, List<PrtValue> locals, int retTo)
        {
            invertedFunStack.PushFun(fun, new List<PrtValue>(locals.Select(v => v.Clone())), retTo);
        }

        public void PrtPushExitFunction()
        {
            stateImpl.TraceLine("<StateLog> Machine {0}-{1} exiting State {2}", this.Name, this.instanceNumber, CurrentState.name);
            PrtFun exitFun = CurrentState.exitFun;
            if (exitFun.IsAnonFun)
            {
                PrtPushFunStackFrame(exitFun, exitFun.CreateLocals(currentPayload));
            }
            else
            {
                PrtPushFunStackFrame(exitFun, exitFun.CreateLocals());
            }
        }

        public bool PrtIsTransitionPresent(PrtValue ev)
        {
            return CurrentState.transitions.ContainsKey(ev);
        }

        public bool PrtIsActionInstalled(PrtValue ev)
        {
            return CurrentActionSet.Contains(ev);
        }

        public void PrtPushTransitionFun(PrtValue ev)
        {
            PrtFun transitionFun = CurrentState.transitions[ev].transitionFun;
            if (transitionFun.IsAnonFun)
            {
                PrtPushFunStackFrame(transitionFun, transitionFun.CreateLocals(currentPayload));
            }
            else
            {
                PrtPushFunStackFrame(transitionFun, transitionFun.CreateLocals());
            }
        }

        public void PrtFunContReturn(List<PrtValue> retLocals)
        {
            continuation.reason = PrtContinuationReason.Return;
            continuation.retVal = PrtValue.@null;
            continuation.retLocals = retLocals;
        }

        public void PrtFunContReturnVal(PrtValue val, List<PrtValue> retLocals)
        {
            continuation.reason = PrtContinuationReason.Return;
            continuation.retVal = val;
            continuation.retLocals = retLocals;
        }

        public void PrtFunContPop()
        {
            continuation.reason = PrtContinuationReason.Pop;
        }

        public void PrtFunContGoto()
        {
            continuation.reason = PrtContinuationReason.Goto;
        }

        public void PrtFunContRaise()
        {
            continuation.reason = PrtContinuationReason.Raise;
        }

        public void PrtFunContSend(PrtFun fun, List<PrtValue> locals, int ret)
        {
            PrtPushFunStackFrame(fun, locals, ret);
            continuation.reason = PrtContinuationReason.Send;
        }

        public void PrtFunContNewMachine(PrtFun fun, List<PrtValue> locals, int ret)
        {
            PrtPushFunStackFrame(fun, locals, ret);
            continuation.reason = PrtContinuationReason.NewMachine;
        }

        public void PrtFunContReceive(PrtFun fun, List<PrtValue> locals, int ret)
        {
            PrtPushFunStackFrame(fun, locals, ret);
            continuation.reason = PrtContinuationReason.Receive;
        }

        public void PrtFunContNondet(PrtFun fun, List<PrtValue> locals, int ret)
        {
            PrtPushFunStackFrame(fun, locals, ret);
            continuation.reason = PrtContinuationReason.Nondet;
        }
        #endregion
    }

    public class PrtIgnoreFun : PrtFun
    {
        public override bool IsAnonFun
        {
            get
            {
                return true;
            }
        }

        public override void Execute(StateImpl application, PrtMachine parent)
        {
            throw new NotImplementedException();
        }

        public override PrtFunStackFrame CreateFunStackFrame(List<PrtValue> locals, int retLoc)
        {
            throw new NotImplementedException();
        }

        public override List<PrtValue> CreateLocals(params PrtValue[] args)
        {
            throw new NotImplementedException();
        }
    }

    public abstract class PrtFun
    {
        public static PrtIgnoreFun IgnoreFun = new PrtIgnoreFun();

        public abstract bool IsAnonFun
        {
            get;
        }

        public List<Dictionary<PrtValue, PrtFun>> receiveCases;

        public PrtFun()
        {
            receiveCases = new List<Dictionary<PrtValue, PrtFun>>();
        }

        public abstract List<PrtValue> CreateLocals(params PrtValue[] args);

        public abstract PrtFunStackFrame CreateFunStackFrame(List<PrtValue> locals, int retLoc);

        public abstract void Execute(StateImpl application, PrtMachine parent);

        public PrtValue ExecuteToCompletion(StateImpl application, PrtMachine parent, params PrtValue[] args)
        {
            parent.PrtPushFunStackFrame(this, CreateLocals(args));
            Execute(application, parent);
            if (parent.continuation.reason != PrtContinuationReason.Return)
            {
                throw new PrtInternalException("Unexpected continuation reason");
            }
            return parent.continuation.retVal.Clone();
        }
    }

    public class PrtEvent
    {
        public static int DefaultMaxInstances = int.MaxValue;

        public string name;
        public PrtType payloadType;
        public int maxInstances;
        public bool doAssume;

        public PrtEvent(string name, PrtType payload, int mInstances, bool doAssume)
        {
            this.name = name;
            this.payloadType = payload;
            this.maxInstances = mInstances;
            this.doAssume = doAssume;
        }

        public override int GetHashCode()
        {
            return Hashing.Hash(name.GetHashCode(), payloadType.ToString().GetHashCode());
        }
    };

    public class PrtTransition
    {
        public PrtFun transitionFun; // isPush <==> fun == null
        public PrtState gotoState;
        public bool isPushTran;
        public PrtTransition(PrtFun fun, PrtState toState, bool isPush)
        {
            this.transitionFun = fun;
            this.gotoState = toState;
            this.isPushTran = isPush;

        }
    };

    public enum StateTemperature
    {
        Cold,
        Warm,
        Hot
    };

    public class PrtState
    {
        public string name;
        public PrtFun entryFun;
        public PrtFun exitFun;
        public Dictionary<PrtValue, PrtTransition> transitions;
        public Dictionary<PrtValue, PrtFun> dos;
        public HashSet<PrtEventValue> deferredSet;
        public bool hasNullTransition;
        public StateTemperature temperature;

        public PrtState(string name, PrtFun entryFun, PrtFun exitFun, bool hasNullTransition, StateTemperature temperature)
        {
            this.name = name;
            this.entryFun = entryFun;
            this.exitFun = exitFun;
            this.transitions = new Dictionary<PrtValue, PrtTransition>();
            this.dos = new Dictionary<PrtValue, PrtFun>();
            this.deferredSet = new HashSet<PrtEventValue>();
            this.hasNullTransition = hasNullTransition;
            this.temperature = temperature;
        }

        public override int GetHashCode()
        {
            return name.GetHashCode();
        }

        public override string ToString()
        {
            return name;
        }
    };

    public class PrtEventNode : IEquatable<PrtEventNode> // IEquatable<PrtEventNode> needed for List<PrtEventNode>
    {
        public PrtValue ev;
        public PrtValue arg;
        public string senderMachineName;
        public string senderMachineStateName;

        public PrtEventNode(PrtValue e, PrtValue payload, string senderMachineName, string senderMachineStateName)
        {
            ev = e;
            arg = payload.Clone();
            this.senderMachineName = senderMachineName;
            this.senderMachineStateName = senderMachineStateName;
        }

        public PrtEventNode Clone()
        {
            return new PrtEventNode(this.ev, this.arg, this.senderMachineName, this.senderMachineStateName);
        }

        public PrtEventNode Clone_Resolve(StateImpl s)
        {
            PrtEventNode x = Clone();
            x.Resolve(s);
            return x;
        }

        public override int GetHashCode()
        {
            return Hashing.Hash(ev.GetHashCode(), arg.GetHashCode());
        }

        public bool Equals(PrtEventNode other)
        {
            return GetHashCode() == other.GetHashCode();
        }

        public override string ToString()
        {
            return "(" + ev.ToString() + "," + arg.ToString() + ")"; // not including senderMachine data since the image (successor) relation doesn't depend on them
        }

        public void Resolve(StateImpl state)
        {
            ev.Resolve(state);
            arg.Resolve(state);
        }
    }

    public class PrtEventNodeComparer : IEqualityComparer<PrtEventNode>
    {
        public int GetHashCode(PrtEventNode ev)
        {
            return ev.GetHashCode();
        }
        public bool Equals(PrtEventNode ev1, PrtEventNode ev2)
        {
            return ev1.GetHashCode() == ev2.GetHashCode();
        }
    }

    /// <summary>
    /// A wrapper for eventqueue
    /// </summary>
    public class PrtEventBuffer
    {
        /// <summary>
        /// queue size bound. For DFS, '0' is interpreted as 'unbounded'
        /// </summary>
        public static int k = 0;
        /// <summary>
        /// prefix of queue that is maintained concretely (not abstracted)
        /// </summary>
        public static int p = 0; 
        /// <summary>
        ///  The index of last dequeued event
        /// </summary>
        public static int idxOfLastDequeuedEvent;

        /// <summary>
        /// Used as concrete and abstract queue
        /// </summary>
        public List<PrtEventNode> events;

        /// <summary>
        /// To mark whether the queue is concrete or not. 
        /// Define it as private, to prevent incorrectly setting it.
        /// </summary>
        private bool concrete;

        /// <summary>
        /// Constructor
        /// </summary>
        public PrtEventBuffer()
        {
            events = new List<PrtEventNode>();
            concrete = true;
        }

        public bool IsConcrete() { return concrete; }
        public bool IsAbstract() { return !IsConcrete(); }

        public int Size() { return events.Count; }  // concrete or abstract
        public bool Empty() { return Size() == 0; }   // concrete or abstract

        /// <summary>
        /// Convert the queue into a list that, for the suffix starting at p, 
        /// keeps the ordering restricted to first-time occurrence but ignores 
        /// multiplicity. This creates a very fine-grained abstraction.
        /// For instance, for p=0,
        /// [X,X,Y] -> [X,Y]            but also
        /// [X,Y,X] -> [X,Y]
        /// A more precise abstraction keeps the queue in a map<PrtEventNode,bool>, 
        /// which stores the elements and whether they occur once or more than once 
        /// (0,1,infinity abstraction). (This would still not distinguish the above 
        /// two examples.)
        /// </summary>
        public void AbstractMe()
        {
            Debug.Assert(IsConcrete());
            concrete = false; // a bit premature, but RemoveDuplicatesInSuffix operates on abstract queues only
            RemoveDuplicatesInSuffix();
        }

        public void RemoveDuplicatesInSuffix()
        {
            Debug.Assert(IsAbstract());
            /// the set of events in the suffix
            HashSet<PrtEventNode> suffixSet = new HashSet<PrtEventNode>(new PrtEventNodeComparer());
            int i = p;
            while (i < Size())
            {
                if (suffixSet.Add(events[i])) // if the event occurs for the first time
                    ++i;
                else
                    events.RemoveAt(i); // has appeared before
            }
        }

        public PrtEventBuffer Clone()
        {
            var clonedVal = new PrtEventBuffer();

            foreach (PrtEventNode ev in events)
                clonedVal.events.Add(ev.Clone());

            clonedVal.concrete = concrete;

            return clonedVal;
        }

        public PrtEventBuffer CloneAndResolve(StateImpl s)
        {
            PrtEventBuffer x = Clone();
            x.Resolve(s);
            return x;
        }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="indent"></param>
    /// <returns></returns>
        public string ToPrettyString(string indent = "")
        {
            string title = indent + "events:           ";
            if (Size() == 0)
                title += "null";
            else
                for (int i = 0; i < Size(); ++i)
                {
                    if (i > 0)
                        title += (IsAbstract() && i == p ? "|" : ",");
                    title += events[i].ToString();
                }
            return title + "\n";
        }

        public override int GetHashCode()
        {
            return Hashing.Hash(Hashing.Hash(events.Select(v => v.GetHashCode())), concrete.GetHashCode());
        }

        public int CalculateInstances(PrtValue e)
        {
            Debug.Assert(IsConcrete());
            return events.Select(en => en.ev).Where(ev => ev == e).Count();
        }

        public void EnqueueEvent(PrtValue e, PrtValue arg, string senderMachineName, string senderMachineStateName)
        {
            Debug.Assert(e is PrtEventValue, "Illegal enqueue of null event");

            var en = new PrtEventNode(e, arg, senderMachineName, senderMachineStateName);

            if (IsConcrete()) // concrete: honor k-bounded queue semantics, instance counters, etc
            {

                if (k > 0 && Size() == k)
                {
                    // Console.WriteLine("PrtEventBuffer.EnqueueEvent: queue bound {0} reached in attempt to enqueue; rejecting send event", k);
                    throw new PrtAssumeFailureException();
                }

                PrtEventValue ev = e as PrtEventValue;
                if (ev.evt.maxInstances != PrtEvent.DefaultMaxInstances) // instance counter is used
                    if (CalculateInstances(e) == ev.evt.maxInstances)
                    {
                        if (ev.evt.doAssume)
                            throw new PrtAssumeFailureException();
                        else
                            throw new PrtMaxEventInstancesExceededException(String.Format(@"< Exception > Attempting to enqueue event {0} more than {1} many times\n", ev.evt.name, ev.evt.maxInstances));
                    }
            }

            events.Add(en); // concrete or abstract

            if (IsAbstract() && Size() >= p + 2)   // if the suffix was not empty before enqueuing this element
                RemoveDuplicatesInSuffix();
        }

        public bool DequeueEvent(PrtImplMachine owner)
        {
            HashSet<PrtValue> deferredSet = owner.CurrentDeferredSet;
            HashSet<PrtValue> receiveSet = owner.receiveSet;

            int iter = 0;
            idxOfLastDequeuedEvent = events.Count + 1; // if, AFTER DequeueEvent, last_... = Count + 1, nothing was dequeued. Otherwise last_... will be <= Count and points to the OLD index dequeued
            while (iter < events.Count)
            {
                if ((receiveSet.Count == 0 && !deferredSet.Contains(events[iter].ev)) ||  // we check deferredSet and receiveSet only against ev (event type), not arg
                     (receiveSet.Count > 0 && receiveSet.Contains(events[iter].ev)))
                {
                    owner.currentTrigger = events[iter].ev;
                    owner.currentPayload = events[iter].arg;
                    owner.currentTriggerSenderInfo = Tuple.Create(events[iter].senderMachineName, events[iter].senderMachineStateName);
                    events.Remove(events[iter]);
                    idxOfLastDequeuedEvent = iter;
                    return true;
                }
                else
                    iter++;
            }

            return false;
        }

        public bool IsEnabled(PrtImplMachine owner)
        {
            HashSet<PrtEventValue> deferredSet;
            HashSet<PrtValue> receiveSet;

            deferredSet = owner.CurrentState.deferredSet;
            receiveSet = owner.receiveSet;
            foreach (var evNode in events)
            {
                if ((receiveSet.Count == 0 && !deferredSet.Contains(evNode.ev))
                     || (receiveSet.Count > 0 && receiveSet.Contains(evNode.ev)))
                {
                    return true;
                }

            }
            return false;
        }

        public void Resolve(StateImpl state)
        {
            events.ForEach(n => n.Resolve(state));
        }
    }

    public class PrtStateStackFrame
    {
        public PrtState state;
        //event value because you cannot defer null value
        public HashSet<PrtValue> deferredSet;
        //action set can have null value
        public HashSet<PrtValue> actionSet;

        public PrtStateStackFrame(PrtState st, HashSet<PrtValue> defSet, HashSet<PrtValue> actSet)
        {
            this.state = st;
            this.deferredSet = new HashSet<PrtValue>();
            foreach (var item in defSet)
                this.deferredSet.Add(item);

            this.actionSet = new HashSet<PrtValue>();
            foreach (var item in actSet)
                this.actionSet.Add(item);
        }

        public PrtStateStackFrame Clone()
        {
            return new PrtStateStackFrame(state, deferredSet, actionSet);
        }

        public override int GetHashCode()
        {
            return state.GetHashCode();
        }

        public override string ToString()
        {
            return state.ToString();
        }
    }

    public class PrtStateStack
    {
        public PrtStateStack()
        {
            stateStack = new Stack<PrtStateStackFrame>();
        }

        private Stack<PrtStateStackFrame> stateStack;

        public PrtStateStackFrame TopOfStack
        {
            get
            {
                if (stateStack.Count > 0)
                    return stateStack.Peek();
                else
                    return null;
            }
        }

        public PrtStateStack Clone()
        {
            var clone = new PrtStateStack();
            foreach (var s in stateStack.Reverse())
            {
                clone.stateStack.Push(s.Clone());
            }
            return clone;
        }

        public void PopStackFrame()
        {
            stateStack.Pop();
        }


        public void PushStackFrame(PrtState state)
        {
            var deferredSet = new HashSet<PrtValue>();
            if (TopOfStack != null)
            {
                deferredSet.UnionWith(TopOfStack.deferredSet);
            }
            deferredSet.UnionWith(state.deferredSet);
            deferredSet.ExceptWith(state.dos.Keys);
            deferredSet.ExceptWith(state.transitions.Keys);

            var actionSet = new HashSet<PrtValue>();
            if (TopOfStack != null)
            {
                actionSet.UnionWith(TopOfStack.actionSet);
            }
            actionSet.ExceptWith(state.deferredSet);
            actionSet.UnionWith(state.dos.Keys);
            actionSet.ExceptWith(state.transitions.Keys);

            //push the new state on stack
            stateStack.Push(new PrtStateStackFrame(state, deferredSet, actionSet));
        }

        public bool HasNullTransitionOrAction()
        {
            if (TopOfStack.state.hasNullTransition)
                return true;
            return TopOfStack.actionSet.Contains(PrtValue.@null);
        }

        public override int GetHashCode()
        {
            return stateStack.Select(v => v.GetHashCode()).Hash();
        }

        public override string ToString()
        {
            return (stateStack.Count == 0 ? "null" : stateStack.Select(v => v.ToString()).Aggregate((s1, s2) => s1 + "," + s2));
        }
    }

    public enum PrtContinuationReason : int
    {
        Return,
        Nondet,
        Pop,
        Raise,
        Receive,
        Send,
        NewMachine,
        Goto
    };

    public abstract class PrtFunStackFrame
    {
        public int returnToLocation;
        public List<PrtValue> locals;

        public PrtFun fun;
        public PrtFunStackFrame(PrtFun fun, List<PrtValue> locals)
        {
            this.fun = fun;
            this.locals = locals;
            returnToLocation = 0;
        }

        public PrtFunStackFrame(PrtFun fun, List<PrtValue> locals, int retLocation)
        {
            this.fun = fun;
            this.locals = locals;
            returnToLocation = retLocation;
        }

        public virtual PrtFunStackFrame Clone()
        {
            return fun.CreateFunStackFrame(new List<PrtValue>(locals.Select(v => v.Clone())), returnToLocation);
        }

        public override int GetHashCode()
        {
            return Hashing.Hash(fun.GetHashCode(), returnToLocation.GetHashCode(), locals.Select(v => v.GetHashCode()).Hash());
        }

        public override string ToString()
        {
            return returnToLocation.ToString() + ",(" + (locals.Count == 0 ? "null" : locals.Select(v => v.ToString()).Aggregate((s1, s2) => s1 + "," + s2)) +")," + fun.ToString();
        }

        public void Resolve(StateImpl state)
        {
            locals.ForEach(l => l.Resolve(state));
        }
    }

    public class PrtFunStack
    {
        private Stack<PrtFunStackFrame> funStack;
        public PrtFunStack()
        {
            funStack = new Stack<PrtFunStackFrame>();
        }

        public PrtFunStack Clone()
        {
            var clonedStack = new PrtFunStack();
            foreach (var frame in funStack.Reverse())
            {
                clonedStack.funStack.Push(frame.Clone());
            }

            return clonedStack;
        }

        public void Clear()
        {
            funStack.Clear();
        }

        public PrtFunStackFrame TopOfStack
        {
            get
            {
                if (funStack.Count == 0)
                    return null;
                else
                    return funStack.Peek();
            }
        }

        public void PushFun(PrtFun fun, List<PrtValue> locals)
        {
            funStack.Push(fun.CreateFunStackFrame(locals, 0));
        }

        public void PushFun(PrtFun fun, List<PrtValue> locals, int retLoc)
        {
            funStack.Push(fun.CreateFunStackFrame(locals, retLoc));
        }

        public PrtFunStackFrame PopFun()
        {
            return funStack.Pop();
        }

        public override int GetHashCode()
        {
            return funStack.Select(v => v.GetHashCode()).Hash();
        }

        /// <summary>
        /// Print all contents (aka all PrtFunStackFrames) as a string
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return (funStack.Count == 0 ? "null" : funStack.Select(v => v.ToString()).Aggregate((s1, s2) => s1 + "|" + s2));
        }

        public void Resolve(StateImpl state)
        {
            foreach (var f in funStack)
            {
                f.Resolve(state);
            }
        }
    }

    public class PrtContinuation
    {
        public PrtContinuationReason reason;
        public PrtValue retVal;
        public List<PrtValue> retLocals;
        // The nondet field is different from the fields above because it is used 
        // by ReentrancyHelper to pass the choice to the nondet choice point.
        // Therefore, nondet should not be reinitialized in this class.
        public bool nondet;

        public PrtContinuation()
        {
            reason = PrtContinuationReason.Return;
            retVal = PrtValue.@null;
            nondet = false;
            retLocals = new List<PrtValue>();
        }

        public PrtContinuation Clone()
        {
            var clonedVal = new PrtContinuation();
            clonedVal.reason = this.reason;
            clonedVal.retVal = this.retVal.Clone();
            foreach (var loc in retLocals)
            {
                clonedVal.retLocals.Add(loc.Clone());
            }

            return clonedVal;
        }

        public bool ReturnAndResetNondet()
        {
            var ret = nondet;
            nondet = false;
            return ret;
        }

        public override int GetHashCode()
        {
            return Hashing.Hash(nondet.GetHashCode(), reason.GetHashCode(), retVal.GetHashCode(), retLocals.Select(v => v.GetHashCode()).Hash());
        }

        // I Print the Boolean nondet before the List retLocals (easier since list length varies)
        public override string ToString()
        {
            return reason.ToString() + "," + retVal.ToString() + "," + nondet.ToString() + "," + (retLocals.Count == 0 ? "null" : retLocals.Select(v => v.ToString()).Aggregate((s1, s2) => s1 + s2));
        }

        public void Resolve(StateImpl state)
        {
            retVal.Resolve(state);
            retLocals.ForEach(l => l.Resolve(state));
        }
    }
}
