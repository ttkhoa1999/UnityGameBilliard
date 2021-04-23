using UnityEngine;
using ThreeDPool.Controllers;

namespace ThreeDPool
{
    class PocketsCollider : MonoBehaviour
    {
        //Collider khi bi cái va chạm với vật thể
        private void OnTriggerEnter(Collider collider)
        {
            CueBallController cueBall = collider.gameObject.GetComponent<CueBallController>();
            if (cueBall != null)
                cueBall.BallPocketed();
        }
    }
}
