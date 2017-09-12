using UnityEngine;
using System.Collections;

public abstract class Task   {


    #region Fields

    #endregion

    #region Properties

    #endregion

    #region Frame    
    public virtual void OnInit() { }
    public virtual void OnClear() { }
    #endregion

    #region Private    

    #endregion

    //public virtual bool IsGetReward() { return false; }
    public virtual void GetReward() { }





}

