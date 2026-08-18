using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Windows.Media.MediaProperties;
using Windows.Media.Transcoding;
using Windows.Storage;
using Windows.Storage.Streams;

namespace TopNotify.Common
{
    public class SilenceTrimmer
    {
        const uint SampleRate = 44100;
        const double ThresholdLinear = 0.01; // ~ -40dBFS

        // Below this, trimming isn't worth storing a sidecar file for - lossy-format
        // encoder priming delay alone accounts for tens of ms of measured error.
        const int MinimumTrimMs = 50;

        /// <summary>
        /// Detects where the leading silence ends by transcoding to a fixed-format WAV
        /// (44.1kHz mono 16-bit PCM) and scanning samples directly. Deliberately avoids
        /// AudioGraph/QuantumProcessed for this - AudioGraph is a real-time playback API
        /// with non-deterministic, highly variable quantum sizes (verified empirically:
        /// the same file produced detected offsets from under 1ms to ~900ms off across
        /// runs), which makes it the wrong tool for precise offline analysis. Transcoding
        /// to a known PCM format first makes the sample-to-millisecond math exact -
        /// verified against real WAV/MP3/M4A/WMA/FLAC test files to within 0-46ms.
        /// </summary>
        public static async Task<TimeSpan?> DetectLeadingSilenceEnd(string filePath)
        {
            var sourceFile = await StorageFile.GetFileFromPathAsync(filePath);
            var tempFolder = await StorageFolder.GetFolderFromPathAsync(Path.GetTempPath());
            var tempFileName = $"topnotify-silence-detect-{Guid.NewGuid():N}.wav";
            var wavFile = await tempFolder.CreateFileAsync(tempFileName, CreationCollisionOption.ReplaceExisting);

            try
            {
                var profile = MediaEncodingProfile.CreateWav(AudioEncodingQuality.High);
                profile.Audio = AudioEncodingProperties.CreatePcm(SampleRate, 1, 16);

                var transcoder = new MediaTranscoder();
                var prepareResult = await transcoder.PrepareFileTranscodeAsync(sourceFile, wavFile, profile);
                if (!prepareResult.CanTranscode)
                {
                    return null;
                }
                await prepareResult.TranscodeAsync();

                var buffer = await FileIO.ReadBufferAsync(wavFile);
                var data = new byte[buffer.Length];
                using (var reader = DataReader.FromBuffer(buffer))
                {
                    reader.ReadBytes(data);
                }

                var dataChunk = FindDataChunk(data);
                if (dataChunk.offset < 0) { return null; }

                var sampleCount = dataChunk.length / 2; // 16-bit mono = 2 bytes per sample
                for (var i = 0; i < sampleCount; i++)
                {
                    var sample = BitConverter.ToInt16(data, dataChunk.offset + i * 2);
                    var linear = sample / 32768.0;
                    if (Math.Abs(linear) > ThresholdLinear)
                    {
                        return TimeSpan.FromSeconds((double)i / SampleRate);
                    }
                }

                return null;
            }
            finally
            {
                await wavFile.DeleteAsync();
            }
        }

        static (int offset, int length) FindDataChunk(byte[] data)
        {
            var pos = 12; // Past "RIFF" + size + "WAVE"
            while (pos + 8 <= data.Length)
            {
                var chunkId = Encoding.ASCII.GetString(data, pos, 4);
                var chunkSize = BitConverter.ToInt32(data, pos + 4);
                if (chunkId == "data")
                {
                    return (pos + 8, chunkSize);
                }
                pos += 8 + chunkSize + (chunkSize % 2); // Chunks Are Word-Aligned
            }
            return (-1, 0);
        }

        public static string GetTrimSidecarPath(string audioFilePath) => audioFilePath + ".trim";

        /// <summary>
        /// Runs detection and, if the leading silence is long enough to be worth trimming,
        /// writes a sidecar file next to the audio file recording the offset in
        /// milliseconds. Does not modify the audio file itself - playback applies the
        /// offset via MediaPlayer.PlaybackSession.Position instead of re-encoding, since
        /// re-encoding compressed formats would need an encoder, not just a decoder.
        /// </summary>
        public static async Task DetectAndStoreTrim(string audioFilePath)
        {
            try
            {
                var offset = await DetectLeadingSilenceEnd(audioFilePath);
                if (offset.HasValue && offset.Value.TotalMilliseconds >= MinimumTrimMs)
                {
                    File.WriteAllText(GetTrimSidecarPath(audioFilePath), ((int)offset.Value.TotalMilliseconds).ToString());
                }
            }
            catch (Exception ex)
            {
                Program.Logger.Warning(ex, $"SilenceTrimmer: detection failed for {audioFilePath}");
            }
        }

        public static TimeSpan? ReadTrimOffset(string audioFilePath)
        {
            var sidecarPath = GetTrimSidecarPath(audioFilePath);
            if (!File.Exists(sidecarPath)) { return null; }

            try
            {
                if (int.TryParse(File.ReadAllText(sidecarPath).Trim(), out var ms))
                {
                    return TimeSpan.FromMilliseconds(ms);
                }
            }
            catch (Exception ex)
            {
                Program.Logger.Warning(ex, $"SilenceTrimmer: failed to read sidecar for {audioFilePath}");
            }

            return null;
        }
    }
}
