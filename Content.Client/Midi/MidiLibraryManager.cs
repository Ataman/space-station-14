using System.IO;
using Robust.Shared.ContentPack;
using Robust.Shared.Utility;

namespace Content.Client.Midi;

/// <summary>
/// Handles storage/management of the users MIDI files stored inside their data directory.
/// </summary>
public sealed partial class MidiLibraryManager
{
    private static readonly ResPath UserMidiDirectory = new("/UserMidis/");

    [Dependency] private IResourceManager _resManager = default!;

    private readonly List<string> _trackList = [];

    public event Action<string>? MidiFileAdded;
    public event Action<string>? MidiFileRemoved;
    public event Action? MidiFilesReset;

    public MidiLibraryManager()
    {
        EnsureMidiDirectoryExists();
        LoadStoredUserMidis();
    }

    public byte[] GetMidiData(string fileName)
    {
        try
        {
            var filePath = new ResPath(UserMidiDirectory + fileName);
            return _resManager.UserData.ReadAllBytes(filePath);
        }
        catch
        {
            return [];
        }
    }

    public IEnumerable<string> GetMidiTracks()
    {
        return _trackList;
    }

    public async void AddMidiFile(string fileName, Stream data)
    {
        try
        {
            EnsureMidiDirectoryExists();
            await using var file = _resManager.UserData.OpenWrite(new ResPath(UserMidiDirectory + fileName));
            await data.CopyToAsync(file);
            _trackList.Add(fileName);
            MidiFileAdded?.Invoke(fileName);
        }
        catch
        {
            // ignored
        }
    }

    public void RenameMidiFile(string oldName, string newName)
    {
        try
        {
            EnsureMidiDirectoryExists();
            var oldPath = new ResPath(UserMidiDirectory + oldName);
            var newPath = new ResPath(UserMidiDirectory + newName);
            oldPath = oldPath.Clean();
            newPath = newPath.Clean();
            _resManager.UserData.Rename(oldPath, newPath);
            _trackList.Remove(oldName);
            MidiFileRemoved?.Invoke(oldName);
            _trackList.Add(newName);
            MidiFileAdded?.Invoke(newName);
        }
        catch
        {
            // ignored
        }
    }

    public void RemoveMidiFile(string fileName)
    {
        DeleteMidiFile(fileName);
        _trackList.Remove(fileName);
        MidiFileRemoved?.Invoke(fileName);
    }

    public void RemoveAllMidiFiles()
    {
        foreach (var fileName in _trackList)
        {
            DeleteMidiFile(fileName);
        }
        _trackList.Clear();
        MidiFilesReset?.Invoke();
    }

    private void DeleteMidiFile(string fileName)
    {
        try
        {
            var path = new ResPath(UserMidiDirectory + fileName).Clean();
            _resManager.UserData.Delete(path);
        }
        catch
        {
            // ignored
        }
    }

    private void LoadStoredUserMidis()
    {
        _trackList.Clear();
        if (!_resManager.UserData.IsDir(UserMidiDirectory))
            return;

        foreach (var path in _resManager.UserData.DirectoryEntries(UserMidiDirectory))
        {
            try
            {
                var filePath = new ResPath(UserMidiDirectory + path);
                if (!filePath.Extension.Equals("midi") && !filePath.Extension.Equals("mid"))
                    continue;

                _trackList.Add(filePath.Filename);
            }
            catch
            {
                // ignored
            }
        }

        MidiFilesReset?.Invoke();
    }

    private void EnsureMidiDirectoryExists()
    {
        if (!_resManager.UserData.Exists(UserMidiDirectory))
            _resManager.UserData.CreateDir(UserMidiDirectory);
    }
}
