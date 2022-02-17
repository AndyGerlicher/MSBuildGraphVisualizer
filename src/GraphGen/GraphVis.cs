using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using GraphVizWrapper;
using GraphVizWrapper.Commands;
using GraphVizWrapper.Queries;
using Microsoft.Build.Execution;
using Microsoft.Build.Graph;

namespace GraphGen
{
    public class GraphVis
    {
        public static string Create(ConcurrentDictionary<string, ProjectGraphNode> projects)
        {
            HashSet<ProjectGraphNode> seen = new HashSet<ProjectGraphNode>();

            var sb = new StringBuilder();
            var edges = new StringBuilder();
            var nodes = new StringBuilder();
            var clusters = new StringBuilder();

            foreach (var group in projects
                .Where(n => !n.Value.ProjectInstance.FullPath.Contains("dirs.proj"))
                .GroupBy(kvp => kvp.Value.ProjectInstance.FullPath, (p, plist) => new { ProjectGroupName = p, Projects = projects.Where(p2=>p2.Value.ProjectInstance.FullPath == p).ToList()}))
            {
                GraphVisCluster cluster = new GraphVisCluster(group.ProjectGroupName);

                foreach (var node in group.Projects)
                {
                    var graphNode = new GraphVisNode(node.Value);
                    cluster.AddNode(graphNode);
                    
                    if (seen.Contains(node.Value)) continue;
                    seen.Add(node.Value);
                    
                    nodes.AppendLine(graphNode.Create());

                    foreach (var subNode in node.Value.ProjectReferences)
                    {
                        var subGraphVisNode = new GraphVisNode(subNode);
                        var edgeString = new GraphVisEdge(graphNode, subGraphVisNode);

                        edges.AppendLine(edgeString.Create());

                        if (!seen.Contains(node.Value))
                            nodes.AppendLine(subGraphVisNode.Create());
                    }
                }

                clusters.AppendLine(cluster.Create());
            }

            sb.AppendLine("digraph prof {");
            sb.AppendLine("  ratio = fill;");
            sb.AppendLine("  nodesep = .1;");
            sb.AppendLine("  ranksep = 3.0;");
            sb.AppendLine("  node [style=filled];");
            sb.Append(clusters);
            sb.Append(edges);
            sb.AppendLine("}");
            GraphVisNode._count = 1;
            return sb.ToString();
        }

        public static void SaveAsPng(string graphText, string outFile)
        {
            // These three instances can be injected via the IGetStartProcessQuery, 
            //                                               IGetProcessStartInfoQuery and 
            //                                               IRegisterLayoutPluginCommand interfaces

            var getStartProcessQuery = new GetStartProcessQuery();
            var getProcessStartInfoQuery = new GetProcessStartInfoQuery();
            var registerLayoutPluginCommand = new RegisterLayoutPluginCommand(getProcessStartInfoQuery, getStartProcessQuery);

            // GraphGeneration can be injected via the IGraphGeneration interface

            var wrapper = new GraphGeneration(getStartProcessQuery,
                getProcessStartInfoQuery,
                registerLayoutPluginCommand);

            byte[] output = wrapper.GenerateGraph(graphText, Enums.GraphReturnType.Png);
            File.WriteAllBytes(outFile, output);

            Console.WriteLine();
            Console.WriteLine($"{output.Length} bytes written to {outFile}.");
        }
    }
}