using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Profiling;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace SceneObjectManager
{
    public class Main : MonoBehaviour
    {
        [SerializeField]
        private Vector2 treeBound = new Vector2(100, 100);
        [SerializeField]
        private float elementWidth = 1;
        [SerializeField]
        private float elementHeigth = 1;
        [SerializeField]
        private float gizmosDepth = 10;
        [SerializeField]
        private GameObject quad;
        [SerializeField] 
        private int totalCount = 0;
        [SerializeField] 
        private float speed = 1;
        
        #region UI 显示

        public Text txtFPS;
        public Text txtNum;
        public Text txtInsertTime;
        public Text txtTotalTime;
        public Text txtSearchTime;
        public InputField inputNum;
        #endregion
        
        private Transform m_Parent;
        private List<QuadView> m_QuadViews = new List<QuadView>();

        private Camera m_Cam;
        private QuadTree m_Tree;

        private float m_MinX, m_MaxX, m_MinY, m_MaxY;
        private float m_MaxInsertTime = 0;
        private float m_MaxSearchTime = 0;
        private bool m_IsMoving = false;

        private bool m_Simulation = false;
        private QuadView m_CamView;
        public Vector2 viewSize = new Vector2(10, 10);
        void Start()
        {
            Application.targetFrameRate = 60;
            
            m_Cam = Camera.main;
            m_Parent = new GameObject("Points").transform;
            var width = treeBound.x;
            var height = treeBound.y;
            m_Tree = new QuadTree(new Rectangle(0, 0, width, height));
            m_MinX = (elementWidth - width) * 0.5f;
            m_MaxX = (width - elementWidth) * 0.5f;
            m_MinY = (elementHeigth - height) * 0.5f;
            m_MaxY = (height - elementHeigth) * 0.5f;
        }

        public void OnReset()
        {
            m_MaxInsertTime = 0;
            m_Tree?.Clear();

            foreach (var quad in m_QuadViews)
            {
                quad.Destroy();
            }
            m_QuadViews.Clear();

            totalCount = 0;
            if(inputNum != null && int.TryParse(inputNum.text, out int count))
            {
                float time = Time.realtimeSinceStartup;
                for (int i = 0; i < count; ++i)
                {
                    float x = UnityEngine.Random.Range(m_MinX, m_MaxX);
                    float y = UnityEngine.Random.Range(m_MinY, m_MaxY);
                
                    AddQuad(new Vector3(x, y, gizmosDepth));
                }
                
                float deltaTime = (Time.realtimeSinceStartup - time) * 1000;
                txtTotalTime.text = deltaTime.ToString("f3") + " ms";
            }
        }

        public void ToggleRunning(bool isMoving)
        {
            this.m_IsMoving = isMoving;
        }
        
        public void ToggleSimulation(bool simulation)
        {
            this.m_Simulation = simulation;
            if (simulation)
            {
                GameObject newObj = Instantiate(quad, Vector3.zero, Quaternion.identity, m_Parent);
                newObj.SetActive(true);
                newObj.transform.localScale = new Vector3(viewSize.x, viewSize.y, 1);

                var bound = new Rectangle(0, 0, viewSize.x, viewSize.y);
                m_CamView = new QuadView(newObj, bound);
                var color = Color.white;
                color.a = 0.5f;
                m_CamView.ChangeColor(color);
            }
            else
            {
                m_CamView.MoveTo(new Vector3(100000, 0, 0), 10000000);
                m_CamView.UpdatePosition(1);
            }
        }
        
        private void Update()
        {
            ClickToAddPoint();
            Profiler.BeginSample("Update Quad========================");
            UpdateQuad();
            Profiler.EndSample();
            UpdateUI();

            if (m_Simulation && m_CamView != null)
            {
                if (m_CamView.isMoving)
                {
                    m_CamView.UpdatePosition(Time.deltaTime);
                }
                else
                {
                    Vector3 mousePosition = Input.mousePosition;
                    mousePosition.z = gizmosDepth + 1;
                    mousePosition = m_Cam.ScreenToWorldPoint(mousePosition);
                    m_CamView.MoveTo(mousePosition, 1000000);
                }
                
                QueryInCamObjs();
            }
        }

        private void OnDrawGizmos()
        {
            if (m_Tree != null)
            {
                m_Tree.DrawBound(gizmosDepth);
            }
        }
        
        void ClickToAddPoint()
        {
            if (Input.GetMouseButtonDown(0))
            {
                Vector3 mousePosition = Input.mousePosition;
                Debug.Log(mousePosition);
                mousePosition.z = gizmosDepth;
                mousePosition = m_Cam.ScreenToWorldPoint(mousePosition);
                if (mousePosition.x < m_MinX || mousePosition.x > m_MaxX || mousePosition.y < m_MinY ||
                    mousePosition.y > m_MaxY)
                {
                    Debug.LogWarning("点击位置超出范围！");
                    return;
                }

                AddQuad(mousePosition);
            }
        }

        void AddQuad(Vector3 pos)
        {
            GameObject newObj = Instantiate(quad, pos, Quaternion.identity, m_Parent);
            newObj.SetActive(true);
            newObj.transform.localScale = new Vector3(elementWidth, elementHeigth, 1);
            
            float time = Time.realtimeSinceStartup;
            var bound = new Rectangle(pos.x, pos.y, elementWidth, elementHeigth);
            var node = new QuadView(newObj, bound);
            m_Tree.InsertNode(node);
            float deltaTime = (Time.realtimeSinceStartup - time) * 1000;
            if (deltaTime > m_MaxInsertTime)
            {
                m_MaxInsertTime = deltaTime;
                txtInsertTime.text = m_MaxInsertTime.ToString("f3") + " ms";
            }
            m_QuadViews.Add(node);
                
            totalCount++;
        }

        private float deltaTime = 0;
        void UpdateUI()
        {
            txtNum.text = totalCount.ToString();
            
            deltaTime += (Time.unscaledDeltaTime - deltaTime) * 0.1f;
            float fps = 1.0f / deltaTime;
            txtFPS.text = fps.ToString("f1");
        }

        void UpdateQuad()
        {
            if (!m_IsMoving || m_QuadViews == null || m_QuadViews.Count <= 0) {
                return;
            }

            foreach (var quad in m_QuadViews)
            {
                if (quad.isMoving)
                {
                    quad.UpdatePosition(Time.deltaTime);
                }
                else
                {
                    var x = UnityEngine.Random.Range((quad.Bound.width - treeBound.x) * 0.5f, (treeBound.x - quad.Bound.width) * 0.5f);
                    var y = UnityEngine.Random.Range((quad.Bound.height - treeBound.y) * 0.5f, (treeBound.y - quad.Bound.height) * 0.5f);
                    var tarPos = new Vector3(x, y, gizmosDepth);
                    quad.MoveTo(tarPos, speed);
                }
            }
                
            m_Tree?.Clear();
            
            foreach (var quad in m_QuadViews)
            {
                m_Tree?.InsertNode(quad);
            }
        }
        
        /// <summary>
        /// 查找摄像机范围内的对象
        /// </summary>
        private void QueryInCamObjs()
        {
            if (m_CamView == null)
            {
                return;
            }
            var time = Time.realtimeSinceStartup;
            var objs = m_Tree.QueryRange(m_CamView.Bound);
            var deltaTime = (Time.realtimeSinceStartup - time) * 1000;
            if (deltaTime > m_MaxSearchTime)
            {
                m_MaxSearchTime = deltaTime;
                txtSearchTime.text = m_MaxSearchTime.ToString("f3") + " ms";
            }
            if (objs != null && objs.Count > 0)
            {
                foreach (QuadView quad in m_QuadViews)
                {
                    var color = Color.white;
                    if (objs.Contains(quad)) 
                    {
                        color = m_CamView.Bound.Intersects(quad.Bound)? Color.red : Color.green;
                    }
                    quad.ChangeColor(color);
                }
            }
            else
            {
                for (int i = 0; i < m_QuadViews.Count; ++i)
                {
                    var quad = m_QuadViews[i];
                    quad.ChangeColor(Color.white);
                }
            }
        }
    }
}