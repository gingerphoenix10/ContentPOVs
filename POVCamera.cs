using System.Text;
using Zorro.Core.Serizalization;

namespace ContentPOVs;

public class POVCamera : ItemDataEntry
{
    public string? plrID;

    public override void Serialize(BinarySerializer binarySerializer)
    {
        binarySerializer.WriteString(plrID, Encoding.UTF8);
    }

    public override void Deserialize(BinaryDeserializer binaryDeserializer)
    {
        plrID = binaryDeserializer.ReadString(Encoding.UTF8);
    }
}