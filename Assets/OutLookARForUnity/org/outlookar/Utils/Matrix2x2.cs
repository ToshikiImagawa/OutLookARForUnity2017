/**
 * 
 *  You can not modify and use this source freely
 *  only for the development of application related AROpenCV.
 * 
 * (c) AROpenCV All rights reserved.
 * by Toshiki Imagawa
**/

using UnityEngine;

public struct Matrix2x2{
	Vector4 pt;
	public float this [int id]{ get { return (id < 4 && id >= 0) ? pt [id] : -1f; } }
	public float Pt11{ get { return pt.x; } }
	public float Pt12{ get { return pt.y; } }
	public float Pt21{ get { return pt.z; } }
	public float Pt22{ get { return pt.w; } }

	public static Matrix2x2 ZERO { get { return new Matrix2x2 (0, 0, 0, 0); } }
	public static Matrix2x2 UNIT { get { return new Matrix2x2 (1f, 0, 1f, 0); } }

	public Matrix2x2 Inverse{
		get{
			float delta = 1 / (Pt11 * Pt22 - Pt12 * Pt21);
			return new Matrix2x2 (
				Pt22 * delta, -Pt12 * delta,
				-Pt21 * delta, Pt11 * delta
			);
		}
	}

	public Matrix2x2(float pt11,float pt12,float pt21,float pt22){
		pt = new Vector4 (pt11, pt12, pt21, pt22);
	}
	public static Vector2 operator* (Matrix2x2 m, Vector2 n)
	{
		return new Vector2(
			m.Pt11*n.x+m.Pt12*n.y,
			m.Pt21*n.x+m.Pt22*n.y
		);
	}
	public static Vector2 operator* (Vector2 m, Matrix2x2 n)
	{
		return new Vector2(
			m.x*n.Pt11+m.y*n.Pt21,
			m.x*n.Pt12+m.y*n.Pt22
		);
	}
	public static Matrix2x2 operator* (Matrix2x2 m, Matrix2x2 n)
	{
		return new Matrix2x2(
			m.Pt11*n.Pt11+m.Pt12*n.Pt21,m.Pt11*n.Pt12+m.Pt12*n.Pt22,
			m.Pt21*n.Pt11+m.Pt22*n.Pt21,m.Pt21*n.Pt12+m.Pt22*n.Pt22
		);
	}

	public static Matrix2x2 operator+ (Matrix2x2 m, Matrix2x2 n)
	{
		return new Matrix2x2 (
			m.Pt11 + n.Pt11, m.Pt12 + n.Pt12,
			m.Pt21 + n.Pt21, m.Pt22 + n.Pt22
		);
	}

	public static Matrix2x2 operator- (Matrix2x2 m, Matrix2x2 n)
	{
		return new Matrix2x2 (
			m.Pt11 - n.Pt11, m.Pt12 - n.Pt12,
			m.Pt21 - n.Pt21, m.Pt22 - n.Pt22
		);
	}

	public override string ToString()
	{
		return "Matrix2x2: \n" +
			"(" + Pt11 + ", " + Pt12 + ",\n" +
			Pt21 + ", " + Pt22 + ")";
	}
}