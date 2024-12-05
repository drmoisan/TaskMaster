using log4net;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UtilitiesCS.NewtonsoftHelpers
{
    public class NConsoleTraceWriter : ITraceWriter
    {
        private TraceLevel _levelFilter = TraceLevel.Verbose;
        public TraceLevel LevelFilter { get => _levelFilter; set => _levelFilter = value; }

        private List<string> _messageFilter = ["Deserialized JSON:", "Serialized JSON:"];
        public List<string> MessageFilter { get => _messageFilter; set => _messageFilter = value; }

        public Action<string, Exception> Log { get; set; }

        public void Trace(TraceLevel level, string message, Exception ex)
        {
            if (!MessageFilter.Select(message.Contains).Aggregate((a, b) => a | b))
            {
                Log?.Invoke(message, ex);
            }
        }

    }   
}
