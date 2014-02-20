using System.Collections.Generic;
using System.Linq;
using JetBrains.Application;
using JetBrains.Application.FileSystemTracker;
using JetBrains.Application.Settings;
using JetBrains.DataFlow;
using JetBrains.DocumentManagers;
using JetBrains.ProjectModel;
using JetBrains.PsiFeatures.VisualStudio.Core.GeneratedFolders.Common;
using JetBrains.ReSharper.Psi;
using JetBrains.ReSharper.Psi.Impl;
using JetBrains.ReSharper.Psi.Modules;
using JetBrains.Util;

namespace EveningCreek.ReSharper.ExternalCode
{
    [SolutionComponent]
    public class FilesProvider : IProjectPsiModuleProviderFilter
    {
        private readonly ChangeManager _changeManager;
        private readonly DocumentManager _documentManager;
        private readonly IFileSystemTracker _fileSystemTracker;
        private readonly IProjectFileExtensions _projectFileExtensions;
        private readonly PsiProjectFileTypeCoordinator _projectFileTypeCoordinator;
        private readonly ISettingsStore _settingsStore;
        private readonly IShellLocks _shellLocks;

        public FilesProvider(
            IProjectFileExtensions projectFileExtensions,
            PsiProjectFileTypeCoordinator projectFileTypeCoordinator,
            ChangeManager changeManager,
            IFileSystemTracker fileSystemTracker,
            IShellLocks shellLocks,
            DocumentManager documentManager,
            ISettingsStore settingsStore)
        {
            _projectFileExtensions = projectFileExtensions;
            _projectFileTypeCoordinator = projectFileTypeCoordinator;
            _changeManager = changeManager;
            _fileSystemTracker = fileSystemTracker;
            _shellLocks = shellLocks;
            _documentManager = documentManager;
            _settingsStore = settingsStore;
        }

        public JetTuple<IProjectPsiModuleHandler, IPsiModuleDecorator> OverrideHandler(Lifetime lifetime, IProject project, IProjectPsiModuleHandler handler)
        {
            if (project.ProjectProperties.ProjectKind != ProjectKind.REGULAR_PROJECT ||
                project.ProjectFileLocation.IsNullOrEmpty())
            {
                return null;
            }

            SettingsKey settingsKey = _settingsStore
                .BindToContextTransient(ContextRange.ApplicationWide)
                .GetKey<SettingsKey>(SettingsOptimization.OptimizeDefault);

            IEnumerable<string> paths = settingsKey
                .Paths
                .EnumIndexedValues()
                .Select(x => x.Value.Trim())
                .Distinct();
            FileSystemPath[] fileSystemPaths = paths
                .Select(FileSystemPath.TryParse)
                .Where(x => !x.IsEmpty)
                .Select(x => x.ToAbsolutePath(project.Location))
                .ToArray();

            var projectHandler = new GeneratedFilesProjectHandler(
                _shellLocks,
                _documentManager,
                _projectFileExtensions,
                _projectFileTypeCoordinator,
                lifetime,
                project,
                handler,
                _changeManager,
                _fileSystemTracker,
                f => new ExternalSourceFileProperties(project, f), 
                project.GetResolveContext(),
                fileSystemPaths);
            return new JetTuple<IProjectPsiModuleHandler, IPsiModuleDecorator>(projectHandler, new GeneratedFilesProjectDecorator(projectHandler));
        }

        private class ExternalSourceFileProperties : DefaultPropertiesForFileInProject
        {
            private readonly IPsiSourceFile _sourceFile;

            public ExternalSourceFileProperties(IProject project, IPsiSourceFile sourceFile)
                : base(project, sourceFile.GetPsiModule())
            {
                _sourceFile = sourceFile;
            }

            public override bool ShouldBuildPsi
            {
                get { return !_sourceFile.LanguageType.IsNullOrUnknown(); }
            }

            public override bool IsNonUserFile
            {
                get { return true; }
            }

            public override bool ProvidesCodeModel
            {
                get { return ShouldBuildPsi; }
            }

            public override bool IsGeneratedFile
            {
                get { return true; }
            }

            public override bool IsICacheParticipant
            {
                get { return true; }
            }
        }
    }
}