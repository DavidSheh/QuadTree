using System.Collections.Generic;

namespace SceneObjectManager
{
    public interface INode
    {
        Rectangle Bound { get; }
    }

    public class TreeNode : INode
    {
        /// <summary>
        /// 当前树节点的边界
        /// </summary>
        public Rectangle Bound { get; private set; }
        /// <summary>
        /// 结点中的对象列表
        /// </summary>
        public readonly LinkedList<INode> objects;
        /// <summary>
        /// 左上结点
        /// </summary>
        public TreeNode LeftTopNode {
            get;
            private set;
        }
        /// <summary>
        /// 右上结点
        /// </summary>
        public TreeNode RightTopNode {
            get;
            private set;
        }
        /// <summary>
        /// 右下结点
        /// </summary>
        public TreeNode RightBottomNode {
            get;
            private set;
        }
        /// <summary>
        /// 左下结点
        /// </summary>
        public TreeNode LeftBottomNode {
            get;
            private set;
        }
        /// <summary>
        /// 树的深度
        /// </summary>
        public int Depth { get; private set; }
        /// <summary>
        /// 结点中的对象数量
        /// </summary>
        public int Count => objects?.Count ?? 0;
        /// <summary>
        /// 是否为叶子结点
        /// </summary>
        public bool IsLeaf { get; private set; }
        public TreeNode Parent { get; private set; }

        public TreeNode(int depth, TreeNode parent, Rectangle bound)
        {
            this.Bound = bound;
            this.Depth = depth;
            this.Parent = parent;
            this.IsLeaf = true;
            objects = new LinkedList<INode>();
        }
        
        /// <summary>
        /// 分割子结点
        /// </summary>
        public void Subdivide()
        {
            IsLeaf = false;
            var x = Bound.x;
            var y = Bound.y;
            var width = Bound.width * 0.5f;
            var height = Bound.height * 0.5f;
            // 左上
            var ltBound = new Rectangle(x - width * 0.5f, y + height * 0.5f, width, height);
            LeftTopNode = new TreeNode(Depth + 1, this, ltBound);
            // 右上
            var rtBound = new Rectangle(x + width * 0.5f, y + height * 0.5f, width, height);
            RightTopNode = new TreeNode(Depth + 1, this, rtBound);
            // 右下
            var rbBound = new Rectangle(x + width * 0.5f, y - height * 0.5f, width, height);
            RightBottomNode = new TreeNode(Depth + 1, this, rbBound);
            // 左下
            var lbBound = new Rectangle(x - width * 0.5f, y - height * 0.5f, width, height);
            LeftBottomNode = new TreeNode(Depth + 1, this, lbBound);
            
            SubAdd();
        }

        private void SubAdd()
        {
            foreach (var obj in objects)
            {
                if (LeftTopNode.Bound.Intersects(obj.Bound))
                {
                    LeftTopNode.AddNode(obj);
                }
                if (RightTopNode.Bound.Intersects(obj.Bound))
                {
                    RightTopNode.AddNode(obj);
                }
                if (RightBottomNode.Bound.Intersects(obj.Bound))
                {
                    RightBottomNode.AddNode(obj);
                }
                if (LeftBottomNode.Bound.Intersects(obj.Bound))
                {
                    LeftBottomNode.AddNode(obj);
                }
            }
        }

        public void AddNode(INode node)
        {
            if (!objects.Contains(node))
            {
                objects.AddLast(node);
            }
        }

        public bool RemoveNode(INode tarNode)
        {
            return objects != null && objects.Remove(tarNode);
        }
        
        public void Clear()
        {
            LeftTopNode?.Clear();
            LeftTopNode = null;
            RightTopNode?.Clear();
            RightTopNode = null;
            RightBottomNode?.Clear();
            RightBottomNode = null;
            LeftBottomNode?.Clear();
            LeftBottomNode = null;
            
            objects.Clear();

            this.Depth = 0;
            this.IsLeaf = true;
            this.Parent = null;
        }
    }
}