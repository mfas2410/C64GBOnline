namespace C64GBOnline.Application.Abstractions;

public interface IRepository<T>
{
    Task Delete(T item);
    Task Delete(object id);
    Task Flush();
    IEnumerable<T> Get();
    T? Get(object id);
    Task Initialize(Expression<Func<T, object>> idColumn);
    Task Upsert(T item);
}
