using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using GraphGen;
using Microsoft.Build.Evaluation;
using Microsoft.Build.Execution;
using Microsoft.Build.Graph;
using MSBuildGraphUI.WG;
using Newtonsoft.Json;

namespace MSBuildGraphUI
{
    public partial class MSBuildGraphForm : Form
    {
        public MSBuildGraphForm()
        {
            InitializeComponent();
        }

        private void _loadButton_Click(object sender, EventArgs e)
        {
            if (_openFileDialog.ShowDialog(this) == DialogResult.OK)
            {
                LoadGraph(new FileInfo(_openFileDialog.FileName));
            }

        }

        private async void LoadGraph(FileInfo project)
        {
            var files = new List<ProjectGraphEntryPoint>();

            if (project.Extension == ".sln")
            {
                files.AddRange(SolutionParser.GetProjectFiles(project.FullName)
                     .Select(f => new ProjectGraphEntryPoint(f)));
            }
            else
            {
                files.Add(new ProjectGraphEntryPoint(project.FullName));
            }

            _statusBarLabel.Text = $@"Loading {project.FullName}...";

            // Load the graph from MSBuild
            var stopwatch = Stopwatch.StartNew();
            var graph = await Task.Factory.StartNew(() =>
                new ProjectGraph(files, ProjectCollection.GlobalProjectCollection, ProjectInstanceFactory));
            stopwatch.Stop();

            _statusBarLabel.Text = $@"{project.Name} loaded {graph.ProjectNodes.Count} node(s) in {stopwatch.ElapsedMilliseconds}ms.";

            var projects = new ConcurrentDictionary<string, ProjectGraphNode>();

            foreach (var item in graph.ProjectNodes)
            {
                var propsHash = HashGlobalProps(item.ProjectInstance.GlobalProperties);
                projects.TryAdd(item.ProjectInstance.FullPath + propsHash, item);
            }

            var graphText = await Task.Factory.StartNew(() => GraphGen.GraphVis.Create(projects));
            var t = Path.GetTempFileName();
            var file = Path.GetTempPath() + "MSB_GRAPH.png";
            await Task.Factory.StartNew(() => GraphGen.GraphVis.SaveAsPng(graphText, file));

            webBrowser1.Url = new Uri(file);
            
            _statusBarLabel.Text = $@"{project.Name} loaded {graph.ProjectNodes.Count} node(s) in {stopwatch.ElapsedMilliseconds}ms. Populating the TreeView...";
            var stopwatch2 = Stopwatch.StartNew();
            await Task.Factory.StartNew(() => PopulateTree(graph));
            stopwatch2.Stop();
            _statusBarLabel.Text = $@"{project.Name} loaded {graph.ProjectNodes.Count} node(s) in {stopwatch.ElapsedMilliseconds}ms. {stopwatch2.ElapsedMilliseconds}ms to draw {_counts} nodes in the TreeView.";
        }

        private ProjectInstance ProjectInstanceFactory(string projectFile, Dictionary<string, string> globalProperties, ProjectCollection projectCollection)
        {
            Action a = () => _statusBarLabel.Text = $@"Loading {projectFile}...";
            this.Invoke(a);

            var sw = Stopwatch.StartNew();
            var pi = new ProjectInstance(
                projectFile,
                globalProperties,
                "Current",
                projectCollection);
            sw.Stop();

            Action a2 = () => _statusBarLabel.Text = $@"Loading {projectFile}. Done in {sw.ElapsedMilliseconds}ms";
            this.Invoke(a2);

            return pi;
        }

        private void PopulateTree(ProjectGraph graph)
        {
            Action a1 = () => _treeVew.Nodes.Clear();
            this.Invoke(a1);

            foreach (var root in graph.GraphRoots)
            {
                Action a = () => _treeVew.Nodes.Add(AdaptGraphNode(root));
                this.Invoke(a);
                
            }
        }

        private int _counts = 0;

        private TreeNode AdaptGraphNode(ProjectGraphNode node)
        {
            Interlocked.Increment(ref _counts);
            TreeNode treeNode = new TreeNode(node.ProjectInstance.FullPath) {Tag = node};

            foreach (var child in node.ProjectReferences)
            {
                treeNode.Nodes.Add(AdaptGraphNode(child));
            }

            return treeNode;
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

        private void _treeVew_AfterSelect_1(object sender, TreeViewEventArgs e)
        {
            var graphNode = (ProjectGraphNode)e.Node.Tag;
            //_propertyGrid.SelectedObject = new { Name = graphNode.ProjectInstance.FullPath, Props = graphNode.ProjectInstance.GlobalProperties};
            _propertyGrid.SelectedObject = graphNode.ProjectInstance;
        }
    }
}
