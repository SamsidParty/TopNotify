using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TopNotify.Common;
using Windows.UI.Notifications;

namespace TopNotify.Daemon
{
    public class Interceptor
    {
        public Settings Settings { get { return InterceptorManager.Instance.CurrentSettings; } }
          

        public virtual void Restart()
        {

        }

        //Called When A Notification Pops Up
        public virtual void OnNotification(UserNotification notification)
        {

        }

        //Run Often Just To Rediscover Windows/Configs And Such
        public virtual void Reflow()
        {
            if (Daemon.Instance.Server != null)
            {
                Daemon.Instance.Server.UpdateHandles();
            }
        }

        public virtual void Start()
        {
            
        }

        public virtual void Update()
        {

        }
    }
}
