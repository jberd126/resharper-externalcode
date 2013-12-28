using System;
using System.Collections.Generic;
using System.ComponentModel;
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

namespace EveningCreek.ReSharper.ExternalCode
{
    /// <summary>
    /// Options page for external code files shown in "Code Inspection" category group.
    /// </summary>
    /// <remarks>
    /// Sequence manually set to be just after "Generated Code" options which has sequence value of 1.
    /// </remarks>
    [OptionsPage(Pid, "Include External Code", typeof(ExternalSourcesThemedIcons.ExternalSources), ParentId = "CodeInspection", Sequence = 1.01)]
    public class ExternalCodeOptionsPage : AStackPanelOptionsPage
    {
        public const string Pid = "CodeInspectionExternalSettings";

        private readonly FormValidators _formValidators;
        private readonly Lifetime _lifetime;
        private readonly IMainWindow _mainWindow;
        private readonly OptionsSettingsSmartContext _settings;
        private readonly IWindowsHookManager _windowsHookManager;
        private StringCollectionEdit _externalCodePathsCollectionEdit;
        private const int _margin = 10;

        public ExternalCodeOptionsPage(IUIApplication environment, OptionsSettingsSmartContext settings, Lifetime lifetime, IShellLocks shellLocks, IWindowsHookManager windowsHookManager, FormValidators formValidators, IMainWindow mainWindow = null)
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

                var titleLabel = new Controls.Label("Add folder paths relative to project to external code files.")
                             {
                                 AutoSize = true,
                                 Dock = DockStyle.Fill
                             };
                tablePanel.Controls.Add(titleLabel);

                string[] externalCodePaths = _settings.EnumIndexedValues(ExternalCodeSettingsAccessor.ExternalCode).ToArray();
                _externalCodePathsCollectionEdit = new StringCollectionEdit(Environment, "External code paths:", null, _mainWindow, _windowsHookManager, _formValidators)
                                                   {
                                                       Dock = DockStyle.Fill
                                                   };
                _externalCodePathsCollectionEdit.Items.Value = externalCodePaths;
                _externalCodePathsCollectionEdit.Items.PropertyChanged += HandleExternalCodePathItemPropertyChanged;
                tablePanel.Controls.Add(_externalCodePathsCollectionEdit, 0, 1);
            }
        }

        private void HandleExternalCodePathItemPropertyChanged(object sender, PropertyChangedEventArgs propertyChangedEventArgs)
        {
            //            string invalidMask = _externalCodePathsCollectionEdit.Items.Value.FirstOrDefault(path => !IsValidPath(path));
            //            if (invalidMask != null)
            //            {
            //                MessageBox.ShowError(string.Format("Path \"{0}\" is not valid relative or absolute path.", invalidMask), "Cannot add external code path.");
            //                _externalCodePathsCollectionEdit.Items.Value = _externalCodePathsCollectionEdit.Items.Value.Where(s => s != invalidMask).ToArray();
            //            })
        }

        /// <summary>
        /// Invoked when OK button in the options dialog is pressed.
        /// If the page returns <c>false</c>, the the options dialog won't be closed, and focus will be put into this page.
        /// </summary>
        public override bool OnOk()
        {
            Expression<Func<ExternalCodeSettingsKey, IIndexedEntry<string, string>>> generatedFileMasks = key => key.ExternalCodePaths;

            string[] newValues = _externalCodePathsCollectionEdit.Items.Value;
            var addedAlreadyGeneratedFileMasks = new HashSet<string>();
            foreach(string entryIndex in _settings.EnumEntryIndices(generatedFileMasks))
            {
                if (!newValues.Contains(entryIndex))
                {
                    _settings.RemoveIndexedValue(generatedFileMasks, entryIndex);
                }
                else
                {
                    addedAlreadyGeneratedFileMasks.Add(entryIndex);
                }
            }
            foreach (string entryIndex in newValues.Where(x => !addedAlreadyGeneratedFileMasks.Contains(x)))
            {
                _settings.SetIndexedValue(generatedFileMasks, entryIndex, entryIndex);
            }

            return base.OnOk();
        }
    }
}