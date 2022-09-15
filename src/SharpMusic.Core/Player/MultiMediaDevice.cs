using System.Collections.ObjectModel;
using CSCore.CoreAudioAPI;
using CSCore.DirectSound;
using CSCore.SoundOut;

namespace SharpMusic.Core.Player;

public class MultiMediaDevice
{
    private ReadOnlyCollection<DirectSoundDevice>? _directSoundDevices;
    private MMDevice[]? _mmDevices;
    private WaveOutDevice[]? _waveOutDevices;
    private List<string>? _deviceNames;
    private string? _targetDevice;

    public MultiMediaDeviceType Type { get; set; }

    public void SetDevice(ISoundOut soundOut)
    {
        var index = _targetDevice is null ? 0 : (_deviceNames ??= GetSupportDevice().ToList()).IndexOf(_targetDevice);
        switch (soundOut)
        {
            case DirectSoundOut directSoundOut:
                directSoundOut.Device = DirectSoundDevices[index].Guid;
                break;
            case WasapiOut wasapiOut:
                wasapiOut.Device = MMDevices[index];
                break;
            case WaveOut waveOut:
                waveOut.Device = WaveOutDevices[index];
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    public IEnumerable<string> GetSupportDevice()
    {
        return Type switch
        {
            MultiMediaDeviceType.DirectSound => DirectSoundDevices.Select(i => i.Description),
            MultiMediaDeviceType.Wasapi => MMDevices.Select(i => i.FriendlyName),
            MultiMediaDeviceType.WaveOut => WaveOutDevices.Select(i => i.Name),
            _ => throw new ArgumentOutOfRangeException()
        };
    }

    public void SelectOutDevice(string target)
    {
        _targetDevice = target;
    }

    public string CurrentDeviceName => _targetDevice ?? GetSupportDevice().First();

    private IList<DirectSoundDevice> DirectSoundDevices =>
        _directSoundDevices ??= DirectSoundDeviceEnumerator.EnumerateDevices();

    // ReSharper disable once InconsistentNaming
    private IList<MMDevice> MMDevices => _mmDevices ??= MMDeviceEnumerator.EnumerateDevices(DataFlow.Render).ToArray();

    private IList<WaveOutDevice> WaveOutDevices => _waveOutDevices ??= WaveOutDevice.EnumerateDevices().ToArray();
}