namespace C64GBOnline.Infrastructure;

public sealed class JsonRepository<T> : IRepository<T>
{
    private readonly Dictionary<object, T> _dictionary = new();
    private readonly string _repositoryFullName;
    private PropertyInfo? _idColumn;
    private bool _isInitialized;

    public JsonRepository(IOptions<LocalOptions> options)
        => _repositoryFullName = Path.Combine(options.Value.Directory, "cache.json");

    public async Task Delete(T item)
    {
        if (!_isInitialized) throw new InvalidOperationException("Not initialized");
        object? key = _idColumn!.GetValue(item);
        if (key is null) throw new ArgumentException("Unable to retreive key", nameof(item));
        await Delete(key);
    }

    public async Task Delete(object id)
    {
        if (!_isInitialized) throw new InvalidOperationException("Not initialized");
        _dictionary.Remove(id);
        await Flush();
    }

    public async Task Flush()
    {
        if (!_isInitialized) throw new InvalidOperationException("Not initialized");
        await using FileStream fileStream = new(_repositoryFullName, FileMode.Create, FileAccess.Write, FileShare.Write, 4096, FileOptions.Asynchronous);
        await JsonSerializer.SerializeAsync(fileStream, _dictionary.Values.ToList(), new JsonSerializerOptions { WriteIndented = true });
    }

    public IEnumerable<T> Get()
    {
        if (!_isInitialized) throw new InvalidOperationException("Not initialized");
        return _dictionary.Values;
    }

    public T? Get(object id)
    {
        if (!_isInitialized) throw new InvalidOperationException("Not initialized");
        _dictionary.TryGetValue(id, out T? item);
        return item;
    }

    public async Task Initialize(Expression<Func<T, object>> idColumn)
    {
        if (_isInitialized) return;

        try
        {
            _idColumn = GetPropertyFromExpression(idColumn);
            await using FileStream fileStream = new(_repositoryFullName, FileMode.Open, FileAccess.Read, FileShare.Read, 4096, FileOptions.Asynchronous | FileOptions.SequentialScan);
            List<T>? _list = await JsonSerializer.DeserializeAsync<List<T>>(fileStream) ?? new List<T>();
            foreach (T item in _list)
            {
                object? key = _idColumn.GetValue(item);
                if (key is not null) _dictionary.TryAdd(key, item);
            }
        }
        catch { }
        finally
        {
            _isInitialized = true;
        }
    }

    public async Task Upsert(T item)
    {
        if (!_isInitialized) throw new InvalidOperationException("Not initialized");
        object? key = _idColumn!.GetValue(item);
        if (key is null) throw new ArgumentException("Unable to retreive key", nameof(item));
        _dictionary[key] = item;
        await Flush();
    }

    private static PropertyInfo GetPropertyFromExpression(Expression<Func<T, object>> propertyLambda)
    {
        MemberExpression? memberExpression = propertyLambda.Body is UnaryExpression unaryExpression
            ? unaryExpression.Operand is MemberExpression expression1 ? expression1 : throw new InvalidCastException(nameof(propertyLambda))
            : propertyLambda.Body is MemberExpression expression2
                ? expression2
                : throw new InvalidCastException(nameof(propertyLambda));
        return (PropertyInfo)memberExpression.Member;
    }
}
