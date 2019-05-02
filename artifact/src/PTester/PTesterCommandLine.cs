using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using P.Runtime;
using Microsoft.PSharp;
using Microsoft.PSharp.Utilities;
using System.Linq;

namespace P.Tester
{

    public enum TestResult
    {
        /// <summary>
        /// No errors were found within the specified limits of the search (if any).
        /// </summary>
        Success = 0,

        /// <summary>
        /// Invalid parameters passed
        /// </summary>
        InvalidParameters = 1,

        /// <summary>
        /// An assertion failure was encountered.
        /// </summary>
        AssertionFailure = 2,

        /// <summary>
        /// An execution was found in which all P machines are blocked and at least 
        /// one liveness monitor is in a hot state.
        /// </summary>
        Deadlock = 3,

        /// <summary>
        /// A lasso violating a liveness monitor was discovered.
        /// </summary>
        AcceptingCyleFound = 4,

        /// <summary>
        /// An internal error was encountered, typically indicating a bug in the compiler or runtime.
        /// </summary>
        InternalError = 5,

        /// <summary>
        /// Search stack size exceeded the maximum size.
        /// </summary>
        StackOverFlowError = 6,

        /// <summary>
        /// The search was canceled.
        /// </summary>
        Canceled = 7,

        /// <summary>
        /// Timeout
        /// </summary>
        Timeout = 8,
    }

    public class CommandLineOptions
    {
        public string inputFileName;
        public bool printStats;
        public int timeout;
        public bool UsePSharp = false;

        /// <summary>
        /// OS realted commandline options
        /// </summary>
        public bool DfsExploration; // unbounded exploration
        public bool OSList; // OS exploration: abstract with list
        public bool OSSet;  // OS exploration: abstract with set
        public bool interactive; // interactive mode, need users press button to continue
        public bool testMCer; // interactive mode, need users press button to continue
        public int k;

        public bool UseStateHashing;
        public bool isRefinement;
        public string LHSModel;
        public string RHSModel;
        public bool verbose;
        public int numberOfSchedules;
        public bool debugHashing;

        public CommandLineOptions()
        {
            inputFileName = null;
            printStats = false;
            timeout = 0;
            isRefinement = false;
            LHSModel = null;
            RHSModel = null;
            verbose = false;
            numberOfSchedules = 1000;
            debugHashing = false;
            DfsExploration = false;
            OSList = false;
            OSSet = false;
            interactive = false;
            UseStateHashing = false;
        }
    }

    public class PTesterCommandLine
    {
        public static CommandLineOptions ParseCommandLine(string[] args)
        {
            var options = new CommandLineOptions();
            for (int i = 0; i < args.Length; i++)
            {
                string arg = args[i];

                if (arg[0] == '-' || arg[0] == '/')
                {
                    /// Remove ToLower() by Peizun. Do this for invariant input
                    ///
                    string option = arg.TrimStart('/', '-');//.ToLower(); 
                    string param = string.Empty;

                    int sepIndex = option.IndexOf(':');

                    if (sepIndex > 0)
                    {
                        param = option.Substring(sepIndex + 1);
                        option = option.Substring(0, sepIndex);
                    }
                    else if (sepIndex == 0)
                    {
                        PrintHelp(arg, "Malformed option");
                        return null;
                    }

                    try
                    {
                        // note: the case string must consist of ALL SMALL letters! Otherwise 
                        // it won't find it (:-(   Found out the hard way
                        switch (option)
                        {
                            case "?":
                            case "h":
                            case "help":
                                PrintHelp(null, null);
                                Environment.Exit((int)TestResult.Success);
                                break;

                            case "stats":
                                options.printStats = true;
                                break;

                            case "v":
                            case "verbose":
                                options.verbose = true;
                                break;

                            case "ns":
                                if (param.Length != 0)
                                { options.numberOfSchedules = int.Parse(param); }
                                break;

                            case "timeout":
                                if (param.Length != 0)
                                { options.timeout = int.Parse(param); }
                                break;

                            case "psharp":
                                options.UsePSharp = true;
                                break;

                            case "dfs":
                                if (param.Length > 0)
                                    throw new ArgumentException("/" + option + ": no argument expected (did you mean to supply a queue-bound? use /queue-bound:" + int.Parse(param).ToString() + ")");
                                options.DfsExploration = true;
                                DfsExploration.useStateHashing = true; // turned on by default for now, since DFS w/o SH is not implemented
                                break;

                            case "os-list":
                                if (param.Length > 0)
                                    throw new ArgumentException("/" + option + ": no argument expected (did you mean to supply a queue-bound? use /queue-bound:" + int.Parse(param).ToString() + ")");
                                options.OSList = true;
                                DfsExploration.useStateHashing = true; // ditto
                                break;

                            case "os-set":
                                throw new NotImplementedException("/os-set: queue set abstraction no longer supported");
                                // I'm keeping the stuff below for later
                                if (param.Length > 0)
                                    throw new ArgumentException("/" + option + ": no argument expected (did you mean to supply a queue-bound? use /queue-bound:" + int.Parse(param).ToString() + ")");
                                options.OSSet = true;
                                DfsExploration.useStateHashing = true; // ditto
                                // PrtEventBuffer.qt = PrtEventBuffer.Queue_Type.set;
                                break;

                            case "queue-bound":
                                if (param.Length == 0)
                                    throw new ArgumentException("queue-bound argument: must supply non-negative parameter");
                                PrtEventBuffer.k = int.Parse(param);
                                if (PrtEventBuffer.k < 0)
                                    throw new ArgumentException("queue-bound argument: must supply NON-NEGATIVE parameter");
                                break;

                            case "interactive":
                                options.interactive = true;
                                break;

                            case "file-dump":
                                DfsExploration.fileDump = true;
                                StateImpl.FileDump = true;
                                break;

                            /// QuTL model checker related options
                            case "queue-prefix":
                                if (param.Length == 0)
                                    throw new ArgumentException("queue-prefix argument: must supply non-negative parameter");
                                PrtEventBuffer.p = int.Parse(param);
                                if (PrtEventBuffer.p < 0)
                                    throw new ArgumentException("queue-prefix argument: must supply NON-NEGATIVE parameter");
                                break;

                            case "qutl":
                                StateImpl.invariant = true;
                                if (param.Length == 0)
                                    throw new ArgumentException("invar argument: must supply QuTL formula in string");
                                string qutl = param;
                                Console.WriteLine("testing..." + qutl);
                                QuTLParser parser = new Runtime.QuTLParser(qutl);
                                P.Runtime.QuTLParser.Print();
                                //Console.WriteLine(QuTLParser.formulae.ToString());
                                Console.WriteLine();
                                break;

                            case "queue":
                                if (param.Length == 0)
                                    Console.WriteLine("Model checking empty queue...");
                                P.Runtime.TestMCer.queueContent = param;
                                options.testMCer = true;
                                break;

                            case "concrete":
                                StateImpl.concrete = true;
                                break;

                            case "processing":
                                if (param.Length == 0)
                                    throw new ArgumentException("For testing only: please specify the processing event");
                                P.Runtime.TestMCer.processEvent = param;
                                break;

                            /// using symmetry reduction
                            case "symmetry":
                                StateImpl.symmetryReduction = true;
                                break;

                            case "hash":
                                options.UseStateHashing = true;
                                break;

                            case "debughash":
                                options.debugHashing = true;
                                break;

                            case "break":
                                System.Diagnostics.Debugger.Launch();
                                break;

                            case "lhs":
                                if (param.Length != 0)
                                {
                                    options.LHSModel = param;
                                    options.RHSModel = null;
                                    options.isRefinement = true;
                                }
                                else
                                {
                                    PrintHelp(arg, "missing file name");
                                    return null;
                                }
                                break;

                            case "rhs":
                                if (param.Length != 0)
                                {
                                    options.RHSModel = param;
                                    options.LHSModel = null;
                                    options.isRefinement = true;
                                }
                                else
                                {
                                    PrintHelp(arg, "missing file name");
                                    return null;
                                }
                                break;

                            default:
                                PrintHelp(arg, "Invalid option");
                                return null;
                        }
                    }
                    catch (ArgumentException e)
                    {
                        Console.WriteLine(e.Message);
                        Console.WriteLine("Exiting.");
                        Environment.Exit(-1);
                    }
                }
                else
                {
                    if (options.inputFileName != null)
                    {
                        PrintHelp(arg, "Only one input file is allowed");
                        return null;
                    }

                    if (!File.Exists(arg))
                    {
                        PrintHelp(arg, "Cannot find input file");
                        return null;
                    }

                    options.inputFileName = arg;
                }
            }

            if (!options.testMCer && !options.isRefinement && options.inputFileName == null)
            {
                PrintHelp(null, "No input file specified");
                return null;
            }
            return options;
        }

        public static void PrintHelp(string arg, string errorMessage)
        {
            if (errorMessage != null)
            {
                if (arg != null)
                    PTesterUtil.PrintErrorMessage(String.Format("Error: \"{0}\" - {1}", arg, errorMessage));
                else
                    PTesterUtil.PrintErrorMessage(String.Format("Error: {0}", errorMessage));
            }

            Console.WriteLine("---------------------------------------------");
            Console.WriteLine("Options ::");
            Console.WriteLine("---------------------------------------------");
            Console.WriteLine("/h                       Print the help message");
            Console.WriteLine("/v or /verbose           Print the execution trace during exploration");
            Console.WriteLine("/ns:<int>                Number of schedulers <int> to explore");
            Console.WriteLine("/lhs:<LHS Model Dll>     Load the pre-computed traces of RHS Model and perform trace containment");
            Console.WriteLine("/rhs:<RHS Model Dll>     Compute all possible trace of the RHS Model using sampling and dump it in a file on disk");
            Console.WriteLine("/psharp                  Run the PSharp Tester");
            Console.WriteLine();
            Console.WriteLine("Flags related to exhaustive state space exploration:");
            Console.WriteLine("/dfs                     Perform DFS exploration of the state space");
            Console.WriteLine("/os-list                 Perform OS exploration (based on DFS) of the state space, with queue list abstraction");
            // Console.WriteLine("/os-set                  Perform OS exploration (based on DFS) of the state space, with queue set  abstraction");
            Console.WriteLine("/queue-bound:k           Bound queue size to k (i.e. a machine's send is disabled when its current buffer is size k) (default: 0=unbounded for DFS, 1 for OS).");
            Console.WriteLine("                         In case of /os search, this bound applies to the first-round queue and is incremented subsequently.");
            Console.WriteLine("/queue-prefix:p          Keep prefix of queue of length p(>=0) /exact/ (abstraction applies to suffix starting at position p) (default: 0)");
            Console.WriteLine("/interactive             Interactive mode: need users to press button to increase queue bound and continue exploration");
            Console.WriteLine("/file-dump               Pretty-print accumulated states into files. For debugging only; this may create LARGE files!");
            Console.WriteLine();
            Console.WriteLine("Flags related to QuTL model checker:");
            Console.WriteLine("/qutl:formula            Use QuTL formula to specify the properties what states should satisfy");
            Console.WriteLine("/queue:events            Model checking queue; using '|' to specify abstract queue, e.g. BB|ABC");
            Console.WriteLine("/processing:event        Specify the processing event");
            Console.WriteLine("/concrete                Invoke concrete model checker to test the correctness of QuTL formula!");
            Console.WriteLine();
            Console.WriteLine("Flags related to efficiency:");
            Console.WriteLine("/symmetry                Apply symmetry reduction to exclude redundant reachable states!");
            // Console.WriteLine("/hash                    Use State Hashing. (DFS without State Hashing is currently not implemented (and probably not meaningful), hence /dfs and /os-... all imply /hash.)");
            Console.WriteLine();
            Console.WriteLine("If none of /psharp, /dfs, /os-... are specified: perform random testing");
        }

        public static void Main(string[] args)
        {

            if (args.Length > 0)
                if (args[0] == "!")     // scratch space for quick testing
                {
                    Environment.Exit(0);
                }

            var options = ParseCommandLine(args);
            if (options == null)
            {
                Environment.Exit((int)TestResult.InvalidParameters);
            }

            if (options.UsePSharp && options.isRefinement)
            {
                Console.WriteLine("Error: Refinement checking isn't yet supported with /psharp flag");
                Environment.Exit((int)TestResult.InvalidParameters);
            }


            if (options.testMCer)
            {
                P.Runtime.TestMCer tester = new TestMCer();
                tester.Testing();
                return;
            }

            if (options.isRefinement)
            {
                var refinementCheck = new RefinementChecking(options);
                if (options.LHSModel == null)
                {
                    refinementCheck.RunCheckerRHS();
                }
                else
                {
                    refinementCheck.RunCheckerLHS();
                }
                return;
            }

            /// --PL: load the assembly (.dll file, a P application) with the given file name
            var asm = Assembly.LoadFrom(options.inputFileName);
            /// --PL: create an instance of P program state
            var s = (StateImpl)asm.CreateInstance("P.Program.Application",
                                                        false,
                                                        BindingFlags.CreateInstance,
                                                        null,
                                                        new object[] { true },
                                                        null,
                                                        new object[] { });

            //StateImpl s = new P.Program.Application(true);

            if (s == null)
                throw new ArgumentException("Invalid assembly");

            DfsExploration.start = s;

            ///-- PL: stopwatch: to measure the elapsed time
            var stopwatch = new System.Diagnostics.Stopwatch();
            stopwatch.Start();
            /// run PTester in an interactive
            if (options.interactive)
            {
                DfsExploration.interactiveMode = true;
            }

            try
            {
                if (options.UsePSharp)
                {
                    PSharpExploration.RunPSharpTester(s);
                }
                else if (options.DfsExploration)
                {
                    DfsExploration.Dfs();                  // Normal DFS exploration from s
                }
                else if (options.OSList || options.OSSet)
                {
                    var ret = DfsExploration.OSIterate(); // Verification using OS with list or set abstraction; exploration starts with s, 
                    if (ret == 0)
                    {
                        Console.WriteLine("Exiting...");
                    }
                }
                else
                {
                    RandomExploration.Explore(s, options);
                }
                Console.WriteLine("Time elapsed: {0} seconds", stopwatch.Elapsed.TotalSeconds.ToString("F2"));
            }
            catch (P.Runtime.PrtInfiniteRaiseLoop inf)
            {
                Console.WriteLine(inf.ToString());
            }
            catch (PrtMaxEventInstancesExceededException e)
            {
                Console.WriteLine(e.ToString());
            }
            finally
            {
                Console.WriteLine("Time elapsed: {0} seconds", stopwatch.Elapsed.TotalSeconds.ToString("F2"));
            }
        }
    }
}
