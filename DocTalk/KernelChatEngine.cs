﻿using LLama.Common;
using LLamaSharp.KernelMemory;
using Microsoft.KernelMemory;
using Microsoft.KernelMemory.Configuration;
using System.Security.Cryptography;
using System.Text;

namespace DocTalk;

/// <summary>
/// Engine for the AI Chat
/// </summary>
/// <remarks>Offers the feature of Microsoft.KernelMemory (<see href="https://github.com/microsoft/kernel-memory"/>) with a LLama model as core (<see href="https://github.com/SciSharp/LLamaSharp"/>)</remarks>
internal partial class KernelChatEngine
{
    /// <summary>
    /// Full path of the LLama model
    /// </summary>
    private string? ModelPath { get; set; } = null;

    /// <summary>
    /// File extensions that this engine can manage
    /// </summary>
    public static readonly string[] SupportedExtensions = [".pdf", ".doc", ".docx", ".ppt", ".pptx", ".txt"];

    /// <summary>
    /// Used to identify the working directory uniquely
    /// </summary>
    public string? ConversationHash { get; private set; } = null;

    /// <summary>
    /// Microsoft.KernelMemory Core
    /// </summary>
    private MemoryServerless? Kernel { get; set; } = null;

    /// <summary>
    /// Inizialize the engine with all documents within a specific folder
    /// </summary>
    /// <param name="directory">Directory, knowledge base for the engine</param>
    /// <returns></returns>
    /// <exception cref="InvalidOperationException">If the engine is not Initialized (<see cref="DownloadModelAsync"/>)</exception>
    /// <exception cref="ArgumentException">If the provided directory does not contain any managed file (<see cref="SupportedExtensions"/>)</exception>
    public async Task InitializeAsync(string directory)
    {
        if (string.IsNullOrEmpty(this.ModelPath) || !File.Exists(this.ModelPath))
            throw new InvalidOperationException($"Model missing, please use {nameof(DownloadModelAsync)} before starting a chat session.");

        var files = Directory.GetFiles(directory).Where(f => SupportedExtensions.Contains(Path.GetExtension(f)?.ToLower()));
        if (!files.Any())
            throw new ArgumentException($"{directory} does not contains any supported file.");

        this.ConversationHash = CalculateMD5Hash(directory);

        var config = new LLamaSharpConfig(this.ModelPath)
        {
            GpuLayerCount = 32,
            Seed = 1338,
            DefaultInferenceParams = new InferenceParams
            {
                AntiPrompts = new List<string> { "\n\n" }
            }
        };

        this.Kernel = new KernelMemoryBuilder()
            .WithLLamaSharpDefaults(config)
            .With(new TextPartitioningOptions
            {
                MaxTokensPerParagraph = 300,
                MaxTokensPerLine = 100,
                OverlappingTokens = 30
            })
            .Build<MemoryServerless>();

        var document = new Document("block");
        foreach (string file in files)
        {
            var extension = Path.GetExtension(file)?.ToLower() ?? "";
            if (SupportedExtensions.Contains(extension))
                document.AddFile(file);
        }

        await this.Kernel.ImportDocumentAsync(document, steps: Constants.PipelineWithoutSummary);
    }

    /// <summary>
    /// Ask the AI engine a question
    /// </summary>
    /// <param name="question">subject of the request: a question or a simple task</param>
    /// <returns>The response generated by the AI</returns>
    /// <exception cref="InvalidOperationException">If the engine is not Initialized (<see cref="DownloadModelAsync"/> and next <see cref="InitializeAsync"/>)</exception>
    public async Task<AnswerDTO> AskAsync(string question)
    {
        if (this.Kernel is null)
            throw new InvalidOperationException($"Initialize the Engine first by calling {nameof(InitializeAsync)}");

        var answer = await this.Kernel.AskAsync(question);
        return new AnswerDTO(question, answer);
    }

    /// <summary>
    /// Retrive the proper model from huggingface.co
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    /// <remarks>Requires a little bit because the model can be heavy</remarks>
    public async Task DownloadModelAsync(CancellationToken cancellationToken = default)
    {
        //https://huggingface.co/TheBloke/Llama-2-7B-Chat-GGUF 
        //Syntax: <owner>/<model>/<file-name>
        var model = "TheBloke/Llama-2-7B-Chat-GGUF/llama-2-7b-chat.Q4_K_M.gguf";

        var split = model.Split('/');
        var modelName = split.Last();
        var modelOwner = string.Join('/', split.Take(split.Length - 1));
        var modelDir = Path.Combine(Program.RootPath, "model");
        this.ModelPath = Path.Combine(modelDir, modelName);

        if (File.Exists(this.ModelPath))
            return;

        using HttpClient httpClient = new() { Timeout = Timeout.InfiniteTimeSpan };
        string requestUri = $"https://huggingface.co/{modelOwner}/resolve/main/{modelName}";
        HttpResponseMessage obj = await httpClient.GetAsync(requestUri, HttpCompletionOption.ResponseHeadersRead, cancellationToken);
        obj.EnsureSuccessStatusCode();

        Directory.CreateDirectory(modelDir);
        using var stream = new FileStream(this.ModelPath, FileMode.CreateNew);
        await obj.Content.CopyToAsync(stream, cancellationToken);
    }

    /// <summary>
    /// A useful function for calculating an MD5 hash from a given string
    /// </summary>
    /// <param name="input"></param>
    /// <returns>The computed MD5 hash</returns>
    private static string CalculateMD5Hash(string input)
    {
        byte[] inputBytes = Encoding.ASCII.GetBytes(input);
        byte[] hashBytes = MD5.HashData(inputBytes);

        StringBuilder sb = new();
        for (int i = 0; i < hashBytes.Length; i++)
            sb.Append(hashBytes[i].ToString("X2"));
        return sb.ToString();
    }
}
