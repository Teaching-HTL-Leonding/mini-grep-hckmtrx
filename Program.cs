using System.Text.RegularExpressions;

#region Constants
const char PARAMETER_CHAR = '-';
const char CASE_INSENSITIVE = 'i';
const char RECURSIVE = 'R';
const char ADDITIONAL_LINES = 'C';
#endregion

#region Main Program
{
    try
    {
        var indicesToExclude = new HashSet<Index>();
        var additionalLinesToPrint = 0;
        var parameters = string.Join("", args
                .Select((argument, index) =>
                {
                    if (argument[0] == PARAMETER_CHAR)
                    {
                        var currentParameter = argument.Substring(1);

                        indicesToExclude.Add(index);
                        if (currentParameter.Contains(ADDITIONAL_LINES) && int.TryParse(args[index + 1], out additionalLinesToPrint))
                        {
                            indicesToExclude.Add(index + 1);
                        }

                        return currentParameter;
                    }
                    else { return ""; }
                })
            ).ToCharArray();

        var arguments = args
            .Where((argument, index) => !indicesToExclude.Contains(index))
            .ToArray();


        var path = arguments[0];
        var filePattern = arguments[1];
        var contentPattern = new Regex($"{(parameters.Contains(CASE_INSENSITIVE) ? $"(?{CASE_INSENSITIVE})" : "")}{arguments[2]}");

        try
        {
            LoopThroughFiles(path, filePattern, (arguments[2], contentPattern), parameters, additionalLinesToPrint);
        }
        catch (DirectoryNotFoundException) { Console.WriteLine($"Directory not found: {path}"); }
    }
    catch (IndexOutOfRangeException) { Console.WriteLine("The Program must receive 3 arguments"); }
}
#endregion

#region Methods
void LoopThroughFiles(string path, string searchPattern, (string Text, Regex Regex) searchText, char[] parameters, int additionalLinesToPrint)
{
    var filesArray = Directory.GetFiles(
        path,
        searchPattern,
        parameters.Contains(RECURSIVE) ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly
    );

    int foundFiles = 0, foundLines = 0, foundOccurences = 0;

    foreach (var file in filesArray)
    {
        var lines = File.ReadAllLines(file);
        var content = string.Join('\n', lines);

        if (searchText.Regex.Match(content).Success)
        {
            foundFiles++;
            Console.WriteLine(file);

            for (int i = 0; i < lines.Length; i++)
            {
                var line = lines[i];

                if (searchText.Regex.Match(line).Success)
                {
                    foundLines++;
                    foundOccurences += searchText.Regex.Matches(line).Count;

                    var formattedOutput = line.Replace(
                        searchText.Text,
                        $">>>{searchText.Text.ToUpper()}<<<",
                        parameters.Contains(CASE_INSENSITIVE) ? StringComparison.CurrentCultureIgnoreCase : StringComparison.CurrentCulture
                    );

                    PrintLines(lines, formattedOutput, i, additionalLinesToPrint);
                }
            }
        }
    }

    Console.WriteLine("SUMMARY:");
    Console.WriteLine($"\tNumber of found files: {foundFiles}");
    Console.WriteLine($"\tNumber of found lines: {foundLines}");
    Console.WriteLine($"\tNumber of occurences: {foundOccurences}");
}

void PrintLines(string[] lines, string formattedOutput, int startIndex, int additionalLinesToPrint)
{
    var lineIndices = Enumerable.Range(0, lines.Length);

    for (int i = startIndex - additionalLinesToPrint; i < startIndex + additionalLinesToPrint + 1; i++)
    {
        if (lineIndices.Contains(i))
        {
            bool originalLine = i == startIndex;

            if (originalLine) { Console.ForegroundColor = ConsoleColor.Yellow; }
            Console.Write($"\t{i + 1}: ");
            if (originalLine) { Console.ResetColor(); }

            Console.WriteLine(originalLine ? formattedOutput : lines[i]);
        }
    }
}
#endregion
