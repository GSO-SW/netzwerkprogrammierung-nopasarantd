using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NoPasaranTD.Networking
{
    public struct NetworkTask
    {
        public NetworkTask(Action<object> handler, object parameter, long tickToPerform)
        {
            Handler = handler;
            Parameter = parameter;
            TickToPerform = tickToPerform;
        }
        public Action<object> Handler { get;}
        public object Parameter { get;}
        public long TickToPerform { get;}
    }
}
