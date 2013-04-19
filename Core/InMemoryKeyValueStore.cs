using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Qx
{
    class InMemoryKeyValueStore : IKeyValueStore
    {
        private SortedDictionary<String, Message> sortedDictionary;

        public InMemoryKeyValueStore() {
            sortedDictionary = new SortedDictionary<string, Message>();
        }

        public void Put(String key, Message value)
        {
            sortedDictionary.Add(key, value);
        }

        public Message Get(String key)
        {
            if (key == null)
            {
                throw new ArgumentNullException();
            }
           return sortedDictionary.Where(x => x.Key == key).Select(x => x.Value).First();
        }

        public void Delete(String key)
        {
            throw new NotImplementedException();
        }
    }
}
