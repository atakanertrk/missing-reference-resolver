using Microsoft.Build.Evaluation;
using Microsoft.Build.Locator;

namespace ReferenceManagerConsoleApp
{
    class Program
    {
        private static List<string> _projectFiles = new List<string>();
        static void Main(string[] args)
        {
            //if (args.Length < 2)
            //{
            //    Console.WriteLine("Usage: ReferenceManagerConsoleApp <solution-path> <fallback-dll-path>");
            //    return;
            //}

            string solutionFilePath = "C:\\Users\\PC\\Desktop\\Projects\\RMSolution\\RMSolution.sln";// args[0];
            string fallbackDllPath = "D:\\references\\";// args[1];

            // Ensure MSBuild is loaded
            MSBuildLocator.RegisterDefaults();

            // Load solution file and get associated csproj files
            var solutionDirectory = Path.GetDirectoryName(solutionFilePath);
            var projectFiles = Directory.GetFiles(solutionDirectory, "*.csproj", SearchOption.AllDirectories);
            _projectFiles = projectFiles.ToList();
            foreach (var projectFile in projectFiles)
            {
                Console.WriteLine($"Processing project: {projectFile}");
                CheckAndFixReferences(projectFile, fallbackDllPath);
            }
        }

        static void CheckAndFixReferences(string projectFilePath, string fallbackDllPath)
        {
            // Load the csproj file using MSBuild
            var project = new Project(projectFilePath);

            // Process the <Reference> elements (DLL references)
            var referenceItems = project.GetItems("Reference").ToList();
            foreach (var item in referenceItems)
            {
                var hintPath = item.GetMetadataValue("HintPath");

                if (!string.IsNullOrEmpty(hintPath) && !File.Exists(hintPath))
                {
                    Console.WriteLine($"Reference not found: {hintPath}");
                    string dllName = Path.GetFileName(hintPath);
                    string fallbackDll = Path.Combine(fallbackDllPath, dllName);

                    if (File.Exists(fallbackDll))
                    {
                        Console.WriteLine($"Replacing with fallback: {fallbackDll}");
                        item.SetMetadataValue("HintPath", fallbackDll);
                        project.Save();
                    }
                    else
                    {
                        Console.WriteLine($"DLL not found in fallback path: {fallbackDll}");
                    }
                }
            }

            var projectReferences = project.GetItems("ProjectReference").ToList();
            // Process the <ProjectReference> elements (project references)
            foreach (var item in projectReferences)
            {
                var referencedProject = item.EvaluatedInclude;
                
                if (!_projectFiles.Contains(referencedProject) || !File.Exists(referencedProject))
                {
                    Console.WriteLine($"Replacing project reference {referencedProject} with DLL reference: {Path.Combine(fallbackDllPath, Path.GetFileNameWithoutExtension(referencedProject))}");

                    // Create a new <Reference> item for the DLL
                    project.RemoveItem(item);
                    project.AddItem("Reference", Path.GetFileNameWithoutExtension(referencedProject), new[]
                    {
                        new KeyValuePair<string, string>("HintPath", Path.Combine(fallbackDllPath,Path.GetFileNameWithoutExtension(referencedProject)+".dll"))
                    });
                    project.Save();
                }
                else
                {
                    Console.WriteLine($"DLL for project reference not found: {""}");
                }
            }
        }
    }
}