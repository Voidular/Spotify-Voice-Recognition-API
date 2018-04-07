using System;
using System.Speech.Recognition;
using System.Speech.Synthesis;
using SpotifyAPI.Local;

namespace SpotifyApi
{
    class VoiceRecognition : IDisposable
    {
        private readonly SpeechRecognitionEngine _speech;

        public VoiceRecognition(System.EventHandler<SpeechRecognizedEventArgs> speechRecongizedEvent, Choices choices)
        {
            _speech = new SpeechRecognitionEngine(new System.Globalization.CultureInfo("en-US"));
            _speech.LoadGrammarAsync(new Grammar(new GrammarBuilder(choices)));
            _speech.SetInputToDefaultAudioDevice();
            _speech.SpeechRecognized += speechRecongizedEvent;
        }

        public void Start()
        {
            _speech.RecognizeAsync(RecognizeMode.Multiple);
        }

        public void Stop()
        {
            _speech.RecognizeAsyncStop();
        }

        public void Dispose()
        {
            _speech.Dispose();
        }
    }

    class SpotifyListener : IDisposable
    {
        private static SpotifyLocalAPI _spotify;
        private readonly SpeechSynthesizer _speechSynthesizer;
        private readonly VoiceRecognition _voiceRecognition;

        SpotifyListener()
        {
            _voiceRecognition = new VoiceRecognition(SpotifySpeechRecognizedEvent,
                new Choices("Hello", "Wake up", "Spotify", "Spotify search", "Spotify pause", "Spotify play", "Pause",
                    "Play"));
            _speechSynthesizer = new SpeechSynthesizer();
            _speechSynthesizer.SetOutputToDefaultAudioDevice();
        }

        private async void SpotifySpeechRecognizedEvent(object sender, SpeechRecognizedEventArgs resultArgs)
        {
            if (resultArgs.Result.Text.Contains("Wake up"))
                Say("Yes?");
            if (resultArgs.Result.Text.Contains("Pause"))
            {
                if (!_spotify.GetStatus().Playing)
                    await _spotify.Play();
                else
                    Say(_spotify.GetStatus().Playing.ToString());
            }
            else if (resultArgs.Result.Text.Contains("Play"))
            {
                if (_spotify.GetStatus().Playing)
                    await _spotify.Pause();
                else
                    Say("There is nothing currently playing.");
            }
        }

        public void Say(string message)
        {
            _speechSynthesizer.Speak(message);
        }

        public static void Connect()
        {
            _spotify = new SpotifyLocalAPI(new SpotifyLocalAPIConfig
            {
                Port = 4371,
                HostUrl = "https://127.0.0.1"
            });
            _spotify.Connect();
        }

        public void Dispose()
        {
            _spotify.Dispose();
            _speechSynthesizer.Dispose();
            _voiceRecognition.Dispose();
        }

        private static void Main(string[] args)
        {
            Console.WriteLine("Spotify Interactive API has started to load...");
            Connect();
            var spotifyListener = new SpotifyListener();
            Console.ReadLine();

        }
    }
}