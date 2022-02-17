using System.Collections.Generic;
using System.IO;

namespace GraphGen
{
    public class GraphVisCluster
    {
        private static int _clusterIdGlobal = 0;
        private static int _clusterId;

        private readonly string _projectName;
        private readonly List<GraphVisNode> _nodes = new List<GraphVisNode>();
        private readonly string _clusterLabel;

        public GraphVisCluster(string projectPath)
        {
            _projectName = Path.GetFileNameWithoutExtension(projectPath).Replace(".", string.Empty);
            _clusterLabel = Path.GetFileName(projectPath);
            _clusterId = _clusterIdGlobal;
            _clusterIdGlobal++;
        }

        public void AddNode(GraphVisNode node)
        {
            _nodes.Add(node);
        }

        public string Create()
        {
            if (_nodes.Count == 1)
            {
                return _nodes[0].Create();
            }

            var result = $@"
        subgraph cluster_{_clusterId} {{
		style=filled;
		color=lightgrey;
		node [style=filled,color=white];
		label = ""{_clusterLabel}"";";

            foreach (var p in _nodes)
            {
                result += p.Create();
            }

            result += "}";
            return result;

        }

    }
}
