using UnityEngine;

namespace Watermelon
{
    public class MultilanguageWordAttribute : PropertyAttribute
    {
        private string m_Filter;
        public string filter
        {
            get { return m_Filter; }
        }

        public MultilanguageWordAttribute(string filter = null)
        {
            m_Filter = filter;
        }
    }
}