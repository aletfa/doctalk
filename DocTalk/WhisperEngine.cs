using Whisper.net;
using Whisper.net.Ggml;

namespace DocTalk;

/// <summary>
/// Whisper AI engine
/// </summary>
/// <remarks>Offers the feature of Whisper AI (<see href="https://github.com/sandrohanea/whisper.net"/>) combined with the audio conversion capabilities of FFmpeg (<see href="https://github.com/BtbN/FFmpeg-Builds"/>)</remarks>
internal class WhisperEngine
{
    /// <summary>
    /// Full path of the Whisper model
    /// </summary>
    private string? ModelPath { get; set; } = null;

    /// <summary>
    /// File extensions that this engine can manage
    /// </summary>
    public static readonly string[] SupportedExtensions = [".wav", ".mp3", ".mp4"];

    /// <summary>
    /// Retrive the proper model from huggingface.co
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    /// <remarks>Requires a little bit because the model can be heavy</remarks>
    public async Task DownloadModelAsync(CancellationToken cancellationToken = default)
    {
        var model = "ggml-base.bin";
        var modelDir = Path.Combine(Program.RootPath, "model");
        this.ModelPath = Path.Combine(modelDir, model);
        if (!File.Exists(this.ModelPath))
        {
            using var modelStream = await WhisperGgmlDownloader.GetGgmlModelAsync(GgmlType.Base, cancellationToken: cancellationToken);
            using var fileWriter = File.OpenWrite(this.ModelPath);
            await modelStream.CopyToAsync(fileWriter, cancellationToken);
        }
    }

    /// <summary>
    /// Returns a txt file containing the text contained in the media file
    /// </summary>
    /// <param name="mediaFile">Full path of the file to conver</param>
    /// <returns></returns>
    /// <exception cref="InvalidOperationException">If the engine is not Initialized (<see cref="DownloadModelAsync"/>)</exception>
    /// <exception cref="NotSupportedException">If the media file is in a not supported format (<see cref="SupportedExtensions"/>)</exception>
    public async Task<string> ExctactTextFileAsync(string mediaFile)
    {
        if (string.IsNullOrEmpty(this.ModelPath) || !File.Exists(this.ModelPath))
            throw new InvalidOperationException($"Model missing, please use {nameof(DownloadModelAsync)} before starting a chat session.");

        string directory = Path.GetDirectoryName(mediaFile)!;
        var extension = Path.GetExtension(mediaFile)?.ToLower() ?? "";

        if (!SupportedExtensions.Contains(extension))
            throw new NotSupportedException($"File with {extension} extensions are not supported");

        string fileNameOnly = Path.GetFileNameWithoutExtension(mediaFile);

        //mpeg to wav file conversion
        if (extension == ".mp3" || extension == ".mp4")
        {
            string wavFile = Path.Combine(directory, $"{fileNameOnly}.wav");
            await AudioConverter.ConvertMpegToWavAsync(mediaFile, wavFile);
            mediaFile = wavFile;
        }

        var txtFile = Path.Combine(directory, $"{fileNameOnly}.txt");

        if (File.Exists(txtFile))
            return txtFile;

        File.Create(txtFile);
        using var factory = WhisperFactory.FromPath(this.ModelPath);

        using var processor = factory.CreateBuilder()
                                     .WithLanguage("auto")
                                     .Build();

        using var audioStream = File.OpenRead(mediaFile);

        await foreach (var result in processor.ProcessAsync(audioStream))
            await File.AppendAllTextAsync(txtFile, result.Text);

        return txtFile;
    }
}
