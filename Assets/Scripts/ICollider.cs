namespace Eq.Unity
{
    public interface ICollider
    {
        void OnCollisionEnter(UnityEngine.Collision collision);

        void OnCollisionExit(UnityEngine.Collision collision);

        void OnCollisionStay(UnityEngine.Collision collision);
    }
}
