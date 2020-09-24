using System.IO;
using TMPro;
using UnityEngine;

public class GameDataReader : MonoBehaviour
{
    public int Version { get; }

    BinaryReader reader;

    public GameDataReader(BinaryReader reader, int version)
    {
        this.reader = reader;
        this.Version = version;
    }

    public float ReaderFloat()
    {
        return reader.ReadSingle();
    }

    public int ReaderInt()
    {
        return reader.ReadInt32();
    }

    public Quaternion ReadQuaternion()
    {
        Quaternion value;
        value.x = reader.ReadSingle();
        value.y = reader.ReadSingle();
        value.z = reader.ReadSingle();
        value.w = reader.ReadSingle();

        return value;
    }

    public Vector3 ReadVector3()
    {
        Vector3 value;
        value.x = reader.ReadSingle();
        value.y = reader.ReadSingle();
        value.z = reader.ReadSingle();

        return value;
    }

    public Color ReadColor()
    {
        Color value;
        value.r = reader.ReadSingle();
        value.g = reader.ReadSingle();
        value.b = reader.ReadSingle();
        value.a = reader.ReadSingle();

        return value;
    }

    public Random.State ReadRandomState()
    {
        //return Random.state;
        return JsonUtility.FromJson<Random.State>(reader.ReadString());
    }
}
