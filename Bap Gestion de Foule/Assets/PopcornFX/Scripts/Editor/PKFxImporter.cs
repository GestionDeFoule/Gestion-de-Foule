using UnityEngine;
using UnityEditor;
using System;
using System.IO;
using System.Text;
using System.Runtime.InteropServices;
using UnityEditor.Experimental.AssetImporters;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;

[ScriptedImporter(1, new[] { "pkfx", "pkat", "pkmm", "pkfm", "pksc", "pkan" })]
public class PKFxImporter : ScriptedImporter
{
	public override void OnImportAsset(AssetImportContext ctx)
	{
		var fileName = ctx.assetPath.Substring("Assets/Resources/".Length);

		PKFxAsset fxa = null;
		Texture2D thumb = null;

		if (Path.GetExtension(ctx.assetPath) == ".pkat")
		{
			fxa = ScriptableObject.CreateInstance<PKFxAtlasAsset>();
			thumb = Resources.Load("Icons/AT") as Texture2D;
		}
		else if (Path.GetExtension(ctx.assetPath) == ".pkan")
		{
			fxa = ScriptableObject.CreateInstance<PKFxAnimationAsset>();
			thumb = Resources.Load("Icons/AN") as Texture2D;
		}
		else if (Path.GetExtension(ctx.assetPath) == ".pksc")
		{
			fxa = ScriptableObject.CreateInstance<PKFxSimCacheAsset>();
			thumb = Resources.Load("Icons/SC") as Texture2D;
		}
		else if (Path.GetExtension(ctx.assetPath) == ".pkfm")
		{
			fxa = ScriptableObject.CreateInstance<PKFxFontMetricsAsset>();
			thumb = Resources.Load("Icons/FM") as Texture2D;
		}
		else if (Path.GetExtension(ctx.assetPath) == ".pkmm")
		{
			fxa = ScriptableObject.CreateInstance<PKFxMeshAsset>();
			thumb = Resources.Load("Icons/MM") as Texture2D;
		}

		if (fxa != null)
		{
			fxa.m_Data = File.ReadAllBytes(ctx.assetPath);
			fxa.name = fxa.m_AssetName = fileName;
#if UNITY_2017_3 || UNITY_2017_4 || UNITY_2018
			ctx.AddObjectToAsset(fileName, fxa, thumb);
#else
			ctx.SetMainAsset(fileName, fxa, thumb);
#endif
		}
	}
}
