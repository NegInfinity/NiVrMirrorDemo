using UnityEngine;

public struct Coord{
	public Vector3 x;
	public Vector3 y;
	public Vector3 z;
	public Vector3 pos;

	public Vector3 localToWorldDir(Vector3 arg){
		return arg.x * x + arg.y * y + arg.z * z;
	}
	public Vector3 localToWorldPos(Vector3 arg){
		return localToWorldDir(arg) + pos;
	}
	public Vector3 worldToLocalDir(Vector3 arg){
		return new Vector3(
			Vector3.Dot(x, arg),
			Vector3.Dot(y, arg),
			Vector3.Dot(z, arg)
		);
	}
	public Vector3 worldToLocalPos(Vector3 arg){
		return worldToLocalDir(arg - pos);
	}
	
	public Coord(Transform src){
		x = src.right;
		y = src.up;
		z = src.forward;
		pos = src.position;
	}
}

