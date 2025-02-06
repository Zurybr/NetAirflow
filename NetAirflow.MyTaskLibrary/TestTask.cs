using Microsoft.Extensions.Logging;
using NetAirflow.Shared.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetAirflow.MyTaskLibrary
{
    public class TestTask : ITask
    {
        public string Name => "TestTask";

        public string Description => "Tarea de ejemplo que escribe un archivo de texto.";

        public IDictionary<string, string> Parameters { get; } = new Dictionary<string, string>
        {
            { "FilePath", "test.txt" }
        };

        // Implementación de los eventos opcionales.
        public event Action OnSuccess = delegate { };
        public event Action<Exception> OnFailed = delegate { };
        public event Action OnCancel = delegate { };


        public TestTask()
        {
            // Asignar acción a OnSuccess para escribir en la carpeta Documentos
            OnSuccess += () =>
            {
                try
                {
                    // Obtener la ruta de la carpeta Documentos
                    string documentsPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
                    string filePath = Path.Combine(documentsPath, "resultado.txt");

                    // Contenido del archivo
                    string content = $"Listo a las {DateTime.Now:HH:mm}";

                    // Escribir el archivo
                    File.WriteAllText(filePath, content);

                    Console.WriteLine($"Archivo creado en: {filePath}");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error al escribir archivo: {ex.Message}");
                }
            };
        }
        public async Task ExecuteAsync(ITaskExecutionContext? context, CancellationToken cancellationToken)
        {
            try
            {
                // Ejemplo: obtener el path del archivo desde los parámetros.
                string filePath = Parameters.ContainsKey("FilePath") ? Parameters["FilePath"] : "default.txt";

                // Simular una operación: escribir un archivo con la fecha y hora actual.
                var content = $"La tarea se ejecutó a las {DateTime.Now}";
                await System.IO.File.WriteAllTextAsync(filePath, content, cancellationToken);

                // Invocar el callback de éxito.
                OnSuccess?.Invoke();

                // También puedes usar el logger del contexto para registrar la operación.
                context?.Logger.LogInformation($"La tarea '{Name}' se ejecutó correctamente y escribió en {filePath}.");
            }
            catch (OperationCanceledException)
            {
                // Si la tarea es cancelada, invocar el callback correspondiente.
                OnCancel?.Invoke();
                context?.Logger.LogInformation($"La tarea '{Name}' fue cancelada.");
            }
            catch (Exception ex)
            {
                // En caso de error, se dispara el callback de fallo.
                OnFailed?.Invoke(ex);
                context?.Logger.LogError($"La tarea '{Name}' falló: {ex.Message}");
            }
        }
    }
}
