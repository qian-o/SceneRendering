using Core.Helpers;
using Silk.NET.Maths;

namespace Core.Models.MikuMikuDance.VPD;

#region Structs
public struct Bone
{
    public string Name;

    public Vector3D<float> Translate;

    public Quaternion<float> Quaternion;
}

public struct Morph
{
    public string Name;

    public float Weight;
}
#endregion

public class VPDFile
{
    public List<Bone> Bones { get; } = new List<Bone>();

    public List<Morph> Morphs { get; } = new List<Morph>();

    public VPDFile(string file)
    {
        ReadVPDFile(this, file);
    }

    private static void ReadVPDFile(VPDFile vpd, string file)
    {
        string[] lines = File.ReadAllLines(file, EncodingType.ShiftJIS.GetEncoding()).Where(item => !string.IsNullOrEmpty(item)).ToArray();
        int endIndex = lines.Length - 1;

        if (!lines.Any() || lines[0] != "Vocaloid Pose Data file")
        {
            throw new Exception("Invalid VPD file.");
        }

        for (int i = 0; i < lines.Length; i++)
        {
            string line = lines[i];
            int commentPos = line.IndexOf("//");
            if (commentPos != -1)
            {
                lines[i] = line[..commentPos].Trim();
            }
        }

        int lineIt = 1;

        // parent file name
        if (lineIt == -1 || lines[lineIt] == lines.Last())
        {
            throw new Exception("VPD File Parse Error.[parent fil name].");
        }

        lineIt++;

        // bone count
        if (lineIt == -1 || lineIt == endIndex)
        {
            throw new Exception("VPD File Parse Error.[bone count].");
        }

        if (!int.TryParse(lines[lineIt].AsSpan(0, lines[lineIt].IndexOf(';')), out int numBones))
        {
            throw new Exception($"VPD File Parse Error.{lineIt + 1}[bone count].");
        }

        vpd.Bones.Resize(numBones);

        lineIt++;

        // bone data
        int boneCount = 0;
        while (boneCount < numBones && lineIt != endIndex)
        {
            Bone bone = new();
            int boneIdx = 0;

            {
                string line = lines[lineIt];
                int delimPos1 = line.IndexOf("Bone");
                if (delimPos1 == -1)
                {
                    throw new Exception($"VPD File Parse Error.{lineIt + 1}[Not Found Bone].");
                }
                delimPos1 += 4;

                int delimPos2 = line.IndexOf('{', delimPos1);
                if (delimPos2 == -1)
                {
                    throw new Exception($"VPD File Parse Error.{lineIt + 1}[Not Found Bone].");
                }

                string numStr = line[delimPos1..delimPos2];
                if (!int.TryParse(numStr, out boneIdx))
                {
                    throw new Exception($"VPD File Parse Error.{lineIt + 1}[Not Found Bone].");
                }
                if (boneIdx >= numBones)
                {
                    throw new Exception($"VPD File Parse Error. {lineIt + 1}:[Bone Index over]");
                }
                bone.Name = line[(delimPos2 + 1)..].Trim();
            }

            ++lineIt;

            {
                string line = lines[lineIt];
                int delimPos = line.IndexOf(";");
                if (delimPos == -1)
                {
                    throw new Exception($"VPD File Parse Error. {lineIt + 1}:[Split error]");
                }

                string[] posStrs = line[..delimPos].Split(',');
                if (posStrs.Length != 3)
                {
                    throw new Exception($"VPD File Parse Error. {lineIt + 1}:[Split error]");
                }

                if (!float.TryParse(posStrs[0], out float x) || !float.TryParse(posStrs[1], out float y) || !float.TryParse(posStrs[2], out float z))
                {
                    throw new Exception($"VPD File Parse Error. {lineIt + 1}:[Split error]");
                }

                bone.Translate = new Vector3D<float>(x, y, z);
            }

            ++lineIt;

            {
                string line = lines[lineIt];
                int delimPos = line.IndexOf(";");
                if (delimPos == -1)
                {
                    throw new Exception($"VPD File Parse Error. {lineIt + 1}:[Split error]");
                }

                string[] posStrs = line[..delimPos].Split(',');
                if (posStrs.Length != 4)
                {
                    throw new Exception($"VPD File Parse Error. {lineIt + 1}:[Split error]");
                }

                if (!float.TryParse(posStrs[0], out float x) || !float.TryParse(posStrs[1], out float y) || !float.TryParse(posStrs[2], out float z) || !float.TryParse(posStrs[3], out float w))
                {
                    throw new Exception($"VPD File Parse Error. {lineIt + 1}:[Split error]");
                }

                bone.Quaternion = new Quaternion<float>(x, y, z, w);
            }

            ++lineIt;

            {
                string line = lines[lineIt];
                if (!line.Contains('}'))
                {
                    throw new Exception($"VPD File Parse Error. {lineIt + 1}:[Split error]");
                }
            }

            ++lineIt;
            boneCount++;

            vpd.Bones[boneIdx] = bone;
        }

        // morph data
        while (lineIt < endIndex)
        {
            Morph morph = new();

            {
                string line = lines[lineIt];
                int delimPos1 = line.IndexOf("Morph");
                if (delimPos1 == -1)
                {
                    throw new Exception($"VPD File Parse Error.{lineIt + 1}[Not Found Morph].");
                }
                delimPos1 += "Morph".Length - 1;

                int delimPos2 = line.IndexOf('{', delimPos1);
                if (delimPos2 == -1)
                {
                    throw new Exception($"VPD File Parse Error.{lineIt + 1}[Not Found Morph].");
                }

                string numStr = line[delimPos1..delimPos2];
                if (!int.TryParse(numStr, out int morphIdx))
                {
                    throw new Exception($"VPD File Parse Error.{lineIt + 1}[Not Found Morph].");
                }
                morph.Name = line[(delimPos2 + 1)..].Trim();
            }

            ++lineIt;

            {
                string line = lines[lineIt];
                int delimPos = line.IndexOf(";");
                if (delimPos == -1)
                {
                    throw new Exception($"VPD File Parse Error. {lineIt + 1}:[Split error]");
                }

                if (float.TryParse(line[..delimPos], out float weight))
                {
                    throw new Exception($"VPD File Parse Error. {lineIt + 1}:[Split error]");
                }

                morph.Weight = weight;
            }

            ++lineIt;

            {
                string line = lines[lineIt];
                if (!line.Contains('}'))
                {
                    throw new Exception($"VPD File Parse Error. {lineIt + 1}:[Split error]");
                }
            }

            ++lineIt;

            vpd.Morphs.Add(morph);
        }
    }
}
