using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

// I cannot make Unity Debug.Assert(s throw to the debugger in any circumstances, but exceptions do go to the debugger
// so write my own that always throws
namespace Assets.Generation.U
{
    static class Assertion
    {
        //public class AssertionFailed : Exception
        //{
        //    public AssertionFailed() : base("Did not add a message, you'll have to debug it...") { }
        //}

        [System.Diagnostics.Conditional("UNITY_ASSERTIONS")]
        [System.Diagnostics.DebuggerStepThrough]
        public static void Assert(bool b)
        {
            if (!b)
            {
                // looks like the debugger won't break on unhandled user exceptions???
                throw new NotSupportedException();
            }
        }
    }
}
