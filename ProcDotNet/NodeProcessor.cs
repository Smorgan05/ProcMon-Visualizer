﻿using ProcDotNet.Classes;

namespace ProcDotNet
{
    public class NodeProcessor
    {
        /// <summary>
        /// Perform Layer 1 Node Mapping
        /// </summary>
        /// <param name="processBuckets"></param>
        /// <returns></returns>
        internal static List<ProcMon> GetTreeList(List<KeyValuePair<ProcMon, List<ProcMon>>> processBuckets)
        {
            //Setup and Pass
            List<KeyValuePair<ProcMon, List<ProcMon>>> orgBuckets = new(processBuckets);

            // Make Return Result
            List<ProcMon> TreeListRes = new List<ProcMon>();

            // Perform One Layer Mapping
            foreach (var process in orgBuckets)
            {
                // First Node in Tree List
                ProcMon singleRoot = process.Key;

                // Add Children
                foreach (var child in process.Value)
                {
                    if (process.Key != child)
                    {
                        singleRoot.Children.Add(child);
                    }
                }
                TreeListRes.Add(singleRoot);
            }

            return TreeListRes;
        }

        internal static List<ProcMon> MakeTreeList(List<ProcMon> processNodes)
        {
            List<ProcMon> result = new();

            foreach (ProcMon branch in processNodes)
            {
                var MapResult = Mapper(processNodes, branch);

                // Top Check
                if (MapResult != null && !result.Contains(branch))
                {
                    // Check for Duplicates
                    result = CheckResult(result, MapResult);
                    //result.Add(branch);
                }
            }

            return result;
        }

        private static List<ProcMon> SingleDedup(List<ProcMon> linkProcessNodes)
        {
            //List<TreeNode<ProcMon>> temp = new List<TreeNode<ProcMon>>(linkProcessNodes);
            List<ProcMon> result = new List<ProcMon>(linkProcessNodes);

            if (linkProcessNodes.Count == 0)
            {
                return result;
            }

            // Layer One dup (prioritize keeping nested over orphan)
            foreach (var item in linkProcessNodes)
            {
                foreach (var single in linkProcessNodes)
                {
                    var tempNode = RecFindByProcessID(single, item.ProcessID);
                    if (tempNode != null && item != single)
                    {
                        result.Remove(tempNode);
                    }
                }
            }

            return result;
        }

        private static List<ProcMon> CheckResult(List<ProcMon> Nodes, ProcMon mapResult)
        {
            var Result = new List<ProcMon>(Nodes);
            var temp = FindNodeFromListByProcessID(Nodes, mapResult.ProcessID);
            var parent = FindNodeFromListByProcessID(Nodes, mapResult.ParentPID);

            // Dup Check
            if (temp == null)
            {
                Result.Add(mapResult);
            }

            //else if (!Nodes.Contains(mapResult) && parent == null)
            //{
            //    Result.Add(mapResult);
            //}

            else if (mapResult != null && parent != null)
            {
                var check = parent.Children.Where(x => x.ProcessID == mapResult.ProcessID);
                if (check.Count() > 1)
                {
                    ProcMon remove = check.OrderByDescending(x => x.Children.Count).ToList().Skip(1).First();
                    parent.Children.Remove(remove);
                }
            }

            //else if (parent != null & mapResult != null && !parent.Children.Contains(mapResult))
            //{
            //    parent.AddChild(mapResult);
            //}

            return SingleDedup(Result);
        }

        internal static ProcMon Mapper(List<ProcMon> Nodes, ProcMon currentNode)
        {
            var ParentNode = FindNodeFromListByProcessID(Nodes, currentNode.ParentPID);
            if (ParentNode != null && ParentNode != currentNode && !ParentNode.Children.Contains(currentNode))
            {
                ParentNode.Children.Add(currentNode);
                Mapper(Nodes, ParentNode);
            }
            else if (ParentNode == null && ParentNode != currentNode)
            {
                // Ensure distinct children
                //currentNode.Children = currentNode.Children.DistinctBy(x => x.Data.ProcessID).ToList();
                return currentNode;
            }

            // Ensure distinct children
            //ParentNode.Children = ParentNode.Children.DistinctBy(x => x.Data.ProcessID).ToList();
            return ParentNode;
        }

        // Node Finders

        internal static ProcMon? FindNodeFromListByProcessID(List<ProcMon> Nodes, int processID)
        {
            foreach (var item in Nodes)
            {
                ProcMon found = RecFindByProcessID(item, processID);
                if (found != null)
                {
                    return found;
                }
            }
            return null;
        }
        internal static ProcMon? RecFindByProcessID(ProcMon Node, int ProcessID)
        {
            // find the string, starting with the current instance
            return RecFindNode(Node, ProcessID);

            static ProcMon? RecFindNode(ProcMon node, int ProcessID)
            {
                if (node.ProcessID == ProcessID)
                    return node;

                foreach (var child in node.Children)
                {
                    var result = RecFindNode(child, ProcessID);
                    if (result != null)
                        return result;
                }
                return null;
            }

        }


        public static ProcMon? FindNodeFromListByParentPID(List<ProcMon> Nodes, int ParentPID)
        {
            foreach (var item in Nodes)
            {
                ProcMon found = RecFindByParentPID(item, ParentPID);
                if (found != null)
                {
                    return found;
                }
            }
            return null;
        }
        internal static ProcMon RecFindByParentPID(ProcMon Node, int ParentPID)
        {
            // find the string, starting with the current instance
            return RecFindNode(Node, ParentPID);

            static ProcMon RecFindNode(ProcMon node, int ParentPID)
            {
                if (node.ParentPID == ParentPID)
                    return node;

                foreach (var child in node.Children)
                {
                    var result = RecFindNode(child, ParentPID);
                    if (result != null)
                        return result;
                }

                return null;
            }
        }

        public static ProcMon? FindNodeFromListByProcessName(List<ProcMon> Nodes, string ProcessName)
        {
            foreach (var item in Nodes)
            {
                ProcMon found = RecFindByProcessName(item, ProcessName);
                if (found != null) return found;
            }
            return null;
        }
        internal static ProcMon RecFindByProcessName(ProcMon Node, string ProcessName)
        {
            // find the string, starting with the current instance
            return RecFindNode(Node, ProcessName);

            static ProcMon RecFindNode(ProcMon node, string ProcessName)
            {
                if (node.ProcessName.Equals(ProcessName, StringComparison.OrdinalIgnoreCase))
                    return node;

                foreach (var child in node.Children)
                {
                    var result = RecFindNode(child, ProcessName);
                    if (result != null)
                        return result;
                }

                return null;
            }

        }
    }
}
