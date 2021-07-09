using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Generation.G
{
    interface IGraphRestore
    {
        bool Restore();
        bool CanBeRestored();
    }
}
