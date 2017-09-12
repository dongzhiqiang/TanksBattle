using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace Xft
{
    public class XWeaponTrail : MonoBehaviour
    {
        public class Element
        {
            public Vector3 PointStart;

            public Vector3 PointEnd;

            public Vector3 Pos
            {
                get
                {
                    return (PointStart + PointEnd) / 2f;
                }
            }


            public Element(Vector3 start, Vector3 end)
            {
                PointStart = start;
                PointEnd = end;
            }

            public Element()
            {

            }
        }

        [System.Serializable]
        public class WidthFade
        {
            public float delay = 0;
            public float beginScale = 1;
            public float endScale = 0f;
            public float duration = 1;
            public AnimationCurve curve = new AnimationCurve(new Keyframe(0f, 0f, 0f, 1f), new Keyframe(1f, 1f, 1f, 0f));//变化曲线
        }
public class ElementPool {
            private readonly Stack<Element> _stack = new Stack<Element>();

            public int CountAll { get; private set; }
            public int CountActive { get { return CountAll - CountInactive; } }
            public int CountInactive { get { return _stack.Count; } }

            public ElementPool(int preCount) {
                for (int i = 0; i < preCount; i++) {
                    Element element = new Element();
                    _stack.Push(element);
                    CountAll++;
                }
            }

            public Element Get() {
                Element element;
                if (_stack.Count == 0) {
                    element = new Element();
                    CountAll++;
                }
                else {
                    element = _stack.Pop();
                }

                return element;
            }

            public void Release(Element element) {
                if (_stack.Count > 0 && ReferenceEquals(_stack.Peek(), element)) {
                    Debug.LogError("Internal error. Trying to destroy object that is already released to pool.");
                }
                _stack.Push(element);
            }
        }
        #region public members

        public static string Version = "1.0.0";

        public Transform PointStart;
        public Transform PointEnd;

        public int MaxFrame = 14;
        //public int Granularity = 60;
        int Granularity = 30;
        public float Fps = 60f;

        public Color MyColor = Color.white;
        public Material MyMaterial;

        //TrailWidth会变化的功能
        public List<WidthFade> m_fades= new List<WidthFade>();
        
        public float m_destroyDelay = 0;
        #endregion



        #region protected members
        protected float mTrailWidth = 0f;
        protected Element mHeadElem = new Element();
        protected List<Element> mSnapshotList = new List<Element>();
protected ElementPool mElemPool;
        protected Spline mSpline = new Spline();
        protected float mFadeT = 1f;
        protected bool mIsFading = false;
        protected float mFadeTime = 1f;
        protected float mElapsedTime = 0f;
        protected float mFadeElapsedime = 0f;
        protected GameObject mMeshObj;
        MeshRenderer m_mr;
        protected VertexPool mVertexPool;
        protected VertexPool.VertexSegment mVertexSegment;
        protected bool mInited = false;
        protected float mBeginTime =0;
        float m_lastDestroyTime = -1;
        
        #endregion

        #region property
        public float UpdateInterval
        {
            get
            {
                return 1f / Fps;
            }
        }
        public Vector3 CurHeadPos
        {
            get { return (PointStart.position + PointEnd.position) / 2f; }
        }
        public float TrailWidth
        {
            get;set;
        }
        #endregion

        #region API
        //you may pre-init the trail to save some performance.
        public void Init()
        {
            if (mInited)
                return;

            mElemPool = new ElementPool(MaxFrame);
            mTrailWidth = (PointStart.position - PointEnd.position).magnitude;
            m_fades.Sort((WidthFade a, WidthFade b) => a.delay.CompareTo(b.delay));
            InitMeshObj();

            InitOriginalElements();

            InitSpline();

            mInited = true;
        }

        public void Activate()
        {
            Init();
            mBeginTime = Time.time;
            m_lastDestroyTime = -1;
            TrailWidth = mTrailWidth;
            

            //check if scene has changed, need to recreate the mesh obj.
            if (mMeshObj == null)
            {
                InitMeshObj();
                //return;
            }

            //gameObject.SetActive(true);
            if (mMeshObj != null)
                mMeshObj.SetActive(true);
            if (m_mr!= null &&!m_mr.enabled)
                m_mr.enabled = true;

            mFadeT = 1f;
            mIsFading = false;
            mFadeTime = 1f;
            mFadeElapsedime = 0f;
            mElapsedTime = 0f;

            //reset all elemts to head pos.
            for (int i = 0; i < mSnapshotList.Count; i++)
            {
                mSnapshotList[i].PointStart = PointStart.position;
                mSnapshotList[i].PointEnd = PointEnd.position;

                mSpline.ControlPoints[i].Position = mSnapshotList[i].Pos;
                mSpline.ControlPoints[i].Normal = mSnapshotList[i].PointEnd - mSnapshotList[i].PointStart;
            }

            //reset vertex too.
            RefreshSpline();
            UpdateVertex();

            
        }

        public void Deactivate()
        {
            //gameObject.SetActive(false);
            if (mMeshObj != null)
                mMeshObj.SetActive(false);
        }

        public void StopSmoothly(float fadeTime)
        {
            mIsFading = true;
            mFadeTime = fadeTime;
        }

        #endregion

        #region unity methods
        void Start()
        {
            GameObjectPoolObject po=this.GetComponent<GameObjectPoolObject>();
            if (po != null)//为了实现延迟销毁机制，要用到对象池
            {
                po.m_onIngoreDestroy = IngorePoolDestroy;
            }
        }
        void Update()
        {

            if (!mInited)
                return;


            //check if scene has changed, need to recreate the mesh obj.
            if (mMeshObj == null)
            {
                InitMeshObj();
                return;
            }


            UpdateTrailWidth();
            UpdateHeadElem();


            mElapsedTime += Time.deltaTime;
            if (mElapsedTime < UpdateInterval)
            {
                return;
            }
            mElapsedTime = 0f;//FIX: Don't use mElapsedTime -= UpdateInterval, it's not compatible with scaleTime. 

            RecordCurElem();
            //结束效果过程中的话多采样几次，以达到渐变消失的效果
            if (m_lastDestroyTime != -1)
            {
                if (Time.time - m_lastDestroyTime <= m_destroyDelay)
                {
                    float factor = (Time.time - m_lastDestroyTime) / m_destroyDelay;
                    int count = (int)((MaxFrame - 2) * factor);
                    for (int i = 0; i <= count; ++i)
                    {
                        RecordCurElem();
                    }
                }
            }


            RefreshSpline();

            UpdateFade();

            UpdateVertex();
            //UpdateIndices();



            if (m_lastDestroyTime != -1 )
            {
                if(Time.time - m_lastDestroyTime > m_destroyDelay)
                {

                    FxDestroy.DoDestroy(this.gameObject, false);
                    m_lastDestroyTime = -1;
                }
                
                
            }
        }


        void LateUpdate()
        {
            if (!mInited)
                return;


            mVertexPool.LateUpdate();
        }


        void OnEnable()
        {
            if (!GameObjectPool.IsPreloading())//预加载中的话不要激活
                Activate();
        }

        void OnDrawGizmos()
        {
            if (PointEnd == null || PointStart == null)
            {
                return;
            }


            float dist = (PointStart.position - PointEnd.position).magnitude;

            if (dist < Mathf.Epsilon)
                return;


            Gizmos.color = Color.red;

            Gizmos.DrawSphere(PointStart.position, dist * 0.04f);


            Gizmos.color = Color.blue;
            Gizmos.DrawSphere(PointEnd.position, dist * 0.04f);

        }
        public bool m_isApplicationQuit = false;
        void OnApplicationQuit()
        {
            m_isApplicationQuit = true;
        }

        void OnDisable()
        {
            if (m_isApplicationQuit)
                return;
            m_lastDestroyTime = -1;            
            Deactivate();
        }
        
        #endregion

        #region local methods
        void UpdateTrailWidth()
        {
            if (m_fades.Count == 0)
                return ;

            //第一个渐变之前的话算第一个的开始
            float time =Time.time - mBeginTime;
            if(time<= m_fades[0].delay)
            {
                TrailWidth=mTrailWidth*m_fades[0].beginScale;
                return;   
            }

            //找到对应的渐变
            WidthFade fade=null;
            for(int i = 0;i<m_fades.Count;++i){
                fade =  m_fades[i];
                if(time <=(fade.delay+fade.duration))
                    break;
            }

            TrailWidth = mTrailWidth * Mathf.Lerp(fade.beginScale, fade.endScale, fade.duration == 0 ? fade.endScale : fade.curve.Evaluate( Mathf.Clamp01((time - fade.delay) / fade.duration)));
        }

        void InitSpline()
        {
            mSpline.Granularity = Granularity;

            mSpline.Clear();

            for (int i = 0; i < MaxFrame; i++)
            {
                mSpline.AddControlPoint(CurHeadPos, PointStart.position - PointEnd.position);
            }
        }

        void RefreshSpline()
        {
            for (int i = 0; i < mSnapshotList.Count; i++)
            {
                mSpline.ControlPoints[i].Position = mSnapshotList[i].Pos;
                mSpline.ControlPoints[i].Normal = mSnapshotList[i].PointEnd - mSnapshotList[i].PointStart;
            }

            mSpline.RefreshSpline();
        }

        void UpdateVertex()
        {
            
            VertexPool pool = mVertexSegment.Pool;

            float disSq = 0;
            Vector3 posLast=Vector3.zero;
            for (int i = 0; i < Granularity; i++)
            {
                int baseIdx = mVertexSegment.VertStart + i * 3;

                float uvSegment = (float)i / Granularity;


                float fadeT = uvSegment * mFadeT;

                Vector2 uvCoord = Vector2.zero;

                Vector3 pos = mSpline.InterpolateByLen(fadeT);
                if (i > 0)
                    disSq +=  (pos- posLast).sqrMagnitude;
                posLast = pos;
                
                //Debug.DrawRay(pos, Vector3.up, Color.red);

                Vector3 up = mSpline.InterpolateNormalByLen(fadeT);
                Vector3 pos0 = pos + (up.normalized * TrailWidth * 0.5f);
                Vector3 pos1 = pos - (up.normalized * TrailWidth * 0.5f);


                // pos0
                pool.Vertices[baseIdx] = pos0;
                pool.Colors[baseIdx] = MyColor;
                uvCoord.x = 0f;
                uvCoord.y = uvSegment;
                pool.UVs[baseIdx] = uvCoord;

                //pos
                pool.Vertices[baseIdx + 1] = pos;
                pool.Colors[baseIdx + 1] = MyColor;
                uvCoord.x = 0.5f;
                uvCoord.y = uvSegment;
                pool.UVs[baseIdx + 1] = uvCoord;

                //pos1
                pool.Vertices[baseIdx + 2] = pos1;
                pool.Colors[baseIdx + 2] = MyColor;
                uvCoord.x = 1f;
                uvCoord.y = uvSegment;
                pool.UVs[baseIdx + 2] = uvCoord;
            }

            //小于一定值就不要显示了，免得有抖动
            float limit = UpdateInterval * 5;
            float limitSq = limit * limit;
            bool e = disSq > limitSq;
            //Debuger.Log("是不是显示：{0} updateInerval:{1} disSq:{2}",e, limitSq, disSq);
            if (m_mr != null && e != m_mr.enabled)
                m_mr.enabled = e;

            mVertexSegment.Pool.UVChanged = true;
            mVertexSegment.Pool.VertChanged = true;
            mVertexSegment.Pool.ColorChanged = true;

        }

        void UpdateIndices()
        {

            VertexPool pool = mVertexSegment.Pool;

            for (int i = 0; i < Granularity - 1; i++)
            {
                int baseIdx = mVertexSegment.VertStart + i * 3;
                int nextBaseIdx = mVertexSegment.VertStart + (i + 1) * 3;

                int iidx = mVertexSegment.IndexStart + i * 12;

                //triangle left
                pool.Indices[iidx + 0] = nextBaseIdx;
                pool.Indices[iidx + 1] = nextBaseIdx + 1;
                pool.Indices[iidx + 2] = baseIdx;
                pool.Indices[iidx + 3] = nextBaseIdx + 1;
                pool.Indices[iidx + 4] = baseIdx + 1;
                pool.Indices[iidx + 5] = baseIdx;


                //triangle right
                pool.Indices[iidx + 6] = nextBaseIdx + 1;
                pool.Indices[iidx + 7] = nextBaseIdx + 2;
                pool.Indices[iidx + 8] = baseIdx + 1;
                pool.Indices[iidx + 9] = nextBaseIdx + 2;
                pool.Indices[iidx + 10] = baseIdx + 2;
                pool.Indices[iidx + 11] = baseIdx + 1;

            }

            pool.IndiceChanged = true;
        }

        void UpdateHeadElem()
        {
            mSnapshotList[0].PointStart = PointStart.position;
            mSnapshotList[0].PointEnd = PointEnd.position;
        }


        void UpdateFade()
        {
            if (!mIsFading)
                return;

            mFadeElapsedime += Time.deltaTime;

            float t = mFadeElapsedime / mFadeTime;

            mFadeT = 1f - t;

            if (mFadeT < 0f)
            {
                Deactivate();
            }
        }

        void RecordCurElem()
        {
            //TODO: use element pool to avoid gc alloc.
             //Element elem = new Element(PointStart.position, PointEnd.position);

            Element elem = mElemPool.Get();
            elem.PointStart = PointStart.position;
            elem.PointEnd = PointEnd.position;
            if (mSnapshotList.Count < MaxFrame)
            {
                mSnapshotList.Insert(1, elem);
            }
            else
            {
			mElemPool.Release(mSnapshotList[mSnapshotList.Count - 1]);
                mSnapshotList.RemoveAt(mSnapshotList.Count - 1);
                mSnapshotList.Insert(1, elem);
            }

        }

        void InitOriginalElements()
        {
            mSnapshotList.Clear();
            //at least add 2 original elements
            mSnapshotList.Add(new Element(PointStart.position, PointEnd.position));
            mSnapshotList.Add(new Element(PointStart.position, PointEnd.position));
        }



        void InitMeshObj()
        {
            //create a new mesh obj
            mMeshObj = new GameObject("_" + gameObject.name);
            DonDestroyRoot.Add("_XWeaponTrailMesh", mMeshObj);//加到不销毁的目录，免得每次都要创建
            mMeshObj.layer = gameObject.layer;
            mMeshObj.SetActive(true);
            MeshFilter mf = mMeshObj.AddComponent<MeshFilter>();
            m_mr = mMeshObj.AddComponent<MeshRenderer>();
            m_mr.castShadows = false;
            m_mr.receiveShadows = false;
            m_mr.GetComponent<Renderer>().sharedMaterial = MyMaterial;
            mf.sharedMesh = new Mesh();

            //init vertexpool
            mVertexPool = new VertexPool(mf.sharedMesh, MyMaterial);
            mVertexSegment = mVertexPool.GetVertices(Granularity * 3, (Granularity - 1) * 12);


            UpdateIndices();

        }

        #endregion
        
        bool IngorePoolDestroy()
        {
            if (m_destroyDelay == 0)
                return false;//不需要延迟销毁
            m_lastDestroyTime = Time.time;
            //this.transform.parent = null;
            return true;
        }

    }

}


