using System.Collections.Generic;
using XNode;


public interface DecodeNode
{
    public void Decode(byte[] bytes, int offset, int length);
}

public abstract class KDNode : Node, DecodeNode
{
    public abstract void Decode(byte[] bytes, int offset, int length);
}

public abstract class KDRNode : KRNode, DecodeNode
{
    public abstract void Decode(byte[] bytes, int offset, int length);
}

public abstract class KDFRNode<T> : KFRNode<T>, DecodeNode where T:class
{
    public abstract void Decode(byte[] bytes, int offset, int length);
}

public abstract class KDFNode<T> : KFNode<T>, DecodeNode where T:class
{
    public abstract void Decode(byte[] bytes, int offset, int length);
}