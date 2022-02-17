using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace GraphGen
{
    public class GraphVisEdge
    {
        private readonly GraphVisNode _node1;
        private readonly GraphVisNode _node2;

        public GraphVisEdge(GraphVisNode node1, GraphVisNode node2)
        {
            _node1 = node1;
            _node2 = node2;
        }

        public string Create()
        {
            //var (n1, _) = GraphVisNode.GetNodeInfo(node);
            //var (n2, _) = GraphVisNode.GetNodeInfo(subNode);

            return $"  {_node1.Name} -> {_node2.Name};"; //[color=\"0.002 0.999 0.999\"];";
        }
    }
}
