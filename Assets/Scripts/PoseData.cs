using System;
using UnityEngine;

namespace Eq.Unity
{
    [Serializable]
    public class PoseData
    {
        public double timestamp;
        public double translateX;
        public double translateY;
        public double translateZ;

        public double orientateX;
        public double orientateY;
        public double orientateZ;
        public double orientateW;

        private GameObject mTargetGameObject;

        public PoseData Clone()
        {
            PoseData copyToPoseData = new PoseData();

            copyToPoseData.timestamp = timestamp;
            copyToPoseData.translateX = translateX;
            copyToPoseData.translateY = translateY;
            copyToPoseData.translateZ = translateZ;
            copyToPoseData.orientateX = orientateX;
            copyToPoseData.orientateY = orientateY;
            copyToPoseData.orientateZ = orientateZ;
            copyToPoseData.orientateW = orientateW;

            return copyToPoseData;
        }

        public GameObject getTargetGameObject()
        {
            return mTargetGameObject;
        }

        public void SetTargetGameObject(GameObject gameObject)
        {
            mTargetGameObject = gameObject;
        }

        public void SetTranslation(Vector3 translate)
        {
            translateX = translate.x;
            translateY = translate.y;
            translateZ = translate.z;
        }

        public void SetOrientation(Vector4 orientation)
        {
            orientateX = orientation.x;
            orientateY = orientation.y;
            orientateZ = orientation.z;
            orientateW = orientation.w;
        }

        public void SetOrientation(Tango.DVector4 orientation)
        {
            orientateX = orientation.x;
            orientateY = orientation.y;
            orientateZ = orientation.z;
            orientateW = orientation.w;
        }

        public Vector3 GetTranslation()
        {
            return new Vector3((float)translateX, (float)translateY, (float)translateZ);
        }

        public Quaternion GetOrientation()
        {
            return new Quaternion((float)orientateX, (float)orientateY, (float)orientateZ, (float)orientateW);
        }
    }
}
