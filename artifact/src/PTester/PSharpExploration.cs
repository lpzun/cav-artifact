using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using P.Runtime;
using Microsoft.PSharp;
using System.Diagnostics;

namespace P.Tester
{
    public static class PSharpExploration
    {
        public static StateImpl main_s;
        public static StateImpl currentImpl;
        public static Coverage coverage;

        public static void RunPSharpTester(StateImpl s)
        {
            main_s = s;
            coverage = new Coverage();

            var configuration = Configuration.Create()
                .WithNumberOfIterations(100);

            configuration.UserExplicitlySetMaxFairSchedulingSteps = true;
            configuration.MaxUnfairSchedulingSteps = 100;
            configuration.MaxFairSchedulingSteps = configuration.MaxUnfairSchedulingSteps * 10;
            configuration.LivenessTemperatureThreshold = configuration.MaxFairSchedulingSteps / 3;

            foreach (var machine in main_s.EnabledMachines)
            {
                coverage.DeclareMachine(machine);
            }

            var engine = Microsoft.PSharp.TestingServices.TestingEngineFactory.CreateBugFindingEngine(
                configuration, PSharpWrapper.Execute);
            engine.Run();

            Console.WriteLine("Bugs found: {0}", engine.TestReport.NumOfFoundBugs);

            if (engine.TestReport.NumOfFoundBugs > 0)
            {
                if (currentImpl.Exception != null && currentImpl.Exception is PrtException)
                {
                    Console.WriteLine("{0}", currentImpl.errorTrace.ToString());
                    Console.WriteLine("ERROR: {0}", currentImpl.Exception.Message);
                }
                else if (currentImpl.Exception != null)
                {
                    Console.WriteLine("{0}", currentImpl.errorTrace.ToString());
                    Console.WriteLine("[Internal Exception]: Please report to the P Team");
                    Console.WriteLine("{0}", currentImpl.Exception.ToString());
                }
                else
                {
                    Console.WriteLine("{0}", currentImpl.errorTrace.ToString());
                    Console.WriteLine("ERROR: Liveness violation");
                }
            }

            Console.WriteLine("Dumping coverage information");
            coverage.Dump("coverage");
            Console.WriteLine("... Writing coverage.txt");
            Console.WriteLine("... Writing coverage.dgml");
        }

        public class PSharpWrapper
        {
            public static void Execute(PSharpRuntime runtime)
            {
                var s = (StateImpl)PSharpExploration.main_s.Clone();

                s.UserBooleanChoice = delegate ()
                {
                    return runtime.Random();
                };

                s.CreateMachineCallback = delegate (PrtImplMachine machine)
                {
                    PSharpExploration.coverage.DeclareMachine(machine);
                };

                s.DequeueCallback = delegate (PrtImplMachine machine, string evName, string senderMachineName, string senderMachineStateName)
                {
                    PSharpExploration.coverage.ReportDequeue(machine, evName, senderMachineName, senderMachineStateName);
                };

                s.StateTransitionCallback = delegate (PrtImplMachine machine, PrtState from, PrtState to, string reason)
                {
                    PSharpExploration.coverage.ReportStateTransition(machine, from, to, reason);
                };

                PSharpExploration.currentImpl = s;

                runtime.CreateMachine(typeof(PSharpMachine), new MachineInitEvent(s));
            }

            public class Unit : Microsoft.PSharp.Event { }

            public class MachineInitEvent : Microsoft.PSharp.Event
            {
                public StateImpl s;

                public MachineInitEvent(StateImpl s)
                {
                    this.s = s;
                }
            }

            class PSharpMachine : Microsoft.PSharp.Machine
            {
                StateImpl currImpl;
                Dictionary<PrtSpecMachine, Type> specToMonitor;

                [Microsoft.PSharp.Start]
                [Microsoft.PSharp.OnEntry(nameof(Configure))]
                [Microsoft.PSharp.OnEventDoAction(typeof(Unit), nameof(Step))]
                class Init : Microsoft.PSharp.MachineState { }

                void Configure()
                {
                    var e = (this.ReceivedEvent as MachineInitEvent);
                    this.currImpl = e.s;
                    this.specToMonitor = new Dictionary<PrtSpecMachine, Type>();

                    // register monitors
                    foreach (var spec in currImpl.GetAllSpecMachines())
                    {
                        var genericTy = typeof(PSharpMonitor<int>).GetGenericTypeDefinition();
                        var specTy = spec.GetType();
                        var monitorTy = genericTy.MakeGenericType(specTy);
                        this.specToMonitor.Add(spec, monitorTy);

                        this.Id.Runtime.RegisterMonitor(monitorTy);
                    }

                    this.Raise(new Unit());
                }

                void Step()
                {
                    if (currImpl.EnabledMachines.Count == 0)
                    {
                        return;
                    }

                    foreach (var tup in specToMonitor)
                    {
                        Event ev = tup.Key.currentTemperature == StateTemperature.Hot ? (Event)new MoveToHot() :
                            tup.Key.currentTemperature == StateTemperature.Warm ? (Event)new MoveToWarm() :
                            (Event)new MoveToCold();

                        this.Monitor(tup.Value, ev);
                    }


                    var num = currImpl.EnabledMachines.Count;
                    var choosenext = this.RandomInteger(num);
                    currImpl.EnabledMachines[choosenext].PrtRunStateMachine();
                    if (currImpl.Exception != null)
                    {
                        if (currImpl.Exception is PrtAssumeFailureException)
                        {
                            return;
                        }
                        else
                        {
                            this.Assert(false);
                        }
                    }

                    this.Raise(new Unit());
                }

            }

            class MoveToHot : Event { }
            class MoveToCold : Event { }
            class MoveToWarm : Event { }

            class PSharpMonitor<T> : Monitor
            {
                [Start]
                [Cold]
                [OnEventDoAction(typeof(MoveToHot), nameof(GotHot))]
                [OnEventDoAction(typeof(MoveToCold), nameof(GotCold))]
                [OnEventDoAction(typeof(MoveToWarm), nameof(GotWarm))]
                class S1 : MonitorState { }

                [Hot]
                [OnEventDoAction(typeof(MoveToHot), nameof(GotHot))]
                [OnEventDoAction(typeof(MoveToCold), nameof(GotCold))]
                [OnEventDoAction(typeof(MoveToWarm), nameof(GotWarm))]
                class S2 : MonitorState { }

                [OnEventDoAction(typeof(MoveToHot), nameof(GotHot))]
                [OnEventDoAction(typeof(MoveToCold), nameof(GotCold))]
                [OnEventDoAction(typeof(MoveToWarm), nameof(GotWarm))]
                class S3 : MonitorState { }

                void GotHot()
                {
                    this.Goto<S2>();
                }

                void GotCold()
                {
                    this.Goto<S1>();
                }

                void GotWarm()
                {
                    this.Goto<S3>();
                }

            }
        }

    }
}
