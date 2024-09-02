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

        /// <summary>
        /// Detects Whether The Interceptor Should Run
        /// </summary>
        public virtual bool ShouldEnable()
        {
            return true;
        }

        /// <summary>
        /// Called When A Notification Pops Up
        /// </summary>
        public virtual void OnNotification(UserNotification notification)
        {

        }

        /// <summary>
        /// Run Often Just To Rediscover Windows/Configs And Such
        /// </summary>
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
