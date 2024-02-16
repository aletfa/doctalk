using Microsoft.Extensions.Configuration;

namespace DocTalk;

internal class Program
{
    static ConsoleColor originalColor;

    public static async Task Main(string[] args)
    {
        originalColor = Console.ForegroundColor;
        Splash();

        WorkingDirectoryDTO docTalk = AcquireWorkingDirectory();

        _ = new ConfigurationBuilder()
            .SetBasePath(RootPath)
            .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
            .Build();

        //Whisper
        //Find and convert all audio files in .txt documents

        WhisperEngine whisperEngine = new();
        await whisperEngine.DownloadModelAsync();
        foreach (string file in docTalk.Files)
        {
            var extension = Path.GetExtension(file)?.ToLower() ?? "";
            if (WhisperEngine.SupportedExtensions.Contains(extension))
            {
                Console.WriteLine($"Whisper for {Path.GetFileName(file)}");
                await whisperEngine.ExctactTextFileAsync(file);
            }
        }

        //Microsoft.KernelMemory + LLama
        //Instruct the AI and start a chat with it

        KernelChatEngine kernelEngine = new();
        await kernelEngine.DownloadModelAsync();
        await kernelEngine.InitializeAsync(docTalk.Directory);

        Console.WriteLine($"Starting chat session: {kernelEngine.ConversationHash}");
        string? question;
        do
        {
            Write("Q: ");
            question = Console.ReadLine() ?? "";
            var answer = await kernelEngine.AskAsync(question);
            Write($"A: ");
            if (answer.IsEmptyAnswer)
                Write("(??) ", ConsoleColor.Yellow);
            Console.WriteLine(answer.Answer);
            foreach (var a in answer.RelevantSources)
                Console.WriteLine($"    * {a}");
        } while (!string.IsNullOrWhiteSpace(question));
    }

    private static void Splash()
    {
        Console.ForegroundColor = ConsoleColor.White;
        Console.WriteLine(@"    ____            ______      ____  ");
        Console.WriteLine(@"   / __ \____  ____/_  __/___ _/ / /__");
        Console.WriteLine(@"  / / / / __ \/ ___// / / __ `/ / //_/");
        Console.WriteLine(@" / /_/ / /_/ / /__ / / / /_/ / / ,<   ");
        Console.WriteLine(@"/_____/\____/\___//_/  \__,_/_/_/|_|  ");
        Console.ForegroundColor = originalColor;
        Console.WriteLine();
        Console.WriteLine("A .Net AI Tool to Chat about your Docs.");
        Write("@aletfa", ConsoleColor.Blue);
        Console.WriteLine(" - https://github.com/aletfa");
        Console.WriteLine();
    }

    /// <summary>
    /// Ask to the user for a valid directory
    /// </summary>
    /// <returns>An object that represent the directory and all file managed by this application within that folder</returns>
    private static WorkingDirectoryDTO AcquireWorkingDirectory()
    {
        var supported = WhisperEngine.SupportedExtensions.Union(KernelChatEngine.SupportedExtensions).Distinct();
        IEnumerable<string>? files;
        string? directory;
        do
        {
            Console.Write("Directory: ");
            directory = Console.ReadLine() ?? "";
            files = Directory.GetFiles(directory).Where(f => supported.Contains(Path.GetExtension(f)?.ToLower()));
            if (!files.Any())
            {
                Console.WriteLine($"No supported files in {directory} ({string.Join(", ", supported)})");
                continue;
            }
            Console.WriteLine($"{directory}: ");
            foreach (string file in files)
            {
                var extension = Path.GetExtension(file)?.ToLower();
                var fileFriendly = file.Substring(directory.Length + 1);
                Write($"   * {fileFriendly}");
                if (WhisperEngine.SupportedExtensions.Contains(extension))
                    Write(" (Whisper)", ConsoleColor.Yellow);
                Console.WriteLine();
            }
            Console.Write("Confirm this directory? (Y/N): ");
            var exit = Console.ReadLine();
            if (exit?.ToLower() != "y" && exit?.ToLower() != "yes" && !string.IsNullOrEmpty(exit))
                continue;

            return new WorkingDirectoryDTO(directory, files);
        } while (true);
    }

    /// <summary>
    /// Root Path of this application (the folder that contains the main program)
    /// </summary>
    public static string RootPath { get; } = Path.GetDirectoryName(typeof(Program).Assembly.Location) ?? "";

    static void Write(string text, ConsoleColor? color = null)
    {
        var selected = color ?? originalColor;
        Console.ForegroundColor = selected;
        Console.Write(text);
        Console.ForegroundColor = originalColor;
    }
}
