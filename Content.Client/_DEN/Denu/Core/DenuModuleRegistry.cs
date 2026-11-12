using System.Linq;
using Content.Shared._DEN.Denu;

namespace Content.Client._DEN.Denu.Core;

public sealed class DenuModuleRegistry
{
    private readonly List<IDenuModule> _modules = new();
    private DenuModuleContext? _context;

    public IReadOnlyList<IDenuModule> Modules => _modules;
    public DenuModuleContext? Context => _context;

    public void Initialize()
    {
        _context = new DenuModuleContext();
    }

    public void RegisterModule(IDenuModule module)
    {
        if (_modules.Any(m => m.Id == module.Id))
        {
            throw new InvalidOperationException($"Module with ID '{module.Id}' is already registered");
        }

        _modules.Add(module);
        _modules.Sort((a, b) => a.TabOrder.CompareTo(b.TabOrder));

        if (_context != null)
        {
            module.Initialize(_context);
        }
    }

    public void UnregisterModule(string id)
    {
        IDenuModule? module = _modules.FirstOrDefault(m => m.Id == id);

        if (module != null)
        {
            module.Dispose();
            _modules.Remove(module);
        }
    }

    public void ApplySync(DenuSettingsRoot settingsRoot, int currentProfileId)
    {
        if (_context == null)
        {
            return;
        }

        _context.ApplySync(settingsRoot, currentProfileId);
    }

    public void ApplySaveEcho(DenuSettingsRoot settingsRoot, int currentProfileId)
    {
        if (_context == null)
        {
            return;
        }

        _context.ApplySaveEcho(settingsRoot, currentProfileId);
    }

    public T? GetModule<T>() where T : class, IDenuModule
    {
        return _modules.OfType<T>().FirstOrDefault();
    }

    public IDenuModule? GetModule(string id)
    {
        return _modules.FirstOrDefault(m => m.Id == id);
    }

    public void Dispose()
    {
        foreach (IDenuModule module in _modules)
        {
            module.Dispose();
        }

        _modules.Clear();
        _context = null;
    }
}
