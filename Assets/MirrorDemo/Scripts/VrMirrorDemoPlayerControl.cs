using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class VrMirrorDemoPlayerControl : MonoBehaviour{
	VRMirrorDemoInputActions vrInput;

	[Header("Controlled VR objects")]
	[SerializeField] UnityEngine.InputSystem.XR.TrackedPoseDriver leftWandObj;
	[SerializeField] UnityEngine.InputSystem.XR.TrackedPoseDriver rightWandObj;
	[SerializeField] UnityEngine.InputSystem.XR.TrackedPoseDriver hmdObj;

	[Header("Movement speed")]
	[SerializeField] float moveSpeed = 2.5f;
	[SerializeField] float turnSpeed = 180.0f;
	Vector2 controlMoveVec = Vector2.zero;
	Vector2 controlLookVec = Vector2.zero;

	void updateMoveVec(InputAction.CallbackContext ctx){
		if (ctx.canceled){
			controlMoveVec = Vector2.zero;
			return;
		}
		controlMoveVec = ctx.ReadValue<Vector2>();
	}

	void updateLookVec(InputAction.CallbackContext ctx){
		if (ctx.canceled){
			controlLookVec = Vector2.zero;
			return;
		}
		controlLookVec = ctx.ReadValue<Vector2>();
	}


	void OnEnable(){
		if (vrInput == null){
			vrInput = new VRMirrorDemoInputActions();
		}
		vrInput.Enable();

		if (leftWandObj){
			leftWandObj.positionAction = vrInput.VRControls.LPointerPos;
			leftWandObj.rotationAction = vrInput.VRControls.LPointerRot;
		}

		if (rightWandObj){
			rightWandObj.positionAction = vrInput.VRControls.RPointerPos;
			rightWandObj.rotationAction = vrInput.VRControls.RPointerRot;
		}

		if (hmdObj){
			hmdObj.positionAction = vrInput.VRControls.HMDCenterEyePos;
			hmdObj.rotationAction = vrInput.VRControls.HMDCenterEyeRot;
		}

		vrInput.VRControls.LAxis2d.started += updateMoveVec;
		vrInput.VRControls.LAxis2d.performed += updateMoveVec;
		vrInput.VRControls.LAxis2d.canceled += updateMoveVec;

		vrInput.VRControls.RAxis2d.started += updateLookVec;
		vrInput.VRControls.RAxis2d.performed += updateLookVec;
		vrInput.VRControls.RAxis2d.canceled += updateLookVec;
	}

	public void OnDisable(){	
		if (vrInput != null)
			vrInput.Disable();
	}

	// Update is called once per frame
	void Update(){
		if (controlMoveVec != Vector2.zero){
			var forward = leftWandObj.transform.forward;
			var right = leftWandObj.transform.right;

			var move2d = controlMoveVec * moveSpeed * Time.deltaTime;

			var diff = forward * move2d.y + right * move2d.x;
			transform.position += diff;
		}

		if (controlLookVec != Vector2.zero){
			var turnAngle = turnSpeed * Time.deltaTime * controlLookVec.x;
			transform.RotateAround(hmdObj.transform.position, Vector3.up, turnAngle);
		}
	}
}
