using Godot;

using static CodeTodoVisualizer.Util.Enums;

using System;
using System.Runtime.CompilerServices;

namespace CodeTodoVisualizer {
	public partial class ImportSettings : Control {
		public delegate void ImportClicked();
		public ImportClicked OnImportClicked;
		[Export] private Button chooseCodeDirectoryButton;
		[Export] private OptionButton languageOptionButton;
		[Export] private OptionButton categoryDelimiterButton;
		[Export] private Button importButton;
		[Export] private FileDialog fileDialog;

		private string codeDirectory;
		private PROGRAMMINGLANGUAGES selectedLanguage;
		private CATEGORYDELIMITERS selectedDelimiter;

		public override void _EnterTree() {
			VisibilityChanged += OnVisibilityChanged;
			languageOptionButton.Clear();
			foreach(string language in Enum.GetNames<PROGRAMMINGLANGUAGES>()) {
				languageOptionButton.AddItem(language);
			}
			chooseCodeDirectoryButton.Pressed += OnChooseCodeDirectoryButtonPressed;
            languageOptionButton.ItemSelected += OnLanguageSelected;
			categoryDelimiterButton.ItemSelected += OnDelimiterSelected;
			importButton.Pressed += OnImportButtonPressed;
			fileDialog.DirSelected += OnFileDialogConfirmed;
		}
		public override void _ExitTree() {
			VisibilityChanged -= OnVisibilityChanged;
			chooseCodeDirectoryButton.Pressed -= OnChooseCodeDirectoryButtonPressed;
            languageOptionButton.ItemSelected -= OnLanguageSelected;
			categoryDelimiterButton.ItemSelected -= OnDelimiterSelected;
			importButton.Pressed -= OnImportButtonPressed;
			fileDialog.DirSelected -= OnFileDialogConfirmed;
		}
		private void OnVisibilityChanged() {
			if(!Visible) return;
			if(Main.Instance.CodeRootPath != string.Empty) {
				codeDirectory = Main.Instance.CodeRootPath;
				chooseCodeDirectoryButton.Text = $"{codeDirectory}";
				selectedLanguage = Main.Instance.SelectedCodeLanguage;
				languageOptionButton.Selected = (int)selectedLanguage;
			}
		}

		private void OnChooseCodeDirectoryButtonPressed() {
			fileDialog.Show();
		}
		private void OnFileDialogConfirmed(string dir) {
			codeDirectory = dir;
			chooseCodeDirectoryButton.Text = $"{codeDirectory}";
		}
		private void OnLanguageSelected(long indexSelected) {
			selectedLanguage = (PROGRAMMINGLANGUAGES)indexSelected;
		}
		private void OnDelimiterSelected(long indexSelected) {
			selectedDelimiter = (CATEGORYDELIMITERS)indexSelected;
		}
		private void OnImportButtonPressed() {
			SaveImportSettingsConfig();
			
			OnImportClicked?.Invoke();
		}
		private void SaveImportSettingsConfig() {
			ConfigFile configFile = new();
			configFile.SetValue(Main.ConfigSectionName, "category_delimiter", (int)selectedDelimiter);
			configFile.SetValue(Main.ConfigSectionName, "code_root_path", ProjectSettings.GlobalizePath(codeDirectory));
			configFile.SetValue(Main.ConfigSectionName, "code_language", (int)selectedLanguage);
			configFile.Save("user://ParseSettings.cfg");
		}
	}
}