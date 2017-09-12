#region Header
/**
 * 名称：相机后期特效的管理器
 
 * 日期：2015.10.23
 * 描述：
 *  这个脚本会由CameraFx自动生成在主相机下，在OnRenderImage的时候调用CameraFx同一个对象下的后期特效脚本的OnRenderImage
 **/
#endregion
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[ExecuteInEditMode]
[RequireComponent(typeof(Camera))]
public class CameraFxMgr : MonoBehaviour
{
    public class CameraHandle : IShareThingHandle
    {
        public string name;
        public CameraFx fx;
        public CameraFxMgr mgr;
        public CameraHandle(string name, CameraFx fx, CameraFxMgr mgr)
        {
            this.fx = fx;
            this.name = name;
            this.mgr = mgr;
        }

        public override void OnLast(IShareThingHandle prev){}
        public override void OnOverlay() { }
        public override void OnEmpty() { }
    }

    #region Fields
    Dictionary<string,ShareThing> m_fxs = new Dictionary<string,ShareThing>();//每一个同名的后期处理是一个共享事物，通过shareThing可以很好的防止特效叠加
    #endregion


    #region Properties
    
    #endregion


    #region Mono Frame
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {

    }
    void OnRenderImage(RenderTexture source, RenderTexture destination)
    {
        if (m_fxs.Count == 0)
        {
            Graphics.Blit(source, destination);
            return;
        }
        else if (m_fxs.Count == 1)//optimize when has only one camera effect
        {
            foreach (ShareThing st in m_fxs.Values)
            {
                st.Get<CameraHandle>().fx.OnUpdateCamera(source, destination);
                break;
            }
        }
        else
        {
            RenderTexture buffer1 = RenderTexture.GetTemporary(source.width, source.height, 0);
            RenderTexture buffer2 = RenderTexture.GetTemporary(source.width, source.height, 0);

            bool oddEven = true;
            bool first=true;
            foreach (ShareThing st in m_fxs.Values)
            {
                if (first)
                {
                    st.Get<CameraHandle>().fx.OnUpdateCamera(source, destination);
                    first =false;
                }
                else
                {
                    if (oddEven)
                    {
                        st.Get<CameraHandle>().fx.OnUpdateCamera(buffer1, buffer2);
                        buffer1.DiscardContents();
                    }
                    else
                    {
                        st.Get<CameraHandle>().fx.OnUpdateCamera(buffer2, buffer1);
                        buffer2.DiscardContents();
                    }
                    oddEven = !oddEven;
                }
            }
            if (oddEven)
                Graphics.Blit(buffer1, destination);
            else
                Graphics.Blit(buffer2, destination);

            RenderTexture.ReleaseTemporary(buffer1);
            RenderTexture.ReleaseTemporary(buffer2);
        }
        
    }
    #endregion
   


    #region Private Methods
    
    #endregion

    public CameraHandle Add(string name,CameraFx fx)
    {
        CameraHandle handle = new CameraHandle(name,fx, this);
        ShareThing st = m_fxs.GetNewIfNo(name);     
        st.Add(handle);

        if (!this.enabled)
            this.enabled = true;
        return handle;
    }

    public void Remove(CameraHandle h)
    {
        if (h.m_shareThing == null)//防止死锁
            return;
        ShareThing st = m_fxs.Get(h.name);
        if (st != h.m_shareThing)
        {
            Debuger.LogError("逻辑错误，找不到shareThing");
            h.m_shareThing = null;//防止死锁
            return;
        }
        st.Remove(h);
        if (st.m_handles.Count == 0)
            m_fxs.Remove(h.name);

        h.fx.Stop();

        if (m_fxs.Count == 0)
        {
            this.enabled = false;
        }
    }
    void Clear()
    {
        foreach(ShareThing st in m_fxs.Values){
            foreach (IShareThingHandle h in st.m_handles)
            {
                h.m_shareThing = null;//注意一定先设为null，不然会造成死锁            
                ((CameraHandle)h).fx.Stop();
            }
            st.Clear();
        }
        m_fxs.Clear();
    }
}
