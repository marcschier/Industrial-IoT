// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.Azure.IIoT.Services.LiteDb.Clients {
    using Microsoft.Azure.IIoT.Serializers.NewtonSoft;
    using Microsoft.Azure.IIoT.Serializers;
    using LiteDB;
    using System;
    using System.Reflection;
    using System.Runtime.Serialization;
    using System.Linq.Expressions;
    using System.Collections.Generic;
    using System.Linq;

    /// <summary>
    /// Provides document db and graph functionality for storage interfaces.
    /// </summary>
    internal static class DocumentSerializer {

        /// <summary>
        /// Mapper instance
        /// </summary>
        internal static BsonMapper Mapper { get; } = CreateMapper();

        /// <summary>
        /// Register type in mapper
        /// </summary>
        /// <typeparam name="T"></typeparam>
        internal static void Register<T>() {
            var dca = typeof(T).GetCustomAttribute<DataContractAttribute>();
            if (dca == null) {
                return; // Poco
            }
            // Map data contract object
            var builder = Mapper.Entity<T>();
            var field = typeof(EntityBuilder<T>).GetMethod("Field");
            var id = typeof(EntityBuilder<T>).GetMethod("Id");
            foreach (var prop in typeof(T).GetProperties()) {
                var dma = prop.GetCustomAttribute<DataMemberAttribute>(true);
                if (dma == null) {
                    continue;
                }
                var paramex = Expression.Parameter(typeof(T));
                var expr = Expression.Lambda(Expression.Property(paramex, prop), paramex);
                if (dma.Name == "id") { // Cosmos db convention - use attribute going forward
                    // Create id accessor
                    builder = (EntityBuilder<T>)id.MakeGenericMethod(prop.PropertyType)
                        .Invoke(builder, new object[] { expr, /*assign auto id=*/ true });
                }
                else {
                    // Create regular field property accessor
                    builder = (EntityBuilder<T>)field.MakeGenericMethod(prop.PropertyType)
                        .Invoke(builder, new object[] { expr, dma.Name ?? prop.Name });
                }
            }
        }

        /// <summary>
        /// Create default mapper
        /// </summary>
        /// <returns></returns>
        private static BsonMapper CreateMapper() {
            var mapper = new BsonMapper();
            var serializer = new NewtonSoftJsonSerializer();

            // Override default time type handling
            mapper.RegisterType(
                ts => ts.HasValue ? (BsonValue)ts.Value.Ticks :
                    BsonValue.Null,
                bs => bs.IsNull ? (TimeSpan?)null :
                    TimeSpan.FromTicks(bs.AsInt64));
            mapper.RegisterType(
                ts => ts.Ticks,
                bs => TimeSpan.FromTicks(bs.AsInt64));
            mapper.RegisterType(
                dt => dt.HasValue ? (BsonValue)dt.Value.ToUnixTimeMilliseconds() :
                    BsonValue.Null,
                bs => bs.IsNull ? (DateTimeOffset?)null :
                    DateTimeOffset.FromUnixTimeMilliseconds(bs.AsInt64));
            mapper.RegisterType(
                dt => dt.ToUnixTimeMilliseconds(),
                bs => DateTimeOffset.FromUnixTimeMilliseconds(bs.AsInt64));
            mapper.RegisterType(
                dt => dt.HasValue ? (BsonValue)dt.Value.ToBinary() :
                    BsonValue.Null,
                bs => bs.IsNull ? (DateTime?)null :
                    DateTime.FromBinary(bs.AsInt64));
            mapper.RegisterType(
                dt => dt.ToBinary(),
                bs => DateTime.FromBinary(bs.AsInt64));

            mapper.RegisterType(
                vv => vv.IsNull() ? BsonValue.Null :
                    (BsonValue)serializer.SerializeToBytes(vv).ToArray(),
                bs => bs.IsNull ? VariantValue.Null :
                    serializer.Parse(bs.AsBinary.AsMemory()));

            mapper.RegisterType<IReadOnlyCollection<byte>>(
                b => b is byte[] binary ? binary : (b?.ToArray() ?? BsonValue.Null),
                bs => bs.AsBinary);

            return mapper;
        }
    }
}
