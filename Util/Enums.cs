using Godot;

namespace CodeTodoVisualizer.Util {
    public class Enums {
        public enum PROGRAMMINGLANGUAGES {
            ALL,
            C,
            CPP,
            CSHARP,
            GDSCRIPT,
            GOLANG,
            JAVA,
            PYTHON,
            RUBY,
            RUST,

        }
        public enum PRIORITY {
            ALL,
            LOWEST,
            LOW,
            MEDIUM,
            HIGH,
            CRITICAL,
        }
        public enum CATEGORYDELIMITERS {
            PIPE,
            HYPHEN,
            UNDERSCORE,
            COMMA
        }
    }
}