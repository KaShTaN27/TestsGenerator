namespace TestsGeneratorApplication; 

public static class Application {
    public static async Task Main(string[] args) {
        var values = new int[3];
        if (ReadInput(values, out var path)) {
            var pipelineService = new PipelineService(values[0], values[1], values[2], path);
            await pipelineService.Generate();    
        }
    }

    private static bool ReadInput(int[] values, out string path) {
        path = "";
        var names = new[] { "read", "write", "process" };
        
        for (var i = 0; i < 3; i++) {
            Console.WriteLine($"Enter {names[i]} degree of parallelism");
            var readLine = Console.ReadLine();
            if (!int.TryParse(readLine!, out var value)) {
                Console.WriteLine($"Invalid {names[i]} degree of parallelism value");
                return false;
            }
            values[i] = value;
        }
        Console.WriteLine($"Enter directory path to source files");
        var enteredPath = Console.ReadLine();
        if (!Directory.Exists(enteredPath)) {
            Console.WriteLine("Invalid directory path");
            return false;
        }
        path = enteredPath;
        return true;
    }
}