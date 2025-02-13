using Godot;
using GC = Godot.Collections;

using static TodoParser.Util.Enums;

namespace TodoParser {
    public class ToDo {
        public ToDo(GC.Array<string> categories, PRIORITY priority,
            string fileName, uint fileLine, string contents) {
                Categories = categories;
                Priority = priority;
                FileName = fileName;
                FileLine = fileLine;
                Contents = contents;
            }
        public GC.Array<string> Categories { get; private set; }
        public PRIORITY Priority { get; private set; }
        public string FileName { get; private set; }
        public uint FileLine { get; private set; }
        public string Contents { get; private set; }

        public override string ToString() {
            string toReturn = $"File: {FileName} - Line: {FileLine}\n";
            toReturn += $"\tPriority: {Priority}\n";
            toReturn += $"\tCategories: {Categories}\n";
            toReturn += $"\tContents: {Contents}";
            return toReturn;
        }
    }
}