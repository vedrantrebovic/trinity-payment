//using Trinity.Model.Localization.EntityTranslations;
//using Trinity.Model.SeedWork;

//namespace Trinity.Model.SharedKernel;

//public class EntityTranslation<TId> : Entity<EntityTranslationId<TId>>, IAggregateRoot
//{
//    public virtual string Text { get; protected set; }
//    public virtual string AbbreviatedText { get; protected set; }
//    public virtual string Description { get; protected set; }

//    protected EntityTranslation(){}

//    public EntityTranslation(EntityTranslationId<TId>id, string text, string abbreviatedText, string description):base(id)
//    {
//        Text = text;
//        AbbreviatedText = abbreviatedText;
//        Description = description;
//    }
//}

//[Serializable]
//public class EntityTranslationId<TId> 
//{
//    public virtual TId EntityId { get; set; }
//    public virtual string LanguageId { get; set; }

//    protected EntityTranslationId() { }

//    public EntityTranslationId(TId entityId, string languageId)
//    {
//        EntityId = entityId;
//        LanguageId = languageId;
//    }

//    public override bool Equals(object obj)
//    {
//        if (obj == null)
//            return false;
//        var id = (EntityTranslationId<TId>)obj;
//        if (id == null)
//            return false;
//        return EntityId.Equals(id.EntityId) && LanguageId == id.LanguageId;
//    }

//    public override int GetHashCode() => HashCode.Combine(this.EntityId, this.LanguageId);
//    public override string ToString()
//    {
//        return $"{LanguageId}:{EntityId}";
//    }
//}

////[Serializable]
////public class EntityTranslationIdentifier
////{
////    public virtual int EntityId { get; set; }
////    public virtual string LanguageId { get; set; }
////    protected EntityTranslationIdentifier() { }

////    public EntityTranslationIdentifier(int entityId, string languageId)
////    {
////        EntityId = entityId;
////        LanguageId = languageId;
////    }

////    public override bool Equals(object obj)
////    {
////        if (obj == null)
////            return false;
////        var id = (EntityTranslationIdentifier)obj;
////        if (id == null)
////            return false;
////        return EntityId == id.EntityId && LanguageId == id.LanguageId;
////    }

////    public override int GetHashCode() => HashCode.Combine(this.EntityId, this.LanguageId);
////    public override string ToString()
////    {
////        return $"{LanguageId}:{EntityId}";
////    }
////}

////[Serializable]
////public class EntityTranslationStringIdentifier
////{
////    public virtual string EntityId { get; set; }
////    public virtual string LanguageId { get; set; }
////    protected EntityTranslationStringIdentifier() { }

////    public EntityTranslationStringIdentifier(string entityId, string languageId)
////    {
////        EntityId = entityId;
////        LanguageId = languageId;
////    }

////    public override bool Equals(object obj)
////    {
////        if (obj == null)
////            return false;
////        var id = (EntityTranslationStringIdentifier)obj;
////        if (id == null)
////            return false;
////        return EntityId == id.EntityId && LanguageId == id.LanguageId;
////    }

////    public override int GetHashCode() => HashCode.Combine(this.EntityId, this.LanguageId);

////    public override string ToString()
////    {
////        return $"{LanguageId}:{EntityId}";
////    }
////}