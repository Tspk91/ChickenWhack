// Unity built-in shader source. Copyright (c) 2016 Unity Technologies. MIT license (see license.txt)

// Simplified Additive Particle shader. Differences from regular Additive Particle one:
// - no Tint color
// - no Smooth particle support
// - no AlphaTest
// - no ColorMask

Shader "Mobile/Particles/AdditiveSeethrough" {
Properties {
    _MainTex ("Particle Texture", 2D) = "white" {}
}

Category {
    Tags { "Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent" "PreviewType"="Plane" }
    Cull Off Lighting Off ZWrite Off Fog { Color (0,0,0,0) }

    BindChannels {
        Bind "Color", color
        Bind "Vertex", vertex
        Bind "TexCoord", texcoord
    }

    SubShader {
        Pass {

			Blend SrcAlpha One

            SetTexture [_MainTex] {
                combine texture * primary
            }
        }

		Pass {

			Blend SrcAlpha OneMinusSrcAlpha

			ZTest Greater

			SetTexture[_MainTex] {
				combine texture * primary
			}

			SetTexture[_MainTex] {
				constantColor(1.5,0.8,1,1)
				combine previous * constant
			}
		}
    }
}
}
