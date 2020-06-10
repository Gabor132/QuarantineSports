using System.Collections.Generic;
using OpenPose;
using Unity.Barracuda;

namespace Core.DatasetObjects
{
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

        public Tensor GetAsTensor()
        {
            Tensor t = new Tensor(1,1,25,3);
            for (var i = 0; i < 75; i += 3)
            {
                t[0, 0, i / 3, 0] = Keypoints[i];
                t[0, 0, i / 3, 1] = Keypoints[i+1];
                t[0, 0, i / 3, 2] = Keypoints[i+2];
            }
            return t;
        }
    }

}