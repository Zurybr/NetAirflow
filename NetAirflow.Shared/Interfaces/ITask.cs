using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace NetAirflow.Shared.Interfaces
{
    public interface ITask
    {
        /// <summary>
        /// Nombre identificador de la tarea.
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Descripción breve de la tarea.
        /// </summary>
        string Description { get; }

        /// <summary>
        /// Parámetros configurables para la tarea.
        /// </summary>
        IDictionary<string, string>? Parameters { get; }

        /// <summary>
        /// Ejecuta la tarea recibiendo un contexto de ejecución y un token de cancelación.
        /// </summary>
        /// <param name="context">Contexto de ejecución con servicios compartidos.</param>
        /// <param name="cancellationToken">Token para cancelar la operación.</param>
        /// <returns>Una tarea asincrónica.</returns>
        Task ExecuteAsync(ITaskExecutionContext? context, CancellationToken cancellationToken);

        #region Callbacks Opcionales

        /// <summary>
        /// Evento opcional que se dispara cuando la tarea se ejecuta exitosamente.
        /// </summary>
        event Action OnSuccess;

        /// <summary>
        /// Evento opcional que se dispara cuando la tarea falla.
        /// Se pasa la excepción que causó el fallo.
        /// </summary>
        event Action<Exception> OnFailed;

        /// <summary>
        /// Evento opcional que se dispara cuando la tarea es cancelada.
        /// </summary>
        event Action OnCancel;

        #endregion
    }
}
