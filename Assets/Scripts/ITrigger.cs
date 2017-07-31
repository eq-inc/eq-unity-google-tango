using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Eq.Unity
{
    public interface ITrigger
    {
        void OnTriggerEnter(UnityEngine.Collider collider);

        void OnTriggerExit(UnityEngine.Collider collider);

        void OnTriggerStay(UnityEngine.Collider collider);
    }
}
