using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetAirflow.Shared.Interfaces
{
    public interface ITaskExecutionContext
    {
        // Ejemplo: Propiedad para registrar logs. Puedes ampliar este contexto según tus necesidades.
        ILogger Logger { get; }
    }
}
