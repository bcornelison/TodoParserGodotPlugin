using Godot;

using static CodeTodoVisualizer.Util.Enums;

using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CodeTodoVisualizer {
	// TODO(HIGH|FEATURE|UNASSIGNED): When you click on a todo item, it should take user to source
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
		}
		public override void _ExitTree() {
			todoVisualizer.OnCategorySelected -= OnCategorySelected;
			todoVisualizer.OnPrioritySelected -= OnPrioritySelected;
			todoVisualizer.OnListContainerResized -= OnContainerResized;
			todoList.ItemActivated -= TodoClicked;
		}
		
		private void OnContainerResized() {
			GD.Print("Resizing CategoryPanels");
			CustomMinimumSize = new((GetParent<HFlowContainer>().Size.X / 5) - 10, (GetParent<HFlowContainer>().Size.X / 3) - 20);
			GD.Print(CustomMinimumSize);
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
			GD.Print($"{todoList.GetSelected().GetParent().GetText(0)}");
			GD.Print($"\t{todoList.GetSelected().GetTooltipText(0)}");
		}
	}
}