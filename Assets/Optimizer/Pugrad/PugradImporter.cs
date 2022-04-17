using UnityEngine;
using UnityEditor;
using UnityEditor.Experimental.AssetImporters;

namespace Pugrad {

// Supported colormap type list
public enum ColormapType { Viridis, Plasma, Magma, Inferno, Turbo, HSLuv }

// Custom importer for .pugrad files
[ScriptedImporter(1, "pugrad")]
public sealed class PugradImporter : ScriptedImporter
{
    [SerializeField] ColormapType _colormap = ColormapType.Viridis;
    [SerializeField] uint _resolution = 256;
    [SerializeField] float _lightness = 0.5f;

    public override void OnImportAsset(AssetImportContext context)
    {
        var texture = new Texture2D((int)_resolution, 1);
        texture.wrapMode = TextureWrapMode.Clamp;
        texture.SetPixels(GenerateColormap(_colormap, _resolution, _lightness));
        texture.Apply();

        context.AddObjectToAsset("colormap", texture);
        context.SetMainObject(texture);
    }

    // static Color[] GenerateColormap(ColormapType type, uint width, float light)
    //   => type switch
    //     { ColormapType.Viridis => MatplotlibColormaps.GenerateViridis(width),
    //       ColormapType.Plasma  => MatplotlibColormaps.GeneratePlasma(width),
    //       ColormapType.Magma   => MatplotlibColormaps.GenerateMagma(width),
    //       ColormapType.Inferno => MatplotlibColormaps.GenerateInferno(width),
    //       ColormapType.Turbo   => TurboColormap.Generate(width),
    //       ColormapType.HSLuv   => HsluvColormap.Generate(width, light),
    //       _ => null };

      static Color[] GenerateColormap(ColormapType type, uint width, float light){
			switch(type){
				case ColormapType.Viridis:
					return MatplotlibColormaps.GenerateViridis(width);
				case ColormapType.Plasma:
					return MatplotlibColormaps.GeneratePlasma(width);
				case ColormapType.Magma:
					return MatplotlibColormaps.GenerateMagma(width);
				case ColormapType.Inferno:
					return MatplotlibColormaps.GenerateInferno(width);
				case ColormapType.Turbo:
					return TurboColormap.Generate(width);
				case ColormapType.HSLuv:
					return HsluvColormap.Generate(width, light);
			}
			return null;
		}
}

} // namespace Pugrad
