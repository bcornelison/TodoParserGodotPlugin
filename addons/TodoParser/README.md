# Todo Parser
Todo Parser is a program that parses through your code files and looks for ToDo comments.


## Usage
When you first run Todo Parser you will be presented with the Import Settigns screen.
1. Choose your code directory - This is the root directory of the code files you wish to scan.
2. If you want to recursively scan (scan all subfolders) the code directory, leave the option ticked. If you want to scan only to root folder, toggle the option slider to off.
3. Choose you language - This enables parsing of a specific language (you can also choose **ALL** if you utilize multiple languages).
4. Choose your category delimiter - This is the delimiter used to separate your categories following the syntax above.
5. If you have not set up an external dotnet editor in Godot's Editor Settings, set it up using the instructions in the Code Editor section.

After it parses through your files it will display a kanban style board listing the todos by category. 

You can hover over an item to read the full description (currently limited to the same line the todo is parsed from), as well as display the line number. 
Double clicking an item takes you to the code editor set up in the Import Settings screen.

You can also click on any of the categories on the left side of the screen to display just the selected category. If you select a priority, only those categories that include a todo with the chosen priority will be displayed.

Clicking on the Import Settings button will take you back to the first screen to change the saved options.

Make any changes, but don't want to re-open the program? Just click the Re-Scan Directory button to parse your directory again.


## Code Editor
In the Editor Settings in Godot (In the main toolbar of Godot -> Editor -> Editor Settings), scroll to the bottom and open Dotnet -> Editor and 
- Choose your editor tyoe
- Browse to your Code Editor's executable
- Fill in the Custom Exec Path Args using the table below.

| Flags Field | Is replaced with               |
|:-----------:|--------------------------------|
|**{project}**| The code's root path           |
|**{file}**   | The path to the file           |
|**{line}**   | The line number of the comment |
| *{col}*     | Currently unused               |

Some examples for various editors:

| Editor          | Exec Flags                               |
|:---------------:|------------------------------------------|
| Geany/Kate      | **{file} --line {line} --column {col}**  |
| Atom            | **{file}:{line}**                        |
| JetBrains Rider | **{project} --line {line} {file}**       |
| VSCode          | **{project} --goto {file}:{line}:{col}** |
| Vim (gVim)      | **"+call cursor({line}, {col})" {file}** |
| Emacs           | **emacs +{line}:{col} {file}**           |
| Sublime Text    | **{project} {file}:{line}:{column}**     |


## Parsing Syntax
The syntax the parser looks for is 

    // TODO(CATEGORY1|CATEGORY2|...): I need to do this
    
where '//' is the selected language's comment syntax and within the ()'s you can have any number of categories separated by the chosen delimiter: '|', '-', '_' or ','.

The TODO is not case-sensitive, but the categories are (meaning THIS and this will be recognized as different categories).


## Priorities
You can also use predefined priorities as a category, included priorities (not case-sensitive):
1. LOWEST
2. LOW
3. MEDIUM (used as the default if one isn't specified)
4. HIGH
5. CRITICAL