using System.Text.RegularExpressions;

#region Constants
const char PARAMETER_CHAR = '-';
const char CASE_INSENSITIVE = 'i';
const char RECURSIVE = 'R';
#endregion

#region Main Program
{
    try
    {
        var parameters = string.Join("", args
                .Select(argument => argument[0] == PARAMETER_CHAR ? argument.Substring(1) : "")
            ).ToCharArray();

        var arguments = args
            .Where(argument => argument[0] != PARAMETER_CHAR)
            .ToArray();


        var path = arguments[0];
        var filePattern = arguments[1];
        var contentPattern = new Regex($"{(parameters.Contains(CASE_INSENSITIVE) ? $"(?{CASE_INSENSITIVE})" : "")}{arguments[2]}");

        try
        {
            LoopThroughFiles(path, filePattern, (arguments[2], contentPattern), parameters);
        }
        catch (DirectoryNotFoundException) { Console.WriteLine($"Directory not found: {path}"); }
    }
    catch (IndexOutOfRangeException) { Console.WriteLine("The Program must receive 3 arguments"); }
}
#endregion

#region Methods
void LoopThroughFiles(string path, string searchPattern, (string Text, Regex Regex) searchText, char[] parameters)
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
                        parameters.Contains(CASE_INSENSITIVE) ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal
                    );
                    Console.WriteLine($"\t{i + 1}: {formattedOutput}");
                }
            }
        }
    }

    Console.WriteLine("SUMMARY:");
    Console.WriteLine($"\tNumber of found files: {foundFiles}");
    Console.WriteLine($"\tNumber of found lines: {foundLines}");
    Console.WriteLine($"\tNumber of occurences: {foundOccurences}");
}
#endregion
