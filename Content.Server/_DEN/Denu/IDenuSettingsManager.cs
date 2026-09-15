using System;
using System.Threading.Tasks;
using Content.Shared._DEN.Denu;

namespace Content.Server._DEN.Denu;

public interface IDenuSettingsManager
{
    Task<DenuSettingsRoot> LoadSettingsAsync(Guid userId);
    Task SaveSettingsAsync(Guid userId, DenuSettingsRoot settings);
}
