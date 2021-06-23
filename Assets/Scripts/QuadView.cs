using UnityEngine;

namespace SceneObjectManager
{
    public class QuadView : INode
    {
        public Rectangle Bound { get; private set; }
        
        private GameObject obj;
        private Transform trans;
        private MeshRenderer render;
        private Vector3 tarPos;
        private Vector3 lastPos;
        public bool isMoving = false;
        private float speed;
        private float factor = 0;

        public QuadView(GameObject obj, Rectangle bound)
        {
            this.obj = obj;
            this.trans = obj.transform;
            this.Bound = bound;
            this.render = obj.GetComponent<MeshRenderer>();
        }

        public Vector3 GetPosition()
        {
            return trans.localPosition;
        }

        public void MoveTo(Vector3 to, float speed)
        {
            this.lastPos = this.trans.localPosition;
            this.tarPos = to;
            this.speed = speed;
            isMoving = true;
            
            factor = 0;
        }

        public void UpdatePosition(float deltaTime)
        {
            factor += deltaTime * speed;
            if (factor > 0.99f)
            {
                isMoving = false;
                this.trans.localPosition = this.tarPos;
                this.Bound = this.Bound.Reposition(this.tarPos.x, this.tarPos.y);
            }
            else
            {
                var pos = Vector3.Lerp(this.lastPos, tarPos, factor);
                this.trans.localPosition = pos;
                this.Bound = this.Bound.Reposition(pos.x, pos.y);
            }
        }

        public void ChangeColor(Color color)
        {
            if (render)
            {
                render.material.color = color;
            }
        }

        public void Destroy()
        {
            render = null;
            GameObject.Destroy(obj);
            obj = null;
        }
    }
}