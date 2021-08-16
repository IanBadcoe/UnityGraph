using Assets.Generation.G;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Assets.Generation.Templates
{
    public interface IHMChild
    {
        public HierarchyMetadata Parent { get; set; }
        public IList<IHMChild> Children { get; }
        bool IsChildNode(INode n);
    }

    public static class HMChildExtensions
    {
        public static IList<T> ChildrenOfType<T>(this IHMChild child) where T : IHMChild
        {
            return child.Children.OfType<T>().ToList();
        }
    }

    // When a template adds to a graph, it inserts one of these into a template tree.
    // This maintains the connection between nodes inserted (and subsequently inserted)
    // and the template they came from, allowing behaviour to be inherited by everything
    // consequent to the template originally expanded, e.g:
    //
    // (( ==>        -> edges in graph
    //    -, /, \    -> references to metadata
    //
    //   also, keeping "graph" 1D to allow drawing ))
    //
    // ==> e1 ==>
    //
    // expand using a template T1:
    //
    //        HM1 --------------> T1
    //       /   \
    // ==> e2 ==> e3 ==>
    //
    // expand again using T2:
    //
    //          HM1 ------------> T1
    //         /   \
    //        /     HM2 --------> T2
    //       /     /   \
    // ==> e3 ==> X ==> Y ==>
    //
    //
    // So, for example:
    // - our initial graph might be platforms hanging in space.
    // - HM1 might put all its contents inside a sone pillar
    // - HM2 might add a "shop" with X = "sales floor", Y = "stock room"
    //
    // And the output of that should place the shop, and whatever e3 expands into (if anything)
    // inside the pillar.  HM1 could also have added nodes G and H:
    // ==> G ==> e2 ==> e3 ==> H ==>
    // if those were required to generate entrances to the pillar
    //
    // and the choice of a shop for inside the pillar might have been influenced by
    // HM1 changing the set of templates available for use inside it (either by changing the
    // filtering/probabilities on a master list, or else by swapping the whole list
    // used inside it...)

    [System.Diagnostics.DebuggerDisplay("{Template.Name}")]
    public class HierarchyMetadata : IHMChild
    {
        HierarchyMetadata m_parent;
        private readonly List<Force> m_extra_forces = new List<Force>();

        public HierarchyMetadata Parent
        {
            get
            {
                return m_parent;
            }
            set
            {
                if (m_parent != null)
                {
                    m_parent.Children.Remove(this);
                }

                m_parent = value;

                if (m_parent != null)
                {
                    m_parent.Children.Add(this);
                }
            }
        }

        public IReadOnlyList<Force> GetExtraForces()
        {
            return m_extra_forces;
        }

        public IList<IHMChild> Children { get; }

        public Template Template { get; }

        public HierarchyMetadata(HierarchyMetadata parent, Template template)
        {
            Parent = parent;
            Children = new List<IHMChild>();
            Template = template;
        }

        public void AddExtraForce(INode n1, INode n2, float targetDist, float forceMultiplier)
        {
            m_extra_forces.Add(new Force(n1, n2, targetDist, forceMultiplier));
        }

        // can implement this better with some caching...
        public bool IsChildNode(INode n)
        {
            foreach(var c in Children)
            {
                if (c.IsChildNode(n))
                    return true;
            }

            return false;
        }
    }
}
