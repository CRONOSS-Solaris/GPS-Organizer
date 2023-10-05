using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GPS_Organizer.Utils
{
    public static class LoggerHelper
    {
        public static void DebugLog(Logger log, GPS_OrganizerConfig config, string message)
        {
            if (config?.DebugMode ?? false)
            {
                log.Info(message);
            }
        }
    }

}
