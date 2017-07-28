using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UtilityMath
{
	public static float epsilon = 0.009f;

	public enum EaseType{
		IN_SINE,
		LINEAR,
		IN_CUBIC,
		OUT_QUART,
	}

	// 移動量からアングルを返す
	public static float LenToAngle(float length, float radius){
		return (length / (2*Mathf.PI*radius)) * 360.0f;
	}

	//イージング関数
	public static double InQuad(double t,double totaltime,double max ,double min )
	{
		max -= min;
		t /= totaltime;
		return max*t*t + min;
	}
	public static double OutQuad(double t,double totaltime,double max ,double min )
	{
		max -= min;
		t /= totaltime;
		return -max*t*(t-2)+min;
	}
	public static double InOutQuad(double t,double totaltime,double max ,double min )
	{
		max -= min;
		t /= totaltime;
		if( t / 2 < 1 )
			return max/2 * t * t + min;
		--t;
		return -max * (t * (t-2)-1) + min;
	}
	public static double InCubic(double t,double totaltime,double max ,double min )
	{
		max -= min;
		t /= totaltime;
		return max * t*t*t + min;
	}
	public static double OutCubic(double t,double totaltime,double max ,double min )
	{
		max -= min;
		t = t/totaltime-1;
		return max * (t*t*t+1) + min;
	}
	public static double InOutCubic(double t,double totaltime,double max ,double min )
	{
		max -= min;
		t /= totaltime;
		if( t/2 < 1 )
			return max/2*t*t*t + min;
		t -= 2;
		return max/2 * (t*t*t+2) + min;
	}
	public static double InQuart(double t,double totaltime,double max ,double min )
	{
		max -= min;
		t /= totaltime;
		return max * t*t*t*t + min;
	}
	public static double OutQuart(double t,double totaltime,double max ,double min )
	{
		max -= min;
		t = t/totaltime-1;
		return -max*( t*t*t*t-1) +min;
	}
	public static double InOutQuart(double t,double totaltime,double max ,double min )
	{
		max -= min;
		t /= totaltime;
		if( t/2 < 1 )
			return max/2 * t*t*t*t +min;
		t -= 2;
		return -max/2 * (t*t*t*t-2) + min;
	}
	public static double InQuint(double t,double totaltime,double max ,double min )
	{
		max -= min;
		t /= totaltime;
		return max*t*t*t*t*t + min;
	}
	public static double OutQuint(double t,double totaltime,double max ,double min )
	{
		max -= min;
		t = t/totaltime-1;
		return max*(t*t*t*t*t+1)+min;
	}
	public static double InOutQuint(double t,double totaltime,double max ,double min )
	{
		max -= min;
		t /= totaltime;
		if( t/2 < 1 )
			return max/2*t*t*t*t*t + min;
		t -= 2;
		return max/2 * (t*t*t*t*t+2) + min;
	}
	public static double InSine(double t,double totaltime,double max ,double min )
	{
		max -= min;
		return -max*Mathf.Cos( (float)(t*(Mathf.Deg2Rad * 90.0f) / totaltime) ) + max + min;
	}
	// double OutSine(double t,double totaltime,double max ,double min )
	// {
	// 	max -= min;
	// 	return max * sin( t*Rad90/totaltime ) + min;
	// }
	public static double InOutSine(double t,double totaltime,double max ,double min )
	{
		max -= min;
		return -max/2* (Mathf.Cos((float)(t * Mathf.PI / totaltime))-1) + min;
	}
	public static double InExp(double t,double totaltime,double max ,double min )
	{
		max -= min;
		return t == 0.0 ? min : max*Mathf.Pow(2,(float)(10*(t/totaltime-1))) + min;
	}
	public static double OutExp(double t,double totaltime,double max ,double min )
	{
		max -= min;
		return t == totaltime ? max + min : max*(-Mathf.Pow(2,(float)(-10*t/totaltime))+1)+min;
	}
	public static double InOutExp(double t,double totaltime,double max ,double min )
	{
		if( t == 0.0 )
			return min;
		if( t == totaltime )
			return max;
		max -= min;
		t /= totaltime;

		if( t/2 < 1 )
			return max/2*Mathf.Pow(2,(float)(10*(t-1))) + min;
		--t;
		return max/2*(-Mathf.Pow(2,(float)(-10*t))+2) + min;

	}
	public static double InCirc(double t,double totaltime,double max ,double min )
	{
		max -= min;
		t /= totaltime;
		return -max*(Mathf.Sqrt((float)(1-t*t))-1)+min;
	}
	public static double OutCirc(double t,double totaltime,double max ,double min )
	{
		max -= min;
		t = t/totaltime-1;
		return max*Mathf.Sqrt( (float)(1-t*t))+min;
	}
	public static double InOutCirc(double t,double totaltime,double max ,double min )
	{
		max -= min;
		t /= totaltime;
		if( t/2 < 1 )
			return -max/2 * (Mathf.Sqrt((float)(1-t*t))-1) + min;
		t -= 2;
		return max/2 * (Mathf.Sqrt((float)(1-t*t))+1) + min;
	}
	public static double InBack(double t,double totaltime,double max ,double min ,double s )
	{
		max -= min;
		t /= totaltime;
		return max*t*t*( (s + 1)*t - s) + min;
	}
	public static double OutBack(double t,double totaltime,double max ,double min ,double s )
	{
		max -= min;
		t = t/totaltime-1;
		return max*(t*t*((s+1)*t*s)+1)+min;
	}
	public static double InOutBack(double t,double totaltime,double max ,double min ,double s )
	{
		max -= min;
		s *= 1.525;
		if( t/2 < 1 )
		{
			return max*(t*t*((s+1)*t-s))+min;
		}
		t -= 2;
		return max/2 * (t*t*((s+1)*t+s)+2) + min;
	}
	public static double OutBounce(double t,double totaltime,double max ,double min )
	{
		max -= min;
		t /= totaltime;

		if( t < 1/2.75 )
			return max*(7.5625*t*t)+min;
		else if(t < 2/2.75 )
		{
			t-= 1.5/2.75;
			return max*(7.5625*t*t+0.75)+min;
		}  
		else if( t< 2.5/2.75 )
		{
			t -= 2.25/2.75;
			return max*(7.5625*t*t+0.9375)+min;
		}
		else
		{
			t-= 2.625/2.75;
			return max*(7.5625*t*t+0.984375) + min;
		}
	}
	public static double InBounce(double t,double totaltime,double max ,double min )
	{
		return max - OutBounce( totaltime - t , totaltime , max - min , 0 ) + min;  
	}
	public static double InOutBounce(double t,double totaltime,double max ,double min )
	{
		if( t < totaltime/2 )
			return InBounce( t*2 , totaltime , max-min , max )*0.5 + min;
		else
			return OutBounce(t*2-totaltime,totaltime,max-min,0)*0.5+min + (max-min)*0.5;
	}
	public static double Linear(double t,double totaltime,double max ,double min )
	{
		return (max-min)*t/totaltime + min;
	}

	// S1(s) = p1 + s*(q1 - p1) および S2(t) = p2 + t*(q2 - p2)の
	// 最近接点C1およびC2を計算、sおよびtを返す
	// 関数の結果はS1(s)とS2(t)の間の距離の平方
	public static float ClosesPtSegmentSegment(Vector3 p1, Vector3 q1, Vector3 p2, Vector3 q2,
								out float s, out float t, out Vector3 c1, out Vector3 c2)
	{
		Vector3 d1 = q1 - p1;
		Vector3 d2 = q2 - p2;
		Vector3 r = p1 - p2;

		float a = Vector3.Dot(d1, d1);
		float e = Vector3.Dot(d2, d2);
		float f = Vector3.Dot(d2, r);

		if (a <= Vector3.kEpsilon && e <= Vector3.kEpsilon){
			s = t = 0.0f;
			c1 = p1;
			c2 = p2;
			return Vector3.Dot(c1 - c2, c1 - c2);
		}

		if (a < Vector3.kEpsilon){
			s = 0.0f;
			t = f / e;
			t = Mathf.Clamp(t, 0.0f, 1.0f);
		}
		else{
			float c = Vector3.Dot(d1, r);
			if (e <=Vector3.kEpsilon){
				t = 0.0f;
				s = -c / a;
				s = Mathf.Clamp(s, 0.0f, 1.0f);
			}
			else{
				float b = Vector3.Dot(d1, d2);
				float denom = a*e - b*b;

				if (denom != 0.0f){
					s = Mathf.Clamp((b*f-c*e)/denom, 0.0f, 1.0f);
				}
				else{
					s = 0.0f;
				}

				t = (b*s + f) / e;

				if (t < 0.0f){
					t = 0.0f;
					s = Mathf.Clamp(-c / a, 0.0f, 1.0f);
				}
				else if (t > 1.0f){
					t = 1.0f;
					s = Mathf.Clamp((b-c)/a, 0.0f, 1.0f);
				}
			}
		}

		c1 = p1 + d1 * s;
		c2 = p2 + d2 * t;
		return Vector3.Dot(c1 - c2, c1 - c2);
	}

	// float補間
	public static IEnumerator<float> FLerp(float start, float end, float time = 1.0f, EaseType type = EaseType.LINEAR, float s = 1.0f){
		float st = start;
		float en = end;
		float t = 0.0f;
		while(t < 1.0f){
			t = Time.deltaTime / time + t;
			float et = (float)GetEasing(type, t, time, 1.0f, 0.0f, s);
			float result = Mathf.LerpAngle(st, en, et);
			yield return (t >= 1.0f) ? end : result;
		}
	}

	// Quaternion補間
	public static IEnumerator<Quaternion> QLerp(Quaternion start, Quaternion end, float time = 1.0f, EaseType type = EaseType.LINEAR, float s = 1.0f){
		Quaternion st = start;
		Quaternion en = end;
		float t = 0.0f;
		while(t < 1.0f){
			t = Time.deltaTime / time + t;
			float et = (float)GetEasing(type, t, time, 1.0f, 0.0f, s);
			Quaternion result = Quaternion.Lerp(st, en, et);
			yield return (t >= 1.0f) ? end : result;
		}
	}

	// Vector3補間
	public static IEnumerator<Vector3> VLerp(Vector3 start, Vector3 end, float time = 1.0f, EaseType type = EaseType.LINEAR, float s = 1.0f){
		Vector3 st = start;
		Vector3 en = end;
		float t = 0.0f;
		while(t < 1.0f){
			t = Time.deltaTime / time + t;
			float et = (float)GetEasing(type, t, time, 1.0f, 0.0f, s);
			Vector3 result = Vector3.Lerp(st, en, et);
			yield return (t >= 1.0f) ? end : result;
		}
	}

	// 2つのベクトルの内積の角度を0~360の範囲で返す
	public static float GetAngleUnlimit(Vector3 baseVel, Vector3 baseUp, Vector3 vel){
		float angle = Mathf.Acos(Vector3.Dot(baseVel, vel)) * Mathf.Rad2Deg;
		
		Vector3 up = Vector3.Cross(baseVel, vel).normalized;
		float dot = Vector3.Dot(baseUp, up);
		if (dot < 0){
			angle = 360 - angle;
		}

		return angle;
	}

	public static double GetEasing(EaseType type, double t,double totaltime,double max ,double min ,double s = 1.0f){
		double result = 0.0f;

		switch(type){
			case EaseType.IN_SINE: result = InSine(t, totaltime, max, min); break;
			case EaseType.LINEAR: result = Linear(t, totaltime, max, min); break;
			case EaseType.IN_CUBIC: result = InCubic(t, totaltime, max, min); break;
			case EaseType.OUT_QUART: result = OutQuart(t, totaltime, max, min); break;
		}

		return result;
	}
}