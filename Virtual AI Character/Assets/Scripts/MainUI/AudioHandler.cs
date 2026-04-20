// Microsoft Azure Speech service

using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Networking;
using Microsoft.CognitiveServices.Speech;
using Microsoft.CognitiveServices.Speech.Audio;
using System.Threading.Tasks;

public class AudioHandler : MonoBehaviour
{
    public delegate void SpeechToTextCallback(string result);
    public delegate void TextToSpeechCallback(AudioClip result);

    private string localTtsUrl = "http://127.0.0.1:8000/tts";
    private float localRequestTimeout = 12f;

    private string azureSubscriptionKey = "1SXl35qi911Sqd0iE6aphYpvHMf4NaiB2cp6NulSOBnVxxnggb5qJQQJ99BJAC5T7U2XJ3w3AAAYACOGOHbN";
    private string azureServiceRegion = "francecentral";
    private string azureVoiceName = "en-US-AnaNeural";

    //  Speech → Text  (Azure)-------------------------------------------------

    void Awake()
    {
        DontDestroyOnLoad(gameObject);
    }

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

        var config = SpeechConfig.FromSubscription(azureSubscriptionKey, azureServiceRegion);
        using (var audioInput = AudioInputStream.CreatePushStream())
        {
            audioInput.Write(wavBytes);
            audioInput.Close();

            using (var audioConfig = AudioConfig.FromStreamInput(audioInput))
            using (var recognizer = new SpeechRecognizer(config, audioConfig))
            {
                var result = await recognizer.RecognizeOnceAsync().ConfigureAwait(false);

                if (result.Reason == ResultReason.RecognizedSpeech)
                    return result.Text;
                if (result.Reason == ResultReason.NoMatch)
                    return "No speech could be recognized.";
                if (result.Reason == ResultReason.Canceled)
                {
                    var cancellation = CancellationDetails.FromResult(result);
                    return $"Speech recognition canceled: {cancellation.Reason}";
                }
                return "Speech recognition failed.";
            }
        }
    }

    //  Text → Speech (Try local first, if failed to Azure) -----------------------------------------------------
    public void SpeakText(string text, TextToSpeechCallback callback)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            Debug.LogWarning("AudioHandler: SpeakText called with empty text.");
            callback?.Invoke(null);
            return;
        }

        StartCoroutine(SpeakTextCoroutine(text, callback));
    }

    public IEnumerator SpeakTextCoroutine(string text, TextToSpeechCallback callback)
    {
        // Try local
        using (UnityWebRequest req = CreateLocalTtsRequest(text))
        {
            req.timeout = Mathf.RoundToInt(localRequestTimeout);

            Debug.Log($"Trying local TTS server");
            yield return req.SendWebRequest();

            if (req.result == UnityWebRequest.Result.Success)
            {
                AudioClip clip = DownloadHandlerAudioClip.GetContent(req);
                if (clip != null && clip.length > 0.05f)
                {
                    Debug.Log("Local TTS succeeded");
                    callback?.Invoke(clip);
                    yield break;
                }
            }

            Debug.LogWarning($"Local TTS server failed ({req.result}): {req.error} → falling back to Azure");
        }

        // Try Azure
        var azureTask = TextToSpeechAzureAsync(text);
        while (!azureTask.IsCompleted) yield return null;

        float[] samples = azureTask.Result;
        if (samples == null || samples.Length == 0)
        {
            Debug.LogError("Azure failed.");
            callback?.Invoke(null);
            yield break;
        }

        AudioClip azureClip = AudioClip.Create("AzureTTS", samples.Length, 1, 16000, false);
        azureClip.SetData(samples, 0);
        callback?.Invoke(azureClip);
    }

    private UnityWebRequest CreateLocalTtsRequest(string text)
    {
        WWWForm form = new WWWForm();
        form.AddField("text", text);

        var request = UnityWebRequest.Post(localTtsUrl, form);
        request.downloadHandler = new DownloadHandlerAudioClip(localTtsUrl, AudioType.WAV);
        request.SetRequestHeader("Accept", "audio/wav");
        request.timeout = 120;

        return request;
    }

    private async Task<float[]> TextToSpeechAzureAsync(string text)
    {
        try
        {
            var config = SpeechConfig.FromSubscription(azureSubscriptionKey, azureServiceRegion);
            config.SpeechSynthesisVoiceName = azureVoiceName;

            using (var synthesizer = new SpeechSynthesizer(config, null))
            {
                var result = await synthesizer.SpeakTextAsync(text).ConfigureAwait(false);

                if (result.Reason == ResultReason.SynthesizingAudioCompleted)
                {
                    return ConvertAudioDataToFloat(result.AudioData);
                }

                if (result.Reason == ResultReason.Canceled)
                {
                    var cancellation = SpeechSynthesisCancellationDetails.FromResult(result);
                    Debug.LogError($"Azure TTS canceled: {cancellation.Reason} - {cancellation.ErrorDetails}");
                }
                else
                {
                    Debug.LogError($"Azure TTS failed: {result.Reason}");
                }

                return null;
            }
        }
        catch (Exception ex)
        {
            Debug.LogError($"Azure TTS exception: {ex.Message}");
            return null;
        }
    }

    //  Helpers --------------------------------------------------------
    private byte[] AudioClipToWav(AudioClip clip)
    {
        int sampleCount = clip.samples * clip.channels;
        float[] samples = new float[sampleCount];
        clip.GetData(samples, 0);

        byte[] pcmData = new byte[sampleCount * 2];
        const int rescaleFactor = 32767;

        for (int i = 0; i < sampleCount; i++)
        {
            short intData = (short)(samples[i] * rescaleFactor);
            byte[] bytes = BitConverter.GetBytes(intData);
            pcmData[i * 2] = bytes[0];
            pcmData[i * 2 + 1] = bytes[1];
        }

        using (var ms = new System.IO.MemoryStream())
        {
            int hz = 16000;
            int channels = 1;

            ms.Write(System.Text.Encoding.UTF8.GetBytes("RIFF"), 0, 4);
            ms.Write(BitConverter.GetBytes(36 + pcmData.Length), 0, 4);
            ms.Write(System.Text.Encoding.UTF8.GetBytes("WAVE"), 0, 4);
            ms.Write(System.Text.Encoding.UTF8.GetBytes("fmt "), 0, 4);
            ms.Write(BitConverter.GetBytes(16), 0, 4);
            ms.Write(BitConverter.GetBytes((short)1), 0, 2);
            ms.Write(BitConverter.GetBytes((short)channels), 0, 2);
            ms.Write(BitConverter.GetBytes(hz), 0, 4);
            ms.Write(BitConverter.GetBytes(hz * channels * 2), 0, 4);
            ms.Write(BitConverter.GetBytes((short)(channels * 2)), 0, 2);
            ms.Write(BitConverter.GetBytes((short)16), 0, 2);
            ms.Write(System.Text.Encoding.UTF8.GetBytes("data"), 0, 4);
            ms.Write(BitConverter.GetBytes(pcmData.Length), 0, 4);
            ms.Write(pcmData, 0, pcmData.Length);

            return ms.ToArray();
        }
    }

    private float[] ConvertAudioDataToFloat(byte[] audioBytes)
    {
        int samples = audioBytes.Length / 2;
        float[] floats = new float[samples];

        for (int i = 0; i < samples; i++)
        {
            short s = (short)(audioBytes[i * 2] | (audioBytes[i * 2 + 1] << 8));
            floats[i] = s / 32768f;
        }
        return floats;
    }
}