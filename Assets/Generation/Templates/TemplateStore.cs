using System.Collections.Generic;
using System.Linq;

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
            Template ret;

            if (m_templates.TryGetValue(name, out ret))
            {
                return ret;
            }

            return null;
        }

        public bool Contains(string name)
        {
            return m_templates.ContainsKey(name);
        }
    }
}