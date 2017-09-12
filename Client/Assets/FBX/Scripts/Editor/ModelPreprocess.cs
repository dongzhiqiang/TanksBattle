#region Header
/**
 * 名称：导入模型的时候默认将动画改成legacy
 
 * 日期：2015.10.26
 * 描述：
 **/
#endregion
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;


public class ModelPreprocess : AssetPostprocessor
{
    public void OnPreprocessModel()
    {
        ModelImporter modelImporter = (ModelImporter)assetImporter;
        if(modelImporter.animationType!= ModelImporterAnimationType.None)
            modelImporter.animationType = ModelImporterAnimationType.Legacy;
    }   
    

}
