using System;
using System.Linq.Expressions;
using JetBrains.Application.Settings.Store;

namespace EveningCreek.ReSharper.ExternalCode
{
    public static class SettingsAccessor
    {
        public static readonly Expression<Func<SettingsKey, IIndexedEntry<string, string>>> Paths = key => key.Paths;
    }
}