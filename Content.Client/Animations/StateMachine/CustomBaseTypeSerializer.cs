using Content.Client.Animations.StateMachine.AnimationStateMachineConditions;
using Robust.Shared.Serialization;
using Robust.Shared.Serialization.Manager;
using Robust.Shared.Serialization.Markdown;
using Robust.Shared.Serialization.Markdown.Mapping;
using Robust.Shared.Serialization.Markdown.Sequence;
using Robust.Shared.Serialization.Markdown.Validation;
using Robust.Shared.Serialization.Markdown.Value;
using Robust.Shared.Serialization.TypeSerializers.Interfaces;

namespace Content.Client.Animations.StateMachine;
public sealed class CustomBaseTypeSerializer<TBase> :
    ITypeSerializer<TBase, MappingDataNode>,
    ITypeSerializer<TBase, ValueDataNode>,
    ITypeSerializer<TBase[], SequenceDataNode>,
    ITypeSerializer<List<TBase>, SequenceDataNode>,
    ITypeSerializer<HashSet<TBase>, SequenceDataNode>,
    ITypeSerializer<Queue<TBase>, SequenceDataNode>
    where TBase : notnull
{
    private static bool BaseInName()
    {
        return typeof(TBase).Name.Contains("Base");
    }

    private static string ExpandName(string name)
    {
        var typeName = typeof(TBase).Name;
        return name.Replace("!", $"!type:{typeName.Replace("Base", string.Empty)}");
    }

    private static string ReduceName(TBase obj)
    {
        var typeName = obj.GetType().Name;
        return typeName.Replace(typeof(TBase).Name.Replace("Base", string.Empty), "!");
    }

    private static void ThrowOnInvalidName(DataNode? node = null)
    {
        if (BaseInName())
            return;

        if (node == null)
        {
            throw new InvalidMappingException(
                "Base class requires 'Base' in its name for this serializer to work.");
        }
        else
        {
            throw new InvalidMappingException(
                $"{node.Start}: Base class requires 'Base' in its name for this serializer to work.");
        }
    }

    private static void ThrowOnNullTag(DataNode node)
    {
        if (node.Tag == null)
        {
            throw new InvalidMappingException(
                $"{node.Start}: Tag cannot be null.");
        }
    }

    private static TBase ReadAsDataNode(
        ISerializationManager serializationManager,
        DataNode node,
        SerializationHookContext hookCtx,
        ISerializationContext? context = null,
        ISerializationManager.InstantiationDelegate<TBase>? instanceProvider = null)
    {
        ThrowOnInvalidName(node);
        ThrowOnNullTag(node);
        node.Tag = ExpandName(node.Tag!);
        return serializationManager.Read(node, hookCtx, context, instanceProvider);
    }

    private static IEnumerable<TBase> ReadCollection(
        ISerializationManager serializationManager,
        IReadOnlyCollection<DataNode> nodes,
        SerializationHookContext hookCtx,
        ISerializationContext? context = null)
    {
        ThrowOnInvalidName();
        foreach (var node in nodes)
        {
            yield return ReadAsDataNode(serializationManager, node, hookCtx, context);
        }
    }

    private static SequenceDataNode WriteAsSequence(
        ISerializationManager serializationManager,
        IEnumerable<TBase> values)
    {
        ThrowOnInvalidName();
        var sequence = new SequenceDataNode();
        foreach (var value in values)
        {
            var node = serializationManager.WriteValue(value.GetType(), notNullableOverride: false);
            node.Tag = ReduceName(value);
            sequence.Add(node);
        }
        return sequence;
    }

    private static ValidationNode ValidateAsDataNode(
        ISerializationManager serializationManager,
        DataNode node,
        ISerializationContext? context = null)
    {
        if (node.Tag == null)
            return new ErrorNode(node, "Tag is null.");
        var copy = node.Copy();
        copy.Tag = ExpandName(node.Tag);
        return serializationManager.ValidateNode<AnimationStateMachineConditionBase>(copy, context);

    }

    public TBase Read(
        ISerializationManager serializationManager,
        ValueDataNode node,
        IDependencyCollection dependencies,
        SerializationHookContext hookCtx,
        ISerializationContext? context = null,
        ISerializationManager.InstantiationDelegate<TBase>? instanceProvider = null)
    {
        return ReadAsDataNode(serializationManager, node, hookCtx, context, instanceProvider);
    }

    public TBase Read(
        ISerializationManager serializationManager,
        MappingDataNode node,
        IDependencyCollection dependencies,
        SerializationHookContext hookCtx,
        ISerializationContext? context = null,
        ISerializationManager.InstantiationDelegate<TBase>? instanceProvider = null)
    {
        return ReadAsDataNode(serializationManager, node, hookCtx, context, instanceProvider);
    }

    public TBase[] Read(
        ISerializationManager serializationManager,
        SequenceDataNode node,
        IDependencyCollection dependencies,
        SerializationHookContext hookCtx,
        ISerializationContext? context = null,
        ISerializationManager.InstantiationDelegate<TBase[]>? instanceProvider = null)
    {
        var list = new TBase[node.Count];
        var i = 0;
        foreach (var dataNode in node)
        {
            list[i++] = ReadAsDataNode(serializationManager, dataNode, hookCtx, context);
        }

        return list;
    }

    public List<TBase> Read(
        ISerializationManager serializationManager,
        SequenceDataNode node,
        IDependencyCollection dependencies,
        SerializationHookContext hookCtx,
        ISerializationContext? context = null,
        ISerializationManager.InstantiationDelegate<List<TBase>>? instanceProvider = null)
    {
        return new List<TBase>(ReadCollection(serializationManager, node.Sequence, hookCtx, context));
    }

    public HashSet<TBase> Read(
        ISerializationManager serializationManager,
        SequenceDataNode node,
        IDependencyCollection dependencies,
        SerializationHookContext hookCtx,
        ISerializationContext? context = null,
        ISerializationManager.InstantiationDelegate<HashSet<TBase>>? instanceProvider = null)
    {
        return new HashSet<TBase>(ReadCollection(serializationManager,
            node.Sequence,
            hookCtx,
            context));
    }

    public Queue<TBase> Read(
        ISerializationManager serializationManager,
        SequenceDataNode node,
        IDependencyCollection dependencies,
        SerializationHookContext hookCtx,
        ISerializationContext? context = null,
        ISerializationManager.InstantiationDelegate<Queue<TBase>>? instanceProvider = null)
    {
        return new Queue<TBase>(ReadCollection(serializationManager,
            node.Sequence,
            hookCtx,
            context));
    }

    public ValidationNode Validate(
        ISerializationManager serializationManager,
        SequenceDataNode node,
        IDependencyCollection dependencies,
        ISerializationContext? context = null)
    {
        var list = new List<ValidationNode>(node.Count);
        foreach (var elem in node)
        {
            list.Add(ValidateAsDataNode(serializationManager, elem, context));
        }
        return new ValidatedSequenceNode(list);
    }

    public ValidationNode Validate(
        ISerializationManager serializationManager,
        MappingDataNode node,
        IDependencyCollection dependencies,
        ISerializationContext? context = null)
    {
        return ValidateAsDataNode(serializationManager, node, context);
    }

    public ValidationNode Validate(
        ISerializationManager serializationManager,
        ValueDataNode node,
        IDependencyCollection dependencies,
        ISerializationContext? context = null)
    {
        return ValidateAsDataNode(serializationManager, node, context);
    }

    public DataNode Write(
        ISerializationManager serializationManager,
        TBase value,
        IDependencyCollection dependencies,
        bool alwaysWrite = false,
        ISerializationContext? context = null)
    {
        ThrowOnInvalidName();
        var node = serializationManager.WriteValue(value.GetType(), notNullableOverride: false);
        node.Tag = ReduceName(value);
        return node;
    }

    public DataNode Write(
        ISerializationManager serializationManager,
        TBase[] value,
        IDependencyCollection dependencies,
        bool alwaysWrite = false,
        ISerializationContext? context = null)
    {
        return WriteAsSequence(serializationManager, value);
    }

    public DataNode Write(
        ISerializationManager serializationManager,
        List<TBase> value,
        IDependencyCollection dependencies,
        bool alwaysWrite = false,
        ISerializationContext? context = null)
    {
        return WriteAsSequence(serializationManager, value);
    }

    public DataNode Write(
        ISerializationManager serializationManager,
        HashSet<TBase> value,
        IDependencyCollection dependencies,
        bool alwaysWrite = false,
        ISerializationContext? context = null)
    {
        return WriteAsSequence(serializationManager, value);
    }

    public DataNode Write(
        ISerializationManager serializationManager,
        Queue<TBase> value,
        IDependencyCollection dependencies,
        bool alwaysWrite = false,
        ISerializationContext? context = null)
    {
        return WriteAsSequence(serializationManager, value);
    }
}
