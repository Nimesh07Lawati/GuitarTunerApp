using Plugin.AudioRecorder;
using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using GuitarTuner.Services;

namespace GuitarTuner.ViewModel
{
    public partial class HomePageViewModel : BindableObject
    {
        private readonly AudioRecorderService _recorder;
        private readonly FrequencyDetector _frequencyDetector;
        private double _frequency;
        private string _note;
        private string _tuningStatus;
        private bool _isListening;

        public event PropertyChangedEventHandler PropertyChanged;

        public double Frequency
        {
            get => _frequency;
            set
            {
                _frequency = value;
                OnPropertyChanged();
                UpdateTuningStatus();
            }
        }

        public string Note
        {
            get => _note;
            set
            {
                _note = value;
                OnPropertyChanged();
            }
        }

        public string TuningStatus
        {
            get => _tuningStatus;
            set
            {
                _tuningStatus = value;
                OnPropertyChanged();
            }
        }

        public bool IsListening
        {
            get => _isListening;
            set
            {
                _isListening = value;
                OnPropertyChanged();
            }
        }

        public HomePageViewModel()
        {
            _recorder = new AudioRecorderService { StopRecordingOnSilence = true };
            _frequencyDetector = new FrequencyDetector();
        }

        public async Task StartTuning()
        {
            if (IsListening) return;
            IsListening = true;
            TuningStatus = "Listening...";

            try
            {
                if (!_recorder.IsRecording)
                {
                    await _recorder.StartRecording();

                    await Task.Run(async () =>
                    {
                        while (IsListening)
                        {
                            if (!_recorder.IsRecording) break;

                            string filePath = _recorder.GetAudioFilePath();
                            Frequency = _frequencyDetector.DetectFrequency(filePath);
                            Note = _frequencyDetector.GetNearestNote(Frequency);

                            await Task.Delay(500);
                        }
                    });
                }
            }
            catch (Exception ex)
            {
                TuningStatus = "Error: " + ex.Message;
            }
        }

        public async Task StopTuning()
        {
            IsListening = false;
            if (_recorder.IsRecording)
            {
                await _recorder.StopRecording();
            }
            TuningStatus = "Stopped";
        }

        private void UpdateTuningStatus()
        {
            double targetFreq = _frequencyDetector.GetStandardFrequency(Note);
            double tolerance = 1.0;

            if (Math.Abs(Frequency - targetFreq) <= tolerance)
            {
                TuningStatus = "Perfectly Tuned ✅";
            }
            else if (Frequency < targetFreq)
            {
                TuningStatus = "Tune Up ⬆️";
            }
            else
            {
                TuningStatus = "Tune Down ⬇️";
            }
        }

      
    }
}
