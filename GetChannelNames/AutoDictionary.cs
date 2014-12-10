using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GetChannelNames
{
    internal class AutoDictionary<TKey, TValue> : Dictionary<TKey,TValue> where TValue : new()
    {
        public new TValue this[TKey key]
        {
            get
            {
                try
                {
                    return base[key];
                }
                catch (KeyNotFoundException)
                {
                    return this[key] = new TValue();
                }
            }

            set
            {
                base[key] = value;
            }
        }
    }
}
