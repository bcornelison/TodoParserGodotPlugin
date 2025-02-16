# Todo Parser
Todo Parser is a program that parses through your code files and looks for ToDo comments. 

![Todo Parser - Kanban board overview](./assets/screenshot.png)

## Usage
When you first run Todo Parser you will be presented with the Import Settigns screen.
1. Choose your code directory - This is the root directory of the code files you wish to scan.
2. If you want to recursively scan (scan all subfolders) the code directory, leave the option ticked. If you want to scan only to root folder, toggle the option slider to off.
3. Choose you language - This enables parsing of a specific language (you can also choose **ALL** if you utilize multiple languages).
4. Choose your category delimiter - This is the delimiter used to separate your categories following the syntax above.
5. Set up your code editor using the instructions in the Code Editor section.
6. Press the Import button to recursively scan through the root directory you have set.

After it parses through your files it will display a kanban style board listing the todos by category. 

You can hover over an item to read the full description (currently limited to the same line the todo is parsed from), as well as display the line number. 
Double clicking an item takes you to the code editor set up in the Import Settings screen.

You can also click on any of the categories on the left side of the screen to display just the selected category. If you select a priority, only those categories that include a todo with the chosen priority will be displayed.

Clicking on the Import Settings button will take you back to the first screen to change the saved options.

Make any changes, but don't want to re-open the program? Just click the Re-Scan Directory button to parse your directory again.


## Code Editor
On the Import settings page you can specify your default code editor. Click the browse button to select the code editor executable. Then in the Custom Editor Args field use the placeholders displayed below to add the arguments required to open the editor at the selected Todo comment.

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