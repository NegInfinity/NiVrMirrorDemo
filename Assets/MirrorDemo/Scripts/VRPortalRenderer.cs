using UnityEngine;
using UnityEngine.InputSystem;

public class VRPortalRenderer: MonoBehaviour/*, IPlayerInputHandler*/{
	[Header("Camera")]
	[SerializeField] Camera sourceCamOverride = null;
	[SerializeField] LayerMask cameraViewMask = 1;
	[SerializeField] int renderTargetSize = 1024;
	[SerializeField] float cameraFov = 90.0f;

	[Header("Portals")]
	[SerializeField] Transform portalEye;
	[SerializeField] bool mirrorMode = true;

	[Header("Shader parameters")]
	[SerializeField] string eyeTexLParam = "EyeTexL";
	[SerializeField] string eyeTexRParam = "EyeTexR";
	[SerializeField] string eyeViewMatLParam = "EyeViewMatrixL";
	[SerializeField] string eyeViewMatRParam = "EyeViewMatrixR";
	[SerializeField] string eyeProjMatLParam = "EyeProjMatrixL";
	[SerializeField] string eyeProjMatRParam = "EyeProjMatrixR";
	[SerializeField] Material targetMaterial;

	[Header("Inputs")]
	[SerializeField] InputActionReference eyePosInputL;
	[SerializeField] InputActionReference eyePosInputR;
	[SerializeField] InputActionReference eyeRotInputL;
	[SerializeField] InputActionReference eyeRotInputR;

	[Header("Internals (do not touch)")]
	[SerializeField] Pose deviceEyePoseL;
	[SerializeField] Pose deviceEyePoseR;
	[SerializeField] Pose worldEyePoseL;
	[SerializeField] Pose worldEyePoseR;

	[SerializeField] GameObject renderCamObj;
	[SerializeField] Camera renderCam;
	[SerializeField] GameObject eyeDebugObjL;
	[SerializeField] GameObject eyeDebugObjR;

	[SerializeField]RenderTexture renderTexL = null;
	[SerializeField]RenderTexture renderTexR = null;

	[SerializeField] Matrix4x4 eyeProjL = Matrix4x4.identity;
	[SerializeField] Matrix4x4 eyeProjR = Matrix4x4.identity;
	[SerializeField] Matrix4x4 eyeViewL = Matrix4x4.identity;
	[SerializeField] Matrix4x4 eyeViewR = Matrix4x4.identity;

	Camera _srcCamera{
		get => sourceCamOverride ? sourceCamOverride: Camera.main;
	}

	/*
	public void onInputActivated(VRInputActions.VRControlsActions controlActions){
		eyePosInputL = controlActions.HMDLEyePos;
		eyePosInputR = controlActions.HMDREyePos;
		eyeRotInputL = controlActions.HMDLEyeRot;
		eyeRotInputR = controlActions.HMDREyeRot;
	}
	public void onInputDectivated(VRInputActions.VRControlsActions controlActions){
	}
	*/

	void OnEnable(){
		renderTexL = new RenderTexture(renderTargetSize, renderTargetSize, 16);
		renderTexR = new RenderTexture(renderTargetSize, renderTargetSize, 16);
		renderTexL.Create();
		renderTexR.Create();

		renderCamObj = new GameObject("Render Camera");
		renderCamObj.hideFlags = HideFlags.DontSave;
		renderCamObj.transform.SetParent(transform);

		renderCam = renderCamObj.AddComponent<Camera>();
		renderCam.hideFlags = HideFlags.DontSave;

		renderCam.enabled = false;

		eyeDebugObjL = new GameObject("DebugEyeL");
		eyeDebugObjL.hideFlags = HideFlags.DontSave;
		eyeDebugObjL.transform.SetParent(transform);

		eyeDebugObjR = new GameObject("DebugEyeR");
		eyeDebugObjR.hideFlags = HideFlags.DontSave;
		eyeDebugObjR.transform.SetParent(transform);
	}

	void OnDisable(){
		if (renderTexL){
			renderTexL.Release();
			renderTexL = null;
		}
		if (renderTexR){
			renderTexR.Release();
			renderTexR = null;
		}
	}

	void enableActionRef(InputActionReference actRef){
		if (actRef.action != null){
			if (!actRef.action.enabled){
				Debug.Log($"Enabled action {actRef}");
				actRef.action.Enable();
			}
		}
		else{
			Debug.Log($"Action is null");
		}
	}
	void updateEyePos(){
		if (eyePosInputL.action != null){
			enableActionRef(eyePosInputL);
			deviceEyePoseL.position = eyePosInputL.action.ReadValue<Vector3>();
		}
		if (eyePosInputR.action != null){
			enableActionRef(eyePosInputR);
			deviceEyePoseR.position = eyePosInputR.action.ReadValue<Vector3>();
		}
		if (eyeRotInputL.action != null){
			enableActionRef(eyeRotInputL);
			deviceEyePoseL.rotation = eyeRotInputL.action.ReadValue<Quaternion>();
		}
		if (eyeRotInputR.action != null){
			enableActionRef(eyeRotInputR);
			deviceEyePoseR.rotation = eyeRotInputR.action.ReadValue<Quaternion>();
		}
		//Debug.Log($"{deviceEyePoseL} {deviceEyePoseR}");
		var cam = _srcCamera;
		var camParent = cam.transform.parent;
		if (!camParent){
			worldEyePoseL = deviceEyePoseL;
			worldEyePoseR = deviceEyePoseR;
		}
		else{
			worldEyePoseL.position = camParent.TransformPoint(deviceEyePoseL.position);
			worldEyePoseL.rotation = camParent.rotation * deviceEyePoseL.rotation;//deviceEyePoseL.rotation * camParent.rotation;
			worldEyePoseR.position = camParent.TransformPoint(deviceEyePoseR.position);
			worldEyePoseR.rotation = camParent.rotation * deviceEyePoseR.rotation;//deviceEyePoseR.rotation * camParent.rotation;
		}
		eyeDebugObjL.transform.position = worldEyePoseL.position;
		eyeDebugObjL.transform.rotation = worldEyePoseL.rotation;
		eyeDebugObjR.transform.position = worldEyePoseR.position;
		eyeDebugObjR.transform.rotation = worldEyePoseR.rotation;
	}

	void renderToTexture(RenderTexture rt, Pose eyePose, out Matrix4x4 viewMat, out Matrix4x4 projMat){
		viewMat = Matrix4x4.identity;
		projMat = Matrix4x4.identity;

		var srcCam = _srcCamera;

		renderCam.enabled = true;
		renderCam.transform.position = eyePose.position;
		renderCam.transform.rotation = eyePose.rotation;

		renderCam.nearClipPlane = srcCam.nearClipPlane;
		renderCam.farClipPlane = srcCam.farClipPlane;
		renderCam.fieldOfView = cameraFov;
		renderCam.cullingMask = cameraViewMask;

		viewMat = renderCam.worldToCameraMatrix;
		Vector3 mirrorPos = Vector3.zero, mirrorNormal = Vector3.up;
		bool useOblique = false;
		if (portalEye && !mirrorMode){
			Coord srcCoord = new(transform);
			Coord dstCoord = new(portalEye);//srcCoord;

			var localEyePos = srcCoord.worldToLocalPos(eyePose.position);
			var srcEyeUp = eyePose.rotation * Vector3.up;
			var srcEyeForward = eyePose.rotation * Vector3.forward;
			var localEyeUp = srcCoord.worldToLocalDir(srcEyeUp);
			var localEyeForward = srcCoord.worldToLocalDir(srcEyeForward);

			var newEyePos = dstCoord.localToWorldPos(localEyePos);
			var newEyeUp = dstCoord.localToWorldDir(localEyeUp);
			var newEyeForward = dstCoord.localToWorldDir(localEyeForward);
			var newEyeRot = Quaternion.LookRotation(newEyeForward, newEyeUp);

			eyePose.position = newEyePos;
			eyePose.rotation = newEyeRot;
			renderCam.transform.position = eyePose.position;
			renderCam.transform.rotation = eyePose.rotation;
		}
		else if (mirrorMode){
			Coord srcCoord = new(transform);
			Coord dstCoord = srcCoord;
			
			var localEyePos = srcCoord.worldToLocalPos(eyePose.position);
			var localEyeUp = srcCoord.worldToLocalDir(eyePose.rotation * Vector3.up);
			var localEyeForward = srcCoord.worldToLocalDir(eyePose.rotation * Vector3.forward);

			var planeNormal = Vector3.up;
			localEyePos = Vector3.Reflect(localEyePos, planeNormal);
			localEyeUp = Vector3.Reflect(localEyeUp, planeNormal);
			localEyeForward = Vector3.Reflect(localEyeForward, planeNormal);

			var newEyePos = dstCoord.localToWorldPos(localEyePos);
			var newEyeUp = dstCoord.localToWorldDir(localEyeUp);
			var newEyeForward = dstCoord.localToWorldDir(localEyeForward);
			var newEyeRot = Quaternion.LookRotation(newEyeForward, newEyeUp);

			eyePose.position = newEyePos;
			eyePose.rotation = newEyeRot;
			renderCam.transform.position = eyePose.position;
			renderCam.transform.rotation = eyePose.rotation;

			mirrorPos = srcCoord.pos;
			mirrorNormal = srcCoord.y;
			useOblique = true;
		}

		projMat = renderCam.projectionMatrix;
		if (useOblique){
			var camMirrorPos = renderCam.worldToCameraMatrix.MultiplyPoint(mirrorPos);
			var camMirrorNormal = renderCam.worldToCameraMatrix.MultiplyVector(mirrorNormal);
			var camClipPlane = new Vector4(
				camMirrorNormal.x, camMirrorNormal.y, camMirrorNormal.z, 
				-Vector3.Dot(camMirrorNormal, camMirrorPos)
			);
			var mirrorProj = renderCam.CalculateObliqueMatrix(camClipPlane);
			renderCam.projectionMatrix = mirrorProj;
		}
		if (mirrorMode){
			projMat *= Matrix4x4.Scale(new Vector3(-1.0f, 1.0f, 1.0f));
		}

		renderCam.targetTexture = rt;

		renderCam.Render();

		renderCam.enabled = false;
	}

	void drawGizmos(Color c){

	}

	void OnDrawGizmosSelected(){
		drawGizmos(Color.white);
	}
	void OnDrawGizmos(){
		drawGizmos(Color.yellow);
	}

	void setShaderParams(){
		if (targetMaterial){
			targetMaterial.SetMatrix(eyeProjMatLParam, eyeProjL);
			targetMaterial.SetMatrix(eyeProjMatRParam, eyeProjR);
			targetMaterial.SetMatrix(eyeViewMatLParam, eyeViewL);
			targetMaterial.SetMatrix(eyeViewMatRParam, eyeViewR);
			targetMaterial.SetTexture(eyeTexLParam, renderTexL);
			targetMaterial.SetTexture(eyeTexRParam, renderTexR);
		}
		Shader.SetGlobalMatrix(eyeProjMatLParam, eyeProjL);
		Shader.SetGlobalMatrix(eyeProjMatRParam, eyeProjR);
		Shader.SetGlobalMatrix(eyeViewMatLParam, eyeViewL);
		Shader.SetGlobalMatrix(eyeViewMatRParam, eyeViewR);
		Shader.SetGlobalTexture(eyeTexLParam, renderTexL);
		Shader.SetGlobalTexture(eyeTexRParam, renderTexR);
	}

	void LateUpdate(){
		updateEyePos();
		renderToTexture(renderTexL, worldEyePoseL, out eyeViewL, out eyeProjL);
		renderToTexture(renderTexR, worldEyePoseR, out eyeViewR, out eyeProjR);
		setShaderParams();
	}
}