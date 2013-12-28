using System;
using System.Linq.Expressions;
using JetBrains.Application.Settings.Store;

namespace EveningCreek.ReSharper.ExternalCode
{
    public static class ExternalCodeSettingsAccessor
    {
        public static readonly Expression<Func<ExternalCodeSettingsKey, IIndexedEntry<string, string>>> ExternalCode = key => key.ExternalCodePaths;
    }
}