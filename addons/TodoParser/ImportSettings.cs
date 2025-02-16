#if TOOLS
using Godot;

using static TodoParserGodotPlugin.Util.Enums;

using System;
using System.Runtime.CompilerServices;

namespace TodoParserGodotPlugin {
	[Tool]
	public partial class ImportSettings : Control {
		public delegate void ImportClicked();
		public ImportClicked OnImportClicked;
		[Export] private Button chooseCodeDirectoryButton;
		[Export] private CheckButton allowRecursiveToggle;
		[Export] private OptionButton languageOptionButton;
		[Export] private OptionButton categoryDelimiterButton;
		[Export] private TextEdit excludeFilterTextEdit;
		[Export] private Button importButton;
		[Export] private FileDialog codeDirectoryDialog;

		private string codeDirectory;
		private PROGRAMMINGLANGUAGES selectedLanguage = PROGRAMMINGLANGUAGES.ALL;
		private CATEGORYDELIMITERS selectedDelimiter = CATEGORYDELIMITERS.PIPE;
		private bool allowRecursive = true;
		private string excludeFilter = string.Empty;

		public override void _Ready() {
			VisibilityChanged += OnVisibilityChanged;
			chooseCodeDirectoryButton.Pressed += OnChooseCodeDirectoryButtonPressed;
			codeDirectoryDialog.DirSelected += OnCodeDirectoryDialogConfirmed;
			allowRecursiveToggle.Pressed += OnAllowRecursiveToggled;
            languageOptionButton.ItemSelected += OnLanguageSelected;
			categoryDelimiterButton.ItemSelected += OnDelimiterSelected;
			excludeFilterTextEdit.TextChanged += OnExcludeFilterTextChanged;
			importButton.Pressed += OnImportButtonPressed;

			languageOptionButton.Clear();
			foreach(string language in Enum.GetNames<PROGRAMMINGLANGUAGES>()) {
				languageOptionButton.AddItem(language);
			}
		}
		public override void _ExitTree() {
			VisibilityChanged -= OnVisibilityChanged;
			chooseCodeDirectoryButton.Pressed -= OnChooseCodeDirectoryButtonPressed;
			codeDirectoryDialog.DirSelected -= OnCodeDirectoryDialogConfirmed;
			allowRecursiveToggle.Pressed -= OnAllowRecursiveToggled;
            languageOptionButton.ItemSelected -= OnLanguageSelected;
			categoryDelimiterButton.ItemSelected -= OnDelimiterSelected;
			excludeFilterTextEdit.TextChanged -= OnExcludeFilterTextChanged;
			importButton.Pressed -= OnImportButtonPressed;
		}
		private void OnVisibilityChanged() {
			if(!Visible) return;
			if(Main.Instance.LoadedSettings) {
			GD.Print($"|{Main.Instance.CodeRootPath}|");
				codeDirectory = Main.Instance.CodeRootPath;
				chooseCodeDirectoryButton.Text = $"{codeDirectory}";

				allowRecursive = Main.Instance.AllowRecursive;
				allowRecursiveToggle.ButtonPressed = allowRecursive;

				selectedLanguage = Main.Instance.SelectedCodeLanguage;
				languageOptionButton.Selected = (int)selectedLanguage;

				selectedDelimiter = Main.Instance.SelectedDelimiter;
				categoryDelimiterButton.Selected = (int)selectedDelimiter;

				excludeFilter = Main.Instance.ExcludeFilter;
				excludeFilterTextEdit.Text = excludeFilter;
			}
		}

		private void OnChooseCodeDirectoryButtonPressed() {
			codeDirectoryDialog.Show();
		}
		private void OnCodeDirectoryDialogConfirmed(string dir) {
			codeDirectory = dir;
			chooseCodeDirectoryButton.Text = $"{codeDirectory}";
		}
		private void OnAllowRecursiveToggled() {
			allowRecursive = allowRecursiveToggle.ButtonPressed;
		}
		private void OnLanguageSelected(long indexSelected) {
			selectedLanguage = (PROGRAMMINGLANGUAGES)indexSelected;
		}
		private void OnDelimiterSelected(long indexSelected) {
			selectedDelimiter = (CATEGORYDELIMITERS)indexSelected;
		}
		private void OnExcludeFilterTextChanged() {
			excludeFilter = excludeFilterTextEdit.Text;
		}
		private void OnImportButtonPressed() {
			SaveImportSettingsConfig();
			
			OnImportClicked?.Invoke();
		}
		private void SaveImportSettingsConfig() {
			ConfigFile configFile = new();
			configFile.SetValue(Main.ConfigSectionName, "code_root_path", ProjectSettings.GlobalizePath(codeDirectory));
			configFile.SetValue(Main.ConfigSectionName, "allow_recursive", allowRecursive);
			configFile.SetValue(Main.ConfigSectionName, "code_language", (int)selectedLanguage);
			configFile.SetValue(Main.ConfigSectionName, "category_delimiter", (int)selectedDelimiter);
			configFile.SetValue(Main.ConfigSectionName, "exclude_filter", excludeFilter);
			configFile.Save("user://ParseSettings.cfg");
		}
	}
}
#endif