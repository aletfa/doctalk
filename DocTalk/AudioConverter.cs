using NAudio.Wave.SampleProviders;
using NAudio.Wave;
using System.IO;
using Xabe.FFmpeg;

namespace DocTalk;

/// <summary>
/// Offers to the program the audio conversion capabilities of FFmpeg (<see href="https://github.com/BtbN/FFmpeg-Builds"/>)
/// </summary>
/// <remarks>Requires the FFmpeg executables into the RootPath (<see cref="Program.RootPath"/>) (<see href="https://github.com/BtbN/FFmpeg-Builds/releases"/>)</remarks>
internal static class AudioConverter
{
    /// <summary>
    /// Convert an existing Mpeg media file into a wav file
    /// </summary>
    /// <param name="mediaFilePath">Original file path</param>
    /// <param name="wavFilePath">Destination file path</param>
    /// <returns></returns>
    public static async Task ConvertMpegToWavAsync(string mediaFilePath, string wavFilePath)
    {
        FFmpeg.SetExecutablesPath(Program.RootPath);
        IConversion conversion = await FFmpeg.Conversions.FromSnippet.ExtractAudio(mediaFilePath, wavFilePath);
        await conversion.Start();

        using var wavStream = new MemoryStream();
        using (var reader = new WaveFileReader(wavFilePath))
        {
            var resampler = new WdlResamplingSampleProvider(reader.ToSampleProvider(), 16000);
            WaveFileWriter.WriteWavFileToStream(wavStream, resampler.ToWaveProvider16());
        }
        wavStream.Seek(0, SeekOrigin.Begin);
        using var file = new FileStream(wavFilePath, FileMode.Open);
        await wavStream.CopyToAsync(file);
    }
}
