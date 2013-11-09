float4 layer_speed = float4( 0.69, 0.52, 0.75, 1.00 );
float time_0_X : Time0_X;

struct VS_OUTPUT
{
   float4 Pos       : POSITION;
   float3 TexCoord0 : TEXCOORD0;
   float3 TexCoord1 : TEXCOORD1;
   float3 TexCoord2 : TEXCOORD2;
   float3 TexCoord3 : TEXCOORD3;
};

VS_OUTPUT vs_main_fire (float4 vPosition: POSITION, float3 vTexCoord0 : TEXCOORD0)
{
   VS_OUTPUT Out = (VS_OUTPUT) 0; 

   // Align quad with the screen
   Out.Pos = float4 (vPosition.x, vPosition.y, 0.0f, 1.0f);

   // Output TexCoord0 directly
   Out.TexCoord0 = vTexCoord0;

   // Base texture coordinates plus scaled time
   Out.TexCoord1.x = vTexCoord0.x;
   Out.TexCoord1.y = vTexCoord0.y + layer_speed.x * time_0_X;

   // Base texture coordinates plus scaled time
   Out.TexCoord2.x = vTexCoord0.x;
   Out.TexCoord2.y = vTexCoord0.y + layer_speed.y * time_0_X;

   // Base texture coordinates plus scaled time
   Out.TexCoord3.x = vTexCoord0.x;
   Out.TexCoord3.y = vTexCoord0.y + layer_speed.z * time_0_X;

   return Out;
}




float distortion_amount2 = float( 0.07 );
float4 height_attenuation = float4( 0.44, 0.29, 0.00, 1.00 );
float distortion_amount1 = float( 0.09 );
float distortion_amount0 = float( 0.12 );

texture fire_base_Tex;

sampler fire_base = sampler_state
{
   Texture = (fire_base_Tex);
   ADDRESSU = CLAMP;
   ADDRESSV = CLAMP;
   MAGFILTER = LINEAR;
   MINFILTER = LINEAR;
   MIPFILTER = LINEAR;
   ADDRESSW = CLAMP;
};

texture fire_distortion_Tex;
sampler fire_distortion = sampler_state
{
   Texture = (fire_distortion_Tex);
   ADDRESSU = WRAP;
   ADDRESSV = WRAP;
   MAGFILTER = LINEAR;
   MINFILTER = LINEAR;
   MIPFILTER = LINEAR;
   ADDRESSW = WRAP;
};

texture fire_opacity_Tex;
sampler fire_opacity = sampler_state
{
   Texture = (fire_opacity_Tex);
   ADDRESSU = CLAMP;
   ADDRESSV = CLAMP;
   MAGFILTER = LINEAR;
   MINFILTER = LINEAR;
   MIPFILTER = LINEAR;
   ADDRESSW = CLAMP;
};

// Bias and double a value to take it from 0..1 range to -1..1 range
float4 bx2(float x)
{
   return 2.0f * x - 1.0f;
}

float4 ps_main_fire (float4 tc0 : TEXCOORD0, float4 tc1 : TEXCOORD1, float4 tc2 : TEXCOORD2, float4 tc3 : TEXCOORD3) : COLOR
{
   // Sample noise map three times with different texture coordinates
   float4 noise0 = tex2D(fire_distortion, tc1);
   float4 noise1 = tex2D(fire_distortion, tc2);
   float4 noise2 = tex2D(fire_distortion, tc3);

   // Weighted sum of signed noise
   float4 noiseSum = bx2(noise0.r) * distortion_amount0 + bx2(noise1.r) * distortion_amount1 + bx2(noise2.r) * distortion_amount2;

   // Perturb base coordinates in direction of noiseSum as function of height (y)
   float4 perturbedBaseCoords = tc0 + noiseSum * (tc0.y * height_attenuation.x + height_attenuation.y);

   // Sample base and opacity maps with perturbed coordinates
   float4 base = tex2D(fire_base, perturbedBaseCoords);
   float4 opacity = tex2D(fire_opacity, perturbedBaseCoords);

   return base * opacity;
}


//--------------------------------------------------------------//
// Technique Section for Effect Workspace.Fire Effects.Fire
//--------------------------------------------------------------//
technique Fire
{
   pass Single_Pass
   {
      ZENABLE = TRUE;
      FILLMODE = SOLID;
      SHADEMODE = GOURAUD;
      ZWRITEENABLE = TRUE;
      ALPHATESTENABLE = FALSE;
      LASTPIXEL = TRUE;
      SRCBLEND = ONE;
      DESTBLEND = ZERO;
      CULLMODE = NONE;
      ALPHAREF = 0x0;
      ALPHAFUNC = LESS;
      DITHERENABLE = FALSE;
      ALPHABLENDENABLE = FALSE;
      FOGENABLE = FALSE;
      SPECULARENABLE = FALSE;
      FOGCOLOR = 0xFFFFFFFF;
      FOGTABLEMODE = NONE;
      FOGSTART = 0.000000;
      FOGEND = 0.000000;
      FOGDENSITY = 0.000000;
      STENCILENABLE = FALSE;
      STENCILFAIL = KEEP;
      STENCILZFAIL = KEEP;
      STENCILPASS = KEEP;
      STENCILFUNC = ALWAYS;
      STENCILREF = 0x0;
      STENCILMASK = 0xffffffff;
      STENCILWRITEMASK = 0xffffffff;
      TEXTUREFACTOR = 0x0;
      WRAP0 = 0;
      WRAP1 = 0;
      WRAP2 = 0;
      WRAP3 = 0;
      WRAP4 = 0;
      WRAP5 = 0;
      WRAP6 = 0;
      WRAP7 = 0;
      CLIPPING = FALSE;
      LIGHTING = FALSE;
      AMBIENT = 0x11111111;
      FOGVERTEXMODE = NONE;
      COLORVERTEX = TRUE;
      LOCALVIEWER = TRUE;
      NORMALIZENORMALS = FALSE;
      DIFFUSEMATERIALSOURCE = COLOR1;
      SPECULARMATERIALSOURCE = COLOR2;
      AMBIENTMATERIALSOURCE = COLOR2;
      EMISSIVEMATERIALSOURCE = COLOR2;
      VERTEXBLEND = DISABLE;
      CLIPPLANEENABLE = 0;
      POINTSIZE = 0.000000;
      POINTSIZE_MIN = 0.000000;
      POINTSPRITEENABLE = FALSE;
      POINTSCALEENABLE = FALSE;
      POINTSCALE_A = 0.000000;
      POINTSCALE_B = 0.000000;
      POINTSCALE_C = 0.000000;
      MULTISAMPLEANTIALIAS = TRUE;
      MULTISAMPLEMASK = 0xffffffff;
      PATCHEDGESTYLE = DISCRETE;
      POINTSIZE_MAX = 0.000000;
      INDEXEDVERTEXBLENDENABLE = FALSE;
      COLORWRITEENABLE = RED | GREEN | BLUE | ALPHA;
      TWEENFACTOR = 0.000000;
      BLENDOP = ADD;
      POSITIONDEGREE = CUBIC;
      NORMALDEGREE = LINEAR;
      ZFUNC = ALWAYS;

      VertexShader = compile vs_1_1 vs_main_fire();
      PixelShader = compile ps_2_0 ps_main_fire();
   }

}