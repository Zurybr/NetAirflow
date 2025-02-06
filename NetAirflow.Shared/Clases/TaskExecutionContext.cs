using Microsoft.Extensions.Logging;
using NetAirflow.Shared.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetAirflow.Shared.Clases
{
    public class TaskExecutionContext : ITaskExecutionContext
    {
        public ILogger Logger { get; set; }

        // Puedes agregar otras propiedades o métodos si lo necesitas.
    }
}
