
using UnityEngine.Events;

[CreateNodeMenu("decode/image",order = 1)]
public class KDImage : KDNode
{
    [Input()] public long len;
    public override void Decode(byte[] bytes, int offset, int length)
    {
        throw new System.NotImplementedException();
    }
}
