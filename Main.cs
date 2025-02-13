using Godot;
using GC = Godot.Collections;

using static CodeTodoVisualizer.Util.Enums;

using System;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace CodeTodoVisualizer {
    // TODO(LOW|FEATURE|ASSIGNED|JOHNDOE): Implement proper error handling instead of printing to console
    // TODO(FEATURE|ASSIGNED|JANEDOE): Allow excluding files/folders (Regex?)
    // TODO(FEATURE|ASSIGNED|JANEDOE): Allow choosing to include subfolders (included by default)
    public partial class Main : Node {
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
        [Export] public ImportSettings importSettingsInstance;
        [Export] public TodoVisualizer todoVisualizerInstance;
        [Export] private PanelContainer progressPopup;
        [Export] private Label progressPopupLabel;
        public static Main Instance { get; private set; }
        public static string ConfigSectionName { get {return "IMPORTCONFIG";} private set{}}
        private bool loadedSettings;
        public string CodeRootPath { get; private set; }
        public PROGRAMMINGLANGUAGES SelectedCodeLanguage { get; private set; }
        public CATEGORYDELIMITERS SelectedDelimiter { get; private set; }
        private List<ToDo> parsedTodos;
        public override void _EnterTree() {
            importSettingsInstance.OnImportClicked += ParseFiles;
            todoVisualizerInstance.OnReScanButtonPressed += ParseFiles;
            todoVisualizerInstance.OnImportSettingsPressed += OpenSettings;

            Instance ??= this;

            loadedSettings = LoadSettings();
            MakeVisible();
        }
        public override void _ExitTree() {
            importSettingsInstance.OnImportClicked -= ParseFiles;
            todoVisualizerInstance.OnReScanButtonPressed -= ParseFiles;
            todoVisualizerInstance.OnImportSettingsPressed -= OpenSettings;

            Instance = null;
        }

        public void MakeVisible() {
            if(!loadedSettings)  {
                importSettingsInstance.Visible = true;
                todoVisualizerInstance.Visible = false;
            } else {
                ParseFiles();
            }
        }

        public bool LoadSettings() {
            if(!Godot.FileAccess.FileExists("user://ParseSettings.cfg")) return false;

            ConfigFile config = new();
            Error err = config.Load("user://ParseSettings.cfg");
            if(err != Error.Ok) {
                // Implement 'Proper' Error handling
                return false;
            }

            SelectedDelimiter = (CATEGORYDELIMITERS)(int)config.GetValue(ConfigSectionName, "category_delimiter");
            CodeRootPath = (string)config.GetValue(ConfigSectionName, "code_root_path");
            SelectedCodeLanguage = (PROGRAMMINGLANGUAGES)(int)config.GetValue(ConfigSectionName, "code_language", (int)PROGRAMMINGLANGUAGES.ALL);
            return true;
        }
        private void ParseFiles() {
            if(!LoadSettings()) return;

            string regexPattern = LanguageCommentRegex[SelectedCodeLanguage];
            regexPattern += @"(\btodo[(](.+?)[)]:?(.+))";

            parsedTodos = [];
            string[] files = Directory.GetFiles(CodeRootPath, "*.*", SearchOption.AllDirectories);

            progressPopupLabel.Text = $"Parsing files in {CodeRootPath}";
            progressPopup.Visible = true;

            foreach(string file in files) {
                if(!LanguageFileExtensions[SelectedCodeLanguage].Contains($"{file.GetExtension()}")) continue;

                Godot.FileAccess openedFile = Godot.FileAccess.Open(file, Godot.FileAccess.ModeFlags.Read);
                if(openedFile == null) {
                    GD.PrintErr($"There was an error processing file: {file}");
                    GD.PrintErr($"{Godot.FileAccess.GetOpenError()}");
                    continue;
                }

                uint currentLineIndex = 0;
                progressPopupLabel.Text = $"Parsing: {file}";
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
                progressPopupLabel.Text = $"Parsing {file} complete";
            }
            progressPopupLabel.Text = "Finished Parsing All Files";
            todoVisualizerInstance.LoadData(parsedTodos);

            Task.Delay(500);
            progressPopup.Visible = false;
            importSettingsInstance.Visible = false;
            todoVisualizerInstance.Visible = true;
        }
        private void OpenSettings() {
            importSettingsInstance.Visible = true;
            todoVisualizerInstance.Visible = false;
        }
    }
}