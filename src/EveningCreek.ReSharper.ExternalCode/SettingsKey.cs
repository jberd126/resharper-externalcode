using JetBrains.Application.Settings;
using JetBrains.Application.Settings.Store;
using JetBrains.ReSharper.Settings;

namespace EveningCreek.ReSharper.ExternalCode
{
    [SettingsKey(typeof(CodeInspectionSettings), "External Code")]
    public class SettingsKey
    {
        [SettingsIndexedEntry("Paths")]
        public IIndexedEntry<string, string> Paths { get; set; }
    }
}