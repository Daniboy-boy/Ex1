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
        // Your code here
    }

    // Returns the frame nunmber of the BVH animation at a given time
    public int GetFrameNumber(float time)
    {
        // Your code here
        return 0;
    }

    // Returns the proportion of time elapsed between the last frame and the next one, between 0 and 1
    public float GetFrameIntervalTime(float time)
    {
        // Your code here
        return 0;
    }

    // Update is called once per frame
    void Update()
    {
        float time = Time.time * animationSpeed;
        if (animate)
        {
            int currFrame = GetFrameNumber(time);
            // Your code here
        }
    }
}
