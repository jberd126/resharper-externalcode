using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Linq.Expressions;
using System.Windows.Forms;
using JetBrains.Application;
using JetBrains.Application.Interop.NativeHook;
using JetBrains.Application.Settings;
using JetBrains.Application.Settings.Store;
using JetBrains.CommonControls;
using JetBrains.CommonControls.Validation;
using JetBrains.DataFlow;
using JetBrains.ReSharper.ExternalSources.Resources;
using JetBrains.Threading;
using JetBrains.UI.Application;
using JetBrains.UI.Controls;
using JetBrains.UI.Options;
using JetBrains.UI.Options.Helpers;

namespace EveningCreek.ReSharper.ExternalSources
{
    /// <summary>
    ///     Options page for external sources shown in "Code Inspection" category group.
    /// </summary>
    /// <remarks>
    ///     The options page intended to display just after "Generated Code" options which has sequence value of 1.
    /// </remarks>
    [OptionsPage(Pid, "External Sources", typeof(ExternalSourcesThemedIcons.ExternalSources), ParentId = "CodeInspection", Sequence = 1.01)]
    public class ExternalSourceOptionsPage : AStackPanelOptionsPage
    {
        public const string Pid = "CodeInspectionExternalSourceSettings";

        private const int _margin = 10;        
        private readonly FormValidators _formValidators;
        private readonly Lifetime _lifetime;
        private readonly IMainWindow _mainWindow;
        private readonly OptionsSettingsSmartContext _settings;
        private readonly IWindowsHookManager _windowsHookManager;
        private StringCollectionEdit _externalCodePathsCollectionEdit;

        public ExternalSourceOptionsPage(
            IUIApplication environment,
            OptionsSettingsSmartContext settings,
            Lifetime lifetime,
            IShellLocks shellLocks,
            IWindowsHookManager windowsHookManager,
            FormValidators formValidators,
            IMainWindow mainWindow = null)
            : base(lifetime, environment, Pid)
        {
            _settings = settings;
            _lifetime = lifetime;
            _windowsHookManager = windowsHookManager;
            _formValidators = formValidators;
            _mainWindow = mainWindow;

            InitControls();
            shellLocks.QueueRecurring(lifetime, "Force settings merge", TimeSpan.FromMilliseconds(300.0), () => OnOk());
        }

        private void InitControls()
        {
            using(new LayoutSuspender(this))
            {
                var tablePanel = new TableLayoutPanel
                                 {
                                     AutoSizeMode = AutoSizeMode.GrowAndShrink,
                                     Margin = Padding.Empty,
                                     Padding = Padding.Empty,
                                     Size = ClientSize
                                 };
                tablePanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100f));
                tablePanel.RowStyles.Add(new RowStyle(SizeType.AutoSize));
                tablePanel.RowStyles.Add(new RowStyle(SizeType.Percent, 50f));
                Controls.Add(tablePanel);

                GroupingEvent sizeEvent = Environment.Threading.GroupingEvents[Rgc.Invariant].CreateEvent(
                    _lifetime,
                    Pid + ".SizeChanged",
                    TimeSpan.FromMilliseconds(300.0), () =>
                                                      {
                                                          var clientSize = new Size(ClientSize.Width - _margin, ClientSize.Height - _margin);
                                                          if(!clientSize.Equals(tablePanel.Size))
                                                          {
                                                              using(new LayoutSuspender(this))
                                                              {
                                                                  tablePanel.Size = clientSize;
                                                              }
                                                          }
                                                      });
                EventHandler handler = (sender, args) => sizeEvent.FireIncoming();
                _lifetime.AddBracket(() => SizeChanged += handler, () => SizeChanged -= handler);

                var titleLabel = new Controls.Label("External source paths relative to project.")
                                 {
                                     AutoSize = true,
                                     Dock = DockStyle.Fill
                                 };
                tablePanel.Controls.Add(titleLabel);

                string[] externalCodePaths = _settings.EnumIndexedValues(ExternalSourceSettingsAccessor.Paths).ToArray();
                _externalCodePathsCollectionEdit = new StringCollectionEdit(Environment, "External source paths:", null, _mainWindow, _windowsHookManager, _formValidators)
                                                   {
                                                       Dock = DockStyle.Fill
                                                   };
                _externalCodePathsCollectionEdit.Items.Value = externalCodePaths;
                tablePanel.Controls.Add(_externalCodePathsCollectionEdit, 0, 1);
            }
        }

        /// <summary>
        ///     Invoked when OK button in the options dialog is pressed.
        ///     If the page returns <c>false</c>, the the options dialog won't be closed, and focus will be put into this page.
        /// </summary>
        public override bool OnOk()
        {
            Expression<Func<ExternalSourceSettingsKey, IIndexedEntry<string, string>>> generatedFileMasks = key => key.Paths;

            string[] addedPaths = _externalCodePathsCollectionEdit.Items.Value;
            var currentPaths = new HashSet<string>();
            foreach(string currentPath in _settings.EnumEntryIndices(generatedFileMasks))
            {
                if(!addedPaths.Contains(currentPath))
                {
                    _settings.RemoveIndexedValue(generatedFileMasks, currentPath);
                }
                else
                {
                    currentPaths.Add(currentPath);
                }
            }
            foreach(string entryIndex in addedPaths.Where(x => !currentPaths.Contains(x)))
            {
                _settings.SetIndexedValue(generatedFileMasks, entryIndex, entryIndex);
            }

            return base.OnOk();
        }
    }
}