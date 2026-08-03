using Avalonia.Media.Imaging;
using CommunityToolkit.Mvvm.ComponentModel;
using NitroStudio2.Services;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace NitroStudio2.Models
{
    /// <summary>
    /// Observable stand-in for WinForms' TreeNode. Keeps the same identity model the editors rely
    /// on: a lookup Name distinct from the displayed Text, an image index into the tree icon set,
    /// a Parent link, and an Index within the parent's collection.
    /// </summary>
    public sealed class EditorTreeNode : ObservableObject
    {
        private string text;
        private int imageIndex;
        private bool isExpanded;
        private bool isSelected;

        public EditorTreeNode(string name, string text, int imageIndex = 0)
        {
            Name = name;
            this.text = text;
            this.imageIndex = imageIndex;
            Nodes = [];
            Nodes.CollectionChanged += (_, _) =>
            {
                foreach (EditorTreeNode child in Nodes)
                {
                    child.Parent = this;
                }
            };
        }

        /// <summary>Lookup key. Matches TreeNode.Name, e.g. "sequences" or "entry3".</summary>
        public string Name { get; }

        public string Text
        {
            get => text;
            set => SetProperty(ref text, value);
        }

        /// <summary>Index into <see cref="Assets.TreeIconNames"/>.</summary>
        public int ImageIndex
        {
            get => imageIndex;
            set
            {
                if (SetProperty(ref imageIndex, value))
                {
                    OnPropertyChanged(nameof(Icon));
                }
            }
        }

        public Bitmap Icon => Assets.TreeIcon(imageIndex);

        public ObservableCollection<EditorTreeNode> Nodes { get; }

        public EditorTreeNode Parent { get; private set; }

        public bool IsExpanded
        {
            get => isExpanded;
            set => SetProperty(ref isExpanded, value);
        }

        public bool IsSelected
        {
            get => isSelected;
            set => SetProperty(ref isSelected, value);
        }

        /// <summary>Actions offered by this node's context menu, or null for none.</summary>
        public IReadOnlyList<MenuAction> ContextActions { get; set; }

        /// <summary>Payload the editors attach to a node, mirroring TreeNode.Tag.</summary>
        public object Tag { get; set; }

        /// <summary>Position within the parent's collection. Matches TreeNode.Index.</summary>
        public int Index => Parent?.Nodes.IndexOf(this) ?? -1;

        /// <summary>Depth below the root. Matches TreeNode.Level.</summary>
        public int Level => Parent is null ? 0 : Parent.Level + 1;

        public EditorTreeNode Add(string name, string text, int imageIndex = 0)
        {
            EditorTreeNode node = new(name, text, imageIndex);
            Nodes.Add(node);
            return node;
        }

        /// <summary>Expands this node and every ancestor, matching expandNodePath.</summary>
        public void ExpandPath()
        {
            for (EditorTreeNode node = this; node is not null; node = node.Parent)
            {
                node.IsExpanded = true;
            }
        }

        public static EditorTreeNode FindByName(
            IEnumerable<EditorTreeNode> nodes,
            string name
        )
        {
            foreach (EditorTreeNode node in nodes)
            {
                if (node.Name == name)
                {
                    return node;
                }
                EditorTreeNode found = FindByName(node.Nodes, name);
                if (found is not null)
                {
                    return found;
                }
            }
            return null;
        }

        /// <summary>Names of every expanded node, depth first. Matches collectExpandedNodes.</summary>
        public static List<string> CollectExpanded(IEnumerable<EditorTreeNode> nodes)
        {
            List<string> expanded = [];
            foreach (EditorTreeNode node in nodes)
            {
                if (node.IsExpanded)
                {
                    expanded.Add(node.Name);
                }
                if (node.Nodes.Count > 0)
                {
                    expanded.AddRange(CollectExpanded(node.Nodes));
                }
            }
            return expanded;
        }

        /// <summary>Indices from the root down to this node, root first.</summary>
        public List<int> PathIndices()
        {
            List<int> path = [];
            for (EditorTreeNode node = this; node is not null; node = node.Parent)
            {
                path.Add(node.Index < 0 ? 0 : node.Index);
            }
            path.Reverse();
            return path;
        }

        /// <summary>Walks a PathIndices result back down, stopping at the deepest node that exists.</summary>
        public static EditorTreeNode FromPathIndices(
            IList<EditorTreeNode> roots,
            IReadOnlyList<int> path
        )
        {
            if (roots.Count == 0 || path.Count == 0)
            {
                return null;
            }
            EditorTreeNode node = roots[System.Math.Min(path[0], roots.Count - 1)];
            foreach (int index in path.Skip(1))
            {
                if (index < 0 || index >= node.Nodes.Count)
                {
                    break;
                }
                node = node.Nodes[index];
            }
            return node;
        }
    }
}
