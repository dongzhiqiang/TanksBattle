#region Header
/**
 * 名称：行为树组件
 
 * 日期：2016.5.13
 * 描述：
 **/
#endregion
using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

namespace Simple.BehaviorTree
{
    public enum enReset
    {
        none,
        clearOnDisable,
        resetOnEnable
    }
    public class BehaviorTree : MonoBehaviour,IBehaviorTree
    {
        public const int Short_Limit = 10;//如果行为树一次就执行完，且中间有耗时行为，那么算短路。短路超过这个次数提示下
        public delegate void OnChangeTree();
        public delegate bool OnCanUpdateTree();


        public event OnChangeTree onChangeTree;//创建或者停止
        public event OnCanUpdateTree onCanUpdateTree;

        public string m_playFile = "";
        public string m_playBehavior = "";
        public bool m_playOnEnable = false;
        public enReset m_resetType = enReset.clearOnDisable;
        public float m_intervalSecond = 10;//一秒10次
        public bool m_runtimeUpdateCancel = false;

        BehaviorTreeCfg m_cfg;
        bool m_isPlaying = false;
        bool m_isUpdating = false;
        bool m_isFirstPlay = true;
        int m_runCounter = 0;//运行计数
        float m_lastUpdateTime = 0;
        ValueMgr m_valueMgr = new ValueMgr();
        Node m_root;
        List<Node> m_nodes = new List<Node>();
        List<Stack<int>> m_stacks = new List<Stack<int>>();//行为树栈，记录着运行到哪个节点了
        List<LinkedList<Conditional>> m_interruptStacks = new List<LinkedList<Conditional>>();//中断栈
        object m_owner;
        int m_ownerPoolId;

        //短路判断逻辑
        int m_shortCounter = 0;//如果行为树一次就执行完，且中间有耗时行为，那么算短路。短路超过这个次数提示下
        bool m_prevIsReRun = false;
        Node m_shortNode = null;

        public BehaviorTreeCfg Cfg { get { return m_cfg; } }
        public string BehaviorName { get { return m_cfg != null ?m_cfg.BehaviorName : ""; } }
        public bool IsPlaying { get { return m_isPlaying; } }
        public bool IsTreeAcitve { get { return m_cfg != null; } }
        public Node Root { get { return m_root; } }
        public int RunCounter { get { return m_runCounter; } }
        public ValueMgr ValueMgr { get { return m_valueMgr; } }

        
        
        #region Mono Frame
        void OnEnable()
        {
            m_runtimeUpdateCancel = false;
            m_lastUpdateTime = 0;
            if (m_playOnEnable)
            {
                if (IsTreeAcitve)
                    RePlay(m_resetType == enReset.resetOnEnable);
                else
                    PlayAuto();
            }

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
            if (m_resetType == enReset.clearOnDisable)
                Stop();
            else
                Pause();
        }

        void Update()
        {
            //否决
            if (onCanUpdateTree != null && !onCanUpdateTree())
                return;
                
            
            
            if (!m_runtimeUpdateCancel&& m_intervalSecond >0&& TimeMgr.instance.logicTime - m_lastUpdateTime > (1f/ m_intervalSecond))
            {
                m_lastUpdateTime = TimeMgr.instance.logicTime;
                CallUpdate();
                
            }
        }
#if UNITY_EDITOR
        void OnDrawGizmos()
        {
            var mgr = BehaviorTreeMgr.instance;
            if (mgr == null|| (BehaviorTreeMgr.instance.m_debug1 != this && BehaviorTreeMgr.instance.m_debug2 != this))
                return;
            
            for (int i = m_stacks.Count - 1; i >= 0; --i)//从后往前出栈
            {
                var s = m_stacks[i];
                foreach(var id in s)
                {
                    m_nodes[id].OnDrawGizmos();
                }
            }
        }
#endif
        #endregion


        #region 对外接口
        public void SetOwner(object owner) 
        {
            if(owner ==null)
            {
                Debuger.LogError("逻辑错误，不能用SetOwner设置一个空的持有者，请用ClearOwner()");
                return;
            }
            m_owner = owner;
            var idType = m_owner as IdType;
            if (idType != null)
                m_ownerPoolId = idType.Id;
        }

        public void ClearOwner()
        {
            m_owner = null;
            m_ownerPoolId = 0;
        }

        public T GetOwner<T>() where T:class
        {
            if (m_owner == null)
                return null;

            //如果是池对象，判断下是不是被回收了
            var idType = m_owner as IdType;
            if (idType != null&& idType.IsDestroy(idType.Id))
            {
                Debuger.LogError("行为树，获取持有者时发现持有者已经放回对象池了，持有者:{0} 行为树:{1}", idType.GetType(), m_cfg!=null?m_cfg.BehaviorName:"");
                return null;
            }

            T t = m_owner as T;
            if (t == null)
            {
                Debuger.LogError("行为树，获取持有者时类型出错，持有者类型:{0} 要获取的类型:{1} 行为树:{2}", idType.GetType(),typeof(T), m_cfg != null ? m_cfg.BehaviorName : "");
                return null;
            }

            return t;
        }

        public void PlayAuto() { Play(m_playFile, m_playBehavior);}

        public void Play(string behavior)
        {
            if(string.IsNullOrEmpty(behavior))
            {
                Play(null,m_resetType);
                return;
            }
            string[] ps = behavior.Split(':');
            if(ps.Length<2 || string.IsNullOrEmpty(ps[0]) ||string.IsNullOrEmpty(ps[1]))
            {
                Debuger.LogError("找不到行为:{0}", behavior);
                return;
            }


            Play(ps[0], ps[1]);
        }

        public void Play(string file, string behavior)
        {
            //如果为空
            if (string.IsNullOrEmpty(file) || string.IsNullOrEmpty(behavior))
                return;

            //创建新的
            BehaviorTreeFileCfg fileCfg = BehaviorTreeMgrCfg.instance.GetFile(file);
            var cfg = fileCfg.GetTree(behavior);
            if (cfg == null)
                return;

            Play(cfg,m_resetType);
        }

        void Play(BehaviorTreeCfg cfg, enReset  resetType )
        {
            m_resetType = resetType;

            if (!this.enabled)
                return;
            //如果有真在播放的，清除掉或者重新播放
            if (m_cfg != null )
            {
                if (m_cfg == cfg)
                {
                    Debuger.LogError("逻辑错误，行为树重新播放了:{0}:{1}", this.m_cfg.File.File, this.m_cfg.name);
                    if (!IsPlaying)
                        RePlay(m_resetType == enReset.resetOnEnable);
                    return;
                }
                else
                    Stop();
            }

            if (cfg == null)
                return;

            InternalCreate(cfg);
            InternalPlay(m_resetType == enReset.resetOnEnable);

        }

        public void RePlay(bool reset)
        {
            if (!this.enabled)
                return;
            if (this.IsTreeAcitve &&!m_isPlaying )
                InternalPlay(reset);
        }

        public void Pause()
        {
            if (!this.enabled)
                return;
            if (m_isPlaying)
                InternalPause();
        }
        
        public void Stop()
        {
            if (!this.enabled)
                return;
            if (m_cfg!=null)
                InternalStop();
        }

        public void CallUpdate()
        {
            if (!this.enabled)
                return;
            if (m_isPlaying)
                InternalUpdate();
        }

        //重新初始化，一般用于编辑器修改树的结构
        public void ReCreate()
        {
            if (m_cfg == null)
                return;
            //bool needPlay = IsPlaying && playIfNeed;
            BehaviorTreeCfg cfg = m_cfg;
            InternalStop();//清空东西,如果播放中，内部会暂停
            InternalCreate(cfg);//重新初始化
            //if (needPlay)
              //  InternalPlay();
        }

        public ShareValueBase<T> GetValue<T>(string name)
        {
            return m_valueMgr.GetValue<T>(name);

        }

        public Node GetNode(int idx)
        {
            return m_nodes[idx];
        }
        #endregion

        #region 实现
        //和InternalStop对应
        void InternalCreate(BehaviorTreeCfg cfg)
        {
            if (m_isUpdating)
            {
                Debuger.LogError("行为树逻辑错误，更新中不能初始化:{0}:{1}", m_cfg.File.File, m_cfg.name);
                return;
            }
            if (m_cfg != null || m_root != null)
            {
                Debuger.LogError("行为树逻辑错误,重复初始化");
            }
            m_cfg = cfg;
            m_isPlaying = false;
            m_isFirstPlay = true;
            m_runCounter = -1;
            m_shortCounter = 0;
            m_shortNode = null;
            m_prevIsReRun = false;
            m_valueMgr.SetCfg(m_cfg.valueMgrCfg);

            //创建节点、初始化节点、计算一些索引
            int idx = 0;
            m_root = DoAddNode(m_cfg.root, ref idx, -1, -1);


            BehaviorTreeMgr.instance.AddTree(this);
            if (onChangeTree != null)
                onChangeTree();
        }

        //创建节点、初始化节点、计算一些索引
        Node DoAddNode(NodeCfg cfg, ref int idx, int parentIdx, int localIdx)
        {
            int curIdx = idx++;
            Node node = BehaviorTreeFactory.CreateNode(cfg);
            node.InitNode(cfg, this, curIdx, parentIdx, localIdx);

            //建立索引
            m_nodes.Add(node);

            //递归遍历子节点
            if (cfg is ParentNodeCfg)
            {
                ParentNodeCfg parentCfg = cfg as ParentNodeCfg;
                ParentNode parentNode = node as ParentNode;

                for (int i = 0, j = parentCfg.children.Count; i < j; ++i)
                {
                    parentNode.ChildrenIdx.Add(idx);
                    DoAddNode(parentCfg.children[i], ref idx, curIdx, i);
                }
            }

            return node;
        }

        //和InternalPause对应
        void InternalPlay(bool reset = false)
        {
            if (IsPlaying)
            {
                Debuger.LogError("行为树逻辑错误，重复播放:{0}:{1}", m_cfg.File.File, m_cfg.name);
                return;
            }
            if (m_isUpdating)
            {
                Debuger.LogError("行为树逻辑错误，更新中不能播放:{0}:{1}", m_cfg.File.File, m_cfg.name);
                return;
            }
            m_isPlaying = true;
            m_shortNode = null;
            m_shortCounter = 0;
            m_prevIsReRun = false;
            bool isFirstPlay = m_isFirstPlay;
            bool needReset = reset || isFirstPlay;
            if (m_isFirstPlay)
                m_isFirstPlay = false;

            //重置下共享变量
            if (needReset )
            {
                m_valueMgr.Reset();
                m_runCounter = -1;
            }
                
            //启用
            for (int i = 0, j = m_nodes.Count; i < j; ++i)
            {
                m_nodes[i].Enable(needReset);
            }

            //创建主栈
            if (m_stacks.Count > 0 || m_interruptStacks.Count>0)
            {
                Debuger.LogError("行为树逻辑出错，播放的时候发现栈没有清空:{0}:{1}", m_cfg.File.File, m_cfg.name);
                ClearStack();
            }
            
            m_stacks.Add(TypePool<Stack<int>>.Get());
            m_interruptStacks.Add(TypePool<LinkedList<Conditional>>.Get());

            //注意，这里不真正运行，update的时候运行
        }

        //和InternalPlay对应
        void InternalPause()
        {
            if (!IsPlaying)
            {
                Debuger.LogError("行为树逻辑错误，重复暂停:{0}:{1}", m_cfg.File.File, m_cfg.name);
                return;
            }

            if (m_isUpdating)
            {
                Debuger.LogError("行为树逻辑错误，更新中不能暂停:{0}:{1}", m_cfg.File.File, m_cfg.name);
                return;
            }

            //没有出栈，先出栈
            ClearStack();

            //禁用
            for (int i = 0, j = m_nodes.Count; i < j; ++i)
            {
                m_nodes[i].Disable();
            }
            m_isPlaying = false;
            m_shortCounter = 0;
            m_prevIsReRun = false;
            m_shortNode = null;
        }
        void ClearStack()
        {
            for (int i = m_stacks.Count - 1; i >= 0; --i)//从后往前出栈
            {
                Stack<int> stack = m_stacks[i];
                LinkedList<Conditional> interruptStack = m_interruptStacks[i];
                
                while (stack.Count != 0)
                {
                    var n = m_nodes[stack.Pop()];
                    n.Pop();
                }
                while (interruptStack.Count != 0)
                {
                    var curInterrupt = interruptStack.Last.Value;
                    interruptStack.RemoveLast();
                    if (!curInterrupt.IsInterrupting)
                    {
                        Debuger.LogError("逻辑错误，在中断栈上却不是中断状态");
                        continue;
                    }
                    curInterrupt.IsInterrupting = false;
                }

                
                TypePool<Stack<int>>.Put(stack);
                TypePool<LinkedList<Conditional>>.Put(interruptStack);

            }
            m_stacks.Clear();
            m_interruptStacks.Clear();
        }

        //中断到条件节点的父节点
        void Interrupt(Stack<int> stack, LinkedList<Conditional> interruptStack, Conditional c)
        {
            var cfg = c.ConditionalCfg;
            
            Node parentNode=null;
            while (stack.Count != 0)
            {
                var n = m_nodes[stack.Pop()];
                n.Pop();
                n.ClearState();

                //中断栈出栈
                var tem = interruptStack.Last;
                while (tem != null)
                {
                    var v = tem.Value;
                    if (v != n && v.ParentIdx != n.Idx)
                        break;
                    v.IsInterrupting = false;
                    interruptStack.RemoveLast();
                    tem = interruptStack.Last;
                }

                //如果已经找到要中断的条件节点的父节点了，
                if (c.ParentIdx == n.Idx)
                {
                    parentNode = n;
                    if (!cfg.resetTreeWhenInterrupt)//重置整个树还是到父节点
                        break;
                }
            }

            //检错下
            if(parentNode==null)
            {
                Debuger.LogError("逻辑出错，中断的时候找不到要中断的父节点:{0} 中断节点id:{1}", m_cfg.BehaviorName, c.Cfg.id);
                return;
            }
                
            //重新入栈，不然就逻辑出错了，会执行到父节点的下一个节点，如果父节点之前不先出栈的话，父节点存在同样的问题
            if (parentNode!= m_root && !cfg.resetTreeWhenInterrupt)
            {
                stack.Push(parentNode.Idx);
                parentNode.Push();
            }


        }
        
        //和InternalCreate对应
        void InternalStop()
        {
           
                
            if (m_cfg == null)
            {
                Debuger.LogError("行为树逻辑错误，重复停止:{0}:{1}", m_cfg.File.File, m_cfg.name);
                return;
            }

            if (m_isUpdating)
            {
                Debuger.LogError("行为树逻辑错误，更新中不能停止:{0}:{1}", m_cfg.File.File, m_cfg.name);
                return;
            }

            //没有暂停要主动暂停
            if (IsPlaying)
                InternalPause();
            

            //回收节点
            for (int i = m_nodes.Count - 1; i >= 0; --i)
            {
                m_nodes[i].Put();
            }
            m_root = null;
            m_nodes.Clear();

            //清空共享变量
            m_valueMgr.Clear();

            //清空配置
            m_cfg = null;
            m_runCounter = -1;
            m_shortCounter = 0;
            m_prevIsReRun = false;
            m_shortNode = null;
            if (BehaviorTreeMgr.instance==null)
            {

            }
            else
                BehaviorTreeMgr.instance.RemoveTree(this);

            if (onChangeTree != null)
                onChangeTree();
        }

        
        void InternalUpdate()
        {
            if (!m_isPlaying)
            {
                Debuger.LogError("不在播放中，却更新了");
                return;
            }
            if (m_cfg == null)//可能因为其他出错导致的，由于这里是不断update的，所以就不打印报错了
                return;
            
            m_isUpdating = true;
            for (int i = m_stacks.Count - 1; i >= 0; --i)//从后往前出栈
            {
                Stack<int> stack = m_stacks[i];
                LinkedList<Conditional> interruptStack = m_interruptStacks[i];

                //中断逻辑,从头判断,越接近根节点优先级更高
                LinkedListNode<Conditional> curInterrupt = interruptStack.First;
                while (curInterrupt != null)
                {
                    var interruptNode = curInterrupt.Value;
                    if (interruptNode.CheckInterupt())
                    {
                        m_shortNode = interruptNode;
                        Interrupt(stack, interruptStack, interruptNode);
                        break;
                    }
                    curInterrupt = curInterrupt.Next;
                }

                bool isReRun = stack.Count == 0;//是不是从头执行
                //检错下
                if (i > 0 && isReRun)
                {
                    Debuger.LogError("行为树逻辑错误,非主栈却为空");
                    TypePool<Stack<int>>.Put(stack);
                    TypePool<LinkedList<Conditional>>.Put(interruptStack);
                    m_stacks.RemoveAt(i);
                    m_interruptStacks.RemoveAt(i);
                    continue;
                }

                //新的一轮遍历开始,增加计数
                if (isReRun)
                {
                    ++m_runCounter;

                    for (int j = 0, k = m_nodes.Count; j < k; ++j)
                        m_nodes[j].ReUpdate();

                   
                }

                //检查短路, 暂时只支持第一个栈
                if (i == 0 )
                {
                    if (isReRun && m_prevIsReRun)
                    {
                        ++m_shortCounter;
                        if (m_shortCounter == Short_Limit && m_shortNode != null)
                        {
                            Debuger.LogError("行为树存在短路现象,{0}。可能出现问题的节点id:{1}({2})", m_cfg.BehaviorName, m_shortNode.Cfg.id, m_shortNode.Cfg.Name);
                        }
                    }
                    else
                        m_shortCounter = 0;

                    m_prevIsReRun = isReRun;
                    m_shortNode = null;
                }
                
                //没有就从头运行(当然前提是主树)，有就从当前运行
                var s = enNodeState.inactive;
                int idx = isReRun ? 0 : stack.Peek();
                do
                {
                    s = RunNode(stack, interruptStack, idx,i);//只能往下递归，往上递归的实现还是得在这个循环里调用这个函数

                   
                    //完全执行完了
                    if (stack.Count == 0)
                        break;

                    //有正在运行的，那么就保留着栈，下个update再执行
                    if (s == enNodeState.running)
                        break;

                    //没有执行完的话，那么向跟部方向再执行
                    idx = stack.Peek();
                }
                while (s != enNodeState.inactive);

                //如果不是主树，那么执行完就清空
                if (i != 0 && stack.Count == 0)
                {
                    TypePool<Stack<int>>.Put(stack);
                    TypePool<LinkedList<Conditional>>.Put(interruptStack);
                    m_stacks.RemoveAt(i);
                    m_interruptStacks.RemoveAt(i);
                }
            }
            m_isUpdating = false;
        }
        //入栈出栈的维护
        enNodeState RunNode(Stack<int> stack, LinkedList<Conditional> interruptStack,  int idx,int stackIdx)
        {
            Node node = m_nodes[idx];

            //没入栈的话入栈
            bool isLastRunning = stack.Count > 0 && stack.Peek() == idx;//上次是不是运行，也就是上次是不是入栈但是没有出栈
            if (!isLastRunning)
            {
                stack.Push(idx);
                node.Push();
            }

            //运行子节点
            ParentNode parentNode = node as ParentNode;
            if (parentNode != null)
            {
                do
                {
                    enNodeState childState;
                    int childIdx = parentNode.GetNextChildIdx();
                    if (childIdx == -1)
                        break;

                    //如果禁用了，那么不执行下面的逻辑
                    Node child= m_nodes[childIdx];
                    if (child.Cfg.Ingore)
                        continue;
                    
                    childState = RunNode(stack, interruptStack, childIdx, stackIdx);
                    if (childState == enNodeState.running || childState == enNodeState.inactive)
                        return childState;
                } while (true);
            }

            //运行自己Executed
            enNodeState s = node.Execute(enExecute.normal);
#if UNITY_EDITOR
            //检错下，条件不能为runing状态
            if (node is Conditional && s == enNodeState.running)
                Debuger.LogError("逻辑错误，条件节点不能为运行中状态：{0}",node.GetType());
            else if(!node.CanRuning && s == enNodeState.running)
                Debuger.LogError("逻辑错误，不支持运行中状态：{0}", node.GetType());
#endif
            //不在运行的话出栈
            if (s != enNodeState.running)
            {
                //检错下
                if (stack.Peek() != idx)
                {
                    Debuger.LogError("行为树逻辑错误,pop的时候发现栈顶不是当前节点:{0}", idx);
                    return enNodeState.inactive;//让这一次的递归马上结束
                }

                if (stackIdx == 0 &&node.Cfg.NodeType.checkShort)
                {
                    if(m_shortNode==null || s == enNodeState.failure)
                        m_shortNode = node;
                }

                //告诉下父节点
                if (node.ParentIdx != -1)
                {
                    ParentNode patentNode2 = m_nodes[node.ParentIdx] as ParentNode;
                    if (patentNode2 == null)
                    {
                        Debuger.LogError("行为树逻辑错误,父节点找不到:{0}", idx);
                        return enNodeState.inactive;//让这一次的递归马上结束
                    }
                    patentNode2.OnChildPop(idx, s);
                }


                stack.Pop();
                node.Pop();

                //中断逻辑，中断节点的父节点出栈的时候取消监听,中断节点出栈的时候开始监听中断
                var curInterrupt = interruptStack.Last;
                while (curInterrupt!= null)
                {
                    var v = curInterrupt.Value;
                    if (v != node && v.ParentIdx != node.Idx)
                        break;
                    v.IsInterrupting = false;
                 
                    interruptStack.RemoveLast();
                    curInterrupt = interruptStack.Last;
                }
                Conditional c = node as Conditional;
                if (c != null && c.ConditionalCfg.interrupt != enInterrupt.none)
                {
                    c.IsInterrupting = true;
                    interruptStack.AddLast(c);
                }
                    

            }

            return s;
        }
        
        #endregion
        //新开一个栈给一个节点运行
        public int AddStack(int idx)
        {
            if (!m_isUpdating)
            {
                Debuger.LogError("入栈属于内部操作，暂时不支持在外部逻辑中调用");
                return -1;
            }
            var node = m_nodes[idx];
            if (node.IsInStack)
            {
                Debuger.LogError("逻辑错误，一个节点在栈上却要求入栈:{0} id:{1}", m_cfg.BehaviorName,idx);
                return -1;
            }

            var stack = TypePool<Stack<int>>.Get();
            var interruptStack = TypePool<LinkedList<Conditional>>.Get();

            m_stacks.Add(stack);
            m_interruptStacks.Add(interruptStack);

            stack.Push(idx);
            node.Push();

            return m_stacks.Count - 1;
        }
    }
}