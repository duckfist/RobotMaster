sampler2D screen;

float4 SwapColor1;
float4 SwapColor2;
float4 SwapWeapon;

float4 MegaMan1;
float4 MegaMan2;
float4 Weapon;

float4 MyShader( float2 Tex : TEXCOORD0) : COLOR0
{
	// Load screen colors
    float4 Color = tex2D( screen, Tex.xy);
	return Color;

	// Swap Mega Man's colors!
	// If the obtained color equals the passed-in "SwapColor", replace it with green.
	if (Color.r == SwapColor1.r && Color.g == SwapColor1.g && Color.b == SwapColor1.b)
	{
		Color.r = MegaMan1.r;
		Color.g = MegaMan1.g;
		Color.b = MegaMan1.b;
	}
	if (Color.r == SwapColor2.r && Color.g == SwapColor2.g && Color.b == SwapColor2.b)
	{
		Color.r = MegaMan2.r;
		Color.g = MegaMan2.g;
		Color.b = MegaMan2.b;
	}
	if (Color.r == SwapWeapon.r && Color.g == SwapWeapon.g && Color.b == SwapWeapon.b)
	{
		Color.r = Weapon.r;
		Color.g = Weapon.g;
		Color.b = Weapon.b;
	}

	// Grayscale
	// Color.rgb = (Color.r + Color.g + Color.b) / 3; 

	// Invert
	//Color.r = 1 - Color.r;
	//Color.g = 1 - Color.g;
	//Color.b = 1 - Color.b;


    return Color;
}


technique PostProcess
{
    pass p1
    {
        PixelShader = compile ps_4_0_level_9_1 MyShader();
    }

}