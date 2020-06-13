using System.Collections.Generic;
using OpenPose;
using Unity.Barracuda;

namespace Core.DatasetObjects
{

    /** 
     * Class to store Keypoints analyzed by Openpose
     */
    public class Frame
    {
        public List<float> Keypoints;
        public int Category;

        public Frame(MultiArray<float> keypoints, int category)
        {
            if (keypoints != null)
            {
                for (int i = 0; i < 25; i++)
                {
                    // The first 25 * 3 keypoints belong to the first person
                    Keypoints = keypoints.GetRange(0, 25 * 3);
                }
            }
            Category = category;
        }

        public Tensor GetAsTensor(TensorShape inputShape)
        {
            Tensor t = new Tensor(1, 25, 3 ,1);
            for (var j = 0; j < 25; j++)
            {
                for (var i = 0; i < 3; i++)
                {
                    t[0, j, i, 0] = Keypoints[j*3 + i];
                }
            }
            return t;
        }
    }

}