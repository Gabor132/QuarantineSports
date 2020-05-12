using OpenPose;

namespace Core.DatasetObjects
{
    public class OutputKeypoints
    {
        public MultiArray<float> PoseKeypoints { get; set; }
        public MultiArray<float> FaceKeypoints { get; set; }
        public Pair<MultiArray<float>> HandKeypoints { get; set; }

        public Category Category;
        
        public OutputKeypoints(OPDatum datum, Category category)
        {
            PoseKeypoints = datum.poseKeypoints;
            FaceKeypoints = datum.faceKeypoints;
            HandKeypoints = datum.handKeypoints;
            Category = category;
        }
    }
}