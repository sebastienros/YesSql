using System;
using System.Collections.Generic;
using System.Linq;

namespace YesSql.Indexes
{
    public class DescribeContext<T> : IDescriptor
    {
        private readonly Dictionary<Type, List<IDescribeFor>> _describes = [];

        public IEnumerable<IndexDescriptor> Describe(params Type[] types)
        {
            return _describes
                .Where(kp => types == null || types.Length == 0 || types.Contains(kp.Key))
                .SelectMany(x => x.Value)
                .Select(kp => new IndexDescriptor
                {
                    Type = kp.IndexType,
                    Map = kp.GetMap(),
                    Reduce = kp.GetReduce(),
                    Delete = kp.GetDelete(),
                    GroupKey = kp.GroupProperty,
                    IndexType = kp.IndexType,
                    Filter = kp.Filter
                });
        }

        public IMapFor<T, IIndex> For(Type indexType) => For<IIndex, object>(indexType);

        public IMapFor<T, TIndex> For<TIndex>() where TIndex : IIndex
        {
            return For<TIndex, object>(typeof(TIndex));
        }

        public IMapFor<T, TIndex> For<TIndex, TKey>() where TIndex : IIndex => For<TIndex, object>(typeof(TIndex));

        public IMapFor<T, TIndex> For<TIndex, TKey>(Type indexType) where TIndex : IIndex
        {
            ArgumentNullException.ThrowIfNull(indexType, nameof(indexType));

            List<IDescribeFor> descriptors;

            if (!_describes.TryGetValue(typeof(T), out descriptors))
            {
                descriptors = _describes[typeof(T)] = [];
            }

            var describeFor = new IndexDescriptor<T, TIndex, TKey>()
            {
                IndexType = indexType,
            };

            descriptors.Add(describeFor);

            return describeFor;
        }
    }
}
