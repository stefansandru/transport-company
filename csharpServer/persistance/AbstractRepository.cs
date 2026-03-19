using System;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using model;

namespace persistance;

public abstract class AbstractRepository<ID, E> : IRepository<ID, E> where ID : IComparable<ID> where E : Entity<ID>
{
    protected DatabaseConnection jdbc;
    protected static readonly ILogger logger = LoggerFactory.Create(builder => builder.AddConsole()).CreateLogger<AbstractRepository<ID, E>>();

    protected AbstractRepository(DatabaseConnection jdbc)
    {
        this.jdbc = jdbc;
    }

    public abstract E? FindById(ID id);

    public abstract IEnumerable<E> FindAll();
    
    public abstract E? Save(E entity);

    public abstract E? Delete(ID id);

    public abstract E? Update(E entity);
}