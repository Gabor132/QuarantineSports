using System.Collections.Generic;
using OpenPose;

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
    }

}