using UnityEngine;

namespace Env3DTouch
{
    public static class RandomSpawnPoint
    {
        public static float armLen = 0.4f;
        public static void AppearInRandomPos(ref Transform target, Transform originPoint)
        {
            // float armLen = 0.4f;
            //inclination and azimuth may be opposite
            float tmpAZ = (float)(Random.Range(0, 3) * 15 + 30) / 180 * Mathf.PI;
            float tmpI = (float)(Random.Range(0, 8) * 45) / 180 * Mathf.PI;
            //random target distance 1 to 4
            int distance = Random.Range(1, 5);
            float x = (float)((distance * 0.5f) * armLen) * Mathf.Sin(tmpAZ) * Mathf.Cos(tmpI);
            float y = (float)((distance * 0.5f) * armLen) * Mathf.Sin(tmpAZ) * Mathf.Sin(tmpI);
            float z = (float)((distance * 0.5f) * armLen) * Mathf.Cos(tmpAZ);
            target.position = new Vector3(x, y, z) + originPoint.position;

            //random box size form 3 to 5
            float boxSize = (float)Random.Range(3, 6);
            target.GetComponent<BoxCollider>().size = new Vector3(boxSize, boxSize, boxSize);
            target.Find("mesh").transform.localScale = new Vector3(boxSize, boxSize, boxSize);
            target.GetComponentInChildren<VibrationHandler>().updateCollider(boxSize);
        }

        public static void UpdateVibrationCollider(ref Transform target){
            float boxSize = target.GetComponent<BoxCollider>().size.x;
            target.GetComponentInChildren<VibrationHandler>().updateCollider(boxSize);
        }
    }
}

