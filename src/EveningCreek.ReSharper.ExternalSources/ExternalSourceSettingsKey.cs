using JetBrains.Application.Settings;
using JetBrains.Application.Settings.Store;
using JetBrains.ReSharper.Settings;

namespace EveningCreek.ReSharper.ExternalSources
{
    [SettingsKey(typeof(CodeInspectionSettings), "External Sources")]
    public class ExternalSourceSettingsKey
    {
        [SettingsIndexedEntry("Paths")]
        public IIndexedEntry<string, string> Paths { get; set; }
    }
}