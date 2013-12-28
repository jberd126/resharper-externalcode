using JetBrains.Application.Settings;
using JetBrains.Application.Settings.Store;
using JetBrains.ReSharper.Settings;

namespace EveningCreek.ReSharper.ExternalCode
{
    [SettingsKey(typeof(CodeInspectionSettings), "Include external code")]
    public class ExternalCodeSettingsKey
    {
        [SettingsIndexedEntry("File paths")]
        public IIndexedEntry<string, string> ExternalCodePaths { get; set; }
    }
}