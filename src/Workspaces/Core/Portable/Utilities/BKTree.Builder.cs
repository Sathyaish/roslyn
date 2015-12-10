﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Roslyn.Utilities
{
    internal partial class BKTree
    {
        private class Builder
        {
            private readonly char[][] values;

            public Builder(IEnumerable<string> values)
            {
                this.values = values.Select(v => v.ToLower()).Distinct().Select(v => v.ToCharArray()).ToArray();
            }

            internal BKTree Create()
            {
                var nodes = new List<Node>();
                foreach (var value in values)
                {
                    if (value.Length > 0)
                    {
                        Add(nodes, value);
                    }
                }

                return new BKTree(nodes.ToArray());
            }

            private static void Add(List<Node> nodes, char[] lowerCaseCharacters)
            {
                if (nodes.Count == 0)
                {
                    nodes.Add(new Node(lowerCaseCharacters));
                    return;
                }

                var currentNodeIndex = 0;
                while (true)
                {
                    var currentNode = nodes[currentNodeIndex];

                    var editDistance = EditDistance.GetEditDistance(currentNode.LowerCaseCharacters, lowerCaseCharacters);
                    // This shoudl never happen.  We dedupe all items before proceeding to the 'Add' step.
                    Debug.Assert(editDistance != 0);

                    if (currentNode.AllChildren == null)
                    {
                        currentNode.AllChildren = new Dictionary<int, int>();
                        nodes[currentNodeIndex] = currentNode;
                        // Fall through. to actually add the child to this node.
                    }
                    else
                    {
                        int childNodeIndex;
                        if (currentNode.AllChildren.TryGetValue(editDistance, out childNodeIndex))
                        {
                            // Edit distances collide.  Move to this child and add this word to it.
                            currentNodeIndex = childNodeIndex;
                            continue;
                        }

                        // Fall through. to actually add the child to this node.
                    }

                    currentNode.AllChildren.Add(editDistance, nodes.Count);
                    nodes.Add(new Node(lowerCaseCharacters));
                    return;
                }
            }
        }
    }
}
