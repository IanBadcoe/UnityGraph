using Assets.Generation.G;
using Assets.Generation.GeomRep;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.Behaviour
{
    // just as a way to allow other behaviours look for who provides the non-behaviour
    // data objects like graphs and loops
    //
    // probably not a permanent way forward, but handy for wiring up test scenes

    public class DataProvider : MonoBehaviour
    {
        public virtual Graph GetGraph()
        {
            return null;
        }

        public virtual IReadOnlyList<Loop> GetLoops()
        {
            return null;
        }

    }
}
