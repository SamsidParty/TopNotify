using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TopNotify.Common;

namespace TopNotify.Daemon
{
    public class DaemonErrorHandler
    {
        public static List<DaemonError> Errors = new List<DaemonError>();

        /// <summary>
        /// Displays An Error To The User Without Closing TopNotify
        /// </summary>
        public static void ThrowNonCritical(DaemonError error)
        {
            Errors.Add(error);
            NotificationTester.Toast("Something Went Wrong", error.Text);
        }

        /// <summary>
        /// Displays An Error To The User And Closes TopNotify
        /// </summary>
        public static void ThrowCritical(DaemonError error)
        {
            Errors.Add(error);
            NotificationTester.Toast("Something Went Wrong", error.Text);
            Environment.Exit(1);
        }
    }
}
