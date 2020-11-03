using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TimeLineRecorder
{
    class ListComponentItem
    {
        private string _text = null;
        private object _value = null;

        public string Text { get => _text; set => _text = value; }
        public object Value { get => _value; set => _value = value; }

        public ListComponentItem(string text, object value = null)
        {
            Text = text;
            Value = value;
        }
        public override string ToString()
        {
            return _text;
        }
    }
}
