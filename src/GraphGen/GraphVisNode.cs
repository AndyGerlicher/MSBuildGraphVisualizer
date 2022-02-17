using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Microsoft.Build.Graph;

namespace GraphGen
{
    public class GraphVisNode
    {
        private readonly ProjectGraphNode _node;
        private readonly string _label;

        public string Name { get; }

        public GraphVisNode(ProjectGraphNode node)
        {
            _node = node;
            var (name, label) = GetNodeInfo(node);
            Name = name;
            _label = label;
        }

        internal string Create()
        {
            var globalPropertiesString = string.Join("\n", _node.ProjectInstance.GlobalProperties.OrderBy(kvp => kvp.Key).Where(kvp => kvp.Key != "IsGraphBuild").Select(kvp => $"{kvp.Key}={kvp.Value}"));
            if (globalPropertiesString.StartsWith("TargetFramework="))
            {
                globalPropertiesString = globalPropertiesString.Substring("TargetFramework=".Length);
            }

            return $"  {Name} [label=\"{_label}\n{globalPropertiesString}\", shape=box];"; //, color=\"0.650 0.200 1.000\"];";
        }

        private static Dictionary<ProjectGraphNode, string> _nodes = new Dictionary<ProjectGraphNode, string>();
        public static int _count = 1;

        private static (string, string) GetNodeInfo(ProjectGraphNode node)
        {
            var label = Path.GetFileNameWithoutExtension(node.ProjectInstance.FullPath);
            if (!_nodes.ContainsKey(node))
            {
                _nodes.Add(node, label.Replace(".", string.Empty) + _count);
                _count++;
            }
            var name = _nodes[node];
            //var name = _current;//label + Program.HashGlobalProps(node.ProjectInstance.GlobalProperties);
            
            return (name, label);
        }
    }
}