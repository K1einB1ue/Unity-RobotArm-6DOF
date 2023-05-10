
using System;
using XNode;

public interface EncodeNode
{
    public void Send(Action<byte[]> send, Func<bool> enable);
}

public abstract class KENode : Node, EncodeNode
{
    public abstract void Send(Action<byte[]> send, Func<bool> enable);
}

public abstract class KERNode : KRNode, EncodeNode
{
    public abstract void Send(Action<byte[]> send, Func<bool> enable);
}

public abstract class KEFRNode<T> : KFRNode<T>, EncodeNode where T:class
{
    public abstract void Send(Action<byte[]> send, Func<bool> enable);
}

public abstract class KEFNode<T> : KFNode<T>, EncodeNode where T:class
{
    public abstract void Send(Action<byte[]> send, Func<bool> enable);
}