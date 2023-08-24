using Core.Helpers;
using Silk.NET.Assimp;
using Silk.NET.Maths;

namespace Core.Models;

public unsafe class Bone
{
    private readonly List<KeyPosition> _positions;
    private readonly List<KeyRotation> _rotations;
    private readonly List<KeyScale> _scales;
    private readonly int _numPositions;
    private readonly int _numRotations;
    private readonly int _numScales;

    public int Id { get; }

    public string Name { get; }

    public Matrix4X4<float> LocalTransform { get; private set; }

    public Bone(int id, string name, NodeAnim* channel)
    {
        Id = id;
        Name = name;
        LocalTransform = Matrix4X4<float>.Identity;

        _positions = new();
        _rotations = new();
        _scales = new();

        _numPositions = (int)channel->MNumPositionKeys;
        _numRotations = (int)channel->MNumRotationKeys;
        _numScales = (int)channel->MNumScalingKeys;

        for (int i = 0; i < _numPositions; i++)
        {
            VectorKey vector = channel->MPositionKeys[i];

            _positions.Add(new KeyPosition((float)vector.MTime, vector.MValue.ToGeneric()));
        }

        for (int i = 0; i < _numRotations; i++)
        {
            QuatKey quat = channel->MRotationKeys[i];

            _rotations.Add(new KeyRotation((float)quat.MTime, quat.MValue.AsQuaternion.ToGeneric()));
        }

        for (int i = 0; i < _numScales; i++)
        {
            VectorKey vector = channel->MScalingKeys[i];

            _scales.Add(new KeyScale((float)vector.MTime, vector.MValue.ToGeneric()));
        }
    }

    public void Update(float animationTime)
    {
        Matrix4X4<float> translation = InterpolatePosition(animationTime);
        Matrix4X4<float> rotation = InterpolateRotation(animationTime);
        Matrix4X4<float> scale = InterpolateScale(animationTime);

        LocalTransform = translation * rotation * scale;
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

            return Matrix4X4.CreateFromQuaternion(_rotations[0].Orientation);
        }

        int p0Index = GetRotationIndex(animationTime);
        int p1Index = p0Index + 1;

        float scaleFactor = GetScaleFactor(_rotations[p0Index].Time, _rotations[p1Index].Time, animationTime);

        Quaternion<float> finalRotation = Quaternion<float>.Slerp(_rotations[p0Index].Orientation, _rotations[p1Index].Orientation, scaleFactor);

        // finalRotation = Quaternion<float>.Normalize(finalRotation);

        return Matrix4X4.CreateFromQuaternion(finalRotation);
    }

    private Matrix4X4<float> InterpolateScale(float animationTime)
    {
        if (_numScales == 1)
        {
            return Matrix4X4.CreateScale(_scales[0].Scale);
        }

        int p0Index = GetScaleIndex(animationTime);
        int p1Index = p0Index + 1;

        float scaleFactor = GetScaleFactor(_scales[p0Index].Time, _scales[p1Index].Time, animationTime);

        Vector3D<float> finalScale = Vector3D.Lerp(_scales[p0Index].Scale, _scales[p1Index].Scale, scaleFactor);

        return Matrix4X4.CreateScale(finalScale);
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

    private int GetScaleIndex(float animationTime)
    {
        for (int i = 0; i < _numScales - 1; i++)
        {
            if (animationTime < _scales[i + 1].Time)
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
