// #define __ERROR_TRACE__

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.IO;

namespace P.Runtime
{

    public abstract class StateImpl : ICloneable
    {

#region Constructors
        /// <summary>
        /// This function is called when the stateimp is loaded first time.
        /// </summary>
        protected StateImpl()
        {
            implMachines = new List<PrtImplMachine>();
            specMachinesMap = new Dictionary<string, PrtSpecMachine>();
            exception = null;
            currentVisibleTrace = new VisibleTrace();
            errorTrace = new StringBuilder();
        }
#endregion

#region Fields

        /// <summary>
        /// Map from the statemachine id to the instance of the statemachine.
        /// </summary>
        private List<PrtImplMachine> implMachines;

        public List<PrtImplMachine> ImplMachines
        {
            get
            {
                return implMachines;
            }
        }

        public List<PrtImplMachine> EnabledMachines
        {
            get
            {
                return implMachines.Where(m => m.currentStatus == PrtMachineStatus.Enabled).ToList();
            }
        }
        /// <summary>
        /// List of spec machines
        /// </summary>
        private Dictionary<string, PrtSpecMachine> specMachinesMap;

        public void DbgCompare(StateImpl state)
        {
            Debug.Assert(implMachines.Count == state.implMachines.Count);
            for (int i = 0; i < implMachines.Count; i++)
            {
                implMachines[i].DbgCompare(state.implMachines[i]);
            }

            Debug.Assert(specMachinesMap.Count == state.specMachinesMap.Count);
            foreach (var tup in specMachinesMap)
            {
                Debug.Assert(state.specMachinesMap.ContainsKey(tup.Key));
                tup.Value.DbgCompare(state.specMachinesMap[tup.Key]);
            }

            Debug.Assert(GetHashCode() == state.GetHashCode());
        }

        /// <summary>
        /// Stores the exception encountered during exploration.
        /// </summary>
        private Exception exception;

        #region The fields for list abstraction
        public enum ExploreMode
        {
            Normal,
            Find_A_AP, // --PL: used for investigation
            Competitor // find competitors
        };

        public static ExploreMode mode = ExploreMode.Normal;
        /// <summary>
        /// Store the hashcode of the successor
        /// </summary>
        public static int succHash;
        /// <summary>
        /// Store the hashcode of the predecessor
        /// </summary>
        public static int predHash;
        public static bool FileDump = false;

        /// <summary>
        /// use invariants to eliminate abstrac successors
        /// </summary>
        public static bool invariant = false;
        public static bool concrete = false;
        public static bool symmetryReduction = false;
        #endregion

        public VisibleTrace currentVisibleTrace;
        public StringBuilder errorTrace;
        public static List<string> visibleEvents = new List<string>();
        public static List<string> visibleInterfaces = new List<string>();
        public delegate PrtImplMachine CreateMachineDelegate(StateImpl application, PrtValue payload);
        public delegate PrtSpecMachine CreateSpecDelegate(StateImpl application);
        public static Dictionary<string, Dictionary<string, string>> linkMap = new Dictionary<string, Dictionary<string, string>>();
        public static Dictionary<string, string> machineDefMap = new Dictionary<string, string>();
        public static Dictionary<string, bool> isSafeMap = new Dictionary<string, bool>();
        public static Dictionary<string, List<string>> specMachineMap = new Dictionary<string, List<string>>();
        public static Dictionary<string, CreateMachineDelegate> createMachineMap = new Dictionary<string, CreateMachineDelegate>();
        public static Dictionary<string, CreateSpecDelegate> createSpecMap = new Dictionary<string, CreateSpecDelegate>();
        public static Dictionary<string, List<PrtEventValue>> interfaceMap = new Dictionary<string, List<PrtEventValue>>();

        public delegate bool GetBooleanChoiceDelegate();
        public GetBooleanChoiceDelegate UserBooleanChoice = null;

        public delegate void CreateMachineCallbackDelegate(PrtImplMachine machine);
        public CreateMachineCallbackDelegate CreateMachineCallback = null;

        public delegate void DequeueCallbackDelegate(PrtImplMachine machine, string evName, string senderMachineName, string senderMachineStateName);
        public DequeueCallbackDelegate DequeueCallback = null;

        public delegate void StateTransitionCallbackDelegate(PrtImplMachine machine, PrtState from, PrtState to, string reason);
        public StateTransitionCallbackDelegate StateTransitionCallback = null;

#endregion

#region Getters and Setters
        public bool Deadlock
        {
            get
            {
                bool enabled = false;
                foreach (var x in implMachines)
                {
                    if (enabled) break;
                    enabled = enabled || (x.currentStatus == PrtMachineStatus.Enabled);
                }
                bool hot = false;
                foreach (var x in specMachinesMap.Values)
                {
                    if (hot) break;
                    hot = hot || x.currentTemperature == StateTemperature.Hot;
                }
                return (!enabled && hot);
            }
        }

        public Exception Exception
        {
            get { return exception; }
            set { exception = value; }
        }


#endregion

#region Clone Function
        public abstract StateImpl MakeSkeleton();

        public object Clone()
        {
            var clonedState = MakeSkeleton();
            // clone all the fields
            clonedState.implMachines = new List<PrtImplMachine>();
            foreach (var machine in implMachines)
            {
                clonedState.implMachines.Add(machine.Clone(clonedState));
            }

            clonedState.specMachinesMap = new Dictionary<string, PrtSpecMachine>();
            foreach (var specMachine in specMachinesMap)
            {
                clonedState.specMachinesMap.Add(specMachine.Key, (specMachine.Value).Clone(clonedState));
            }

            clonedState.exception = this.exception;

            clonedState.currentVisibleTrace = new VisibleTrace();
            foreach (var item in currentVisibleTrace.Trace)
            {
                clonedState.currentVisibleTrace.Trace.Add(item);
            }
#if __ERROR_TRACE__
            ///  Whether store the error traces or not. This will eat huge memory if turn it on.  
            clonedState.errorTrace = new StringBuilder(errorTrace.ToString());
#endif
            clonedState.Resolve();

            return clonedState;
        }

        public void Resolve()
        {
            foreach (var m in implMachines)
            {
                m.Resolve(this);
            }

            foreach (var m in specMachinesMap.Values)
            {
                m.Resolve(this);
            }
        }
#endregion
        /// <summary>
        /// 
        /// </summary>
        public void AbstractMe()
        {
            foreach (var m in ImplMachines)
            {
                m.AbstractMe();
            }                
        }
        /// <summary>
        /// -- PL: compute all abstract successors of current state. Note: the computation is done by
        /// abstract interpretation. 
        /// </summary>
        /// <param name="abstract_succs"></param>
        /// <param name="abstract_succs_SW"></param>
        public void CollectAbstractSuccessors(HashSet<int> abstract_succs, StreamWriter abstract_succs_SW)
        {
            for (int currIndex = 0; currIndex < ImplMachines.Count; ++currIndex)
            {
                PrtImplMachine machine = ImplMachines[currIndex];

                /// reject disabled machines
                if (!(machine.currentStatus == PrtMachineStatus.Enabled))
                    continue;

                /// reject machines not dequeing or receiving. I assume these are the only two 
                /// that can lead to a call to PrtDequeueEvent
                if (!(machine.nextSMOperation == PrtNextStatemachineOperation.DequeueOperation ||
                      machine.nextSMOperation == PrtNextStatemachineOperation.ReceiveOperation))
                    continue;

                /// reject machines with empty queues. Apparently enabled machines whose 
                /// nextSMOperation is dequeue or receive may have still an empty queue.
                if (machine.eventQueue.Empty())
                    continue;

                /// reject "non-abstract" queues: those with empty suffix. In this case the abstraction is precise,
                /// so the successor function cannot generate anything new
                if (machine.eventQueue.Size() <= PrtEventBuffer.p) // prefix
                    continue;

                CollectAbstractSuccessorsFromList(currIndex, abstract_succs, abstract_succs_SW);
            }
        }

        /// <summary>
        /// Compute the abstract successors caused by updating the queue of <paramref name="currIndex"/> machine.
        /// 
        /// </summary>
        /// <param name="currIndex">The index for the machine about to explore</param>
        /// <param name="abstract_succs"></param>
        /// <param name="abstract_succs_SW"></param>
        void CollectAbstractSuccessorsFromList(int currIndex, HashSet<int> abstract_succs, StreamWriter abstract_succs_SW)
        {
            List<PrtEventNode> preDequeEvents = ImplMachines[currIndex].eventQueue.events; // pre-dequeue events list

            var choiceVector = new List<bool>();
            bool more;
            do
            {
                StateImpl          succ     = (StateImpl)Clone();
                PrtImplMachine     machineOfSucc  = succ.ImplMachines[currIndex]; // the machine of successor indexing in currIndex
                PrtEventBuffer     queueOfMachine = machineOfSucc.eventQueue; // the message queue of successor machine
                List<PrtEventNode> queueEvents = queueOfMachine.events;   // the events list of successor machine

                more = machineOfSucc.PrtRunStateMachineNextChoice(choiceVector);
                Debug.Assert(preDequeEvents.Count - queueOfMachine.Size() <= 1); // we have dequeued at most one event
                /// If dequeing was unsuccessful, then there is nothing left to do. 
                /// No more nondeterministic choices to try
                if (preDequeEvents.Count == queueOfMachine.Size() || succ.CheckFailure(0)) // 
                    return;                                               // 

                int idxOfEventDequeuedFromSuffix = Math.Max(PrtEventBuffer.idxOfLastDequeuedEvent, PrtEventBuffer.p);
                PrtEventNode eventDequeuedFromSuffix = preDequeEvents[idxOfEventDequeuedFromSuffix];

                /// <remarks>Choice 1: </remarks> The eventDequeuedFromSuffix, aka the last dequeued event, existed 
                /// exactly once in the concrete suffix. 
                /// Then it is gone after the dequeue, in both abstract and concrete. The new state abstract is valid.
                succ.AddToAbstractSuccessorsIfInvSat(currIndex, this, abstract_succs, abstract_succs_SW);

                ///  <remarks>Choice 1: </remarks> The eventDequeuedFromSuffix, aka the last dequeued event, existed 
                ///  >= twice in concrete suffix. 
                ///  We need to find all positions where to re-introduce it in the abstract queue, and try them all 
                ///  non-deterministically.
                for (int pos = idxOfEventDequeuedFromSuffix; pos < queueOfMachine.Size(); ++pos)
                {
                    // insert eventDequeuedFromSuffix at position pos (push the rest to the right)
                    queueEvents.Insert(pos, eventDequeuedFromSuffix);
                    succ.AddToAbstractSuccessorsIfInvSat(currIndex, this, abstract_succs, abstract_succs_SW);
                    queueEvents.RemoveAt(pos); // restore previous state
                }
                queueEvents.Add(eventDequeuedFromSuffix); // finally, insert eventDequeuedFromSuffix at end
                succ.AddToAbstractSuccessorsIfInvSat(currIndex, this, abstract_succs, abstract_succs_SW);
            } while (more);
        }

        public class SuccessorFound : System.Exception
        {
            /// <summary>
            /// a: a state
            /// ap: a', the successor of a
            /// so a -> ap
            /// </summary>
            public StateImpl a, ap;
            public SuccessorFound(StateImpl a, StateImpl ap)
            {
                this.a = a;
                this.ap = ap;
            }
        }

        /// <summary>
        /// Add <paramref name="this"/> to abstract successors if <paramref name="this"/> satisfies the invariants.
        /// </summary>
        /// <param name="currIndex"></param>
        /// <param name="pred"></param>
        /// <param name="abstract_succs"></param>
        /// <param name="abstract_succs_SW"></param>
        void AddToAbstractSuccessorsIfInvSat(int currIndex, StateImpl pred, HashSet<int> abstract_succs, StreamWriter abstract_succs_SW)
        {
            int p_hash = symmetryReduction ? pred.GetHashCode(true) : pred.GetHashCode();
            int   hash = symmetryReduction ?      GetHashCode(true) :      GetHashCode();

            // Console.WriteLine(this.ToPrettyString());

            if (mode == ExploreMode.Find_A_AP) // if we are locating a and ap
            {
                if (succHash == hash)
                {
                    predHash = p_hash;
                    throw new SuccessorFound(pred, this);
                }
            }

            if ( invariant ? CheckAbstractStateInvariant(currIndex) && CheckTransInvariant(currIndex, pred) : true )
            {
                if (abstract_succs.Add(hash))
                    if (FileDump)
                    {
                        abstract_succs_SW.Write(ToPrettyString());
                        abstract_succs_SW.WriteLine("==================================================");
                    }
            } 
        }

        /// <summary>
        /// --PL: check if state fails assume
        /// </summary>
        /// <param name="depth">Useless for now</param>
        /// <returns> 
        /// - true: if current state fails assume
        /// - false: othewise
        /// </returns>
        public bool CheckFailure(int depth)
        {
            //if (UseDepthBounding && depth > DepthBound)    // ignore this for now. These parameters are part of class DfsExploration, so can't use here
            //{
            //    return true;
            //}

            if (Exception == null)
            {
                return false;
            }
            if (Exception is PrtAssumeFailureException)
            {
                return true;
            }
            else if (Exception is PrtException)
            {
                Console.WriteLine(errorTrace.ToString());
                Console.WriteLine("ERROR: {0}", Exception.Message);
                Environment.Exit(-1);
            }
            else
            {
                Console.WriteLine(errorTrace.ToString());
                Console.WriteLine("[Internal Exception]: Please report to the P Team");
                Console.WriteLine(Exception.ToString());
                Environment.Exit(-1);
            }
            return false;
        }

        /// <summary>
        /// Call concrete model checker to test invariants
        /// If invariant does not satisfy, then throw an exception.
        /// </summary>
        /// <returns></returns>
        public bool CheckConcreteStateInvariant()
        {
            if (!invariant || !concrete)
                return true;
            for (int currIndex = 0; currIndex < ImplMachines.Count; ++currIndex)
            {
                if (!CheckConcreteStateInvariant(currIndex))
                    return false;
            }
#if false // for debugging
            Console.WriteLine("conctete model checking passed........");
#endif 
            return true;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="currIndex"></param>
        /// <returns></returns>
        public bool CheckConcreteStateInvariant(int currIndex)
        {
            var client = implMachines[currIndex];
            Debug.Assert(!client.eventQueue.IsAbstract());
            var queueOfClient = client.eventQueue.events;
            //return ConcreteChecker.Check(currIndex, client.GetEventValue().ToString(), queueOfClient);
            return true;
        }

        ///<summary>
        /// This is like a typedef and must, unfortunately, pollute the global scope. 
        /// Should be inside below function def.
        /// delegate bool Event_is_and_queue_contains(PrtImplMachine machine, string s);
        /// STATE AND TRANSITION INVARIANTS.
        /// This is application dependent, so eventually this needs to be done differently.
        /// 1. STATE INVARIANTS
        /// Abstract states obtained by abstraction from a reachable concrete 
        /// state satisfy all invariants by construction.
        /// But abstract states are also obtained via the succesor function 
        /// from another abstract state. This function must overapproximate 
        /// and may therefore violate some invariant.

        /// currIndex = index of the ImplMachine that has just been run (other 
        /// machines have not changed, so their invariants need not be checked)
        /// 
        ///</summary>
        public bool CheckAbstractStateInvariant(int currIndex)  
        {
            var client = implMachines[currIndex];
            Debug.Assert(client.eventQueue.IsAbstract());

            var queueOfClient = client.eventQueue.events;
            return AbstractChecker.Check(currIndex, client.GetEventValue().ToString(), queueOfClient);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="currIndex"></param>
        /// <param name="pred"></param>
        /// <returns></returns>
        public bool CheckTransInvariant(int currIndex, StateImpl pred)
        {
            return true;
        }

        /// <summary>
        /// Implementation note: there are currently three distinct methods 
        /// that crawl over the hierarchy of a StateImpl and collect various 
        /// kinds of information:
        /// - GetHashCode computes the combined hash value of the state
        /// - ToString    computes a combined string representation of the state
        /// - Clone       computes a copy of the state, in fresh memory.
        /// The descent into the StateImpl hierarchy is at least very similar, 
        /// if not identical. Sometimes we RELY on it being identical, e.g. 
        /// the hash and the string(both are used to store a state, in different 
        /// contexts). This code redundancy is unreliable and error prone.
        /// </summary>
        /// <returns></returns>
        public override int GetHashCode()
        {
            var hash1 = implMachines.Select(impl => impl.GetHashCode()).Hash();
            var hash2 = specMachinesMap.Select(pair => pair.Value.GetHashCode()).Hash();
            return Hashing.Hash(hash1, hash2);
        }

        /// <summary>
        /// A GetHashCode function with symmetry reduction
        /// </summary>
        /// <param name="symmetryReduction"></param>
        /// <returns></returns>
        public int GetHashCode(bool symmetryReduction)
        {
            Console.WriteLine("With symmetry reduction");
            if (symmetryReduction == false)
                return GetHashCode();

            var hash1 = implMachines.Select(impl => impl.GetHashCode()).Hash(true);
            var hash2 = specMachinesMap.Select(pair => pair.Value.GetHashCode()).Hash();
            return Hashing.Hash(hash1, hash2);
        }
        /// <summary>
        /// A good convention for converting a state into a compressed 
        /// string might be to define a separator char (like '|')
        /// that is used twice, thrice, etc. depending on how deep in 
        /// the state hierarchy you are. This way you need to reserve 
        /// only 1 char
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            // the debugger seems to use the following code to Print
#if !DEBUG
            Console.WriteLine("StateImpl.ToString: error: should not reach this function");
            throw new NotImplementedException();
#endif
            string result = "";

            // dump each ImplMachine to a string
            foreach (PrtImplMachine m in implMachines)
            {
                result += m.ToString() + "|";
            }

            result += "#";

            // dump each specMachine to a string
            foreach (KeyValuePair<string, PrtSpecMachine> pair in specMachinesMap)
            {
                result += pair.Key + "|";
            }

            return result;
        }

        /// <summary>
        /// --PL: define this as we can't use the ToString??
        /// </summary>
        /// <param name="indent"></param>
        /// <returns></returns>
        public string ToPrettyString(string indent = "")
        {
            string result = "Hash: " + GetHashCode().ToString() + "\n";

            if (implMachines.Count == 0)
                result += indent + "ImplMachines: (none)\n";
            else
            {
                ushort i = 0;
                // dump each ImplMachine to a string
                foreach (PrtImplMachine m in implMachines)
                {
                    result += indent + "ImplMachine " + (i++).ToString() + " Hash: " + m.GetHashCode() + ":\n";
                    result += m.ToPrettyString(indent + "  ");
                }
            }

            if (specMachineMap.Count == 0)
                result += indent + "SpecMachines: (none)\n";
            else
            {
                ushort i = 0;
                // for each specMachine, why do we Print only the key?
                foreach (KeyValuePair<string, PrtSpecMachine> pair in specMachinesMap)
                {
                    result += indent + "SpecMachine " + (i++).ToString() + ": " + pair.Key + "\n";
                }
            }

            return result;
        }

        public List<PrtSpecMachine> GetAllSpecMachines()
        {
            return specMachinesMap.Values.ToList();
        }

        private List<PrtSpecMachine> GetSpecMachines(string currMachine)
        {
            var allSpecMachines = specMachineMap.Where(mon => mon.Value.Contains(currMachine))
                                        .Select(item => item.Key)
                                        .Select(monName => specMachinesMap[monName]).ToList();
            return allSpecMachines;
        }
        public PrtInterfaceValue CreateInterface(PrtMachine currMach, string interfaceOrMachineName, PrtValue payload)
        {
            //add visible action to trace
            if (visibleInterfaces.Contains(interfaceOrMachineName))
            {
                currentVisibleTrace.AddAction(interfaceOrMachineName, payload.ToString());
            }

            var renamedImpMachine = linkMap[currMach.renamedName][interfaceOrMachineName];
            var impMachineName = machineDefMap[renamedImpMachine];
            var machine = createMachineMap[impMachineName](this, payload);
            machine.isSafe = isSafeMap[renamedImpMachine];
            machine.renamedName = renamedImpMachine;
            AddImplMachineToStateImpl(machine);

            CreateMachineCallback?.Invoke(machine);

            // TraceLine("<CreateLog> Machine {0}-{1} was created by machine {2}-{3}", currMach.renamedName, currMach.instanceNumber, machine.renamedName, machine.instanceNumber);
            TraceLine("<CreateLog> Machine {0}-{1} was created by machine {2}-{3}", machine.renamedName, machine.instanceNumber, currMach.renamedName, currMach.instanceNumber);

            if (interfaceMap.ContainsKey(interfaceOrMachineName))
            {
                return new PrtInterfaceValue(machine, interfaceMap[interfaceOrMachineName]);
            }
            else
            {
                return new PrtInterfaceValue(machine, machine.self.permissions);
            }
        }

        public void CreateMainMachine(string mainInterface)
        {


            if (!machineDefMap.ContainsKey(mainInterface))
            {
                throw new PrtInternalException($"No {mainInterface} interface in the module machine definition map");
            }
            var impMachineName = machineDefMap[mainInterface];
            var machine = createMachineMap[impMachineName](this, PrtValue.@null);
            machine.isSafe = isSafeMap[mainInterface];
            machine.renamedName = mainInterface;
            AddImplMachineToStateImpl(machine);
            TraceLine("<CreateLog> server machine {0} was created by machine Runtime", impMachineName);

        }

        public void CreateSpecMachine(string renamedSpecName)
        {
            TraceLine("<CreateLog> Spec Machine {0} was created by machine Runtime", renamedSpecName);
            var impSpecMachine = machineDefMap[renamedSpecName];
            var machine = createSpecMap[impSpecMachine](this);
            machine.isSafe = isSafeMap[renamedSpecName];
            machine.renamedName = renamedSpecName;
            AddSpecMachineToStateImpl(machine);
        }
        public int NextMachineInstanceNumber(Type machineType)
        {
            return implMachines.Where(m => m.GetType() == machineType).Count() + 1;
        }

        public void Announce(PrtEventValue ev, PrtValue payload, PrtMachine parent)
        {
            if (ev.Equals(PrtValue.@null))
            {
                throw new PrtIllegalEnqueueException("Enqueued event must not be null");
            }

            PrtType prtType = ev.evt.payloadType;
            //assertion to check if argument passed inhabits the payload type.
            if (prtType is PrtNullType)
            {
                if (!payload.Equals(PrtValue.@null))
                {
                    throw new PrtIllegalEnqueueException("Did not expect a payload value");
                }
            }
            else if (!PrtValue.PrtInhabitsType(payload, prtType))
            {
                throw new PrtInhabitsTypeException(String.Format("Payload <{0}> does not match the expected type <{1}> with event <{2}>", payload.ToString(), prtType.ToString(), ev.evt.name));
            }

            var allSpecMachines = GetSpecMachines(parent.renamedName);
            foreach (var mon in allSpecMachines)
            {
                if (mon.observes.Contains(ev))
                {
                    TraceLine("<AnnounceLog> Enqueued Event <{0}, {1}> to Spec Machine {2}", ev, payload, mon.Name);
                    mon.PrtEnqueueEvent(ev, payload, parent);
                }
            }
        }

        public void AddImplMachineToStateImpl(PrtImplMachine machine)
        {
            implMachines.Add(machine);
        }

        public void AddSpecMachineToStateImpl(PrtSpecMachine spec)
        {
            specMachinesMap.Add(spec.renamedName, spec);
        }

        public void Trace(string message, params object[] arguments)
        {
            message = message.Replace(@"\n", System.Environment.NewLine);
            errorTrace.Append(String.Format(message, arguments));
        }

        public void TraceLine(string message, params object[] arguments)
        {
            message = message.Replace(@"\n", System.Environment.NewLine);
            errorTrace.AppendLine(String.Format(message, arguments));
        }



        public void SetPendingChoicesAsBoolean(PrtImplMachine process)
        {
            //TODO: NOT IMPLEMENT YET
            //throw new NotImplementedException();
        }

        public Boolean GetSelectedChoiceValue(PrtImplMachine process)
        {
            if (UserBooleanChoice != null)
            {
                return UserBooleanChoice();
            }
            else
            {
                // throw new NotImplementedException();
                return (new Random(DateTime.Now.Millisecond)).Next(10) > 5;
            }
        }
    }

    public class StateImplComparer : IEqualityComparer<StateImpl>
    {
        public int GetHashCode(StateImpl s) { return s.GetHashCode(); }

        public bool Equals(StateImpl s1, StateImpl s2) { return s1.GetHashCode() == s2.GetHashCode(); }
    }

}
