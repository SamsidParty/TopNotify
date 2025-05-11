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
          

        public virtual void Restart() { }

        /// <summary>
        /// Used To Update The Interceptor When Certain Keys Are Pressed Or Released
        /// </summary>
        public virtual void OnKeyUpdate() { }

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
        public virtual void Reflow() { }
        public virtual void Start() { }
        public virtual void Update() { }

    }
}
