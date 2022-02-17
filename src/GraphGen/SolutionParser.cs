using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace GraphGen
{
    public static class SolutionParser
    {
        private static readonly Regex SolutionFileRegex = new Regex(
            @"^Project\(""\{(?<ProjectTypeGuid>[\wA-F]{8}-[\wA-F]{4}-[\wA-F]{4}-[\wA-F]{4}-[\wA-F]{12})\}""\) = ""(?<ProjectName>.+)"", ""(?<ProjectFile>.+)"", ""\{(?<ProjectGuid>[\wA-F]{8}-[\wA-F]{4}-[\wA-F]{4}-[\wA-F]{4}-[\wA-F]{12})\}""\r?$",
            RegexOptions.Multiline | RegexOptions.Compiled);

        private static readonly IReadOnlyDictionary<string, string> ProjectTypeNamedGuids =
            new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                {"vbProjectGuid", "F184B08F-C81C-45F6-A57F-5ABD9991F28F"},
                {"csProjectGuid", "FAE04EC0-301F-11D3-BF4B-00C04F79EFBC"},
                {"cpsProjectGuid", "13B669BE-BB05-4DDF-9536-439F39A36129"},
                {"cpsCsProjectGuid", "9A19103F-16F7-4668-BE54-9A1E7A4F7556"},
                {"cpsVbProjectGuid", "778DAE3C-4631-46EA-AA77-85C1314464D9"},
                {"vjProjectGuid", "E6FDF86B-F3D1-11D4-8576-0002A516ECE8"},
                {"vcProjectGuid", "8BC9CEB8-8B4A-11D0-8D11-00A0C91BC942"},
                {"fsProjectGuid", "F2A71F9B-5D33-465A-A702-920D77279786"},
                {"dbProjectGuid", "C8D11400-126E-41CD-887F-60BD40844F9E"},
                {"wdProjectGuid", "2CFEAB61-6A3B-4EB8-B523-560B4BEEF521"},
                {"webProjectGuid", "E24C65DC-7377-472B-9ABA-BC803B73C61A"}
            };

        private static readonly IReadOnlyCollection<string> ProjectTypeGuids = ProjectTypeNamedGuids.Values.ToHashSet(StringComparer.OrdinalIgnoreCase);

        public static IEnumerable<string> GetProjectFiles(string solutionFile)
        {
            Trace.Assert(File.Exists(solutionFile), $"Solution file does not exist: {solutionFile}");

            var matches = SolutionFileRegex.Matches(File.ReadAllText(solutionFile));

            var solutionDirectory = Path.GetDirectoryName(solutionFile);

            Trace.Assert(solutionDirectory != null);

            foreach (Match match in matches)
            {
                var projectTypeGuids = match.Groups["ProjectTypeGuid"].Captures.GetEnumerator().ToEnumerable().OfType<Capture>().ToArray();

                Trace.Assert(projectTypeGuids.Length == 1, "A solution project must have a single project type guid");

                // reject entries we're not interested in (e.g. solution folders)
                if (!ProjectTypeGuids.Contains(projectTypeGuids.First().Value))
                {
                    continue;
                }

                var projectFileGroup = match.Groups["ProjectFile"].Captures.GetEnumerator().ToEnumerable().OfType<Capture>().ToArray();

                Trace.Assert(projectFileGroup.Length == 1, "A solution project must have a single file");

                yield return Path.Combine(solutionDirectory, projectFileGroup.First().Value);
            }
        }
    }
}
