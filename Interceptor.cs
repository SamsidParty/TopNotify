using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SamsidParty_TopNotify
{
    public class Interceptor
    {
        public Settings Settings { get { return InterceptorManager.Instance.CurrentSettings; } }
          

        public virtual void Start()
        {
            
        }

        public virtual void Update()
        {

        }
    }
}
