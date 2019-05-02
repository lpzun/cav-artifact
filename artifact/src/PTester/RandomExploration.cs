using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using P.Runtime;
using System.Diagnostics;

namespace P.Tester
{
    static class RandomExploration
    {
        private static int max_queue_size;

        public static void Explore(StateImpl s, CommandLineOptions options)
        {
            int maxNumOfSchedules = 10000;
            int maxDepth = 5000;
            int numOfSchedules = 0;
            int numOfSteps = 0;
            var randomScheduler = new Random(0); // DateTime.Now.Millisecond);

            max_queue_size = 0;

            while (numOfSchedules < maxNumOfSchedules)
            {
                var currImpl = (StateImpl)s.Clone();
                if (numOfSchedules % 1000 == 0)
                {
                    Console.WriteLine("-----------------------------------------------------");
                    Console.WriteLine("Total Schedules Explored: {0}", numOfSchedules);
                    Console.WriteLine("-----------------------------------------------------");
                }
                numOfSteps = 0;
                while (numOfSteps < maxDepth)
                {

                    var num = currImpl.EnabledMachines.Count;

                    if (num == 0)
                    {
                        break;
                    }
                                        
                    var choosenext = randomScheduler.Next(0, num);

                    currImpl.EnabledMachines[choosenext].PrtRunStateMachine();

#if DEBUG
                    // update maximum encountered queue size
                    foreach (PrtImplMachine m in currImpl.ImplMachines)
                        max_queue_size = Math.Max(max_queue_size, m.eventQueue.Size());
#endif

                    if (currImpl.Exception != null)
                    {
                        if (currImpl.Exception is PrtAssumeFailureException)
                        {
                            break;
                        }
                        else if (currImpl.Exception is PrtException)
                        {
                            Console.WriteLine(currImpl.errorTrace.ToString());
                            Console.WriteLine("ERROR: {0}", currImpl.Exception.Message);
                            Environment.Exit(-1);
                        }
                        else
                        {
                            Console.WriteLine(currImpl.errorTrace.ToString());
                            Console.WriteLine("[Internal Exception]: Please report to the P Team");
                            Console.WriteLine(currImpl.Exception.ToString());
                            Environment.Exit(-1);
                        }
                    }

                    numOfSteps++;

                    //print the execution if verbose
                    if (options.verbose)
                    {
                        Console.WriteLine("-----------------------------------------------------");
                        Console.WriteLine("Execution {0}", numOfSchedules);
                        Console.WriteLine(currImpl.errorTrace.ToString());
                    }
                }
                numOfSchedules++;
            }

            Console.WriteLine("");
            Console.WriteLine("Maximum queue size observed during this random test: {0}", max_queue_size);

        }

        public static void DebugHashAndClone(StateImpl s, CommandLineOptions options)
        {
            int maxNumOfSchedules = 10000;
            int maxDepth = 5000;
            int numOfSchedules = 0;
            int numOfSteps = 0;
            var randomScheduler = new Random(0); // DateTime.Now.Millisecond);
            while (numOfSchedules < maxNumOfSchedules)
            {
                var currImpl = (StateImpl)s.Clone();
                if (numOfSchedules % 1000 == 0)
                {
                    Console.WriteLine("-----------------------------------------------------");
                    Console.WriteLine("Total Schedules Explored: {0}", numOfSchedules);
                    Console.WriteLine("-----------------------------------------------------");
                }
                numOfSteps = 0;
                while (numOfSteps < maxDepth)
                {
                    StateImpl clone = null;

                    clone = (StateImpl)currImpl.Clone();
                    if (clone.GetHashCode() != currImpl.GetHashCode())
                    {
                        currImpl.DbgCompare(clone);
                        Debug.Assert(false);
                    }

                    Console.WriteLine("-----------------");
                    Console.WriteLine(currImpl.errorTrace.ToString());
                    Console.WriteLine("-----------------");
                    Console.WriteLine(clone.errorTrace.ToString());
                    Console.WriteLine("-----------------");

                    if (currImpl.EnabledMachines.Count == 0)
                    {
                        break;
                    }

                    var num = currImpl.EnabledMachines.Count;
                    var choosenext = randomScheduler.Next(0, num);

                    int seed = 0;

                    seed = randomScheduler.Next();
                    var choices = new Random(seed);

                    currImpl.UserBooleanChoice = delegate ()
                    {
                        return choices.Next(2) == 0;
                    };

                    currImpl.EnabledMachines[choosenext].PrtRunStateMachine();

                    choices = new Random(seed);
                    clone.UserBooleanChoice = delegate ()
                    {
                        return choices.Next(2) == 0;
                    };

                    clone.EnabledMachines[choosenext].PrtRunStateMachine();
                    if (clone.GetHashCode() != currImpl.GetHashCode())
                    {
                        Console.WriteLine("numOfSchedules: {0}", numOfSchedules);
                        Console.WriteLine("numSteps: {0}", numOfSteps);
                        Console.WriteLine("-----------------");
                        Console.WriteLine(currImpl.errorTrace.ToString());
                        Console.WriteLine("-----------------");
                        Console.WriteLine(clone.errorTrace.ToString());
                        Console.WriteLine("-----------------");

                        currImpl.DbgCompare(clone);
                        System.Diagnostics.Debug.Assert(false);
                    }


                    if (currImpl.Exception != null)
                    {
                        if (currImpl.Exception is PrtAssumeFailureException)
                        {
                            break;
                        }
                        else if (currImpl.Exception is PrtException)
                        {
                            Console.WriteLine(currImpl.errorTrace.ToString());
                            Console.WriteLine("ERROR: {0}", currImpl.Exception.Message);
                            Environment.Exit(-1);
                        }
                        else
                        {
                            Console.WriteLine(currImpl.errorTrace.ToString());
                            Console.WriteLine("[Internal Exception]: Please report to the P Team");
                            Console.WriteLine(currImpl.Exception.ToString());
                            Environment.Exit(-1);
                        }
                    }

                    numOfSteps++;

                    //print the execution if verbose
                    if (options.verbose)
                    {
                        Console.WriteLine("-----------------------------------------------------");
                        Console.WriteLine("Execution {0}", numOfSchedules);
                        Console.WriteLine(currImpl.errorTrace.ToString());
                    }
                }
                numOfSchedules++;
            }

        }


    }
}
