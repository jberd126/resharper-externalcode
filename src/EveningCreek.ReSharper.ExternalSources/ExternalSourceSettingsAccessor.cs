using System;
using System.Linq.Expressions;
using JetBrains.Application.Settings.Store;

namespace EveningCreek.ReSharper.ExternalSources
{
    public static class ExternalSourceSettingsAccessor
    {
        public static readonly Expression<Func<ExternalSourceSettingsKey, IIndexedEntry<string, string>>> Paths = key => key.Paths;
    }
}