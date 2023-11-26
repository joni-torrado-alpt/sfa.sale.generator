using System.Data.Common;
using System.Reflection;
using Microsoft.EntityFrameworkCore;

namespace sfa.sale.generator.core;

public static class DbContextExtensions
{
    public static async Task<List<T>> ExecuteStoredProc<T>(this DbCommand command)
    {
        using (command)
        {
            if (command.Connection.State == System.Data.ConnectionState.Closed)
                command.Connection.Open();
            try
            {
                using (var reader = await command.ExecuteReaderAsync())
                {
                    return reader.MapToList<T>();
                }
            }
            catch (Exception e)
            {
                throw (e);
            }
            finally
            {
                command.Connection.Close();
            }
        }
    }

    public static DbCommand WithSqlParam(this DbCommand cmd, string paramName, object paramValue)
    {
        if (string.IsNullOrEmpty(cmd.CommandText))
            throw new InvalidOperationException(
              "Call LoadStoredProc before using this method");
        var param = cmd.CreateParameter();
        param.ParameterName = paramName;
        param.Value = paramValue;
        cmd.Parameters.Add(param);
        return cmd;
    }

    public static DbCommand LoadStoredProc(this DbContext context, string storedProcName)
    {
        var cmd = context.Database.GetDbConnection().CreateCommand();
        cmd.CommandText = storedProcName;
        cmd.CommandType = System.Data.CommandType.StoredProcedure;
        return cmd;
    }

    public static void SeedEnumTable<TEnum, TEntity>(this ModelBuilder mb)
        where TEnum : Enum, IConvertible
        where TEntity : BaseEntityEnum<TEnum>, new()
    {
        if (!typeof(TEnum).IsEnum) throw new ArgumentException("TEnum must be an enumerated type");
        var itemValues = (TEnum[])Enum.GetValues(typeof(TEnum));
        foreach (var itemValue in itemValues)
        {
            var item = new TEntity();
            item.InitForSeeding(itemValue);
            mb.Entity<TEntity>().HasData(item);
        }
    }

    // public static T DetachForClone<T>(this DbContext db, T entity) where T : class
    // {
    //     db.Entry(entity).State = EntityState.Detached;
    //     SetEntityIdToDefault(entity);

    //     var relatedCollections = typeof(T).GetRelatedListCustomObjectPropertyNames();
    //     SetRelatedTypeIdToDefault(entity, relatedCollections);

    //     return entity;
    // }

    // public static IList<T> DetachForClone<T>(this DbContext db, IList<T> entities) where T : class
    // {
    //     foreach (var entity in entities)
    //         db.DetachForClone(entity);
    //     return entities;
    // }

    /// <summary>
    /// Sets Id Property to default.
    /// TODO: Maybe enforce BaseEntityGeneric here
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="entity"></param>
    public static void SetEntityIdToDefault<T>(T entity) where T : class
    {
        var id = entity.GetType().GetProperty("Id");
        if (id is not null)
            id.SetValue(entity, 0);
    }

    /// <summary>
    /// Sets Id Property to default.
    /// TODO: Maybe enforce BaseEntityGeneric heree
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="entity"></param>
    public static void SetRelatedTypeIdToDefault<T>(T entity, IEnumerable<Type> relatedTypes) where T : class
    {
        foreach (var related in relatedTypes)
            SetRelatedTypeIdToDefault(entity, related);
    }

    /// <summary>
    /// Sets Id Property to default.
    /// TODO: Maybe enforce BaseEntityGeneric heree
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="entity"></param>
    public static void SetRelatedTypeIdToDefault<T>(T entity, IEnumerable<string> relatedCollectionProperties) where T : class
    {
        foreach (var related in relatedCollectionProperties)
            SetRelatedTypeIdToDefault(entity, related);
    }

    /// <summary>
    /// Sets Id Property to default.
    /// TODO: Maybe enforce BaseEntityGeneric heree
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="entity"></param>
    public static void SetRelatedTypeIdToDefault<T>(T entity, Type related)
    {
        var relatedValue = typeof(T).GetProperty(related.Name)?.GetValue(entity);
        if (relatedValue is null) return;

        var relatedValueAsList = relatedValue as System.Collections.IEnumerable;
        if (relatedValueAsList is not null)
        {
            if (!relatedValueAsList.GetEnumerator().MoveNext()) return;
            foreach (var relatedItem in relatedValueAsList)
                SetEntityIdToDefault(relatedItem);
        }
        else if (relatedValue is not null)
        {
            //SetEntityIdToDefault(relatedValue);
        }
    }

    /// <summary>
    /// Sets Id Property to default.
    /// TODO: Maybe enforce BaseEntityGeneric heree
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="entity"></param>
    public static void SetRelatedTypeIdToDefault<T>(T entity, string related)
    {
        var relatedValue = typeof(T).GetProperty(related)?.GetValue(entity);
        if (relatedValue is null) return;

        var relatedValueAsList = relatedValue as System.Collections.IEnumerable;
        if (relatedValueAsList is not null)
        {
            if (!relatedValueAsList.GetEnumerator().MoveNext()) return;
            foreach (var relatedItem in relatedValueAsList)
                SetEntityIdToDefault(relatedItem);
        }
        else if (relatedValue is not null)
        {
            //TODO : This isn't workign 100%. This assumes that ForeignKeyId exists to make the relation.
            typeof(T).GetProperty(related).SetValue(entity, null);
            //SetEntityIdToDefault(relatedValue);
        }
    }

    /// <summary>
    /// Loads all navigations data of the entity to the ChangeTracker to be tracked by EF
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="dataTaskDbContext"></param>
    /// <param name="id"></param>
    /// <returns></returns>
    public static async Task<T> FindWithLoadNavigationsAsync<T>(this DbContext dbContext, object id) where T : class
    {
        var data = await dbContext.Set<T>().FindAsync(id);
        if (data is null) return data;
        var entry = dbContext.Entry(data);
        foreach (var navigation in entry.Navigations)
            await navigation.LoadAsync();
        foreach (var reference in entry.References)
            await reference.LoadAsync();
        return data;
    }

    private static List<T> MapToList<T>(this DbDataReader dr)
    {
        var objList = new List<T>();
        var props = typeof(T).GetRuntimeProperties();

        var colMapping = dr.GetColumnSchema()
          .Where(x => props.Any(y => y.Name.ToLower() == x.ColumnName.ToLower()))
          .ToDictionary(key => key.ColumnName.ToLower());

        if (dr.HasRows)
        {
            while (dr.Read())
            {
                T obj = Activator.CreateInstance<T>();
                foreach (var prop in props)
                {
                    if (colMapping.ContainsKey(prop.Name.ToLower()))
                    {
                        var val = dr.GetValue(colMapping[prop.Name.ToLower()].ColumnOrdinal.Value);
                        prop.SetValue(obj, val == DBNull.Value ? null : val);
                    }
                }
                objList.Add(obj);
            }
        }
        return objList;
    }
}