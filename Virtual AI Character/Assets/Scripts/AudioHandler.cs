// Microsoft Azure Speech service

using System;
using System.Collections;
using System.IO;
using UnityEngine;
using System.Threading.Tasks;
using Microsoft.CognitiveServices.Speech;
using Microsoft.CognitiveServices.Speech.Audio;

public class AudioHandler : MonoBehaviour
{
    public delegate void SpeechToTextCallback(string result);
    public delegate void TextToSpeechCallback(AudioClip result);

    // Azure setup //
    private string subscriptionKey = "1SXl35qi911Sqd0iE6aphYpvHMf4NaiB2cp6NulSOBnVxxnggb5qJQQJ99BJAC5T7U2XJ3w3AAAYACOGOHbN";
    private string serviceRegion = "francecentral";

    // public AudioSource speechAudioSource;

    public void ProcessAudio(AudioClip clip, SpeechToTextCallback callback)
    {
        if (clip == null)
        {
            Debug.LogWarning("AudioHandler: Received null AudioClip");
            callback?.Invoke("No audio recorded");
            return;
        }
        StartCoroutine(ProcessAudioCoroutine(clip, callback));
    }

    private IEnumerator ProcessAudioCoroutine(AudioClip clip, SpeechToTextCallback callback)
    {
        string recognizedText = "Recognizing...";
        var task = SpeechToTextAsync(clip);
        while (!task.IsCompleted) yield return null;

        if (task.Exception != null)
        {
            recognizedText = $"Error: {task.Exception.Message}";
        }
        else recognizedText = task.Result;

        callback?.Invoke(recognizedText);
    }

    private async Task<string> SpeechToTextAsync(AudioClip clip)
    {
        byte[] wavBytes = AudioClipToWav(clip);

        var config = SpeechConfig.FromSubscription(subscriptionKey, serviceRegion);
        using (var audioInput = AudioInputStream.CreatePushStream())
        {
            audioInput.Write(wavBytes);
            audioInput.Close();

            using (var audioConfig = AudioConfig.FromStreamInput(audioInput))
            using (var recognizer = new SpeechRecognizer(config, audioConfig))
            {
                var result = await recognizer.RecognizeOnceAsync().ConfigureAwait(false);

                if (result.Reason == ResultReason.RecognizedSpeech)
                {
                    return result.Text;
                }
                else if (result.Reason == ResultReason.NoMatch)
                {
                    return "No speech could be recognized.";
                }
                else if (result.Reason == ResultReason.Canceled)
                {
                    var cancellation = CancellationDetails.FromResult(result);
                    return $"Speech recognition canceled: {cancellation.Reason}";
                }
                else
                {
                    return "Speech recognition failed.";
                }
            }
        }
    }

    private byte[] AudioClipToWav(AudioClip clip)
    {
        int sampleCount = clip.samples * clip.channels;
        float[] samples = new float[sampleCount];
        clip.GetData(samples, 0);

        byte[] pcmData = new byte[sampleCount * 2];
        int rescaleFactor = 32767; // convert float to Int16

        for (int i = 0; i < sampleCount; i++)
        {
            short intData = (short)(samples[i] * rescaleFactor);
            byte[] byteArr = BitConverter.GetBytes(intData);
            pcmData[i * 2] = byteArr[0];
            pcmData[i * 2 + 1] = byteArr[1];
        }

        using (MemoryStream memoryStream = new MemoryStream())
        {
            // WAV header
            int hz = 16000; // 16kHz
            int channels = 1; // mono

            // RIFF header
            memoryStream.Write(System.Text.Encoding.UTF8.GetBytes("RIFF"), 0, 4);
            memoryStream.Write(BitConverter.GetBytes(36 + pcmData.Length), 0, 4);
            memoryStream.Write(System.Text.Encoding.UTF8.GetBytes("WAVE"), 0, 4);
            // fmt subchunk
            memoryStream.Write(System.Text.Encoding.UTF8.GetBytes("fmt "), 0, 4);
            memoryStream.Write(BitConverter.GetBytes(16), 0, 4); // SubChunk1Size
            memoryStream.Write(BitConverter.GetBytes((short)1), 0, 2); // AudioFormat PCM
            memoryStream.Write(BitConverter.GetBytes((short)channels), 0, 2);
            memoryStream.Write(BitConverter.GetBytes(hz), 0, 4);
            memoryStream.Write(BitConverter.GetBytes(hz * channels * 2), 0, 4); // ByteRate
            memoryStream.Write(BitConverter.GetBytes((short)(channels * 2)), 0, 2); // BlockAlign
            memoryStream.Write(BitConverter.GetBytes((short)16), 0, 2); // BitsPerSample
            // data subchunk
            memoryStream.Write(System.Text.Encoding.UTF8.GetBytes("data"), 0, 4);
            memoryStream.Write(BitConverter.GetBytes(pcmData.Length), 0, 4);
            // Write PCM data
            memoryStream.Write(pcmData, 0, pcmData.Length);

            return memoryStream.ToArray();
        }
    }

    public void SpeakText(string text, TextToSpeechCallback callback)
    {
        if (string.IsNullOrEmpty(text))
        {
            Debug.LogWarning("AudioHandler: SpeakText called with empty text.");
            callback?.Invoke(null);
            return;
        }
        StartCoroutine(SpeakTextCoroutine(text, callback));
    }

    private IEnumerator SpeakTextCoroutine(string text, TextToSpeechCallback callback)
    {
        var task = TextToSpeechAsync(text);
        while (!task.IsCompleted) yield return null;

        float[] audioData = task.Result;
        if (audioData == null)
        {
            Debug.LogError($"Text-to-Speech error: {task.Exception.Message}");
        }
        else
        {
            int sampleCount = audioData.Length;
            AudioClip audioClip = AudioClip.Create("TTS_AudioClip", sampleCount, 1, 16000, false);
            audioClip.SetData(audioData, 0);
            callback?.Invoke(audioClip);
        }
    }

    private async Task<float[]> TextToSpeechAsync(string text)
    {
        var config = SpeechConfig.FromSubscription(subscriptionKey, serviceRegion);
        config.SpeechSynthesisVoiceName = "en-US-AnaNeural";

        using (var synthesizer = new SpeechSynthesizer(config, null))
        {
            var result = await synthesizer.SpeakTextAsync(text).ConfigureAwait(false);

            if (result.Reason == ResultReason.SynthesizingAudioCompleted)
            {
                return ConvertAudioDataToFloat(result.AudioData);
            }
            else if (result.Reason == ResultReason.Canceled)
            {
                var cancellation = SpeechSynthesisCancellationDetails.FromResult(result);
                Debug.LogError($"TTS canceled: {cancellation.Reason} - {cancellation.ErrorDetails}");
                return null;
            }
            else
            {
                Debug.LogError("TTS synthesis failed.");
                return null;
            }
        }
    }

    private float[] ConvertAudioDataToFloat(byte[] audioBytes)
    {
        int samples = audioBytes.Length / 2;
        float[] audioFloats = new float[samples];

        for (int i = 0; i < samples; i++)
        {
            short sample = (short)(audioBytes[i * 2] | (audioBytes[i * 2 + 1] << 8));
            audioFloats[i] = sample / 32768f;
        }
        return audioFloats;
    }
}