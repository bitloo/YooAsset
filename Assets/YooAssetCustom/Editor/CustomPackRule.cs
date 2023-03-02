using System;
using System.IO;
using UnityEditor;
using YooAsset;
using YooAsset.Editor;

/// <summary>
/// 以收集器路径下顶级文件夹为资源包名，没有顶级文件夹的以收集器路径为资源包名
/// 注意：顶级文件夹下所有文件打进一个资源包
/// 例如：收集器路径为 "Assets/UIPanel"
/// 例如："Assets/UIPanel/Shop/Image/backgroud.png" --> "assets_uipanel_shop.bundle"
/// 例如："Assets/UIPanel/Shop/View/main.prefab" --> "assets_uipanel_shop.bundle"
/// 例如："Assets/UIPanel/sub.prefab" --> "assets_uipanel.bundle"
/// </summary>
[DisplayName("资源包名: 收集器下顶级文件夹路径或收集器路径")]
public class PackTopOrRootDirectory : IPackRule
{
    PackRuleResult IPackRule.GetPackRuleResult(PackRuleData data)
    {
	    string assetPath = data.AssetPath.Replace(data.CollectPath, string.Empty);
	    assetPath = assetPath.TrimStart('/');
	    string[] splits = assetPath.Split('/');
	    if (splits.Length > 0)
	    {
		    string bundleName;
		    if (Path.HasExtension(splits[0]))
		    {
			    bundleName = GetCollectorName(data.CollectPath);
		    }
		    else
		    {
			    bundleName = $"{data.CollectPath}/{splits[0]}";
		    }
		    PackRuleResult result = new PackRuleResult(bundleName, DefaultPackRule.AssetBundleFileExtension);
		    return result;
	    }
	    else
	    {
		    throw new Exception($"Not found root directory : {assetPath}");
	    }
    }

    bool IPackRule.IsRawFilePackRule()
    {
	    return false;
    }
    
    private string GetCollectorName(string collectPath)
    {
	    if (!AssetDatabase.IsValidFolder(collectPath))
	    {
		    return StringUtility.RemoveExtension(collectPath);
	    }
	    return collectPath;
    }
}