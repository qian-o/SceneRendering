using Silk.NET.Maths;

namespace Core.Models.MikuMikuDance;

public class MikuMikuBone
{
    private readonly List<KeyPosition> _positions;
    private readonly List<KeyRotation> _rotations;
    private readonly int _numPositions;
    private readonly int _numRotations;

    public int Id { get; }

    public string Name { get; }

    public Matrix4X4<float> LocalTransform { get; private set; }

    public MikuMikuBone(int id, string name, VmdBoneFrame[] boneFrames)
    {
        Id = id;
        Name = name;
        LocalTransform = Matrix4X4<float>.Identity;

        _positions = new();
        _rotations = new();

        _numPositions = boneFrames.Length;
        _numRotations = boneFrames.Length;

        for (int i = 0; i < _numPositions; i++)
        {
            VmdBoneFrame frame = boneFrames[i];

            _positions.Add(new KeyPosition(frame.Frame / MikuMikuAnimation.TicksPerSecond, frame.Position));
        }

        for (int i = 0; i < _numRotations; i++)
        {
            VmdBoneFrame frame = boneFrames[i];

            _rotations.Add(new KeyRotation(frame.Frame / MikuMikuAnimation.TicksPerSecond, frame.Rotation));
        }
    }

    public void Update(float animationTime)
    {
        Matrix4X4<float> translation = InterpolatePosition(animationTime);
        Matrix4X4<float> rotation = InterpolateRotation(animationTime);

        LocalTransform = rotation * translation;
    }

    private Matrix4X4<float> InterpolatePosition(float animationTime)
    {
        if (_numPositions == 1)
        {
            return Matrix4X4.CreateTranslation(_positions[0].Position);
        }

        int p0Index = GetPositionIndex(animationTime);
        int p1Index = p0Index + 1;

        float scaleFactor = GetScaleFactor(_positions[p0Index].Time, _positions[p1Index].Time, animationTime);

        Vector3D<float> finalPosition = Vector3D.Lerp(_positions[p0Index].Position, _positions[p1Index].Position, scaleFactor);

        return Matrix4X4.CreateTranslation(finalPosition);
    }

    private Matrix4X4<float> InterpolateRotation(float animationTime)
    {
        if (_numRotations == 1)
        {
            Quaternion<float> rotation = Quaternion<float>.Normalize(_rotations[0].Orientation);

            return Matrix4X4.CreateFromQuaternion(rotation);
        }

        int p0Index = GetRotationIndex(animationTime);
        int p1Index = p0Index + 1;

        float scaleFactor = GetScaleFactor(_rotations[p0Index].Time, _rotations[p1Index].Time, animationTime);

        Quaternion<float> finalRotation = Quaternion<float>.Slerp(_rotations[p0Index].Orientation, _rotations[p1Index].Orientation, scaleFactor);

        finalRotation = Quaternion<float>.Normalize(finalRotation);

        return Matrix4X4.CreateFromQuaternion(finalRotation);
    }

    private int GetPositionIndex(float animationTime)
    {
        for (int i = 0; i < _numPositions - 1; i++)
        {
            if (animationTime < _positions[i + 1].Time)
            {
                return i;
            }
        }

        return 0;
    }

    private int GetRotationIndex(float animationTime)
    {
        for (int i = 0; i < _numRotations - 1; i++)
        {
            if (animationTime < _rotations[i + 1].Time)
            {
                return i;
            }
        }

        return 0;
    }

    private static float GetScaleFactor(float lastTimeStamp, float nextTimeStamp, float animationTime)
    {
        float midWayLength = animationTime - lastTimeStamp;
        float framesDiff = nextTimeStamp - lastTimeStamp;

        float scaleFactor = midWayLength / framesDiff;

        return scaleFactor;
    }
}