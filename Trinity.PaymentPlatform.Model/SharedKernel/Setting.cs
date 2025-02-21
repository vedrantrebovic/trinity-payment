using System.Globalization;
using Trinity.PaymentPlatform.Model.SeedWork;

namespace Trinity.PaymentPlatform.Model.SharedKernel;

public class Setting<TId> : Entity<SettingIdentifier<TId>>
{
    public const char Separator = '|';
    public virtual string Value { get; protected set; }
    public virtual string Type { get; protected set; }

    protected Setting(){}

    protected Setting(SettingIdentifier<TId> id, string value, string type)
    {
        Id = id;
        Value = value;
        Type = type;
    }

    public static Setting<TId> Create<T>(T value, SettingIdentifier<TId> id) where T: IConvertible
    {
        string type = value.GetType().FullName;
        string val = Convert.ChangeType(value, typeof(string), CultureInfo.InvariantCulture).ToString();

        return new Setting<TId>(id, val, type);
    }

    public virtual void UpdateValue(string value)
    {
        string val = Convert.ChangeType(value, System.Type.GetType(Type), CultureInfo.InvariantCulture).ToString();
        Value = value;
    }

    public virtual T GetValue<T>()
    {
        return (T)Convert.ChangeType(Value, typeof(T), CultureInfo.InvariantCulture);
    }
}

[Serializable]
public class SettingIdentifier<TIn>
{
    public virtual TIn EntityId { get; set; }
    public virtual string Key { get; set; }

    protected SettingIdentifier(){}

    public SettingIdentifier(TIn entityId, string key)
    {
        EntityId = entityId;
        Key = key;
    }

    public override bool Equals(object obj)
    {
        if (obj == null)
            return false;
        var id = (SettingIdentifier<TIn>)obj;
        if (id == null)
            return false;
        
        return id.EntityId.Equals(EntityId) && Key == id.Key;
    }

    public override int GetHashCode() => HashCode.Combine(this.EntityId, this.Key);
}

//public abstract class SettingIdentifier
//{
//    public virtual string Key { get; set; }
//}

//[Serializable]
//public class SettingIntIdentifier : SettingIdentifier
//{
//    public virtual int EntityId { get; set; }
//    protected SettingIntIdentifier() { }

//    public SettingIntIdentifier(int entityId, string key)
//    {
//        EntityId = entityId;
//        Key = key;
//    }

//    public override bool Equals(object obj)
//    {
//        if (obj == null)
//            return false;
//        var id = (SettingIntIdentifier)obj;
//        if (id == null)
//            return false;
//        return EntityId == id.EntityId && Key == id.Key;
//    }

//    public override int GetHashCode() => HashCode.Combine(this.EntityId, this.Key);
//}

//[Serializable]
//public class SettingLongIdentifier : SettingIdentifier
//{
//    public virtual long EntityId { get; set; }
//    protected SettingLongIdentifier() { }

//    public SettingLongIdentifier(long entityId, string key)
//    {
//        EntityId = entityId;
//        Key = key;
//    }

//    public override bool Equals(object obj)
//    {
//        if (obj == null)
//            return false;
//        var id = (SettingLongIdentifier)obj;
//        if (id == null)
//            return false;
//        return EntityId == id.EntityId && Key == id.Key;
//    }

//    public override int GetHashCode() => HashCode.Combine(this.EntityId, this.Key);
//}

//[Serializable]
//public class SettingStringIdentifier : SettingIdentifier
//{
//    public virtual string EntityId { get; set; }
//    protected SettingStringIdentifier() { }

//    public SettingStringIdentifier(string entityId, string key)
//    {
//        EntityId = entityId;
//        Key = key;
//    }

//    public override bool Equals(object obj)
//    {
//        if (obj == null)
//            return false;
//        var id = (SettingStringIdentifier)obj;
//        if (id == null)
//            return false;
//        return EntityId == id.EntityId && Key == id.Key;
//    }

//    public override int GetHashCode() => HashCode.Combine(this.EntityId, this.Key);
//}

//[Serializable]
//public class SettingGuidIdentifier : SettingIdentifier
//{
//    public virtual Guid EntityId { get; set; }
//    protected SettingGuidIdentifier() { }

//    public SettingGuidIdentifier(Guid entityId, string key)
//    {
//        EntityId = entityId;
//        Key = key;
//    }

//    public override bool Equals(object obj)
//    {
//        if (obj == null)
//            return false;
//        var id = (SettingGuidIdentifier)obj;
//        if (id == null)
//            return false;
//        return EntityId == id.EntityId && Key == id.Key;
//    }

//    public override int GetHashCode() => HashCode.Combine(this.EntityId, this.Key);
//}