using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Documents;
using JetBrains.Application.FileSystemTracker;
using JetBrains.Application.Settings;
using JetBrains.DataFlow;
using JetBrains.DocumentManagers;
using JetBrains.DocumentModel;
using JetBrains.IDE;
using JetBrains.ProjectModel;
using JetBrains.ProjectModel.Properties;
using JetBrains.ReSharper.ExternalSources.ReSharperIntegration;
using JetBrains.ReSharper.Feature.Services.Goto;
using JetBrains.ReSharper.Feature.Services.Navigation;
using JetBrains.ReSharper.Feature.Services.Navigation.Navigation.NavigationProviders;
using JetBrains.ReSharper.Feature.Services.Navigation.Occurences;
using JetBrains.ReSharper.Feature.Services.Navigation.Search;
using JetBrains.ReSharper.Feature.Services.Occurences;
using JetBrains.ReSharper.Feature.Services.Occurences.Presentation;
using JetBrains.ReSharper.Features.StructuralSearch.Finding;
using JetBrains.ReSharper.I18n.Services.Navigation;
using JetBrains.ReSharper.Psi;
using JetBrains.ReSharper.Psi.ExtensionsAPI.Caches2;
using JetBrains.ReSharper.Psi.JavaScript.Impl.Tree;
using JetBrains.ReSharper.Psi.Resolve;
using JetBrains.ReSharper.Psi.Services.StructuralSearch.Impl;
using JetBrains.ReSharper.Psi.Xml.Impl.Tree;
using JetBrains.Text;
using JetBrains.UI.PopupMenu;
using JetBrains.UI.PopupWindowManager;
using JetBrains.Util;
using TextRange = JetBrains.Util.TextRange;

namespace EveningCreek.ReSharper.ExternalCode
{
    [JetBrains.ReSharper.Psi.ShellFeaturePartAttribute()]
    public class ExternalFiles : IGotoFileProvider
    {
        private bool _isInitialised = false;
        private readonly ISettingsStore _settingsStore;
        private readonly IFileSystemTracker _fileSystemTracker;
        private readonly Lifetime _lifetime;
        private readonly ISettingsChangedEventSource _settingsChangedEventSource;

        private IDictionary<FileSystemPath, MatchingInfo> trackedFiles = new Dictionary<FileSystemPath, MatchingInfo>();


        public ExternalFiles(ISettingsStore settingsStore, IFileSystemTracker fileSystemTracker, Lifetime lifetime, ISettingsChangedEventSource settingsChangedEventSource)
        {
            _settingsStore = settingsStore;
            this._fileSystemTracker = fileSystemTracker;
            this._lifetime = lifetime;
            this._settingsChangedEventSource = settingsChangedEventSource;
        }

        public bool IsApplicable(INavigationScope scope, GotoContext gotoContext, IdentifierMatcher matcher)
        {
            initialiseFileWatchers(scope);
            return true;
        }

        private void initialiseFileWatchers(INavigationScope scope)
        {
            if (_isInitialised)
            {
                return;
            }
            _isInitialised = true;

            var solutionFilePath = scope.GetSolution().SolutionFilePath;


            var settingsKey = _settingsStore
                .BindToContextTransient(ContextRange.ApplicationWide)
                .GetKey<ExternalCodeSettingsKey>(SettingsOptimization.OptimizeDefault);

            _settingsChangedEventSource.Changed.Advise(_lifetime, (arg) =>
            {
                if (arg.ChangedEntries.Any(_ => _.LocalName == "Paths" && _.Parent.LocalName == "ExternalCode"))
                    updateFilePaths(settingsKey, solutionFilePath);
            });

            updateFilePaths(settingsKey, solutionFilePath);
        }

        private void updateFilePaths(ExternalCodeSettingsKey settingsKey, FileSystemPath solutionFilePath)
        {
            IEnumerable<string> paths = settingsKey
                .Paths
                .EnumIndexedValues()
                .Select(x => x.Value.Trim())
                .Distinct();

            FileSystemPath[] fileSystemPaths = paths
                .Select(FileSystemPath.TryParse)
                .Select(x => x.ToAbsolutePath(solutionFilePath.Directory))
                .ToArray();

            foreach (var fileSystemPath in fileSystemPaths)
            {
                loadFilesForPath(fileSystemPath);
                _fileSystemTracker.AdviseFileChanges(_lifetime, fileSystemPath, OnChangeAction);
            }
        }

        private void loadFilesForPath(FileSystemPath fileSystemPath)
        {
            if (!fileSystemPath.ExistsDirectory)
                return;

            var filesInPath = Directory.GetFiles(fileSystemPath.FullPath, "*.*", SearchOption.TopDirectoryOnly);
            foreach (var file in filesInPath)
            {
                var systemPath = FileSystemPath.Parse(file);
                if (systemPath.ExistsFile)
                {
                    trackedFiles[systemPath] = infoForFile(file);
                }
            }
        }

        private static MatchingInfo infoForFile(string file)
        {
            return new MatchingInfo(file, new[] { new IdentifierMatch(), });
        }

        private void OnChangeAction(FileSystemChangeDelta fileSystemChangeDelta)
        {
            foreach (var systemChangeDelta in fileSystemChangeDelta.GetChildren())
            {
                OnChangeAction(systemChangeDelta);
            }

            if (!fileSystemChangeDelta.NewPath.ExistsFile)
                return;

            if (fileSystemChangeDelta.ChangeType == FileSystemChangeType.DELETED)
            {
                if (trackedFiles.ContainsKey(fileSystemChangeDelta.OldPath))
                {
                    trackedFiles.Remove(fileSystemChangeDelta.OldPath);
                }

                return;
            }

            handleAddRename(fileSystemChangeDelta);
        }

        private void handleAddRename(FileSystemChangeDelta fileSystemChangeDelta)
        {
            if (fileSystemChangeDelta.OldPath != null)
            {
                if (trackedFiles.ContainsKey(fileSystemChangeDelta.OldPath))
                {
                    trackedFiles.Remove(fileSystemChangeDelta.OldPath);
                }
            }
            if (fileSystemChangeDelta.NewPath != null)
            {
                trackedFiles[fileSystemChangeDelta.NewPath] = infoForFile(fileSystemChangeDelta.NewPath.FullPath);
            }
        }

        public IEnumerable<MatchingInfo> FindMatchingInfos(IdentifierMatcher matcher, INavigationScope scope,
            GotoContext gotoContext,
            Func<bool> checkForInterrupt)
        {
            return trackedFiles.Where(_ => _.Key.Name.Contains(matcher.Filter)).Select(_ => _.Value);
        }

        public IEnumerable<IOccurence> GetOccurencesByMatchingInfo(MatchingInfo navigationInfo, INavigationScope scope,
            GotoContext gotoContext,
            Func<bool> checkForInterrupt)
        {
            var fileSystemPath = FileSystemPath.Parse(navigationInfo.Identifier);

            return new List<IOccurence>()
            {                
                new FileOccurence(fileSystemPath, new TextRange(), new SimpleMenuItem() {Text = fileSystemPath.Name, }),
            };
        }

        public Func<int, int> ItemsPriorityFunc { get; private set; }
    }
}