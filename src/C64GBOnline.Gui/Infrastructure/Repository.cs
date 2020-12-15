using System;
using System.Collections.Generic;
using LiteDB;

namespace C64GBOnline.Gui.Infrastructure
{
    public sealed class Repository<T> : IDisposable
    {
        private readonly object _padLock = new();
        private readonly string _repositoryFullName;
        private ILiteCollection<T>? _liteCollection;
        private ILiteDatabase? _liteDatabase;

        public Repository(string repositoryFullName) => _repositoryFullName = repositoryFullName;

        public void Dispose()
        {
            lock (_padLock)
            {
                _liteDatabase?.Dispose();
            }
        }

        public void Initialize()
        {
            _liteDatabase = new LiteDatabase(_repositoryFullName);
            _liteCollection = _liteDatabase.GetCollection<T>();
        }

        public IEnumerable<T> FindAll()
        {
            if (_liteCollection is null) throw new InvalidOperationException("Not initialized");
            IEnumerable<T> result;
            lock (_padLock)
            {
                result = _liteCollection.FindAll();
            }

            return result;
        }

        public void Upsert(T item)
        {
            if (_liteCollection is null) throw new InvalidOperationException("Not initialized");
            lock (_padLock)
            {
                _liteCollection.Upsert(item);
            }
        }
    }
}