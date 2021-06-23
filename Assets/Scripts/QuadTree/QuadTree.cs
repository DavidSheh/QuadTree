using System.Collections.Generic;
using UnityEngine;

namespace SceneObjectManager
{
    public class QuadTree
    {
        public const int MaxDepth = 4;
        public const int MaxChildCount = 4;
        
        private List<INode> m_QueryResult = new List<INode>();
        
        public Rectangle? Bound { get; private set; }
        public TreeNode Root { get; private set; }

        public QuadTree(Rectangle bound)
        {
            this.Bound = bound;
            Root = new TreeNode(0, null, bound);
        }

        /// <summary>
        /// 插入一个结点
        /// </summary>
        public void InsertNode(INode node)
        {
            if (Root != null)
            {
                InsertNode(Root, node);
            }
        }
        
        /// <summary>
        /// 向某个结点中插入一个结点
        /// </summary>
        private void InsertNode(TreeNode node, INode tarNode)
        {
            if(node.IsLeaf)
            {
                if(node.Depth < MaxDepth && node.Count + 1 > MaxChildCount)
                {
                    node.Subdivide();
                    InsertNode(node, tarNode);
                }
                else
                {
                    node.AddNode(tarNode);
                }
            }
            else
            {
                if (node.LeftTopNode != null && node.LeftTopNode.Bound.Intersects(tarNode.Bound)) 
                {
                    InsertNode(node.LeftTopNode, tarNode);
                }
                if (node.RightTopNode != null && node.RightTopNode.Bound.Intersects(tarNode.Bound))
                {
                    InsertNode(node.RightTopNode, tarNode);
                }
                if (node.RightBottomNode != null && node.RightBottomNode.Bound.Intersects(tarNode.Bound))
                {
                    InsertNode(node.RightBottomNode, tarNode);
                }
                if (node.LeftBottomNode != null && node.LeftBottomNode.Bound.Intersects(tarNode.Bound))
                {
                    InsertNode(node.LeftBottomNode, tarNode);
                }
            }
        }
        
        /// <summary>
        /// 查找指定范围内的对象
        /// </summary>
        /// <param name="bound">指定范围</param>
        /// <returns>返回找到的对象</returns>
        public List<INode> QueryRange(Rectangle bound)
        {
            m_QueryResult.Clear();
            return QueryRange(Root, bound);
        }
        
        /// <summary>
        /// 在指定结点中查找目标范围内的对象
        /// </summary>
        /// <param name="node">指定结点</param>
        /// <param name="bound">目标范围</param>
        /// <returns>返回找到的对象列表</returns>
        private List<INode> QueryRange(TreeNode node, Rectangle bound)
        {
            if (node.IsLeaf)
            {
                foreach (var c in node.objects)
                {
                    m_QueryResult.Add(c);
                }
            }
            else {
                CheckChildNode(node.LeftTopNode, bound);
                CheckChildNode(node.RightTopNode, bound);
                CheckChildNode(node.RightBottomNode, bound);
                CheckChildNode(node.LeftBottomNode, bound);
            }
            return m_QueryResult;
        }

        private void CheckChildNode(TreeNode node, Rectangle bound)
        {
            if (node != null && node.Bound.Intersects(bound)) 
            {
                var temp = QueryRange(node, bound);
                if(temp != null) 
                {
                    foreach (INode t in temp)
                    {
                        // 避免边界对象重复加入，因为边界对象会存在多个结点中
                        if (!m_QueryResult.Contains(t))
                        {
                            m_QueryResult.Add(t);
                        }
                    }
                }
            }
        }

        public void Clear()
        {
            Root?.Clear();
        }
        
        private Queue<INode> nodeQueue = new Queue<INode>();
        public void DrawBound(float depth)
        {
            Gizmos.color = Color.yellow;
            nodeQueue.Enqueue(Root);
            while (nodeQueue.Count > 0)
            {
                var node = nodeQueue.Dequeue() as TreeNode;
                Gizmos.DrawWireCube(new Vector3(node.Bound.x, node.Bound.y, depth),
                    new Vector3(node.Bound.width, node.Bound.height));
                if (!node.IsLeaf)
                {
                    if (node.LeftTopNode != null) {
                        nodeQueue.Enqueue(node.LeftTopNode);
                    }
                    if (node.RightTopNode != null) {
                        nodeQueue.Enqueue(node.RightTopNode);
                    }
                    if (node.RightBottomNode != null) {
                        nodeQueue.Enqueue(node.RightBottomNode);
                    }
                    if (node.LeftBottomNode != null) {
                        nodeQueue.Enqueue(node.LeftBottomNode);
                    }
                }
            }
        }
    }
}