using GraphVizWrapper;
using GraphVizWrapper.Commands;
using GraphVizWrapper.Queries;
using Microsoft.Build.Evaluation;
using Microsoft.Build.Locator;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Build.Graph;

namespace GraphGen
{
    class Program
    {
        static void Main(string[] args)
        {
            var msbuildpath = args[0];
            var projectFile = args[1];
            var outFile = args.Length > 2 ? args[2] : "out.png";

            MSBuildLocator.RegisterMSBuildPath(msbuildpath);

            var graphText = new Program().LoadGraph(projectFile);
            GraphVis.SaveAsPng(graphText, outFile);
        }

        private string LoadGraph(string file)
        {
            Console.WriteLine("Loading graph...");
            var sw = Stopwatch.StartNew();
            var graph = new ProjectGraph(file, ProjectCollection.GlobalProjectCollection); //, ProjectInstanceFactory)
            Console.WriteLine($@"{file} loaded {graph.ProjectNodes.Count} node(s) in {sw.ElapsedMilliseconds}ms.");

            var projects = new ConcurrentDictionary<string, ProjectGraphNode>();

            foreach (var item in graph.ProjectNodes)
            {
                var propsHash = HashGlobalProps(item.ProjectInstance.GlobalProperties);
                projects.TryAdd(item.ProjectInstance.FullPath + propsHash, item);
            }

            return GraphVis.Create(projects);
        }

        private const char ItemSeparatorCharacter = '\u2028';

        private static string HashGlobalProps(IDictionary<string, string> globalProperties)
        {
            using (var sha1 = SHA1.Create())
            {
                var stringBuilder = new StringBuilder();
                foreach (var item in globalProperties)
                {
                    stringBuilder.Append(item.Key);
                    stringBuilder.Append(ItemSeparatorCharacter);
                    stringBuilder.Append(item.Value);
                }

                var hash = sha1.ComputeHash(Encoding.UTF8.GetBytes(stringBuilder.ToString()));

                stringBuilder.Clear();

                foreach (var b in hash)
                {
                    stringBuilder.Append(b.ToString("x2"));
                }

                return stringBuilder.ToString();
            }
        }
    }
}
