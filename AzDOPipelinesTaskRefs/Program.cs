using System.Diagnostics.CodeAnalysis;

FileStream? ostrm = null;
StreamWriter writer;

string? rootDir = Environment.CurrentDirectory;
bool useFileOutput = false;

if (args.Length >= 1)
{
    rootDir = args[0]?.ToString();

    bool.TryParse(args[1], out useFileOutput);
}

if (useFileOutput)
{
    string outputFile = "output.txt";
    TextWriter oldOut = Console.Out;
    File.Delete(outputFile);
    ostrm = new FileStream(outputFile, FileMode.OpenOrCreate, FileAccess.ReadWrite);
    writer = new StreamWriter(ostrm);
    Console.SetOut(writer);
}

Console.ForegroundColor = ConsoleColor.Gray;
var taskList = await ParseTaskList();

Console.WriteLine();

var ymlFilePaths = FindYamlFiles(rootDir);

Console.WriteLine();

var markers = await ValidateFiles(taskList, ymlFilePaths);

Console.WriteLine();

WriteLineMarkers(markers);

Console.WriteLine();

Console.ForegroundColor = ConsoleColor.Gray;

if (useFileOutput)
{
    await ostrm!.FlushAsync();
    ostrm!.Close();
}
else
{
    Console.WriteLine();
    Console.WriteLine("Press any key to exit");
    Console.ReadKey();
}


async Task<IEnumerable<string>> ParseTaskList()
{
    var outputTasks = new List<string>();

    Console.WriteLine("Parsing Azure Pipelines Task Reference");

    var rawHtmlResponse = await new HttpClient().GetStreamAsync("https://learn.microsoft.com/en-us/azure/devops/pipelines/tasks/reference/?view=azure-pipelines");

    StreamReader streamReader = new StreamReader(rawHtmlResponse);

    string? line;

    while ((line = streamReader.ReadLine()) != null)
    {
        var firstWords = line.Replace("\t", " ").Replace("data-linktype=\"relative-path\">", "").Split(' ');

        foreach (var firstWord in firstWords)
        {
            if (firstWord != null && firstWord.Contains("@") && firstWord.Contains("</a>"))
                outputTasks.Add(firstWord.Remove(firstWord.IndexOf("</a>")));
        }
    }

    // Extend the list of valida tasks
    outputTasks.AddRange(new[]
    {
        "UseGitVersion@5",
        "GitVersion@5",
        "AzureRMWebAppDeployment@4",
        "gitversion/setup@0",
        "gitversion/execute@0",
        "gittools.gitversion.gitversion-task.GitVersion@3",
        "dependabot@1",
        "PackageAzureDevOpsExtension@3",
        "PublishAzureDevOpsExtension@3",
        "DotNetCoreInstaller@2",
        "TfxInstaller@3",
        "IsAzureDevOpsExtensionValid@3",
        "PublishPipelineArtifact@1",
        "GitHubRelease@1",
        "SnykSecurityScan@0"        
    });

    var parsedTasks = outputTasks.OrderBy(task => task).Distinct(new TaskComparer());

    foreach (var task in parsedTasks)
    {
        Console.WriteLine($" - {task}");
    }

    Console.WriteLine($"{parsedTasks.Count()} tasks parsed.");

    return parsedTasks;
}

List<FileInfo> FindYamlFiles(string startFolder)
{
    Console.WriteLine($"Looking up YML files in {startFolder} root directory.");

    DirectoryInfo dir = new System.IO.DirectoryInfo(startFolder);

    IEnumerable<FileInfo> fileList = dir.GetFiles("*.*", SearchOption.AllDirectories);

    IEnumerable<FileInfo> fileQuery = from file in fileList
                                      where file.Extension == ".yml" || file.Extension == ".yaml"
                                      orderby file.Name
                                      select file;

    foreach (FileInfo fi in fileQuery)
    {
        Console.WriteLine($" - {fi.FullName}");
    }

    Console.WriteLine($"{fileQuery.Count()} files found.");

    return fileQuery.ToList();
}

async Task<IEnumerable<CheckNeededMarker>> ValidateFiles(IEnumerable<string> taskList, IEnumerable<FileInfo> ymlFilePaths)
{
    int taskCount = 0;

    List<CheckNeededMarker> checkNeededMarkers = new List<CheckNeededMarker>();

    foreach (var ymlFile in ymlFilePaths)
    {
        var marker = new CheckNeededMarker(ymlFile);

        IEnumerable<string> linesWithTasks = (await File.ReadAllLinesAsync(ymlFile.FullName)).Where(line => line.Contains("- task: "));

        foreach (var line in linesWithTasks)
        {
            if (line.Split(new[] { ' ' }).Any(taskList.Contains))
            {
                Console.ForegroundColor = ConsoleColor.Green;

                Console.WriteLine($"\tFile: {ymlFile.Name}\t{line}");
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine($"\tFile: {ymlFile.Name}\t{line}");

                var tasksNoVersion = taskList.Select(t => t.Remove(t.IndexOf("@")));
                var invalidask = line.Remove(line.IndexOf("@")).Replace("- task: ", "");

                if (tasksNoVersion.Any(x => x.ToLowerInvariant().Equals(invalidask.ToLowerInvariant())))
                {
                    Console.WriteLine($"Match found for {invalidask}");
                }

                marker.Lines.Add(line);
                if (checkNeededMarkers.All(x => x.File.FullName != marker.File.FullName))
                {
                    checkNeededMarkers.Add(marker);
                }
            }
        }

        taskCount += linesWithTasks.Count();
    }

    Console.WriteLine($"{taskCount} tasks checked.");

    return checkNeededMarkers;
}

void WriteLineMarkers(IEnumerable<CheckNeededMarker> markers)
{
    Console.ForegroundColor = ConsoleColor.Magenta;
    Console.WriteLine();
    Console.WriteLine($"YML files requiring attention ({markers.Count()})");

    foreach (var marker in markers)
    {
        Console.ForegroundColor = ConsoleColor.DarkRed;
        Console.WriteLine($"\tFile: {marker.File.Name} ({marker.Lines.Count()} task(s))");
        foreach (var line in marker.Lines.Distinct())
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"\t     {line.TrimStart()}");
        }
    }
}

class CheckNeededMarker
{
    public CheckNeededMarker(FileInfo ymlFile)
    {
        File = ymlFile;
    }

    public FileInfo File { get; set; }
    public string ValidTask { get; set; }
    public List<string> Lines { get; set; } = new List<string>();
}

class TaskComparer : EqualityComparer<string>
{
    public override bool Equals(string? x, string? y)
    {
        return x.ToLowerInvariant().Equals(y.ToLowerInvariant());
    }

    public override int GetHashCode([DisallowNull] string obj)
    {
        return obj.GetHashCode();
    }
}