using System;
using System.Windows.Media.Effects;
using HandyControl.Tools;

namespace HandyControl.Media.Effects;

public class ColorComplementEffect : EffectBase
{
    private static readonly PixelShader Shader;

    static ColorComplementEffect()
    {
        Shader = new PixelShader
        {
            UriSource = ResourceHelper.GetComponentUri("Resources/Effects/ColorComplementEffect.ps")
        };
    }

    public ColorComplementEffect()
    {
        PixelShader = Shader;

        UpdateShaderValue(InputProperty);
    }
}
