using NetAirflow.Shared.Interfaces;
using System.IO.Compression;
using System.Reflection;

namespace NetAirflow.Web.Comun
{
    public class DdlService
    {
        // Lista privada para almacenar las tareas cargadas
        private List<ITask> _tasks = new List<ITask>();

        // Propiedad para obtener las tareas cargadas
        public IEnumerable<ITask> GetTasks() => _tasks;
        public ITask? GetTaskByName(string name)
        {
            // Retorna la primera tarea que tenga el nombre igual al especificado (ignorando mayúsculas y minúsculas).
            return _tasks.FirstOrDefault(task =>
                string.Equals(task.Name, name, StringComparison.OrdinalIgnoreCase));
        }

        // Propiedad para almacenar la lista de rutas de los archivos DLL
        public IEnumerable<string> PathListDDLS { get; private set; }

    
        public void LoadAll()
        {
            this.UnzipTask();
            this.LoadDdlList();
            this.LoadTasksFromDlls();
            //Task.Run(async () =>
            //{
            //    // Llamar a ExecuteAsync y esperar su resultado
            //    await _tasks.FirstOrDefault().ExecuteAsync(null, CancellationToken.None);
            //});
        }

        public void UnzipTask()
        {
            // Ruta de la carpeta Zips
            string ddlFolderPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Zips");

            // Verificar si la carpeta Zips existe, si no, crearla
            if (!Directory.Exists(ddlFolderPath))
            {
                try
                {
                    Directory.CreateDirectory(ddlFolderPath);
                    Console.WriteLine($"La carpeta 'Zips' no existía y ha sido creada en {ddlFolderPath}.");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error al crear la carpeta 'Zips': {ex.Message}");
                    return;
                }
            }

            // Obtener todos los archivos .zip en la carpeta Zips
            var zipFiles = Directory.GetFiles(ddlFolderPath, "*.zip");

            // Si no hay archivos .zip en la carpeta, mostrar un mensaje
            if (zipFiles.Length == 0)
            {
                Console.WriteLine("No se encontraron archivos .zip en la carpeta 'Zips'.");
                return;
            }

            // Iterar sobre cada archivo .zip
            foreach (var zipFile in zipFiles)
            {
                try
                {
                    // Descomprimir el archivo ZIP
                    using (ZipArchive zip = ZipFile.OpenRead(zipFile))
                    {
                        // Buscar el archivo setup.conf dentro del ZIP
                        var setupConfEntry = zip.Entries.FirstOrDefault(entry => entry.FullName.Equals("setup.conf", StringComparison.OrdinalIgnoreCase));

                        // Si el archivo setup.conf no existe en el ZIP
                        if (setupConfEntry == null)
                        {
                            // Crear un archivo setup.conf con el contenido esperado
                            using (var stream = new MemoryStream())
                            using (var writer = new StreamWriter(stream))
                            {
                                writer.WriteLine("Name=NetAirflow.MyTaskLibrary.dll");
                                writer.WriteLine("Directory=NetAirflow.MyTaskLibrary");
                                writer.Flush();
                                stream.Seek(0, SeekOrigin.Begin);

                                // Crear el archivo setup.conf dentro del ZIP
                                zip.CreateEntry("setup.conf").Open().Write(stream.ToArray(), 0, (int)stream.Length);
                            }

                            Console.WriteLine($"El archivo 'setup.conf' no estaba presente en '{zipFile}' y se ha creado.");
                        }

                        // Leer el contenido del archivo setup.conf
                        using (var reader = new StreamReader(setupConfEntry.Open()))
                        {
                            string content = reader.ReadToEnd();

                            // Buscar las líneas de interés en el setup.conf
                            var nameLine = content.Split('\n').FirstOrDefault(line => line.StartsWith("Name="));
                            var directoryLine = content.Split('\n').FirstOrDefault(line => line.StartsWith("Directory="));

                            if (nameLine != null && directoryLine != null)
                            {
                                // Obtener el nombre y directorio desde las líneas
                                string name = nameLine.Split('=')[1].Trim();
                                string directory = directoryLine.Split('=')[1].Trim();

                                // Crear la carpeta en DDLS/{Directory}
                                string targetDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "DDLS", directory);

                                // Si la carpeta ya existe, eliminarla con todo su contenido
                                if (Directory.Exists(targetDirectory))
                                {
                                    try
                                    {
                                        Directory.Delete(targetDirectory, true);
                                        Console.WriteLine($"La carpeta '{targetDirectory}' y su contenido han sido eliminados.");
                                    }
                                    catch (Exception ex)
                                    {
                                        Console.WriteLine($"Error al eliminar la carpeta '{targetDirectory}': {ex.Message}");
                                        return; // Detener si hubo un error al eliminar la carpeta
                                    }
                                }

                                // Crear la nueva carpeta vacía
                                Directory.CreateDirectory(targetDirectory);
                                Console.WriteLine($"La carpeta '{targetDirectory}' ha sido recreada.");

                                // Descomprimir todo el contenido del archivo ZIP (incluyendo setup.conf)
                                foreach (var entry in zip.Entries)
                                {
                                    string destinationPath = Path.Combine(targetDirectory, entry.FullName);

                                    // Asegurarse de que la carpeta de destino exista
                                    string destinationDir = Path.GetDirectoryName(destinationPath)!;
                                    if (!Directory.Exists(destinationDir))
                                    {
                                        Directory.CreateDirectory(destinationDir);
                                    }

                                    // Extraer el archivo
                                    entry.ExtractToFile(destinationPath, overwrite: true);
                                }

                                Console.WriteLine($"Archivo ZIP '{zipFile}' descomprimido correctamente en '{targetDirectory}'.");
                            }
                            else
                            {
                                Console.WriteLine($"El archivo 'setup.conf' no contiene la información esperada.");
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error al procesar el archivo ZIP '{zipFile}': {ex.Message}");
                }
            }
        }





        public void LoadDdlList()
        {
            string ddlFolderPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "DDLS");

            // Verificar si la carpeta DDLS existe
            if (!Directory.Exists(ddlFolderPath))
            {
                Console.WriteLine($"La carpeta de DLLs no existe: {ddlFolderPath}");
                return;
            }

            // Buscar todas las carpetas dentro de DDLS
            var directories = Directory.GetDirectories(ddlFolderPath);

            // Crear una lista para almacenar las rutas de los DLLs
            var dllPaths = new List<string>();

            // Iterar sobre cada carpeta
            foreach (string directory in directories)
            {
                string setupConfPath = Path.Combine(directory, "setup.conf");

                // Verificar si el archivo setup.conf existe dentro de la carpeta
                if (File.Exists(setupConfPath))
                {
                    try
                    {
                        // Leer el contenido de setup.conf
                        var setupConfContent = File.ReadAllLines(setupConfPath);

                        // Buscar las líneas que contienen 'Name=' y 'Directory='
                        var nameLine = setupConfContent.FirstOrDefault(line => line.StartsWith("Name="));
                        var directoryLine = setupConfContent.FirstOrDefault(line => line.StartsWith("Directory="));

                        // Si se encuentran las líneas necesarias
                        if (nameLine != null && directoryLine != null)
                        {
                            string dllName = nameLine.Split('=')[1].Trim();
                            string folderName = directoryLine.Split('=')[1].Trim();

                            // Formar la ruta completa del archivo DLL
                            string dllPath = Path.Combine(directory, dllName);

                            // Verificar si el archivo DLL existe en la carpeta
                            if (File.Exists(dllPath))
                            {
                                dllPaths.Add(dllPath);
                                Console.WriteLine($"Se ha encontrado y agregado el archivo DLL: {dllPath}");
                            }
                            else
                            {
                                Console.WriteLine($"El archivo DLL '{dllPath}' no se encontró en la carpeta '{directory}'.");
                            }
                        }
                        else
                        {
                            Console.WriteLine($"Archivo setup.conf en '{directory}' no contiene las líneas esperadas.");
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error al procesar setup.conf en '{directory}': {ex.Message}");
                    }
                }
                else
                {
                    Console.WriteLine($"No se encontró el archivo 'setup.conf' en la carpeta: {directory}");
                }
            }

            // Asignar la lista de rutas de DLLs a la propiedad PathListDDLS
            PathListDDLS = dllPaths;

            // Mostrar la lista de DLLs cargados
            Console.WriteLine("Lista de archivos DLL encontrados:");
            foreach (var dllPath in PathListDDLS)
            {
                Console.WriteLine(dllPath);
            }
        }

        public void LoadTasksFromDlls()
        {
            if(PathListDDLS == null || PathListDDLS?.Count() == 0)
            {
                Console.WriteLine("No se encontraron archivos DLL para cargar.");
                return;
            }
            // Usar la lista PathListDDLS para cargar las tareas
            foreach (string dllPath in PathListDDLS!)
            {
                try
                {
                    Assembly assembly = Assembly.LoadFrom(dllPath);
                    var taskTypes = assembly.GetTypes()
                                            .Where(t => typeof(ITask).IsAssignableFrom(t) && !t.IsInterface && !t.IsAbstract);

                    foreach (Type taskType in taskTypes)
                    {
                        if (Activator.CreateInstance(taskType) is ITask taskInstance)
                        {
                            _tasks.Add(taskInstance);
                            Console.WriteLine($"Cargada tarea: {taskInstance.Name}");
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error cargando DLL {dllPath}: {ex.Message}");
                }
            }
        }












    }
}
