using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

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
    }
}
