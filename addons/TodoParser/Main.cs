#if TOOLS
using Godot;
using GC = Godot.Collections;

using static TodoParserGodotPlugin.Util.Enums;

using System;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace TodoParserGodotPlugin {
    [Tool]
    // TODO(LOW|FEATURE|ASSIGNED|JOHNDOE): Implement proper error handling instead of printing to console
    // TODO(FEATURE|ASSIGNED|JANEDOE): Allow excluding files/folders (Regex?)
    public partial class Main : EditorPlugin {
        public GC.Dictionary<PROGRAMMINGLANGUAGES, string> LanguageCommentRegex = new() {
            { PROGRAMMINGLANGUAGES.ALL, @"(\/\/|#|""""""|\/\*)\s?" },
            { PROGRAMMINGLANGUAGES.C, @"(\/\/|\/\*)\s?" },
            { PROGRAMMINGLANGUAGES.CPP, @"(\/\/|\/\*)\s?" },
            { PROGRAMMINGLANGUAGES.CSHARP, @"(\/\/|\/\*)\s?" },
            { PROGRAMMINGLANGUAGES.GDSCRIPT, @"(#)\s?" },
            { PROGRAMMINGLANGUAGES.GOLANG, @"(\/\/|\/\*)\s?" },
            { PROGRAMMINGLANGUAGES.JAVA, @"(\/\/|\/\*)\s?" },
            { PROGRAMMINGLANGUAGES.PYTHON, @"(#|"""""")\s?" },
            { PROGRAMMINGLANGUAGES.RUBY, @"(#)\s?" },
            { PROGRAMMINGLANGUAGES.RUST, @"(\/\/|\/\*)\s?" },
        };
        public  GC.Dictionary<PROGRAMMINGLANGUAGES, GC.Array<string>> LanguageFileExtensions = new() {
            { PROGRAMMINGLANGUAGES.ALL, ["c","h", //C
                "c", "i", "ii", "cc", "cp", "cxx", "cpp", "CPP", "c++", "C", "hh", "H", //CPP
                "hp", "hxx", "hpp", "HPP", "h++", "tcc", //CPP cont'd
                "cs", //C#
                "gd", //GDScript
                "go", //GoLang
                "java", //Java
                "py", "py3", "pyw", //Python
                "rb", //Ruby
                "rs"] }, //Rust
            { PROGRAMMINGLANGUAGES.C, ["c","h"] },
            { PROGRAMMINGLANGUAGES.CPP, ["c", "i", "ii", "h", "cc", "cp", "cxx", "cpp", "CPP", 
                "c++", "C", "hh", "H", "hp", "hxx", "hpp", "HPP", "h++", "tcc"] },
            { PROGRAMMINGLANGUAGES.CSHARP, ["cs"] },
            { PROGRAMMINGLANGUAGES.GDSCRIPT, ["gd"] },
            { PROGRAMMINGLANGUAGES.GOLANG, ["go"] },
            { PROGRAMMINGLANGUAGES.JAVA, ["java"] },
            { PROGRAMMINGLANGUAGES.PYTHON, ["py", "py3", "pyw"] },
            { PROGRAMMINGLANGUAGES.RUBY, ["rb"] },
            { PROGRAMMINGLANGUAGES.RUST, ["rs"] },
        };
        public const string PLUGINPATH = "res://addons/TodoParser/";
        private PackedScene importSettingsScene = ResourceLoader.Load<PackedScene>($"{PLUGINPATH}import_settings.tscn");
        public ImportSettings importSettingsInstance;
        private PackedScene todoVisualizersScene = ResourceLoader.Load<PackedScene>($"{PLUGINPATH}todo_visualizer.tscn");
        public TodoVisualizer todoVisualizerInstance;
        // private PanelContainer progressPopup;
        // private Label progressPopupLabel;
        public static Main Instance { get; private set; }
        public static string ConfigSectionName { get {return "IMPORTCONFIG";} private set{}}
        public bool LoadedSettings { get; private set; }
        public string CodeRootPath { get; private set; }
        public PROGRAMMINGLANGUAGES SelectedCodeLanguage { get; private set; }
        public CATEGORYDELIMITERS SelectedDelimiter { get; private set; }
        public bool AllowRecursive { get; private set; }
		public string ExcludeFilter { get; private set; }
		public string CodeEditorPath { get; private set; }
		public string CustomEditorArgs { get; private set; }
        private List<ToDo> parsedTodos;
        public override void _EnterTree() {
            LoadedSettings = LoadSettings();

            importSettingsInstance = importSettingsScene.Instantiate<ImportSettings>();
            importSettingsInstance.Visible = false;
            EditorInterface.Singleton.GetEditorMainScreen().AddChild(importSettingsInstance);
            todoVisualizerInstance = todoVisualizersScene.Instantiate<TodoVisualizer>();
            todoVisualizerInstance.Visible = false;
            EditorInterface.Singleton.GetEditorMainScreen().AddChild(todoVisualizerInstance);

            importSettingsInstance.OnImportClicked += ParseFiles;
            todoVisualizerInstance.OnReScanButtonPressed += ParseFiles;
            todoVisualizerInstance.OnImportSettingsPressed += OpenSettings;

            Instance ??= this;
        }
        public override void _ExitTree() {
            importSettingsInstance.OnImportClicked -= ParseFiles;
            todoVisualizerInstance.OnReScanButtonPressed -= ParseFiles;
            todoVisualizerInstance.OnImportSettingsPressed -= OpenSettings;

            Instance = null;
        }

        public override bool _HasMainScreen(){
            return true;
        }

        public override void _MakeVisible(bool visible) {
            if(!visible) {
                importSettingsInstance.Visible = visible;
                todoVisualizerInstance.Visible = visible;
                return;
            }
            if(!LoadedSettings)  {
                importSettingsInstance.Visible = visible;
                todoVisualizerInstance.Visible = !visible;
            } else {
                ParseFiles();
            }
        }

        public override string _GetPluginName(){
            return "Todo Parser";
        }

        public override Texture2D _GetPluginIcon(){
            return EditorInterface.Singleton.GetBaseControl().GetThemeIcon("ResourcePreloader", "EditorIcons");
        }

        public bool LoadSettings() {
            if(!Godot.FileAccess.FileExists("user://ParseSettings.cfg")) return false;

            ConfigFile config = new();
            Error err = config.Load("user://ParseSettings.cfg");
            if(err != Error.Ok) {
                // Implement 'Proper' Error handling
                return false;
            }

            CodeRootPath = (string)config.GetValue(ConfigSectionName, "code_root_path");
            SelectedCodeLanguage = (PROGRAMMINGLANGUAGES)(int)config.GetValue(ConfigSectionName, "code_language", (int)PROGRAMMINGLANGUAGES.ALL);
            SelectedDelimiter = (CATEGORYDELIMITERS)(int)config.GetValue(ConfigSectionName, "category_delimiter");
            AllowRecursive = (bool)config.GetValue(ConfigSectionName, "allow_recursive", true);
			ExcludeFilter = (string)config.GetValue(ConfigSectionName, "exclude_filter", string.Empty);
			CodeEditorPath = (string)EditorInterface.Singleton.GetEditorSettings().GetSetting("dotnet/editor/custom_exec_path");
			CustomEditorArgs = (string)EditorInterface.Singleton.GetEditorSettings().GetSetting("dotnet/editor/custom_exec_path_args");

            return true;
        }
        private void ParseFiles() {
            if(!LoadSettings()) return;

            string regexPattern = LanguageCommentRegex[SelectedCodeLanguage];
            regexPattern += @"(\btodo[(](.+?)[)]:?(.+))";

            parsedTodos = [];
            SearchOption searchOption = AllowRecursive ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly;
            GD.Print($"searchOption: {searchOption}");

            string[] files = Directory.GetFiles(CodeRootPath, "*.*", searchOption);

            // progressPopupLabel.Text = $"Parsing files in {CodeRootPath}";
            // progressPopup.Visible = true;

            foreach(string file in files) {
                if(!LanguageFileExtensions[SelectedCodeLanguage].Contains($"{file.GetExtension()}")) continue;

                Godot.FileAccess openedFile = Godot.FileAccess.Open(file, Godot.FileAccess.ModeFlags.Read);
                if(openedFile == null) {
                    GD.PrintErr($"There was an error processing file: {file}");
                    GD.PrintErr($"{Godot.FileAccess.GetOpenError()}");
                    continue;
                }

                uint currentLineIndex = 0;
                // progressPopupLabel.Text = $"Parsing: {file}";
                while(!openedFile.EofReached()) {
                    currentLineIndex++;
                    string currentLineContents = openedFile.GetLine();

                    if(!Regex.IsMatch(currentLineContents, regexPattern, RegexOptions.IgnoreCase)) continue;

                    Match matchedRegex = Regex.Match(currentLineContents, regexPattern, RegexOptions.IgnoreCase);
                    GroupCollection matchedGroups = matchedRegex.Groups;
                    
                    string delimiter = SelectedDelimiter switch {
                        CATEGORYDELIMITERS.PIPE => "|",
                        CATEGORYDELIMITERS.HYPHEN => "-",
                        CATEGORYDELIMITERS.UNDERSCORE => "_",
                        CATEGORYDELIMITERS.COMMA => ",",
                        _ => "|",
                    };

                    string[] todoCategories = matchedGroups[3].Value.Split(delimiter);
                    string todoContents = matchedGroups[4].Value.Trim();

                    PRIORITY parsedPriority = PRIORITY.MEDIUM;
                    GC.Array<string> parsedCategories = [];

                    foreach (string parsedCategory in todoCategories) {
                        if(Enum.TryParse(parsedCategory, true, out PRIORITY priority)) {
                            parsedPriority = priority;
                        } else {
                            parsedCategories.Add(parsedCategory);
                        }
                    }
                    ToDo currentTodo = new(parsedCategories, parsedPriority, file, currentLineIndex, todoContents);
                    parsedTodos.Add(currentTodo);
                }
                // progressPopupLabel.Text = $"Parsing {file} complete";
            }
            // progressPopupLabel.Text = "Finished Parsing All Files";
            todoVisualizerInstance.LoadData(parsedTodos);

            // Task.Delay(500);
            // progressPopup.Visible = false;
            importSettingsInstance.Visible = false;
            todoVisualizerInstance.Visible = true;
        }
        private void OpenSettings() {
            importSettingsInstance.Visible = true;
            todoVisualizerInstance.Visible = false;
        }
    }
}
#endif