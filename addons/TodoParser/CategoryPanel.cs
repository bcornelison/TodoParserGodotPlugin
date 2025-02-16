#if TOOLS
using Godot;

using static TodoParserGodotPlugin.Util.Enums;

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Globalization;
using System.Data;
using System.Threading;

namespace TodoParserGodotPlugin {
	[Tool]
	// TODO(LOWEST|EXAMPLE): LOWEST Priority
	// TODO(LOW|EXAMPLE): LOW Priority
	// TODO(MEDIUM|EXAMPLE): MEDIUM Priority
	// TODO(HIGH|EXAMPLE): HIGH Priority
	// TODO(CRITICAL|EXAMPLE): CRITICAL Priority
	public partial class CategoryPanel : Control {
		[Export] private RichTextLabel categoryLabel;
		[Export] private Tree todoList;
		public TodoVisualizer todoVisualizer;
		public string category;
		private List<string> filesInCategory = [];
		private List<ToDo> todosInCategory = [];
		private TreeItem root;
		private readonly List<string> parsedFiles = [];
		private readonly List<string> parsedContents = [];
		private TreeItem fileSubtree = null;
		public List<ToDo> loadedData;
		public override void _EnterTree() {
			todoVisualizer.OnCategorySelected += OnCategorySelected;
			todoVisualizer.OnPrioritySelected += OnPrioritySelected;
			todoVisualizer.OnListContainerResized += OnContainerResized;
			todoList.ItemActivated += TodoClicked;

			categoryLabel.Text = $"[center]{category}[/center]";

			foreach(ToDo todo in loadedData) {
				if(!todo.Categories.Contains(category)) continue;
				todosInCategory.Add(todo);
				if(!filesInCategory.Contains(todo.FileName)) filesInCategory.Add(todo.FileName);
			}
			
			PopulateTree();
			OnContainerResized();
		}
		public override void _ExitTree() {
			todoVisualizer.OnCategorySelected -= OnCategorySelected;
			todoVisualizer.OnPrioritySelected -= OnPrioritySelected;
			todoVisualizer.OnListContainerResized -= OnContainerResized;
			todoList.ItemActivated -= TodoClicked;
		}
		
		private void OnContainerResized() {
			float parentXSize = GetParent<HFlowContainer>().Size.X;
			if(parentXSize <= 0) return;
			CustomMinimumSize = new((parentXSize / 4) - 10, (parentXSize / 3) - 20);
		}
		private void PopulateTree(PRIORITY selectedPriority = PRIORITY.ALL) {
			fileSubtree = null;
			parsedContents.Clear();
			parsedFiles.Clear();
			todoList.Clear();
			root = todoList.CreateItem();
			todoList.HideRoot = true;
			foreach(string file in filesInCategory) {
				if(!parsedFiles.Contains(file) || fileSubtree == null) {
					string partialFileName;
					if(file.Contains('/')) partialFileName = file.Split('/')[^1];
					else partialFileName = file.Split('\\')[^1];
					fileSubtree = todoList.CreateItem(root);
					fileSubtree.SetText(0, partialFileName);
					fileSubtree.SetTooltipText(0, file);
					parsedFiles.Add(file);
				}
				foreach(ToDo todo in todosInCategory) {
					if(todo.FileName == file) {
						if(fileSubtree == null) continue;
						if(parsedContents.Contains(todo.Contents)) continue;
						if(todo.Priority != selectedPriority && selectedPriority != PRIORITY.ALL) continue;
						TreeItem todoItem = todoList.CreateItem(fileSubtree);
						todoItem.SetText(0, todo.Contents);
						todoItem.SetTooltipText(0, $"Line: {todo.FileLine}\n{todo.Contents}");
						todoItem.SetCustomColor(0, todoVisualizer.PriorityColors[todo.Priority]);
						parsedContents.Add(todo.Contents);
					}
				}
			}
		}
		
		private void OnCategorySelected(string selectedCategory) {
			PopulateTree();
			if(selectedCategory == category) Visible = true;
			else Visible = false;
		}
		private void OnPrioritySelected(PRIORITY selectedPriority) {
			bool hasTodoWithPriority = false;
			foreach(ToDo todo in todosInCategory) {
				if(todo.Priority == selectedPriority) hasTodoWithPriority = true;
			}
			if(hasTodoWithPriority || selectedPriority == PRIORITY.ALL) Visible = true;
			else {
				Visible = false; 
				return;
			}

			PopulateTree(selectedPriority);
		}
		private void TodoClicked() {
			string project = Main.Instance.CodeRootPath;
			string file = todoList.GetSelected().GetParent().GetTooltipText(0);
			string lineNumber = todoList.GetSelected().GetTooltipText(0).Split("\n")[0].Split(": ")[1];
			string col = string.Empty;
			string execCommand = Main.Instance.CodeEditorPath;
			string execArgs = Main.Instance.CustomEditorArgs;

			// Argument parsing logic below from Godot source code: 
			// https://github.com/godotengine/godot/blob/96cdbbe5bd5e068953b5e972daaee37850686c31/modules/mono/editor/GodotTools/GodotTools/GodotSharpEditor.cs
			List<string> args = [];
			int from = 0;
			int numChars = 0;
			bool insideQuotes = false;
			bool hasFileFlag = false;
			execArgs = execArgs.ReplaceN("{line}", lineNumber.ToString(CultureInfo.InvariantCulture));
			execArgs = execArgs.ReplaceN("{col}", col.ToString(CultureInfo.InvariantCulture));
			execArgs = execArgs.StripEdges(true, true);
			execArgs = execArgs.Replace("\\\\", "\\", StringComparison.Ordinal);

			for (int i = 0; i < execArgs.Length; ++i) {
				if (execArgs[i] == '"' && (i == 0 || execArgs[i - 1] != '\\') && i != execArgs.Length - 1) {
					if (!insideQuotes) {
						from++;
					}
					insideQuotes = !insideQuotes;
				} else if ((execArgs[i] == ' ' && !insideQuotes) || i == execArgs.Length - 1) {
					if (i == execArgs.Length - 1 && !insideQuotes) {
						numChars++;
					}

					var arg = execArgs.Substr(from, numChars);
					if (arg.Contains("{file}", StringComparison.OrdinalIgnoreCase)) {
						hasFileFlag = true;
					}

					arg = arg.ReplaceN("{project}", project);
					arg = arg.ReplaceN("{file}", file);
					args.Add(arg);

					from = i + 1;
					numChars = 0;
				} else {
					numChars++;
				}
			}

			if (!hasFileFlag) {
				args.Add(file);
			}
			OS.Execute(execCommand, [.. args]);
		}
	}
}
#endif