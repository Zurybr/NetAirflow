# Creating a New DDL Task for NetAirflow

## Prerequisites

- Visual Studio or similar IDE
- .NET SDK

## Steps

### 1. Create Class Library Project

1. Create a new .NET Class Library project
2. Name it following the pattern: `NetAirflow.[YourTaskName]`

### 2. Implement ITask Interface

Create a new class that implements `ITask`:

```csharp
public class YourTask : ITask
{
    public string Name => "YourTaskName";
    public string Description => "Brief description of your task";
    public IDictionary<string, string>? Parameters { get; }

    // Optional event implementations
    public event Action OnSuccess = delegate { };
    public event Action<Exception> OnFailed = delegate { };
    public event Action OnCancel = delegate { };

    public async Task ExecuteAsync(ITaskExecutionContext? context, CancellationToken cancellationToken)
    {
        try
        {
            // Your task implementation here

            OnSuccess?.Invoke();
        }
        catch (Exception ex)
        {
            OnFailed?.Invoke(ex);
            throw;
        }
    }
}
```

### 3. Configure Project File

Add the following to your `.csproj` file:

```xml
<Target Name="CreateSetupConf" AfterTargets="Build">
    <WriteLinesToFile
        File="$(OutputPath)setup.conf"
        Lines="Name=NetAirflow.[YourTaskName].dll%0ADirectory=NetAirflow.[YourTaskName]"
        Overwrite="true" />
</Target>
```

### 4. Event Handler Example

```csharp
public YourTask()
{
    OnSuccess += () =>
    {
        string documentsPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
        string filePath = Path.Combine(documentsPath, "result.txt");
        File.WriteAllText(filePath, $"Completed at {DateTime.Now:HH:mm}");
    };
}
```

## Deployment

The task will be loaded by the manager in the directory specified in `setup.conf`.
