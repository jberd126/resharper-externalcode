using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using JetBrains.ReSharper.Feature.Services.Navigation.Search;
using JetBrains.ReSharper.Feature.Services.Occurences;
using JetBrains.ReSharper.Feature.Services.Occurences.Presentation;
using JetBrains.UI.PopupMenu;

namespace EveningCreek.ReSharper.ExternalCode
{
    [OccurencePresenter]
    public class FileOccurencePresenter : IOccurencePresenter
    {
        public bool Present(IMenuItemDescriptor descriptor, IOccurence occurence, OccurencePresentationOptions occurencePresentationOptions)
        {
            FileOccurence decompiledFileOccurence = occurence as FileOccurence;
            if (decompiledFileOccurence == null)
                return false;
            if (decompiledFileOccurence.CachedPresentation != null)
            {
                IMenuItemDescriptor cachedPresentation = decompiledFileOccurence.CachedPresentation;
                descriptor.Icon = cachedPresentation.Icon;
                descriptor.Text = cachedPresentation.Text;
                descriptor.ShortcutText = cachedPresentation.ShortcutText;
                descriptor.TailGlyph = cachedPresentation.TailGlyph;
                descriptor.Tooltip = cachedPresentation.Tooltip;
                descriptor.Tag = ((object)occurence);
                descriptor.Style = cachedPresentation.Style;
            }
            return true;
        }

        public bool IsApplicable(IOccurence occurence)
        {
            return occurence is FileOccurence;
        }
    }
}
