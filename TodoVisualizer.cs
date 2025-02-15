using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Reflection.Metadata;
using static TodoParserGodotPlugin.Util.Enums;

namespace TodoParserGodotPlugin {
	// TODO(BUG|FEATURE): Should categories be case-sensitive (priorities are not)
	public partial class TodoVisualizer : Control {
		public delegate void ReScanButtonPressed();
		public ReScanButtonPressed OnReScanButtonPressed;
		public delegate void ImportSettingsPressed();
		public ImportSettingsPressed OnImportSettingsPressed;
		public delegate void PrioritySelected(PRIORITY selectedPriority);
		public PrioritySelected OnPrioritySelected;
		public delegate void CategorySelected(string selectedItem);
		public CategorySelected OnCategorySelected;
		public delegate void ListContainerResized();
		public ListContainerResized OnListContainerResized;

		[Export] private ItemList todoCategoryList;
		[Export] private Button importSettingsButton;
		[Export] private Button reScanButton;
		[Export] private HFlowContainer categoryPanelContainer;
		[Export] private PackedScene categoryPanel;

		[ExportGroup("Priority Colors")]
			[Export] private Color LOWEST;
			[Export] private Color LOW;
            [Export] private Color MEDIUM;
            [Export] private Color HIGH;
            [Export] private Color CRITICAL;
			public Dictionary<PRIORITY,Color> PriorityColors { get; private set; }

		private List<string> parsedCategories = [];

		public override void _EnterTree() {
			PriorityColors = new() {
				{PRIORITY.LOWEST, LOWEST},
				{PRIORITY.LOW, LOW},
				{PRIORITY.MEDIUM, MEDIUM},
				{PRIORITY.HIGH, HIGH},
				{PRIORITY.CRITICAL, CRITICAL},
			};
			importSettingsButton.Pressed += OnImportSettingsButtonPressed;
			reScanButton.Pressed += OnReImportPressed;
			todoCategoryList.ItemSelected += OnCategoryListItemSelected;
			categoryPanelContainer.Resized += OnCategoryPanelContainerResized;
		}
        public override void _ExitTree(){
			importSettingsButton.Pressed -= OnImportSettingsButtonPressed;
			reScanButton.Pressed -= OnReImportPressed;
			todoCategoryList.ItemSelected -= OnCategoryListItemSelected;
			categoryPanelContainer.Resized -= OnCategoryPanelContainerResized;
			if(Main.Instance == null) return;
        }
        
		private void OnCategoryPanelContainerResized() {
			OnListContainerResized?.Invoke();
		}
		public void LoadData(List<ToDo> loadedData) {
			ParsePrioritesCategories(loadedData);
			foreach(string category in parsedCategories) {
				CategoryPanel catPanel = categoryPanel.Instantiate<CategoryPanel>();
				catPanel.category = category;
				catPanel.loadedData = loadedData;
				catPanel.todoVisualizer = this;
				categoryPanelContainer.AddChild(catPanel);
			}
		}
		private void OnReImportPressed() {
			foreach(Node child in categoryPanelContainer.GetChildren()) child.QueueFree();
			OnReScanButtonPressed?.Invoke();
		}
		private void ParsePrioritesCategories(List<ToDo> loadedData) {
			parsedCategories.Clear();
			todoCategoryList.Clear();
			foreach(string priority in Enum.GetNames<PRIORITY>()) {
				todoCategoryList.AddItem(priority);
			}
			foreach(ToDo todo in loadedData) {
				foreach(string category in todo.Categories) {
					if(!parsedCategories.Contains(category)) {
						parsedCategories.Add(category);
						todoCategoryList.AddItem(category);
					}
				}
			}
		}

		private void OnCategoryListItemSelected(long itemSelected) {
			bool parsedPriority = Enum.TryParse(
				todoCategoryList.GetItemText((int)itemSelected), out PRIORITY selectedPriority);

			if(parsedPriority) {
				OnPrioritySelected.Invoke(selectedPriority);
			} else OnCategorySelected?.Invoke(todoCategoryList.GetItemText((int)itemSelected));
		}
		private void OnImportSettingsButtonPressed() {
			foreach(Node child in categoryPanelContainer.GetChildren()) child.QueueFree();
			OnImportSettingsPressed?.Invoke();
		}
	}
}