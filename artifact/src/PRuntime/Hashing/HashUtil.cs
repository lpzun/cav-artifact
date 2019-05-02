using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace P.Runtime
{
    /// <summary>
    /// A simple hashing utility
    /// </summary>
    public static class Hashing
    {
        /// <summary>
        /// Nullary hash (useful for iteration)
        /// </summary>
        /// <returns></returns>
        public static int Hash()
        {
            return 19;
        }

        public static int Hash(int value)
        {
            return Hash(Hash(), value);
        }

        public static int Hash(int value1, int value2)
        {
            return 31 * value1 + 17 * value2;
        }

        public static int Hash(params int[] values)
        {
            var ret = Hash();
            foreach (var v in values)
            {
                ret = Hash(ret, v);
            }
            ret = Hash(ret, values.Length);
            return ret;
        }

        public static int Hash(this IEnumerable<int> collection)
        {
            var ret = Hash();
            foreach (var v in collection)
            {
                ret = Hash(ret, v);
            }
            ret = Hash(ret, collection.Count());
            return ret;
        }

        /// <summary>
        /// Implemented for symmetry reduction
        /// </summary>
        /// <param name="collection"></param>
        /// <returns></returns>
        public static int Hash(this IEnumerable<int> collection, bool isSymmetryReduction)
        {
            var ret = Hash();
            if (!isSymmetryReduction)
            {
                Console.Write("N," + collection.Count() + ",");
                foreach (var v in collection)
                {
                    ret = Hash(ret, v);
                    Console.Write(v + ",");
                }
            }
            else
            {
                var list = collection.ToList();
                if (list.Count() >= 3)
                {
                    var hash1 = list[1];
                    var hash2 = list[2];
                    if (hash2 < hash1)
                    {
                        list[1] = hash2;
                        list[2] = hash1;
                    }

                }
                Console.Write("Y," + list.Count() + ",");
                foreach (var v in list)
                {
                    ret = Hash(ret, v);
                    Console.Write(v + ",");
                }
            }
            ret = Hash(ret, collection.Count());
            return ret;
        }
    }
}
