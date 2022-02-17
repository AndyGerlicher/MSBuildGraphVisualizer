using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Build.Graph;

namespace MSBuildGraphUI.WG
{
    public class WebGraph
    {
        public static string Create(ProjectGraph graph)
        {
            StringBuilder sb = new StringBuilder();

            var projects = new ConcurrentDictionary<string, ProjectGraphNode>();
            foreach (var item in graph.ProjectNodes)
            {
                var propsHash = HashGlobalProps(item.ProjectInstance.GlobalProperties);
                projects.TryAdd(item.ProjectInstance.FullPath + propsHash, item);
            }

            var indent = "\t";
            sb.AppendLine("digraph prof {");
            sb.AppendLine(indent + "ratio = fill;");
            sb.AppendLine(indent + "node [style=filled];");

            foreach (var node in projects)
            {
                if (node.Value.ProjectInstance.FullPath.Contains("dirs.proj"))
                {
                    continue;
                }

                foreach (var subnode in node.Value.ProjectReferences)
                {
                    var n1 = Path.GetFileNameWithoutExtension(node.Value.ProjectInstance.FullPath).Replace(".", "_");
                    var n2 = Path.GetFileNameWithoutExtension(subnode.ProjectInstance.FullPath).Replace(".", "_");
                    sb.AppendLine($"{indent}{n1} -> {n2} [color=\"0.002 0.999 0.999\"];");
                }
            }

            sb.AppendLine("}");
            return sb.ToString();
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
