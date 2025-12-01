// ---- COMPONENT c_trig:sin_cos trig_helper ----

float4 _trig_helper_vector_cos_rotations_triangle(		// triangle wave approximation to cosine(2*pi*a)
	float4 a)
{
	return 1.0f - abs(a - round(a)) * 4.0f;				// invert and scale to [-1, 1]
}

float4 _trig_helper_vector_sin_rotations_estimate_clamped(float4 a)
{
	float4 y=	a * (-16.0f * abs(a) + 8.0f);
	return		y * (0.225f * abs(y) + 0.775f);
}	

float4 _trig_helper_vector_sin_rotations_estimate(float4 a)
{
	float4 w=	a - round(a);			// wrap to [-0.5, 0.5] range
	return _trig_helper_vector_sin_rotations_estimate_clamped(w);
}

float4 _trig_helper_vector_pseudo_sin_rotations_clamped(float4 a)
{
	return a * (abs(a) * -16.0f + 8.0f);
}

float4 _trig_helper_vector_pseudo_sin_rotations(float4 a)
{
	float4 w=	a - round(a);			// wrap to [-0.5, 0.5] range
	return _trig_helper_vector_pseudo_sin_rotations_clamped(w);
}

float4 _trig_helper_vector_cos_rotations_estimate(float4 a)
{
	return _trig_helper_vector_sin_rotations_estimate(a + 0.25f);
}

float4 _trig_helper_vector_sin_cos_rotations_estimate(float4 a)
{
	return _trig_helper_vector_sin_rotations_estimate(a + float4(0.0f, 0.25f, 0.0f, 0.25f));
}

float4 _trig_helper_vector_sin_estimate_clamped(float4 a)
{
	float4 y=	a * (-0.40528473456935108577551785f * abs(a) + 1.2732395447351626861510701f);
	return		y * (0.225f * abs(y) + 0.775f);
}	

float4 _trig_helper_vector_sin_estimate(float4 a)
{
	float4 rotations=	a * 0.15915494309189533576888376f;				// map radians to rotations
	return _trig_helper_vector_sin_rotations_estimate(rotations);
}

float4 _trig_helper_vector_cos_estimate(float4 a)
{
	float4 rotations=	(a * 0.15915494309189533576888376f + 0.25f);	// map radians to rotations, and offset by 1/4 rotation (cos(rot)= sin(rot+0.25)
	return _trig_helper_vector_sin_rotations_estimate(rotations);
}

float4 _trig_helper_vector_sin_cos_estimate(float4 a)
{
	float4 rotations=	(a * 0.15915494309189533576888376f + float4(0.0f, 0.25f, 0.0f, 0.25f));
	return _trig_helper_vector_sin_rotations_estimate(rotations);
}

// takes in 0..1 as an angle, outputs x= cos(angle), y= sin(angle), z= -sin(angle, w= cos(angle)
// i.e. a 2D rotation matrix
float4 _trig_helper_vector_rotation_matrix_estimate_in_rotations(float4 a)
{
	float4 rotations=	(a + float4(0.25f, 0.0f, 0.5f, 0.25f));
	return _trig_helper_vector_sin_rotations_estimate(rotations);
}

// a faster version of the signum function that returns 1 for the argument 0
float _trig_helper_signum_no_zero(float x)
{
	return x >= 0.0 ? 1.0 : -1.0;
}

// returns a normalized (0..1) atan2 approximation
float _trig_helper_normalized_atan2_estimate(float y, float x)
{
	float apx = abs(x);
	float apy = abs(y);
	float spx = _trig_helper_signum_no_zero(x);
	float spy = _trig_helper_signum_no_zero(y);
	float d = (apx - apy)/(apx + apy);
	float da = (d*(1.0/8.0)+(1.0/8.0));
	float ds = (da*spx + (1.0/4.0)) * spy;
	return 1.0f-frac(ds);
}

// ---- COMPONENT c_fake_bitwise_ops:c_fake_bitwise_ops fake_bitwise_ops ----

float4 _fake_bitwise_ops_fake_xor(
	float4 a,
	float4 b)
{
	return fmod(a + b, 2);
}	

// ---- COMPONENT c_bytecode_op_functions:c_bytecode_op_functions g_bytecode_op_functions ----

float4 bytecode_op_triangle(
	float4 X)
{
	float4 wrapped= X - round(X);				// wrap to [-0.5, 0.5] range
	float4 abs_wrap= abs(wrapped);				// abs turns into triangle wave between [0, 0.5]
	float4 triangle_result= abs_wrap * 2.0f;	// scale to [0, 1] range

	return triangle_result;
}

float4 bytecode_op_jitter(
	float4 X)
{
	float4 rotations= X.xxxx * float4(4.67f, 2.99f, 1.08f, 1.35f) + float4(0.52f, 0.37f, 0.16f, 0.79f);

	// optimized scaled-sum-of-sines
	float4 a= rotations - round(rotations);	// wrap to [-0.5, 0.5] range	
	float4 ma= abs(a) * -16.0f + 8.0f;
	float4 sa= a * 0.25f;
	float v= dot(sa, ma) + 0.5f;

	// hermite smooth interpolation (3*v^2 - 2*v^3)
	float v2=			v*v;
	float jitter_result= (-2.0f * v + 3.0f) * v2;

	return jitter_result.xxxx;
}

float4 bytecode_op_wander(
	float4 X)
{
	float4 rot0= X.xxxx * float4(4.08f, 1.02f, 3.0f/5.37f, 3.0f/9.67f) + float4(0.92f, 0.33f, 0.26f, 0.54f);
	float4 rot1= X.xxxx * float4(1.83f, 3.09f, 0.39f, 0.87f) + float4(0.12f, 0.37f, 0.16f, 0.79f);
	float4 sines0= _trig_helper_vector_pseudo_sin_rotations(rot0);
	float4 sines1= _trig_helper_vector_pseudo_sin_rotations(rot1) * float4(0.02f, 0.02f, 0.28f, 0.28f);
	float wander_result= 0.5f + dot(sines0, sines1);

	return wander_result.xxxx;
}

float4 bytecode_op_rand(
	float4 X)
{
	// these magic numbers are 1/(prime/1000000)
	float v0= floor(X.x);
	float val0= dot(v0.xxxx, float4(1.0f/1.043501f, 1.0f/0.794471f, 1.0f/0.113777f, 1.0f/0.015101f));
	val0= frac(val0);

	//			val0=	bbs(val0);		// Blum-Blum-Shub randomimzer
	val0= val0 * val0 * 251.0f;
	val0= frac(val0);

	return val0.xxxx;
}

float4 bytecode_op_rand_smooth(
	float4 X)
{
	float v= X.x;
	float v0= round(v);
	float v1= v0 + 1.0f;
	float f= v - v0;
	float f2= f * f;

	// hermite smooth interpolation (3*f^2 - 2*f^3)
	float smooth_f= (-2.0f * f + 3.0f) * f2;

	// these magic numbers are 1/(prime/1000000)
	float val0= dot(v0.xxxx, float4(1.0f/1.043501f, 1.0f/0.794471f, 1.0f/0.113777f, 1.0f/0.015101f));
	float val1=	dot(v1.xxxx, float4(1.0f/1.043501f, 1.0f/0.794471f, 1.0f/0.113777f, 1.0f/0.015101f));

	val0= frac(val0);
	val1= frac(val1);

//			val0=	bbs(val0);		// Blum-Blum-Shub randomimzer
	val0= val0 * val0 * 251.0f;
	val0= frac(val0);

//			val10=	bbs(val1);		// Blum-Blum-Shub randomimzer
	val1= val1 * val1 * 251.0f;
	val1= frac(val1);

	float rand_smooth_result= lerp(val0, val1, smooth_f);

	return rand_smooth_result.xxxx;
}

// evals a cubic polynomial across four channels with estrin form
float4 bytecode_op_spline4_const(
	float4 X,
	float4 C3,
	float4 C2,
	float4 C1,
	float4 C0,
	float4 thresholds)
{
	float4 high= C3 * X + C2;
	float4 low= C1 * X + C0;
	float4 X2= X * X;
	float4 evaluated_spline= high * X2 + low;

	float4 threshold_mask= step(thresholds, X);
	float4 channel_mask= float4(_fake_bitwise_ops_fake_xor(threshold_mask, threshold_mask.yzww).xyz, threshold_mask.w);
	float4 spline_result_in_4= evaluated_spline * channel_mask;
	float spline_result= spline_result_in_4.x + spline_result_in_4.y + spline_result_in_4.z + spline_result_in_4.w;

	return spline_result.xxxx;
}

// evals a cubic polynomial across eight channels with estrin form
float4 bytecode_op_spline8_const(
	float4 X,
	float4 C3,
	float4 C2,
	float4 C1,
	float4 C0,
	float4 D3,
	float4 D2,
	float4 D1,
	float4 D0,
	float4 C_thresholds,
	float4 D_thresholds)
{
	float4 C_high= C3 * X + C2;
	float4 C_low= C1 * X + C0;
	float4 D_high= D3 * X + D2;
	float4 D_low= D1 * X + D0;
	float4 X2= X * X;
	float4 C_evaluated_spline= C_high * X2 + C_low;
	float4 D_evaluated_spline= D_high * X2 + D_low;

	float4 C_threshold_mask= step(C_thresholds, X);
	float4 D_threshold_mask= step(D_thresholds, X);
	float4 C_channel_mask= float4(_fake_bitwise_ops_fake_xor(C_threshold_mask, C_threshold_mask.yzww).xyz, C_threshold_mask.w);
	float4 D_channel_mask= float4(_fake_bitwise_ops_fake_xor(D_threshold_mask, D_threshold_mask.yzww).xyz, D_threshold_mask.w);
	float4 C_spline_result_in_4= C_evaluated_spline * C_channel_mask;
	float4 D_spline_result_in_4= D_evaluated_spline * D_channel_mask;
	float C_spline_result= C_spline_result_in_4.x + C_spline_result_in_4.y + C_spline_result_in_4.z + C_spline_result_in_4.w;
	float D_spline_result= D_spline_result_in_4.x + D_spline_result_in_4.y + D_spline_result_in_4.z + D_spline_result_in_4.w;
	float spline_result= D_threshold_mask.x ? D_spline_result : C_spline_result;

	return spline_result.xxxx;
}

float4 bytecode_op_spline8_chain_const(
	float4 X,
	float4 Recursion,
	float4 C3,
	float4 C2,
	float4 C1,
	float4 C0,
	float4 D3,
	float4 D2,
	float4 D1,
	float4 D0,
	float4 C_thresholds,
	float4 D_thresholds)
{
	float4 C_high= C3 * X + C2;
	float4 C_low= C1 * X + C0;
	float4 D_high= D3 * X + D2;
	float4 D_low= D1 * X + D0;
	float4 X2= X * X;
	float4 C_evaluated_spline= C_high * X2 + C_low;
	float4 D_evaluated_spline= D_high * X2 + D_low;

	float4 C_threshold_mask= step(C_thresholds, X);
	float4 D_threshold_mask= step(D_thresholds, X);
	float4 C_channel_mask= float4(_fake_bitwise_ops_fake_xor(C_threshold_mask, C_threshold_mask.yzww).xyz, C_threshold_mask.w);
	float4 D_channel_mask= float4(_fake_bitwise_ops_fake_xor(D_threshold_mask, D_threshold_mask.yzww).xyz, D_threshold_mask.w);

	float4 C_spline_result_in_4= C_evaluated_spline * C_channel_mask;
	float4 D_spline_result_in_4= D_evaluated_spline * D_channel_mask;
	float C_spline_result= C_spline_result_in_4.x + C_spline_result_in_4.y + C_spline_result_in_4.z + C_spline_result_in_4.w;
	float D_spline_result= D_spline_result_in_4.x + D_spline_result_in_4.y + D_spline_result_in_4.z + D_spline_result_in_4.w;

	float spline_result_intermediate= C_threshold_mask.x ? C_spline_result : Recursion.x;
	float spline_result= D_threshold_mask.x ? D_spline_result : spline_result_intermediate;

	return spline_result.xxxx;
}

// evals a cubic polynomial across four channels with estrin form
float4 bytecode_op_cubic(
	float4 X,
	float4 coefficients)
{
	float4 high= coefficients.x * X + coefficients.yyyy;
	float4 low= coefficients.z * X + coefficients.wwww;
	float4 X2= X * X;
	float4 cubic_result= high * X2 + low;

	return cubic_result;
}

float4 bytecode_op_gradient4_const(
	float4 X,
	float4 BaseColor,	
	float4 Cred,	
	float4 Cgreen,	
	float4 Cblue,		
	float4 Calpha,	
	float4 Cthresholds)
{
	// Compute the weighting of each gradient delta based upon the X position of evaluation.
	float4 Coffsets_from_x= X - Cthresholds;
	float4 Csegment_interval= float4(Cthresholds.yzw, 1.0f) - Cthresholds;
	float4 Csafe_division= (Coffsets_from_x >= 0.0f) ? float4(1.0f, 1.0f, 1.0f, 1.0f) : float4(0.0f ,0.0f, 0.0f, 0.0f);
	float4 Cdivision= (Csegment_interval != 0.0f) ? (Coffsets_from_x / Csegment_interval) :  Csafe_division;
	float4 Cpercentages= saturate(Cdivision);

	// Compute the influence that each of the colors will contribute to the final color.
	float4 Xinfluence= Cred * Cpercentages;
	float4 Yinfluence= Cgreen * Cpercentages;
	float4 Zinfluence= Cblue * Cpercentages;
	float4 Winfluence= Calpha * Cpercentages;

	// Add the colors into the base color.
	float4 gradient_result= BaseColor + float4(	dot(1.0f, Xinfluence),
												dot(1.0f, Yinfluence),
												dot(1.0f, Zinfluence),
												dot(1.0f, Winfluence));
	return gradient_result;
}

float4 bytecode_op_gradient8_const_old(
	float4 X,
	float4 BaseColor,	
	float4 Cred,	
	float4 Cgreen,	
	float4 Cblue,		
	float4 Calpha,	
	float4 Dred,
	float4 Dgreen,
	float4 Dblue,
	float4 Dalpha,
	float4 Cthresholds,
	float4 Dthresholds)
{
	// Compute the weighting of each gradient delta based upon the X position of evaluation.
	float4 Coffsets_from_x= X - Cthresholds;
	float4 Csegment_interval= float4(Cthresholds.yzw, 1.0f) - Cthresholds;
	float4 Csafe_division= (Coffsets_from_x >= 0.0f) ? float4(1.0f, 1.0f, 1.0f, 1.0f) : float4(0.0f ,0.0f, 0.0f, 0.0f);
	float4 Cdivision= (Csegment_interval != 0.0f) ? (Coffsets_from_x / Csegment_interval) :  Csafe_division;
	float4 Cpercentages= saturate(Cdivision);
	
	float4 Doffsets_from_x= X - Dthresholds;
	float4 Dsegment_interval= float4(Dthresholds.yzw, 1.0f) - Dthresholds;
	float4 Dsafe_division= (Doffsets_from_x >= 0.0f) ? float4(1.0f, 1.0f, 1.0f, 1.0f) : float4(0.0f ,0.0f, 0.0f, 0.0f);
	float4 Ddivision= (Dsegment_interval != 0.0f) ? (Doffsets_from_x / Dsegment_interval) :  Dsafe_division;
	float4 Dpercentages= saturate(Ddivision);
	
	// Compute the influence that each of the colors will contribute to the final color.
	float4 Xinfluence= (Cred * Cpercentages) + (Dred * Dpercentages);
	float4 Yinfluence= (Cgreen * Cpercentages) + (Dgreen * Dpercentages);
	float4 Zinfluence= (Cblue * Cpercentages) + (Dblue * Dpercentages);
	float4 Winfluence= (Calpha * Cpercentages) + (Dalpha * Dpercentages);

	// Add the colors into the base color.
	float4 gradient_result= BaseColor + float4(	dot(1.0f, Xinfluence),
												dot(1.0f, Yinfluence),
												dot(1.0f, Zinfluence),
												dot(1.0f, Winfluence));
	return gradient_result;
}

// test
float4 bytecode_op_gradient8_const(
    float4 input,
    float4 param_0,
    float4 param_1,
    float4 param_2,
    float4 param_3,
    float4 param_4,
    float4 param_5,
    float4 param_6,
    float4 param_7,
    float4 param_8,
    float4 param_9,
    float4 param_10)
{
    // Small epsilon used in original to avoid division by ~0
    const float EPS = 0.0001f;

    // v4 in the original was Vec4::ONE
    float4 ones = float4(1.0, 1.0, 1.0, 1.0);

    // v6 and v8 are param_9 and param_10
    float4 v6 = param_9;
    float4 v8 = param_10;

    // v9 = input - v6
    float4 v9 = input - v6;

    // v10 = input - v8
    float4 v10 = input - v8;

    // Reconstruct v11 from SSE-shuffle logic:
    // original v11 = (swizzle(v8, [y,z,w,x]) with w=1) - v8
    // so componentwise:
    float4 v11;
    v11.x = v8.y - v8.x;
    v11.y = v8.z - v8.y;
    v11.z = v8.w - v8.z;
    v11.w = 1.0 - v8.w;

    // Reconstruct v12 from original SSE code:
    // v12.x = v6.y - v6.x;
    // v12.y = v6.z - v6.y;
    // v12.z = v6.w - v6.z;
    // v12.w = v8.x - v6.w;
    float4 v12;
    v12.x = v6.y - v6.x;
    v12.y = v6.z - v6.y;
    v12.z = v6.w - v6.z;
    v12.w = v8.x - v6.w;

    // v13 = abs(v12) > EPS
    bool4 v13b = bool4(abs(v12.x) > EPS, abs(v12.y) > EPS, abs(v12.z) > EPS, abs(v12.w) > EPS);

    // v14 = abs(v11) > EPS
    bool4 v14b = bool4(abs(v11.x) > EPS, abs(v11.y) > EPS, abs(v11.z) > EPS, abs(v11.w) > EPS);

    // v15: per-component:
    // if abs(v12) < EPS: v15 = (v9 <= 0) ? 1 : 0
    // else: v15 = v9 / v12
    // then clamp to [0,1]
    float4 v15;
    {
        // per-component fallback (v9 <= 0) -> 1 else 0
        float4 fallback15 = float4(
            (v9.x <= 0.0) ? 1.0 : 0.0,
            (v9.y <= 0.0) ? 1.0 : 0.0,
            (v9.z <= 0.0) ? 1.0 : 0.0,
            (v9.w <= 0.0) ? 1.0 : 0.0
        );

        float4 ratio15 = float4(
            v9.x / v12.x,
            v9.y / v12.y,
            v9.z / v12.z,
            v9.w / v12.w
        );

        v15.x = v13b.x ? ratio15.x : fallback15.x;
        v15.y = v13b.y ? ratio15.y : fallback15.y;
        v15.z = v13b.z ? ratio15.z : fallback15.z;
        v15.w = v13b.w ? ratio15.w : fallback15.w;

        // clamp to [0,1]
        v15 = clamp(v15, 0.0, 1.0);
    }

    // v16: same pattern but with v10 and v11
    float4 v16;
    {
        float4 fallback16 = float4(
            (v10.x <= 0.0) ? 1.0 : 0.0,
            (v10.y <= 0.0) ? 1.0 : 0.0,
            (v10.z <= 0.0) ? 1.0 : 0.0,
            (v10.w <= 0.0) ? 1.0 : 0.0
        );

        float4 ratio16 = float4(
            v10.x / v11.x,
            v10.y / v11.y,
            v10.z / v11.z,
            v10.w / v11.w
        );

        v16.x = v14b.x ? ratio16.x : fallback16.x;
        v16.y = v14b.y ? ratio16.y : fallback16.y;
        v16.z = v14b.z ? ratio16.z : fallback16.z;
        v16.w = v14b.w ? ratio16.w : fallback16.w;

        v16 = clamp(v16, 0.0, 1.0);
    }

    // Now compute the four output channels.
    // The SSE code builds four scalars, each equal to:
    // out.x = param_0.x + dot(param_1, v15) + dot(param_5, v16)
    // out.y = param_0.y + dot(param_2, v15) + dot(param_6, v16)
    // out.z = param_0.z + dot(param_3, v15) + dot(param_7, v16)
    // out.w = param_0.w + dot(param_4, v15) + dot(param_8, v16)

    float out_x = param_0.x + dot(param_1, v15) + dot(param_5, v16);
    float out_y = param_0.y + dot(param_2, v15) + dot(param_6, v16);
    float out_z = param_0.z + dot(param_3, v15) + dot(param_7, v16);
    float out_w = param_0.w + dot(param_4, v15) + dot(param_8, v16);

    return float4(out_x, out_y, out_z, out_w);
}