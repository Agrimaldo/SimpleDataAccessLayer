using System;

namespace SimpleDataAccessLayer.Util.Attributes
{
    public class DescriptionAttribute : Attribute
    {
        public DescriptionAttribute(string text)
        {
            this.Text = text;
        }
        public string Text { get; set; }
    }
}
