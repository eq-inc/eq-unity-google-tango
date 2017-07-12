using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Eq.Unity
{
    [Serializable]
    class PoseData
    {
        public double timestamp;
        public double translateX;
        public double translateY;
        public double translateZ;

        public double orientateX;
        public double orientateY;
        public double orientateZ;
        public double orientateW;
    }
}
