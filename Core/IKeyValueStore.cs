using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Qx
{
    interface IKeyValueStore
    {
        Message Get(String key);
        void Put(String key, Message value);
        void Delete(String key);
    }
}
