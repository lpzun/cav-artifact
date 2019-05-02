using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace P.Runtime
{
    /// <summary>
    /// Queue temporal logic tokens (QuTL)
    ///   tokens used in QuTL
    /// </summary>
    static class Symbol
    {
        /// <summary>
        /// current processing event
        /// </summary>
        public static readonly string PROCESS_EVENT = "$";
        /// <summary>
        ///  temporal operators
        ///   F: future
        ///   X: next
        ///   G: always
        /// </summary>
        public static readonly string TMP_F = "F";
        public static readonly string TMP_X = "X";
        public static readonly string TMP_G = "G";
        /// <summary>
        /// queue operators
        ///   #: counting operator, to count the nubmer of the event in the queue
        ///   #Q: the size of queue
        /// </summary>
        public static readonly string COUNT = "#";
        public static readonly string SIZE = "#Q";
        /// <summary>
        /// Boolean logic operators
        ///   ~: negation
        ///   &: logic AND
        ///   |: logic OR
        ///   => implication
        /// </summary>
        public static readonly string NEGATION = "~";
        public static readonly string AND = "&";
        public static readonly string OR = "|";
        public static readonly string IMPLICATION = "=>";
        /// <summary>
        /// relation operators
        ///    =: equal
        ///   !=: not equal
        ///    <: less than
        ///   <=: less than or equal to
        ///    >: greater than
        ///   >=: greater than or equal to
        /// </summary>
        public static readonly string EQUAL = "=";
        public static readonly string NOT_EQUAL = "!=";
        public static readonly string LESS_THAN = "<";
        public static readonly string LESS_THAN_EQ = "<=";
        public static readonly string GREATER_THAN = ">";
        public static readonly string GREATER_THAN_EQ = ">=";
        /// <summary>
        /// arithmetic operators
        ///   +: addition
        ///   -: minus
        ///  (): parenthesis
        /// </summary>
        public static readonly string ADDITION = "+";
        public static readonly string SUBTRACTION = "-";
        public static readonly string PARENTHSIS = "()";
        /// <summary>
        /// constant bool value:
        /// -- true
        /// -- false
        /// </summary>
        public static readonly string CONST_T = "true";
        public static readonly string CONST_F = "false";

        public static readonly int NON_DET_INT = int.MaxValue;
    }

    /// <summary>
    /// The types of AST node:
    ///  1. temporal operator
    ///  2. event
    ///  3. value: an integer value
    /// </summary>
    public enum AstNodeType
    {
        OPERATOR = 0,
        EVENT = 1,
        VALUE = 2
    }

    /// <summary>
    /// Queue temporal logic operators
    /// </summary>
    public enum QuTLOperator
    {
        /// <summary>
        /// Current processin event
        /// </summary>
        PROCESS_EVENT = 0,
        /// <summary>
        ///  temporal operators
        /// </summary>
        TMP_F,
        TMP_X,
        TMP_G,
        /// <summary>
        /// queue operators
        /// </summary>
        COUNT,
        SIZE,
        /// <summary>
        /// 
        /// </summary>
        NEGATION,
        AND,
        OR,
        IMPLICATION,
        /// <summary>
        /// relation operators
        /// </summary>
        EQUAL,
        NOT_EQUAL,
        LESS_THAN,
        LESS_THAN_EQ,
        GREATER_THAN,
        GREATER_THAN_EQ,
        /// <summary>
        /// arithmetic operators
        /// </summary>
        ADDITION,
        SUBTRACTION,
        PARENTHSIS,
        /// <summary>
        /// constant bool value:
        /// -- true
        /// -- false
        /// </summary>
        CONST_T,
        CONST_F,
        /// <summary>
        /// 
        /// </summary>
        NULLOP
    }

    /// <summary>
    /// A class to define a experssion node. 
    /// The node could be the following types:
    ///   -- QuTL operator,
    ///   -- event, or
    ///   -- constant
    /// </summary>
    public class ExprNode
    {
        #region Constructors
        public ExprNode(QuTLOperator op)
        {
            this.type = AstNodeType.OPERATOR;
            this.op = op;
            this.ev = null;
            this.vl = int.MinValue;
        }
        public ExprNode(string ev)
        {
            this.type = AstNodeType.EVENT;
            this.op = QuTLOperator.NULLOP;
            this.ev = ev;
            this.vl = int.MinValue;
        }
        public ExprNode(int vl)
        {
            this.type = AstNodeType.VALUE;
            this.op = QuTLOperator.NULLOP;
            this.ev = null;
            this.vl = vl;
        }
        #endregion

        #region Fields
        private AstNodeType type;
        private QuTLOperator op;
        private String ev;
        private int vl; // value

        public AstNodeType Type { get => type; set => type = value; }
        public QuTLOperator Operator { get => op; set => op = value; }
        public string Event { get => ev; set => ev = value; }
        public int Value { get => vl; set => vl = value; }
        #endregion

        /// <summary>
        /// A override method
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            StringBuilder sb = new StringBuilder("");
            switch (type)
            {
                case AstNodeType.EVENT:
                    sb.Append(ev);
                    break;
                case AstNodeType.VALUE:
                    sb.Append(vl);
                    break;
                default:
                    {
                        switch (op)
                        {
                            case QuTLOperator.PROCESS_EVENT:
                                sb.Append(Symbol.PROCESS_EVENT);
                                break;
                            case QuTLOperator.TMP_F:
                                sb.Append(Symbol.TMP_F);
                                break;
                            case QuTLOperator.TMP_G:
                                sb.Append(Symbol.TMP_G);
                                break;
                            case QuTLOperator.TMP_X:
                                sb.Append(Symbol.TMP_X);
                                break;
                            case QuTLOperator.COUNT:
                                sb.Append(Symbol.COUNT);
                                break;
                            case QuTLOperator.SIZE:
                                sb.Append(Symbol.SIZE);
                                break;
                            case QuTLOperator.NEGATION:
                                sb.Append(Symbol.NEGATION);
                                break;
                            case QuTLOperator.AND:
                                sb.Append(Symbol.AND);
                                break;
                            case QuTLOperator.OR:
                                sb.Append(Symbol.OR);
                                break;
                            case QuTLOperator.IMPLICATION:
                                sb.Append(Symbol.IMPLICATION);
                                break;
                            case QuTLOperator.EQUAL:
                                sb.Append(Symbol.EQUAL);
                                break;
                            case QuTLOperator.NOT_EQUAL:
                                sb.Append(Symbol.NOT_EQUAL);
                                break;
                            case QuTLOperator.LESS_THAN:
                                sb.Append(Symbol.LESS_THAN);
                                break;
                            case QuTLOperator.LESS_THAN_EQ:
                                sb.Append(Symbol.LESS_THAN_EQ);
                                break;
                            case QuTLOperator.GREATER_THAN:
                                sb.Append(Symbol.GREATER_THAN);
                                break;
                            case QuTLOperator.GREATER_THAN_EQ:
                                sb.Append(Symbol.GREATER_THAN_EQ);
                                break;
                            case QuTLOperator.ADDITION:
                                sb.Append(Symbol.ADDITION);
                                break;
                            case QuTLOperator.SUBTRACTION:
                                sb.Append(Symbol.SUBTRACTION);
                                break;
                            case QuTLOperator.PARENTHSIS:
                                break;
                            default:
                                sb.Append("op");
                                break;
                        }
                    }
                    break;
            }
            return sb.ToString();
        }
    }

    /// <summary>
    /// A node for Abstract syntax tree
    /// </summary>
    public class AstNode
    {
        #region Constructors
        public AstNode(ExprNode node)
        {
            this.enode = node;
            this.left = null;
            this.right = null;
        }
        #endregion

        #region Fields
        public ExprNode enode;
        public AstNode left;
        public AstNode right;
        #endregion
    }

    /// <summary>
    /// Parser for QuTL formulae
    /// Note: Current version only works for Reverse Polish Notation (RPN).
    ///       We will improve this in the future. 
    /// </summary>
    public class QuTLParser
    {
        #region
        public QuTLParser(string expr)
        {
            var sformulae = expr.Split(':');
            if (sformulae.Length != 0)
            {
                formulae = new AstNode[sformulae.Length];
                for (int i = 0; i < sformulae.Length; ++i)
                {
                    formulae[i] = BuildAst(sformulae[i]);
                }
            }
        }
        #endregion

        #region fields
        //private readonly List<ExprNode> phi;
        public static AstNode[] formulae;
        #endregion

        public AstNode BuildAst(string expr)
        {
            return BuildAst(BuildExprNode(expr));
        }

        private AstNode BuildAst(List<ExprNode> phi)
        {
            Stack<AstNode> worklist = new Stack<AstNode>();
            foreach (var node in phi)
            {
                switch (node.Type)
                {
                    case AstNodeType.EVENT:
                        worklist.Push(new AstNode(node));
                        break;
                    case AstNodeType.VALUE:
                        worklist.Push(new AstNode(node));
                        break;
                    default:
                        BuildAst(node, ref worklist);
                        break;
                }
            }
            return worklist.Peek();
        }

        /// <summary>
        /// A method to build an abstract syntax tree from input RPN
        /// </summary>
        /// <param name="node">the root of the tree</param>
        /// <param name="worklist"></param>
        /// <returns></returns>
        private AstNode BuildAst(ExprNode node, ref Stack<AstNode> worklist)
        {
            var curr = new AstNode(node);
            switch (node.Operator)
            {
                case QuTLOperator.PROCESS_EVENT:
                    {
                        worklist.Push(curr);
                    }
                    break;
                case QuTLOperator.TMP_F:
                case QuTLOperator.TMP_G:
                case QuTLOperator.TMP_X:
                    {
                        var lch = worklist.Pop();
                        curr.left = lch;
                        worklist.Push(curr);
                    }
                    break;
                case QuTLOperator.COUNT:
                    {
                        var lch = worklist.Pop();
                        curr.left = lch;
                        worklist.Push(curr);
                    }
                    break;
                case QuTLOperator.SIZE:
                    {
                        worklist.Push(curr);
                    }
                    break;
                case QuTLOperator.NEGATION:
                    {
                        var lch = worklist.Pop();
                        curr.left = lch;
                        worklist.Push(curr);
                    }
                    break;
                case QuTLOperator.AND:
                case QuTLOperator.OR:
                case QuTLOperator.IMPLICATION:
                case QuTLOperator.EQUAL:
                case QuTLOperator.NOT_EQUAL:
                case QuTLOperator.LESS_THAN:
                case QuTLOperator.LESS_THAN_EQ:
                case QuTLOperator.GREATER_THAN:
                case QuTLOperator.GREATER_THAN_EQ:
                case QuTLOperator.ADDITION:
                case QuTLOperator.SUBTRACTION:
                    {
                        var rch = worklist.Pop();
                        curr.right = rch;

                        var lch = worklist.Pop();
                        curr.left = lch;

                        worklist.Push(curr);
                    }
                    break;
                case QuTLOperator.PARENTHSIS:
                    {
                        var lch = worklist.Pop();
                        curr.left = lch;
                        worklist.Push(curr);
                    }
                    break;
                default:
                    throw new Exception("Unknow QuTL operator!");

            }
            return null;
        }

        /// <summary>
        /// A method to parse an input RPN and convert RPN into a list of ExprNode
        /// </summary>
        /// <param name="expr">an input RPN</param>
        /// <returns></returns>
        private List<ExprNode> BuildExprNode(string expr)
        {
            var eArray = expr.Split(null);
            List<ExprNode> phi = new List<ExprNode>();
            foreach (var op in eArray)
            {
                ExprNode node = null;
                if (op == Symbol.PROCESS_EVENT)
                {
                    node = new ExprNode(QuTLOperator.PROCESS_EVENT);
                }
                else if (op == Symbol.TMP_F)
                {
                    node = new ExprNode(QuTLOperator.TMP_F);
                }
                else if (op == Symbol.TMP_X)
                {
                    node = new ExprNode(QuTLOperator.TMP_X);
                }
                else if (op == Symbol.TMP_G)
                {
                    node = new ExprNode(QuTLOperator.TMP_G);
                }
                else if (op == Symbol.COUNT)
                {
                    node = new ExprNode(QuTLOperator.COUNT);
                }
                else if (op == Symbol.SIZE)
                {
                    node = new ExprNode(QuTLOperator.SIZE);
                }
                else if (op == Symbol.NEGATION)
                {
                    node = new ExprNode(QuTLOperator.NEGATION);
                }
                else if (op == Symbol.AND)
                {
                    node = new ExprNode(QuTLOperator.AND);
                }
                else if (op == Symbol.OR)
                {
                    node = new ExprNode(QuTLOperator.OR);
                }
                else if (op == Symbol.IMPLICATION)
                {
                    node = new ExprNode(QuTLOperator.IMPLICATION);
                }
                else if (op == Symbol.EQUAL)
                {
                    node = new ExprNode(QuTLOperator.EQUAL);
                }
                else if (op == Symbol.NOT_EQUAL)
                {
                    node = new ExprNode(QuTLOperator.NOT_EQUAL);
                }
                else if (op == Symbol.LESS_THAN)
                {
                    node = new ExprNode(QuTLOperator.LESS_THAN);
                }
                else if (op == Symbol.LESS_THAN_EQ)
                {
                    node = new ExprNode(QuTLOperator.LESS_THAN_EQ);
                }
                else if (op == Symbol.GREATER_THAN)
                {
                    node = new ExprNode(QuTLOperator.GREATER_THAN);
                }
                else if (op == Symbol.GREATER_THAN_EQ)
                {
                    node = new ExprNode(QuTLOperator.GREATER_THAN_EQ);
                }
                else if (op == Symbol.ADDITION)
                {
                    node = new ExprNode(QuTLOperator.ADDITION);
                }
                else if (op == Symbol.SUBTRACTION)
                {
                    node = new ExprNode(QuTLOperator.SUBTRACTION);
                }
                else if (op == Symbol.PARENTHSIS)
                {
                    node = new ExprNode(QuTLOperator.PARENTHSIS);
                }
                else if (op == Symbol.CONST_T)
                {
                    node = new ExprNode(1);
                }
                else if (op == Symbol.CONST_F)
                {
                    node = new ExprNode(0);
                }
                else if (op.All(char.IsDigit))
                {
                    node = new ExprNode(int.Parse(op));
                }
                else
                {
                    node = new ExprNode(op);
                }
                phi.Add(node);
            }

            //Console.WriteLine("OUTPUT Node:");
            //phi.ForEach(node => Console.Write(node.ToString() + " "));
            //Console.WriteLine();

            return phi;
        }

        public static void Print()
        {
            for (int i = 0; i < formulae.Length; i++)
            {
                Console.Write(i +": ");
                Print(formulae[i]);
                Console.WriteLine();
            }
        }
        /// <summary>
        ///  For testing only: print the qutl formula which is organized 
        ///  as an AST. 
        /// </summary>
        /// <param name="root"></param>
        public static void Print(AstNode root)
        {
            if (root == null)
                return;
            var node = root.enode;
            bool isPar = node.Type == AstNodeType.OPERATOR
                        && node.Operator == QuTLOperator.PARENTHSIS;
            if (isPar)
                Console.Write("(");

            if (root.right != null)
            {
                Print(root.left);
                Console.Write(" " + node.ToString() + " ");
                Print(root.right);
            }
            else
            {
                Console.Write(node.ToString() + " ");
                Print(root.left);
            }
            if (isPar)
                Console.Write(")");
        }

    }

    /// <summary>
    /// A state of Labeled transition systems (LTS).
    /// </summary>
    class State
    {
        #region Constructors
        public State(int id, string label, bool selfloop = false)
        {
            this.id = id;
            this.labels = new HashSet<string> { label };
            this.selfLoop = selfloop;
        }

        public State(int id, HashSet<string> labels, bool selfloop = false)
        {
            this.id = id;
            this.labels = labels;
            this.selfLoop = selfloop;
        }
        #endregion

        #region Fileds
        public int id;
        public HashSet<string> labels;

        private readonly bool selfLoop;
        public bool SelfLoop => selfLoop;
        #endregion

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("s" + id + ": " + labels.Count + ": ");
            sb.Append(string.Join(" ", labels.ToArray()));
            return sb.ToString();
        }
    }

    /// <summary>
    /// Labeled transition systems (LTS). We constructed it from a queue
    /// </summary>
    class LTS
    {
        #region Constructors
        public LTS(List<string> Q)
        {
            this.BuilLTS(0, Q);
        }

        public LTS(int p, List<string> Q)
        {
            this.BuilLTS(p, Q);
            this.size = p == Q.Count ? Q.Count : int.MaxValue;
        }
        #endregion

        #region Fields
        public State[] states;
        public List<int>[] transitions;
        public int initial;
        public int accepting;
        private readonly int size;

        public int SizeQ => size;
        #endregion

        /// <summary>
        /// build the labelled transition system from an abstract queue
        /// </summary>
        /// <param name="p">the prefix of abstract queue </param>
        /// <param name="Q">the content of abstract queue</param>
        private void BuilLTS(int p, List<string> Q)
        {
            #region Initialization
            /// the number of events in the queue
            /// Console.WriteLine(p + "<" + Q.Count);
            int k = Q.Count;
            int numOfStates = p + 2 * (k - p) + 1;
            if (numOfStates < 0)
                throw new QuTLException("LTS construction failed: the umber of states can never be negative!");
            this.transitions = new List<int>[numOfStates];
            this.states = new State[numOfStates];
            for (int i = 0; i < numOfStates; ++i)
            {
                transitions[i] = new List<int>();
                states[i] = null;
            }
            #endregion

            #region Handle non-kleene star parts
            int stateID = 0, predID = 0; /// state stateID, increment
            this.initial = stateID;
            for (int i = 0; i <= Q.Count; ++i)
            {
                states[stateID] = new State(stateID, i < Q.Count ? Q[i] : "");
                if (i > 0)
                    transitions[predID].Add(stateID);
                predID = stateID;
                stateID += i < p ? 1 : 2;
            }
            this.accepting = numOfStates - 1; /// set up the accepting state
            #endregion

            #region Handle kleene star parts
            stateID = p + 1; /// 
            for (int i = p; i < Q.Count; ++i)
            {
                HashSet<string> labels = new HashSet<string>();
                for (int j = p; j <= i; ++j)
                {
                    labels.Add(Q[j]);
                }

                states[stateID] = new State(stateID, labels, true);
                transitions[stateID - 1].Add(stateID);
                transitions[stateID].Add(stateID); // self loop
                transitions[stateID].Add(stateID + 1);

                stateID += 2;
            }
            #endregion
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            StringBuilder sb = new StringBuilder("");
            for (int i = 0; i < states.Count(); ++i)
            {
                sb.Append(states[i].ToString());
                if (transitions[i] != null)
                    transitions[i].ForEach(s => sb.Append(" " + s));
                sb.Append("\n");
            }
            return sb.ToString();
        }
    }

    /// <summary>
    /// Abstract QuTL model checker
    /// </summary>
    public class AbstractChecker
    {
        #region Constructors
        public AbstractChecker()
        {

        }
        #endregion

        #region Out-of-date code
        public static bool Check(int currIndex, string last, List<PrtEventNode> Q)
        {
#if true
            List<string> sQ = new List<string>();
            foreach (var e in Q)
            {
                sQ.Add(e.ev.ToString());
            }
            return Check(currIndex, last, P.Runtime.PrtEventBuffer.p, sQ);
#endif

#if true
            Console.WriteLine("---------- I am in QuTL model checker");
            // can't have just dequeued DONE and then there are still DONE's in the queue
            if (last == "DONE" && Q.Find(e => e.ev.ToString() == "DONE") != null)
            {
                Console.WriteLine("-------- state invariant is inviolated");
                return false;
            }

            for (int i = 0; i < Q.Count - 1; ++i)
            {
                var curr = Q[i].ev.ToString();
                var next = Q[i + 1].ev.ToString();

                // PING cannot be followed by WAIT
                if (curr == "PING" && next == "WAIT")
                {
                    return false;
                }
            }
#endif

#if false
            PrtImplMachine  Main  = implMachines[0]; 
            Debug.Assert( Main .eventQueue.is_abstract()); 
            List<PrtEventNode>  Main_q  =  Main .eventQueue.events;
            PrtImplMachine Client = implMachines[1]; 
            Debug.Assert(Client.eventQueue.is_abstract()); 
            List<PrtEventNode> Client_q = Client.eventQueue.events;

            Event_is_and_queue_contains event_is_and_queue_contains = delegate(PrtImplMachine m, string s)
            {
                List<PrtEventNode> q = m.eventQueue.events;
                return m.get_eventValue().ToString() == s && q.Find(ev => ev.ev.ToString() == s) != null;
            };

            // these lemmas state that none of the events in question occur more than once in the queue
            if (event_is_and_queue_contains(Main,   "req_share")      ||
                event_is_and_queue_contains(Main,   "req_excl")       ||
                event_is_and_queue_contains(Main,   "invalidate_ack") ||

                event_is_and_queue_contains(Client, "grant_share")    ||
                event_is_and_queue_contains(Client, "grant_excl")     ||
                event_is_and_queue_contains(Client, "ask_share")      ||
                event_is_and_queue_contains(Client, "ask_excl")       ||
                event_is_and_queue_contains(Client, "invalidate"))
                return false;
#endif
            return true;
        }
        #endregion

        public static bool Check(int currIndex, int p, List<string> Q)
        {
            try
            {
                return Check(currIndex, null, p, Q);
            }
            catch (QuTLException e)
            {
                Console.WriteLine(e.Message);
            }
            return false;
        }

        /// <summary>
        /// Evaluate an abstract queue
        /// </summary>
        /// <param name="currIndex">a index to indicate to which P machine the Q belongs</param>
        /// <param name="ev">the processing event. If ev == null, then ther is no event being processed now</param>
        /// <param name="p">the starting index</param>
        /// <param name="Q">an abstract queue, will build a LTS based on it</param>
        /// <returns>bool: return true if phi is stisfiable, and false otherwise</returns>
        public static bool Check(int currIndex, string ev, int p, List<string> Q)
        {
            try
            {
                LTS lts = new LTS(p, Q);
#if false
            Console.WriteLine(lts.ToString());
#endif
                return Eval(ev, lts, 0, QuTLParser.formulae[currIndex]) == 1;
            }
            catch (QuTLException qe)
            {
                Console.WriteLine(qe.Message);
            }
            catch (InvalidOperationException ioe)
            {
                Console.WriteLine(ioe.Message);
            }
            return false;
        }

        /// <summary>
        /// A method to perform model checking: 
        ///    lts |= phi
        /// </summary>
        /// <param name="ev">the processing event. If ev == null, then ther is no event being processed now</param>
        /// <param name="lts">a LTS constructed from queue</param>
        /// <param name="start">the starting index</param>
        /// <param name="phi">a QuTL sub-formula</param>
        /// <returns>the intermediate result</returns>
        private static int Eval(string ev, LTS lts, int i, AstNode phi)
        {
            if (i == lts.accepting || phi == null)
                return 1;
            var root = phi.enode;
            switch (root.Type)
            {
                case AstNodeType.EVENT:
                    return lts.states[i].labels.Contains(root.Event) ? 1 : 0;
                case AstNodeType.VALUE:
                    return root.Value;
                default:
                    return EvalOp(ev, lts, i, phi);
            }
        }

        /// <summary>
        /// A method to perform model checking: 
        ///    lts |= phi
        /// </summary>
        /// <param name="ev">the processing event. If ev == null, then ther is no event being processed now</param>
        /// <param name="lts">a LTS constructed from queue</param>
        /// <param name="start">the starting index</param>
        /// <param name="phi">a QuTL sub-formula</param>
        /// <returns>the intermediate result</returns>
        private static int EvalOp(string ev, LTS lts, int start, AstNode phi)
        {
            switch (phi.enode.Operator)
            {
                case QuTLOperator.TMP_F:
                    {
                        for (int i = start; i < lts.accepting - 1; ++i)
                        {
                            if (Eval(ev, lts, i, phi.left) == 1)
                                return 1;
                        }
                        return 0;
                    }
                case QuTLOperator.TMP_G:
                    {
                        return EvalG(ev, lts, start, phi.left);
                    }
                case QuTLOperator.TMP_X:
                    {
                        foreach (var next in lts.transitions[start])
                        {
                            if (Eval(ev, lts, next, phi.left) == 1)
                                return 1;
                        }
                        return 0;
                    }
                case QuTLOperator.COUNT:
                    {
                        if (phi.left == null || phi.left.enode.Type != AstNodeType.EVENT)
                            throw new QuTLException("Illegal formula: counting");
                        return EvalCount(phi.left.enode.Event, lts, start).Item1;
                    }
                case QuTLOperator.SIZE:
                    {
                        return lts.SizeQ;
                    }
                case QuTLOperator.NEGATION:
                    {
                        var res = EvalNegation(ev, lts, start, phi);
                        return res;
                    }
                case QuTLOperator.AND:
                    {
                        var lch = Eval(ev, lts, start, phi.left);
                        var rch = Eval(ev, lts, start, phi.right);
                        return lch & rch;
                    }
                case QuTLOperator.OR:
                    {
                        var lch = Eval(ev, lts, start, phi.left);
                        var rch = Eval(ev, lts, start, phi.right);
                        return lch | rch;
                    }
                case QuTLOperator.IMPLICATION:
                    {
                        var lch = Eval(ev, lts, start, phi.left);
                        var rch = Eval(ev, lts, start, phi.right);
                        return (lch ^ 1) | rch;
                    }
                case QuTLOperator.EQUAL:
                case QuTLOperator.NOT_EQUAL:
                case QuTLOperator.LESS_THAN:
                case QuTLOperator.LESS_THAN_EQ:
                case QuTLOperator.GREATER_THAN:
                case QuTLOperator.GREATER_THAN_EQ:
                    {
                        if (phi.left == null || phi.right == null)
                            throw new QuTLException("Illegal QuTL formula!");
                        if (phi.left.enode.Operator != QuTLOperator.COUNT
                            && phi.left.enode.Operator != QuTLOperator.PROCESS_EVENT)
                            throw new QuTLException("Illegal QuTL formula!");
                        if (phi.left.enode.Operator == QuTLOperator.PROCESS_EVENT)
                        {
                            return EvalProcessEvent(ev, phi) ? 1 : 0;
                        }
                        else
                        {
                            var lch = phi.left.left.enode.Event;
                            var rch = Eval(ev, lts, start, phi.right);
                            return EvalComp(lch, rch, phi.enode.Operator, lts, start);
                        }
                    }
                case QuTLOperator.ADDITION:
                    {
                        var lch = Eval(ev, lts, start, phi.left);
                        var rch = Eval(ev, lts, start, phi.right);
                        return lch + rch;
                    }
                case QuTLOperator.SUBTRACTION:
                    {
                        var lch = Eval(ev, lts, start, phi.left);
                        var rch = Eval(ev, lts, start, phi.right);
                        return lch - rch;
                    }
                case QuTLOperator.PARENTHSIS:
                    {
                        return Eval(ev, lts, start, phi.left);
                    }
                case QuTLOperator.PROCESS_EVENT:
                    {
                        return phi.left.enode.Event == ev ? 1 : 0;
                    }
                default:
                    throw new QuTLException("Illegal operator");
            }
        }

        /// <summary>
        /// Assume $ always in the left child
        /// </summary>
        /// <param name="ev">the Processing event. If ev == null, then ther is no event being processed now</param>
        /// <param name="phi">a QuTL formula</param>
        /// <returns>bool: return true if phi is stisfiable, and false otherwise</returns>
        private static bool EvalProcessEvent(string ev, AstNode phi)
        {
            //Console.Write("Processing event: " + ev);
            if (ev == null)
                return false;
            switch (phi.enode.Operator)
            {
                case QuTLOperator.EQUAL:
                    {
                        //Console.WriteLine(" = " + phi.right.enode.Event);
                        return ev == phi.right.enode.Event;
                    }
                case QuTLOperator.NOT_EQUAL:
                    {
                        //Console.WriteLine(" != " + phi.right.enode.Event);
                        return ev != phi.right.enode.Event;
                    }
                default:
                    throw new QuTLException("Unsupported opertator");
            }
        }

        private static int EvalG(string ev, LTS lts, int start, AstNode phi)
        {
            if (Eval(ev, lts, start, phi) == 0)
                return 0;
            foreach (var next in lts.transitions[start])
            {
                //Console.WriteLine(next);
                if (next == start)
                    continue;
                if (next == lts.accepting || EvalG(ev, lts, next, phi) == 1)
                    return 1;
            }
            return 0;
        }

        /// <summary>
        /// A method to evaluate negation of a formula
        ///    lts |= phi
        /// </summary>
        /// <param name="ev">the processing event. If ev == null, then ther is no event being processed now</param>
        /// <param name="lts">a LTS constructed from queue</param>
        /// <param name="start">the starting index</param>
        /// <param name="phi">a QuTL sub-formula</param>
        /// <returns>the intermediate result</returns>
        private static int EvalNegation(string ev, LTS lts, int start, AstNode phi)
        {
            if (phi.enode.Type != AstNodeType.EVENT)
            {
                return Eval(ev, lts, start, phi.left) ^ 1;
            }
            else
            {
                return lts.states[start].labels.Contains(phi.enode.Event) ? 0 : 1;
            }
        }


        /// <summary>
        /// Not a good design, need to revisit
        /// </summary>
        /// <param name="ev"></param>
        /// <param name="c"></param>
        /// <param name="op"></param>
        /// <returns></returns>
        private static int EvalComp(string ev, int c, QuTLOperator op, LTS lts, int start)
        {
            var pair = EvalCount(ev, lts, start);
            var cnt = pair.Item1;
            var loop = pair.Item2;
            switch (op)
            {
                case QuTLOperator.EQUAL:
                    return (cnt == c || (cnt < c && loop)) ? 1 : 0;
                case QuTLOperator.NOT_EQUAL:
                    return (cnt > c || (cnt < c && !loop)) ? 1 : 0;
                case QuTLOperator.LESS_THAN:
                    return (cnt < c) ? 1 : 0;
                case QuTLOperator.LESS_THAN_EQ:
                    return (cnt <= c) ? 1 : 0;
                case QuTLOperator.GREATER_THAN:
                    return (loop || cnt > c) ? 1 : 0;
                case QuTLOperator.GREATER_THAN_EQ:
                    return (loop || cnt >= c) ? 1 : 0;
                default:
                    throw new QuTLException("Illegal binary relation operation");
            }
        }

        /// <summary>
        /// Evaluate the counting operation
        /// </summary>
        /// <param name="ev">the event need to count</param>
        /// <param name="lts">the labeled transition system</param>
        /// <param name="stateID">the starting point to do the counting operation</param>
        /// <returns></returns>
        private static Tuple<int, bool> EvalCount(string ev, LTS lts, int stateID)
        {
            int cnt = 0;
            bool inf = false;
            for (int i = stateID; i < lts.accepting; ++i)
            {
                if (lts.states[i].labels.Contains(ev))
                {
                    if (lts.states[i].SelfLoop)
                    {
                        if (!inf)
                            inf = true;
                    }
                    else
                    {
                        ++cnt;
                    }
                }
            }
            return System.Tuple.Create(cnt, inf);
        }
    }

    /// <summary>
    /// Concrete QuTL model checker
    /// </summary>
    public class ConcreteChecker
    {
        #region Constructors
        public ConcreteChecker()
        {

        }
        #endregion
        public static bool Check(int currIndex, string last, List<PrtEventNode> Q)
        {
            List<string> sQ = new List<string>();
            foreach (var e in Q)
            {
                sQ.Add(e.ev.ToString());
            }
            return Check(currIndex, last, sQ);
        }
        /// <summary>
        /// Implement the concrete QuTL model checker
        /// </summary>
        /// <param name="ev">the processing event: it has just been dequeued</param>
        /// <param name="Q"> the current queue</param>
        /// <returns></returns>
        public static bool Check(int currIndex, string ev, List<string> Q)
        {
#if false // for debugging
            foreach (var e in Q)
                Console.Write("===" + e);
            Console.WriteLine("Q is empty...");
#endif
            if (currIndex >= QuTLParser.formulae.Length)
                throw new QuTLException("Formulae is out of bound!");
            return Eval(ev, Q, 0, QuTLParser.formulae[currIndex]) == 1;
        }

        /// <summary>
        /// Recursive version of QuTL model checker
        /// </summary>
        /// <param name="ev">the processing event</param>
        /// <param name="Q">current queue</param>
        /// <param name="i">current index of queue: mark the current progress of evaluation</param>
        /// <param name="phi">the whole fomula of phi</param>
        /// <returns></returns>
        private static int Eval(string ev, List<string> Q, int i, AstNode phi)
        {
            /// empty queue
            if (i == Q.Count || phi == null)
                return StateImpl.concrete ? 1 : 0;
            var root = phi.enode;
            switch (root.Type)
            {
                case AstNodeType.EVENT:
                    return root.Event == Q[i] ? 1 : 0;
                case AstNodeType.VALUE:
                    return root.Value;
                default:
                    return EvalOp(ev, Q, i, phi);
            }
        }

        /// <summary>
        /// A method to perform model checking: 
        ///    lts |= phi
        /// </summary>
        /// <param name="ev">the processing event. If ev == null, then ther is no event being processed now</param>
        /// <param name="lts">a LTS constructed from queue</param>
        /// <param name="start">the starting index</param>
        /// <param name="phi">a QuTL sub-formula</param>
        /// <returns>the intermediate result</returns>
        private static int EvalOp(string ev, List<string> Q, int start, AstNode phi)
        {
            switch (phi.enode.Operator)
            {
                case QuTLOperator.TMP_F:
                    {
                        for (int i = start; i < Q.Count; ++i)
                        {
                            if (Eval(ev, Q, i, phi.left) == 1)
                                return 1;
                        }
                        return 0;
                    }
                case QuTLOperator.TMP_G:
                    {
                        for (int i = start; i < Q.Count; ++i)
                        {
                            if (Eval(ev, Q, i, phi.left) == 0)
                                return 0;
                        }
                        return 1;
                    }
                case QuTLOperator.TMP_X:
                    {
                        return Eval(ev, Q, start + 1, phi.left);
                    }
                case QuTLOperator.COUNT:
                    {
                        if (phi.left == null || phi.left.enode.Type != AstNodeType.EVENT)
                            throw new QuTLException("Illegal formula: counting");
                        return EvalCount(phi.left.enode.Event, Q, start);
                    }
                case QuTLOperator.SIZE:
                    {
                        return Q.Count;
                    }
                case QuTLOperator.NEGATION:
                    {
                        var lch = Eval(ev, Q, start, phi.left);
                        return lch ^ 1;
                    }
                case QuTLOperator.AND:
                    {
                        var lch = Eval(ev, Q, start, phi.left);
                        var rch = Eval(ev, Q, start, phi.right);
                        return lch & rch;
                    }
                case QuTLOperator.OR:
                    {
                        var lch = Eval(ev, Q, start, phi.left);
                        var rch = Eval(ev, Q, start, phi.right);
                        return lch | rch;
                    }
                case QuTLOperator.IMPLICATION:
                    {
                        var lch = Eval(ev, Q, start, phi.left);
                        var rch = Eval(ev, Q, start, phi.right);
                        return (lch ^ 1) | rch;
                    }
                case QuTLOperator.EQUAL:
                    {
                        if (phi.left.enode.Operator == QuTLOperator.PROCESS_EVENT)
                        {
                            return EvalProcessEvent(ev, phi) ? 1 : 0;
                        }
                        else
                        {
                            var lch = Eval(ev, Q, start, phi.left);
                            var rch = Eval(ev, Q, start, phi.right);
                            return lch == rch ? 1 : 0;
                        }
                    }
                case QuTLOperator.NOT_EQUAL:
                    {
                        if (phi.left.enode.Operator == QuTLOperator.PROCESS_EVENT)
                        {
                            return EvalProcessEvent(ev, phi) ? 1 : 0;
                        }
                        else
                        {
                            var lch = Eval(ev, Q, start, phi.left);
                            var rch = Eval(ev, Q, start, phi.right);
                            return lch != rch ? 1 : 0;
                        }
                    }
                case QuTLOperator.LESS_THAN:
                    {
                        var lch = Eval(ev, Q, start, phi.left);
                        var rch = Eval(ev, Q, start, phi.right);
                        return lch < rch ? 1 : 0;
                    }
                case QuTLOperator.LESS_THAN_EQ:
                    {
                        var lch = Eval(ev, Q, start, phi.left);
                        var rch = Eval(ev, Q, start, phi.right);
                        return lch <= rch ? 1 : 0;
                    }
                case QuTLOperator.GREATER_THAN:
                    {
                        var lch = Eval(ev, Q, start, phi.left);
                        var rch = Eval(ev, Q, start, phi.right);
                        return lch > rch ? 1 : 0;
                    }
                case QuTLOperator.GREATER_THAN_EQ:
                    {
                        var lch = Eval(ev, Q, start, phi.left);
                        var rch = Eval(ev, Q, start, phi.right);
                        return lch >= rch ? 1 : 0;
                    }
                case QuTLOperator.ADDITION:
                    {
                        var lch = Eval(ev, Q, start, phi.left);
                        var rch = Eval(ev, Q, start, phi.right);
                        return lch + rch;
                    }
                case QuTLOperator.SUBTRACTION:
                    {
                        var lch = Eval(ev, Q, start, phi.left);
                        var rch = Eval(ev, Q, start, phi.right);
                        return lch - rch;
                    }
                case QuTLOperator.PARENTHSIS:
                    {
                        return Eval(ev, Q, start, phi.left);
                    }
                case QuTLOperator.PROCESS_EVENT:
                    {
                        return phi.left.enode.Event == ev ? 1 : 0;
                    }
                default:
                    throw new QuTLException("Illegal operator");
            }
        }



        /// <summary>
        /// A method to evaluate the current processing event
        /// </summary>
        /// <param name="ev">the processing event. If ev == null, then ther is no event being processed now</param>
        /// <param name="phi">a QuTL sub-formula</param>
        /// <returns></returns>
        private static bool EvalProcessEvent(string ev, AstNode phi)
        {
            //Console.Write("Processing event: " + ev);
            if (ev == null)
                return false;
            switch (phi.enode.Operator)
            {
                case QuTLOperator.EQUAL:
                    {
                        //Console.WriteLine(" = " + phi.right.enode.Event);
                        return ev == phi.right.enode.Event;
                    }
                case QuTLOperator.NOT_EQUAL:
                    {
                        //Console.WriteLine(" != " + phi.right.enode.Event);
                        return ev != phi.right.enode.Event;
                    }
                default:
                    throw new QuTLException("Unsupported opertator");
            }
        }

        /// <summary>
        /// A method to evaluate count methods
        /// </summary>
        /// <param name="ev">the processing event. If ev == null, then ther is no event being processed now</param>
        /// <param name="Q">the concrete queue</param>
        /// <param name="start">starting index</param>
        /// <returns></returns>
        private static int EvalCount(string ev, List<string> Q, int start)
        {
            int cnt = 0;
            foreach (var e in Q)
            {
                if (e == ev)
                    ++cnt;
            }
            return cnt;
        }
    }

    /// <summary>
    /// QuTL exception
    /// </summary>
    public class QuTLException : Exception
    {
        public QuTLException()
        {
        }

        public QuTLException(string message) : base(message)
        {
        }

        public QuTLException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected QuTLException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }



    /// <summary>
    /// A class to test QutL Model Checker:
    ///   It contains a set of Unit Tests.
    /// </summary>
    public class TestMCer
    {
        public static string queueContent = null;
        public static string processEvent = null;

        public TestMCer()
        {
            this.p = 0;
            this.Q = null;
            this.isAbstract = false;
        }

        private int p;
        private List<string> Q;
        private bool isAbstract;

        public void Parse(string sQ)
        {
            if (sQ == null || sQ.Length == 0)
                this.Q = new List<string>();
            var A = sQ.Split('|');
            this.Q = A[0].Split('.').ToList();
            if (A.Length == 2)
            {
                this.p = Q.Count();
                isAbstract = true;
                foreach (var e in A[1].Split('.'))
                {
                    this.Q.Add(e);
                }
            }
        }

        public void Testing()
        {
            this.Parse(queueContent);
            bool checkResult = false;
            if (isAbstract)
            {
                Console.WriteLine("Abstract model checking...");
                checkResult = AbstractChecker.Check(0, processEvent, p, this.Q);
            }
            else
            {
                Console.WriteLine("Concrete model checking...");
                checkResult = ConcreteChecker.Check(0, processEvent, this.Q);
            }

            QuTLParser.Print();
            Console.WriteLine(checkResult ? " holds" : " does not hold");
        }
    }
}
