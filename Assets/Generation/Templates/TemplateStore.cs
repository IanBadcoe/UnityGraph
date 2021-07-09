using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace Generation.Templates
{
    public class TemplateStore
    {
        private readonly Dictionary<string, Template> m_templates = new Dictionary<string, Template>();
        public List<Template> GetTemplatesCopy()
        {
            return new List<Template>(m_templates.Values.ToList());
        }
    }
}