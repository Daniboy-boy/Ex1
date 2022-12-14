using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterAnimator : MonoBehaviour
{
    public TextAsset BVHFile; // The BVH file that defines the animation and skeleton
    public bool animate; // Indicates whether or not the animation should be running
    public bool interpolate; // Indicates whether or not frames should be interpolated
    [Range(0.01f, 2f)] public float animationSpeed = 1; // Controls the speed of the animation playback

    public BVHData data; // BVH data of the BVHFile will be loaded here
    public float t = 0; // Value used to interpolate the animation between frames
    public float[] currFrameData; // BVH channel data corresponding to the current keyframe
    public float[] nextFrameData; // BVH vhannel data corresponding to the next keyframe

    // Start is called before the first frame update
    void Start()
    {
        BVHParser parser = new BVHParser();
        data = parser.Parse(BVHFile);
        CreateJoint(data.rootJoint, Vector3.zero);
    }

    // Returns a Matrix4x4 representing a rotation aligning the up direction of an object with the given v
    public Matrix4x4 RotateTowardsVector(Vector3 v)
    {
        v.Normalize();
        var rX = MatrixUtils.RotateX(-(90 - Mathf.Atan2(v.y, v.z) * Mathf.Rad2Deg));
        var rZ = MatrixUtils.RotateZ(90 - Mathf.Atan2(Mathf.Sqrt(v.y * v.y + v.z * v.z), v.x) * Mathf.Rad2Deg);
        return rX.inverse * rZ.inverse;
    }

    // Creates a Cylinder GameObject between two given points in 3D space
    public GameObject CreateCylinderBetweenPoints(Vector3 p1, Vector3 p2, float diameter)
    {
        GameObject cylinder = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        var directionVector = p2 - p1;
        var tMat = MatrixUtils.Translate((p1 + p2) / 2);
        var rMat = RotateTowardsVector(directionVector);
        var sMat = MatrixUtils.Scale(new Vector3(diameter,directionVector.magnitude/2 , diameter));
        var mMat = tMat * rMat * sMat;
        MatrixUtils.ApplyTransform(cylinder, mMat);
        return cylinder;
    }

    // Creates a GameObject representing a given BVHJoint and recursively creates GameObjects for it's child joints
    public GameObject CreateJoint(BVHJoint joint, Vector3 parentPosition)
    {
        joint.gameObject = new GameObject(joint.name);
        GameObject newJoint = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        newJoint.transform.parent = joint.gameObject.transform;

        Matrix4x4 scaleMat = MatrixUtils.Scale(new Vector3(2, 2, 2));
        if (joint.name.Equals("Head"))
            scaleMat = MatrixUtils.Scale(new Vector3(8, 8, 8));

        MatrixUtils.ApplyTransform(newJoint, scaleMat);

        var currPosition = parentPosition + joint.offset;
        Matrix4x4 translationMat = MatrixUtils.Translate(currPosition);
        MatrixUtils.ApplyTransform(joint.gameObject, translationMat);
        foreach (var child in joint.children)
            
        {
            CreateJoint(child, currPosition);
            var newCylinder =  CreateCylinderBetweenPoints(currPosition,
                currPosition+child.offset, 0.6f);
            newCylinder.transform.parent = joint.gameObject.transform;
        }

        return joint.gameObject;
    }

    // Transforms BVHJoint according to the keyframe channel data, and recursively transforms its children
    public void TransformJoint(BVHJoint joint, Matrix4x4 parentTransform)
    {
        Matrix4x4 rotationMat;
        if (interpolate)
        {
            var curRotation = QuaternionUtils.FromEuler(new Vector3(currFrameData[joint.rotationChannels.x], currFrameData[joint.rotationChannels.y], currFrameData[joint.rotationChannels.z]), joint.rotationOrder);
            var nextRotation = QuaternionUtils.FromEuler(new Vector3(nextFrameData[joint.rotationChannels.x], nextFrameData[joint.rotationChannels.y], nextFrameData[joint.rotationChannels.z]), joint.rotationOrder);
            rotationMat = MatrixUtils.RotateFromQuaternion(QuaternionUtils.Slerp(curRotation, nextRotation, t));
        }
        else
        {
            // M = TRS
            // there is no scaling. S = I4
            // find the natrices that construct R
            var zRotation = MatrixUtils.RotateZ(currFrameData[joint.rotationChannels.z]);
            var yRotation = MatrixUtils.RotateY(currFrameData[joint.rotationChannels.y]);
            var xRotation = MatrixUtils.RotateX(currFrameData[joint.rotationChannels.x]);
            //find the ordering of mult for R
            rotationMat = joint.rotationOrder.x == 0 ? xRotation : (joint.rotationOrder.y == 0 ? yRotation : zRotation);
            rotationMat = joint.rotationOrder.x == 1 ? rotationMat*xRotation : (joint.rotationOrder.y == 1 ? rotationMat*yRotation : rotationMat*zRotation);
            rotationMat = joint.rotationOrder.x == 2 ? rotationMat*xRotation : (joint.rotationOrder.y == 2 ? rotationMat*yRotation : rotationMat*zRotation);
        }

        var globalM = parentTransform * rotationMat;

        MatrixUtils.ApplyTransform(joint.gameObject, globalM);
        
        foreach (var child in joint.children)
        {
            var mPos = MatrixUtils.Translate(child.offset);
            TransformJoint(child, globalM * mPos);
        }
    }

    // Returns the frame nunmber of the BVH animation at a given time
    public int GetFrameNumber(float time)
    {
        return (int)(time / data.frameLength) % data.numFrames;
    }

    // Returns the proportion of time elapsed between the last frame and the next one, between 0 and 1
    public float GetFrameIntervalTime(float time)
    {
        return time % data.frameLength / data.frameLength;
    }

    // Update is called once per frame
    void Update()
    {
        float time = Time.time * animationSpeed;

        if (animate)
        {
            int currFrame = GetFrameNumber(time);
            t = GetFrameIntervalTime(time);
            currFrameData = data.keyframes[currFrame];
            var positionVec = new Vector3(currFrameData[data.rootJoint.positionChannels.x],
            currFrameData[data.rootJoint.positionChannels.y], currFrameData[data.rootJoint.positionChannels.z]);

            int nextFrameIndex = currFrame + 1 == data.keyframes.Count ? currFrame : currFrame + 1;
            nextFrameData = data.keyframes[nextFrameIndex];

            if (interpolate)
            {
                var nextPosition = new Vector3(nextFrameData[data.rootJoint.positionChannels.x],
                    nextFrameData[data.rootJoint.positionChannels.y], nextFrameData[data.rootJoint.positionChannels.z]);
                positionVec = Vector3.Lerp(positionVec, nextPosition, t);
            }

            Matrix4x4 rootVec = MatrixUtils.Translate(positionVec);
            TransformJoint(data.rootJoint, rootVec);
        }
    }
}
