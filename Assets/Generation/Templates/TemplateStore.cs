using Assets.Generation.G;
using Assets.Generation.GeomRep;
using Assets.Generation.U;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Assets.Generation.Templates
{
    public class TemplateStore
    {
        protected readonly Dictionary<string, Template> m_templates = new Dictionary<string, Template>();

        public List<Template> GetTemplatesCopy()
        {
            return new List<Template>(m_templates.Values.ToList());
        }

        public int NumTemplates()
        {
            return m_templates.Count;
        }

        public bool AddTemplate(Template t)
        {
            if (Contains(t.Name))
            {
                return false;
            }

            m_templates.Add(t.Name, t);

            return true;
        }

        public Template FindByName(string name)
        {

            if (m_templates.TryGetValue(name, out Template ret))
            {
                return ret;
            }

            return null;
        }

        public bool Contains(string name)
        {
            return m_templates.ContainsKey(name);
        }

        public virtual void MakeSeed(Graph g, ClRand clRand)
        {
            Node start = g.AddNode("Start", "<", 3f, 0.1f, CircularGeomLayout.Instance);
            Node expander = g.AddNode("engine.StepperController", "e", 1f, CircularGeomLayout.Instance);
            Node end = g.AddNode("End", ">", 3f, 0.1f, CircularGeomLayout.Instance);

            start.Position = new Vector2(0, -4);
            expander.Position = new Vector2(0, 0);
            end.Position = new Vector2(4, 0);

            g.Connect(start, expander, 4.5f, 1, CorridorLayout.Default, 0.1f);
            g.Connect(expander, end, 4.5f, 1, CorridorLayout.Default, 0.1f);
        }
    }
}