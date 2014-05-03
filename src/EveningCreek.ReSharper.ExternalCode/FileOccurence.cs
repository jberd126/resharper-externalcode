using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using JetBrains.IDE;
using JetBrains.ProjectModel;
using JetBrains.ReSharper.Feature.Services.Navigation.Search;
using JetBrains.ReSharper.Feature.Services.Occurences;
using JetBrains.ReSharper.Psi;
using JetBrains.UI.PopupMenu;
using JetBrains.UI.PopupWindowManager;
using JetBrains.Util;

namespace EveningCreek.ReSharper.ExternalCode
{
    public class FileOccurence : IOccurence
    {
        private readonly FileSystemPath myFilePath;
        private readonly TextRange myTextRange;
        private readonly IMenuItemDescriptor myCachedPresentation;

        public IMenuItemDescriptor CachedPresentation
        {
            get
            {
                return this.myCachedPresentation;
            }
        }

        public TextRange TextRange
        {
            get
            {
                return this.myTextRange;
            }
        }

        public ProjectModelElementEnvoy ProjectModelElementEnvoy
        {
            get
            {
                return (ProjectModelElementEnvoy)null;
            }
        }

        public DeclaredElementEnvoy<ITypeMember> TypeMember
        {
            get
            {
                return (DeclaredElementEnvoy<ITypeMember>)null;
            }
        }

        public DeclaredElementEnvoy<ITypeElement> TypeElement
        {
            get
            {
                return (DeclaredElementEnvoy<ITypeElement>)null;
            }
        }

        public DeclaredElementEnvoy<INamespace> Namespace
        {
            get
            {
                return (DeclaredElementEnvoy<INamespace>)null;
            }
        }

        public OccurenceType OccurenceType
        {
            get
            {
                return OccurenceType.Compiled;
            }
        }

        public bool IsValid
        {
            get { return true; }
        }

        public object MergeKey
        {
            get
            {
                return myFilePath.ToString();
            }
        }

        public IList<IOccurence> MergedItems
        {
            get
            {
                return (IList<IOccurence>)EmptyList<IOccurence>.InstanceList;
            }
        }

        public OccurencePresentationOptions PresentationOptions { get; set; }

        public FileOccurence(FileSystemPath filePath, TextRange textRange, IMenuItemDescriptor cachedPresentation)
        {
            this.myFilePath = filePath;
            this.myTextRange = textRange;
            this.myCachedPresentation = cachedPresentation;
            this.PresentationOptions = OccurencePresentationOptions.DefaultOptions;
        }

        public bool Navigate(ISolution solution, PopupWindowContextSource windowContext, bool transferFocus, TabOptions tabOptions)
        {
            var txtControl = EditorManager.GetInstance(solution).OpenFile(myFilePath, true, TabOptions.NormalTab);
            return true;
        }

        public string DumpToString()
        {
            return string.Format("[DFO]. Path: {0} \nRange: {1}", (object)this.myFilePath, (object)this.myTextRange);
        }
    }
}
