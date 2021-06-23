using UnityEngine;

namespace SceneObjectManager
{
    public struct Rectangle
    {
        /// <summary>
        /// 矩形的中心点 x 坐标
        /// </summary>
        public float x { get; private set; }
        /// <summary>
        /// 矩形的中心点 y 坐标
        /// </summary>
        public float y { get; private set; }
        /// <summary>
        /// 矩形的宽
        /// </summary>
        public float width { get; private set; }
        /// <summary>
        /// 矩形的高
        /// </summary>
        public float height { get; private set; }

        public Rectangle(float x, float y, float width, float height)
        {
            this.x = x;
            this.y = y;
            this.width = width;
            this.height = height;
        }

        /// <summary>
        /// 矩形是否包含指定点
        /// </summary>
        /// <param name="x">点的x坐标</param>
        /// <param name="y">点的y坐标</param>
        /// <returns></returns>
        public bool Contains(float x, float y)
        {
            var minX = this.x - this.width * 0.5f;
            var minY = this.y - this.height * 0.5f;
            var maxX = this.x + this.width * 0.5f;
            var maxY = this.y + this.height * 0.5f;

            return x >= minX && x <= maxX && y >= minY && y <= maxY;
        }

        /// <summary>
        /// 判断两个矩形是否相交
        /// </summary>
        /// <param name="tarRect">目标矩形</param>
        /// <returns></returns>
        public bool Intersects(Rectangle tarRect)
        {
            var minX = this.x - this.width * 0.5f;
            var minY = this.y - this.height * 0.5f;
            var maxX = this.x + this.width * 0.5f;
            var maxY = this.y + this.height * 0.5f;

            var tarMinX = tarRect.x - tarRect.width * 0.5f;
            var tarMinY = tarRect.y - tarRect.height * 0.5f;
            var tarMaxX = tarRect.x + tarRect.width * 0.5f;
            var tarMaxY = tarRect.y + tarRect.height * 0.5f;

            return Mathf.Max(minX, tarMinX) < Mathf.Min(maxX, tarMaxX) &&
                   Mathf.Max(minY, tarMinY) < Mathf.Min(maxY, tarMaxY);
        }

        public Rectangle Reposition(float x, float y)
        {
            this.x = x;
            this.y = y;
            return this;
        }
        
        public Rectangle Resize(float w, float h)
        {
            this.width = w;
            this.height = h;
            return this;
        }
    }
}