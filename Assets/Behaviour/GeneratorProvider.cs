using Assets.Generation.Gen;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Behaviour
{
    // just as a way to allow other behaviours look for who provides the non-behaviour
    // Generator (or be assigned it in the inspector)
    public class GeneratorProvider : MonoBehaviour
    {
        public virtual Generator GetGenerator()
        {
            return null;
        }
    }
}
