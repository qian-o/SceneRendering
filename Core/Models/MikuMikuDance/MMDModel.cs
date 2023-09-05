using Silk.NET.Maths;

namespace Core.Models.MikuMikuDance;

#region Structs
public struct MMDSubMesh
{
    public int BeginIndex;

    public int VertexCount;

    public int MaterialID;
}
#endregion

public unsafe abstract class MMDModel
{
    public abstract MMDNodeManager? GetNodeManager();

    public abstract MMDIkManager? GetIkManager();

    public abstract MMDMorphManager? GetMorphManager();

    public abstract int GetVertexCount();

    public abstract Vector3D<float>* GetPositions();

    public abstract Vector3D<float>* GetNormals();

    public abstract Vector2D<float>* GetUVs();

    public abstract Vector3D<float>* GetUpdatePositions();

    public abstract Vector3D<float>* GetUpdateNormals();

    public abstract Vector2D<float>* GetUpdateUVs();

    public abstract int GetIndexElementSize();

    public abstract int GetIndexCount();

    public abstract void* GetIndices();

    public abstract int GetMaterialCount();

    public abstract MMDMaterial* GetMaterials();

    public abstract int GetSubMeshCount();

    public abstract MMDSubMesh* GetSubMeshes();

    public abstract MMDPhysics? GetMMDPhysics();

    // ノードを初期化する
    public abstract void InitializeAnimation();
}
