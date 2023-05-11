﻿using System.Text.RegularExpressions;

#region Main Program
{
    try
    {
        var parameters = string.Join("", args
                .Select(argument => argument[0] == '-' ? argument.Substring(1) : "")
            ).ToCharArray();

        var arguments = args
                        .Where(argument => argument[0] != '-')
                        .ToArray();


        var path = arguments[0];
        var filePattern = arguments[1];
        var contentPattern = new Regex($"{(parameters.Contains('i') ? "(?i)" : "")}{arguments[2]}");

        try
        {
            LoopThroughFiles(path, filePattern, (arguments[2], contentPattern), parameters.Contains('R'));
        }
        catch (DirectoryNotFoundException) { Console.WriteLine($"Directory not found: {path}"); }
    }
    catch (IndexOutOfRangeException) { Console.WriteLine("The Program must receive 3 arguments"); }
}
#endregion

#region Methods
void LoopThroughFiles(string path, string searchPattern, (string Text, Regex Regex) searchText, bool recursively)
{
    var filesArray = Directory.GetFiles(path, searchPattern, recursively ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly);

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

                    var formattedOutput = searchText.Regex.Replace(line, $">>>{searchText.Text.ToUpper()}<<<");
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