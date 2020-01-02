using System.Collections.ObjectModel;
using UnityEngine;

namespace of2.Curves
{
    public interface ICurve
    {
        ReadOnlyCollection<Vector3> Positions { get; }
        ReadOnlyCollection<float> Times { get; }

        Vector3 Evaluate(float t);
        Vector3 Velocity(float t);
        Vector3 Acceleration(float t);
        float FindParameterByDistance(float t1, float s);
        float ArcLength(float t1, float t2);
        float SegmentArcLength(int i, float u1, float u2);
        bool AddPositionToIndex(Vector3 pos, int index);
        bool RemovePosition(int index);
        bool SetPosition(int index, Vector3 pos);
        bool SetTime(int index, float time);
    }
}