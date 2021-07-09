using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using UnityEngine;

namespace Assets.Generation.Templates
{
    public class TemplateStore : MonoBehaviour
    {
        protected readonly Dictionary<string, Template> m_templates = new Dictionary<string, Template>();

        public List<Template> GetTemplatesCopy()
        {
            return new List<Template>(m_templates.Values.ToList());
        }

        protected void AddTemplate(Template t)
        {
            m_templates[t.Name] = t;
        }
    }
}