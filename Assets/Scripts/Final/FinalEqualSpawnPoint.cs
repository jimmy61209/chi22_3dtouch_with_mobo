using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using System; 
using Random = UnityEngine.Random;

namespace Env3DTouch
{
    public static class FinalEqualSpawnPoint
    {
        public static int count = 0;
        static List<int[]> distance = new List<int[]>();
        public const int trial_num = 36;
        public static float armLen = 0.4f;

        public static int AZIndex;
        public static int IIndex;
        public static int currentDistance;
        public static int currentBoxSize;

        public static void AppearInPredefinePos(ref Transform target, Transform originPoint)
        {
            // float armLen = 0.4f;

            AZIndex = Random.Range(0, 3);
            IIndex = Random.Range(0, 8);
            currentDistance = distance[count][0] + 1;
            currentBoxSize = (distance[count][1] + 3);


            //inclination and azimuth may be opposite
            float tmpAZ = (float)(AZIndex * 15 + 30) / 180 * Mathf.PI;
            float tmpI = (float)(IIndex * 45) / 180 * Mathf.PI;
            //random target distance 1 to 4
            // int distance = Random.Range(1, 5);
            float x = (float)(((distance[count][0] + 1) * 0.5f) * armLen) * Mathf.Sin(tmpAZ) * Mathf.Cos(tmpI);
            float y = (float)(((distance[count][0] + 1) * 0.5f) * armLen) * Mathf.Sin(tmpAZ) * Mathf.Sin(tmpI);
            float z = (float)(((distance[count][0] + 1) * 0.5f) * armLen) * Mathf.Cos(tmpAZ);
            target.position = new Vector3(x, y, z) + originPoint.position;

            //random box size form 3 to 5
            float boxSize = (float)(distance[count][1] + 3);
            target.GetComponent<BoxCollider>().size = new Vector3(boxSize, boxSize, boxSize);
            target.Find("mesh").transform.localScale = new Vector3(boxSize, boxSize, boxSize);
            target.GetComponentInChildren<FinalVibrationHandler>().updateCollider(boxSize);

            count++;
        }

        public static void ResetSpawnOrder(){
            distance = new List<int[]>();
             for(int i = 0; i < trial_num; i++){
                distance.Add(new int[]{i % 4, i % 3});
            }
            distance = distance.OrderBy(a => Guid.NewGuid()).ToList();
            count = 0;
        }
    }
}

