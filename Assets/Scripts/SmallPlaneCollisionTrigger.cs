using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Eq.Unity
{
    public class SmallPlaneCollisionTrigger : BaseAndroidBehaviour, ITrigger
    {
        public void OnTriggerEnter(Collider collider)
        {
            mLogger.CategoryLog(LogCategoryMethodIn);

            WallPerceptionMainController controller = FindObjectOfType<WallPerceptionMainController>();
            SphereCollider ownCollider = GetComponent<SphereCollider>();
            if (ownCollider != null)
            {
                mLogger.CategoryLog(LogCategoryMethodTrace, "destroy game object: own collider name = " + ownCollider.name + ", other collider name = " + collider.name);
                mLogger.CategoryLog(LogCategoryMethodTrace, "own: center = " + ownCollider.center + ", radius = " + ownCollider.radius);
                mLogger.CategoryLog(LogCategoryMethodTrace, "others: collider = " + collider);

                if(collider is SphereCollider)
                {
                    SphereCollider otherSphereCollider = collider as SphereCollider;
                    float centerDistance = Vector3.Distance(ownCollider.center, otherSphereCollider.center);
                    mLogger.CategoryLog(LogCategoryMethodTrace, "center distance = " + centerDistance + ", center = " + ownCollider.center + ", " + otherSphereCollider.center + ", radius = " + ownCollider.radius + ", " + otherSphereCollider.radius);
                }

                int ownCreateFrameAt = int.Parse(ownCollider.name);
                int createFrameAt = int.Parse(collider.name);
                GameObject destroyGameObject = null;

                // 後に作成したGameObjectを削除
                if (ownCreateFrameAt < createFrameAt)
                {
                    destroyGameObject = collider.gameObject;
                }
                else
                {
                    destroyGameObject = gameObject;
                }

                controller.DestroyPlaneGameObject(destroyGameObject);
            }

            mLogger.CategoryLog(LogCategoryMethodOut);
        }

        public void OnTriggerExit(Collider collider)
        {
        }

        public void OnTriggerStay(Collider collider)
        {
        }
    }
}
