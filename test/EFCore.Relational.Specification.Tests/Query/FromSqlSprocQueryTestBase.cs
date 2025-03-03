﻿// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.TestModels.Northwind;
using Microsoft.EntityFrameworkCore.TestUtilities;
using Xunit;

// ReSharper disable InconsistentNaming
namespace Microsoft.EntityFrameworkCore.Query
{
    public abstract class FromSqlSprocQueryTestBase<TFixture> : IClassFixture<TFixture>
        where TFixture : NorthwindQueryRelationalFixture<NoopModelCustomizer>, new()
    {
        protected FromSqlSprocQueryTestBase(TFixture fixture) => Fixture = fixture;

        protected TFixture Fixture { get; }

        [ConditionalTheory]
        [InlineData(false)]
        [InlineData(true)]
        public virtual async Task From_sql_queryable_stored_procedure(bool async)
        {
            using (var context = CreateContext())
            {
                var query = context
                    .Set<MostExpensiveProduct>()
                    .FromSqlRaw(TenMostExpensiveProductsSproc, GetTenMostExpensiveProductsParameters());

                var actual = async
                    ? await query.ToArrayAsync()
                    : query.ToArray();

                Assert.Equal(10, actual.Length);

                Assert.True(
                    actual.Any(
                        mep =>
                            mep.TenMostExpensiveProducts == "Côte de Blaye"
                            && mep.UnitPrice == 263.50m));
            }
        }

        [ConditionalTheory]
        [InlineData(false)]
        [InlineData(true)]
        public virtual async Task From_sql_queryable_stored_procedure_projection(bool async)
        {
            using (var context = CreateContext())
            {
                var query = context
                    .Set<MostExpensiveProduct>()
                    .FromSqlRaw(TenMostExpensiveProductsSproc, GetTenMostExpensiveProductsParameters())
                    .Select(mep => mep.TenMostExpensiveProducts);

                var actual = async
                    ? await query.ToArrayAsync()
                    : query.ToArray();

                Assert.Equal(10, actual.Length);
                Assert.True(actual.Any(r => r == "Côte de Blaye"));
            }
        }

        [ConditionalTheory]
        [InlineData(false)]
        [InlineData(true)]
        public virtual async Task<Exception> From_sql_queryable_stored_procedure_re_projection(bool async)
        {
            using (var context = CreateContext())
            {
                var query = context
                    .Set<MostExpensiveProduct>()
                    .FromSqlRaw(TenMostExpensiveProductsSproc, GetTenMostExpensiveProductsParameters())
                    .Select(
                        mep =>
                            new MostExpensiveProduct { TenMostExpensiveProducts = "Foo", UnitPrice = mep.UnitPrice });
                try
                {
                    var _ = async
                        ? await query.ToArrayAsync()
                        : query.ToArray();

                    Assert.True(false);
                    return null;
                }
                catch (Exception e)
                {
                    return e;
                }
            }
        }

        [ConditionalTheory]
        [InlineData(false)]
        [InlineData(true)]
        public virtual async Task From_sql_queryable_stored_procedure_re_projection_on_client(bool async)
        {
            using (var context = CreateContext())
            {
                var query = context
                    .Set<MostExpensiveProduct>()
                    .FromSqlRaw(TenMostExpensiveProductsSproc, GetTenMostExpensiveProductsParameters());

                var actual = (async ? await query.ToListAsync() : query.ToList())
                    .Select(
                        mep =>
                            new MostExpensiveProduct { TenMostExpensiveProducts = "Foo", UnitPrice = mep.UnitPrice }).ToArray();

                Assert.Equal(10, actual.Length);
                Assert.True(actual.All(mep => mep.TenMostExpensiveProducts == "Foo"));
            }
        }

        [ConditionalTheory]
        [InlineData(false)]
        [InlineData(true)]
        public virtual async Task From_sql_queryable_stored_procedure_with_parameter(bool async)
        {
            using (var context = CreateContext())
            {
                var query = context
                    .Set<CustomerOrderHistory>()
                    .FromSqlRaw(CustomerOrderHistorySproc, GetCustomerOrderHistorySprocParameters());

                var actual = async
                    ? query.ToArray()
                    : await query.ToArrayAsync();

                Assert.Equal(11, actual.Length);

                Assert.True(
                    actual.Any(
                        coh =>
                            coh.ProductName == "Aniseed Syrup"
                            && coh.Total == 6));
            }
        }

        [ConditionalTheory]
        [InlineData(false)]
        [InlineData(true)]
        public virtual async Task<Exception> From_sql_queryable_stored_procedure_composed(bool async)
        {
            using (var context = CreateContext())
            {
                try
                {
                    var query = context
                        .Set<MostExpensiveProduct>()
                        .FromSqlRaw(TenMostExpensiveProductsSproc, GetTenMostExpensiveProductsParameters())
                        .Where(mep => mep.TenMostExpensiveProducts.Contains("C"))
                        .OrderBy(mep => mep.UnitPrice);

                    var _ = async
                        ? await query.ToArrayAsync()
                        : query.ToArray();

                    Assert.True(false);
                    return null;
                }
                catch (Exception e)
                {
                    return e;
                }
            }
        }

        [ConditionalTheory]
        [InlineData(false)]
        [InlineData(true)]
        public virtual async Task From_sql_queryable_stored_procedure_composed_on_client(bool async)
        {
            using (var context = CreateContext())
            {
                var query = context
                    .Set<MostExpensiveProduct>()
                    .FromSqlRaw(TenMostExpensiveProductsSproc, GetTenMostExpensiveProductsParameters());

                var actual = (async
                        ? await query.ToListAsync()
                        : query.ToList())
                    .Where(mep => mep.TenMostExpensiveProducts.Contains("C"))
                    .OrderBy(mep => mep.UnitPrice)
                    .ToArray();

                Assert.Equal(4, actual.Length);
                Assert.Equal(46.00m, actual.First().UnitPrice);
                Assert.Equal(263.50m, actual.Last().UnitPrice);
            }
        }

        [ConditionalTheory]
        [InlineData(false)]
        [InlineData(true)]
        public virtual async Task<Exception> From_sql_queryable_stored_procedure_with_parameter_composed(bool async)
        {
            using (var context = CreateContext())
            {
                try
                {
                    var query = context
                        .Set<CustomerOrderHistory>()
                        .FromSqlRaw(CustomerOrderHistorySproc, GetCustomerOrderHistorySprocParameters())
                        .Where(coh => coh.ProductName.Contains("C"))
                        .OrderBy(coh => coh.Total);

                    var _ = async
                        ? await query.ToArrayAsync()
                        : query.ToArray();

                    Assert.True(false);
                    return null;
                }
                catch (Exception e)
                {
                    return e;
                }
            }
        }

        [ConditionalTheory]
        [InlineData(false)]
        [InlineData(true)]
        public virtual async Task From_sql_queryable_stored_procedure_with_parameter_composed_on_client(bool async)
        {
            using (var context = CreateContext())
            {
                var query = context
                    .Set<CustomerOrderHistory>()
                    .FromSqlRaw(CustomerOrderHistorySproc, GetCustomerOrderHistorySprocParameters());

                var actual = (async
                        ? await query.ToListAsync()
                        : query.ToList())
                    .Where(coh => coh.ProductName.Contains("C"))
                    .OrderBy(coh => coh.Total)
                    .ToArray();

                Assert.Equal(2, actual.Length);
                Assert.Equal(15, actual.First().Total);
                Assert.Equal(21, actual.Last().Total);
            }
        }

        [ConditionalTheory]
        [InlineData(false)]
        [InlineData(true)]
        public virtual async Task<Exception> From_sql_queryable_stored_procedure_take(bool async)
        {
            using (var context = CreateContext())
            {
                try
                {
                    var query = context
                        .Set<MostExpensiveProduct>()
                        .FromSqlRaw(TenMostExpensiveProductsSproc, GetTenMostExpensiveProductsParameters())
                        .OrderByDescending(mep => mep.UnitPrice)
                        .Take(2);

                    var _ = async
                        ? await query.ToArrayAsync()
                        : query.ToArray();

                    Assert.True(false);
                    return null;
                }
                catch (Exception e)
                {
                    return e;
                }
            }
        }

        [ConditionalTheory]
        [InlineData(false)]
        [InlineData(true)]
        public virtual async Task From_sql_queryable_stored_procedure_take_on_client(bool async)
        {
            using (var context = CreateContext())
            {
                var query = context
                    .Set<MostExpensiveProduct>()
                    .FromSqlRaw(TenMostExpensiveProductsSproc, GetTenMostExpensiveProductsParameters());

                var actual = (async
                        ? await query.ToListAsync()
                        : query.ToList())
                    .OrderByDescending(mep => mep.UnitPrice)
                    .Take(2)
                    .ToArray();

                Assert.Equal(2, actual.Length);
                Assert.Equal(263.50m, actual.First().UnitPrice);
                Assert.Equal(123.79m, actual.Last().UnitPrice);
            }
        }

        [ConditionalTheory]
        [InlineData(false)]
        [InlineData(true)]
        public virtual async Task<Exception> From_sql_queryable_stored_procedure_min(bool async)
        {
            using (var context = CreateContext())
            {
                try
                {
                    var query = context.Set<MostExpensiveProduct>()
                        .FromSqlRaw(TenMostExpensiveProductsSproc, GetTenMostExpensiveProductsParameters());

                    var _ = async
                        ? await query.MinAsync(mep => mep.UnitPrice)
                        : query.Min(mep => mep.UnitPrice);

                    Assert.True(false);
                    return null;
                }
                catch (Exception e)
                {
                    return e;
                }
            }
        }

        [ConditionalTheory]
        [InlineData(false)]
        [InlineData(true)]
        public virtual async Task From_sql_queryable_stored_procedure_min_on_client(bool async)
        {
            using (var context = CreateContext())
            {
                var query = context.Set<MostExpensiveProduct>()
                    .FromSqlRaw(TenMostExpensiveProductsSproc, GetTenMostExpensiveProductsParameters());

                Assert.Equal(
                    45.60m,
                    (async
                        ? await query.ToListAsync()
                        : query.ToList())
                    .Min(mep => mep.UnitPrice));
            }
        }

        [ConditionalTheory]
        [InlineData(false)]
        [InlineData(true)]
        public virtual async Task<Exception> From_sql_queryable_stored_procedure_with_include_throws(bool async)
        {
            using (var context = CreateContext())
            {
                try
                {
                    var query = context.Set<Product>()
                        .FromSqlRaw("SelectStoredProcedure")
                        .Include(p => p.OrderDetails);

                    var _ = async
                        ? await query.ToArrayAsync()
                        : query.ToArray();

                    Assert.True(false);
                    return null;
                }
                catch (Exception e)
                {
                    return e;
                }
            }
        }

        [ConditionalTheory]
        [InlineData(false)]
        [InlineData(true)]
        public virtual async Task<Exception> From_sql_queryable_with_multiple_stored_procedures(bool async)
        {
            using (var context = CreateContext())
            {
                var query = from a in context.Set<MostExpensiveProduct>()
                                .FromSqlRaw(TenMostExpensiveProductsSproc, GetTenMostExpensiveProductsParameters())
                            from b in context.Set<MostExpensiveProduct>()
                                .FromSqlRaw(TenMostExpensiveProductsSproc, GetTenMostExpensiveProductsParameters())
                            where a.TenMostExpensiveProducts == b.TenMostExpensiveProducts
                            select new { a, b };

                try
                {
                    var _ = async
                        ? await query.ToArrayAsync()
                        : query.ToArray();

                    Assert.True(false);
                    return null;
                }
                catch (Exception e)
                {
                    return e;
                }
            }
        }

        [ConditionalTheory]
        [InlineData(false)]
        [InlineData(true)]
        public virtual async Task From_sql_queryable_with_multiple_stored_procedures_on_client(bool async)
        {
            using (var context = CreateContext())
            {
                var query1 = context.Set<MostExpensiveProduct>()
                    .FromSqlRaw(TenMostExpensiveProductsSproc, GetTenMostExpensiveProductsParameters());

                var query2 = context.Set<MostExpensiveProduct>()
                    .FromSqlRaw(TenMostExpensiveProductsSproc, GetTenMostExpensiveProductsParameters());

                var results1 = async ? await query1.ToListAsync() : query1.ToList();
                var results2 = (async ? await query2.ToListAsync() : query2.ToList());

                var actual = (from a in results1
                              from b in results2
                              where a.TenMostExpensiveProducts == b.TenMostExpensiveProducts
                              select new { a, b }).ToArray();

                Assert.Equal(10, actual.Length);
            }
        }

        [ConditionalTheory]
        [InlineData(false)]
        [InlineData(true)]
        public virtual async Task<Exception> From_sql_queryable_stored_procedure_and_select(bool async)
        {
            using (var context = CreateContext())
            {
                var query = from mep in context.Set<MostExpensiveProduct>()
                                .FromSqlRaw(TenMostExpensiveProductsSproc, GetTenMostExpensiveProductsParameters())
                            from p in context.Set<Product>()
                                .FromSqlRaw(NormalizeDelimetersInRawString("SELECT * FROM [Products]"))
                            where mep.TenMostExpensiveProducts == p.ProductName
                            select new { mep, p };

                try
                {
                    var _ = async
                        ? await query.ToArrayAsync()
                        : query.ToArray();

                    Assert.True(false);
                    return null;
                }
                catch (Exception e)
                {
                    return e;
                }
            }
        }

        [ConditionalTheory]
        [InlineData(false)]
        [InlineData(true)]
        public virtual async Task From_sql_queryable_stored_procedure_and_select_on_client(bool async)
        {
            using (var context = CreateContext())
            {
                var query1 = context.Set<MostExpensiveProduct>()
                    .FromSqlRaw(TenMostExpensiveProductsSproc, GetTenMostExpensiveProductsParameters());
                var query2 = context.Set<Product>()
                    .FromSqlRaw(NormalizeDelimetersInRawString("SELECT * FROM [Products]"));

                var results1 = async ? await query1.ToListAsync() : query1.ToList();
                var results2 = async ? await query2.ToListAsync() : query2.ToList();

                var actual = (from mep in results1
                              from p in results2
                              where mep.TenMostExpensiveProducts == p.ProductName
                              select new { mep, p }).ToArray();

                Assert.Equal(10, actual.Length);
            }
        }

        [ConditionalTheory]
        [InlineData(false)]
        [InlineData(true)]
        public virtual async Task<Exception> From_sql_queryable_select_and_stored_procedure(bool async)
        {
            using (var context = CreateContext())
            {
                var query = from p in context.Set<Product>().FromSqlRaw(NormalizeDelimetersInRawString("SELECT * FROM [Products]"))
                            from mep in context.Set<MostExpensiveProduct>()
                                .FromSqlRaw(TenMostExpensiveProductsSproc, GetTenMostExpensiveProductsParameters())
                            where mep.TenMostExpensiveProducts == p.ProductName
                            select new { mep, p };

                try
                {
                    var _ = async
                        ? await query.ToArrayAsync()
                        : query.ToArray();

                    Assert.True(false);
                    return null;
                }
                catch (Exception e)
                {
                    return e;
                }
            }
        }

        [ConditionalTheory]
        [InlineData(false)]
        [InlineData(true)]
        public virtual async Task From_sql_queryable_select_and_stored_procedure_on_client(bool async)
        {
            using (var context = CreateContext())
            {
                var query1 = context.Set<Product>()
                    .FromSqlRaw(NormalizeDelimetersInRawString("SELECT * FROM [Products]"));
                var query2 = context.Set<MostExpensiveProduct>()
                    .FromSqlRaw(TenMostExpensiveProductsSproc, GetTenMostExpensiveProductsParameters());

                var results1 = async ? await query1.ToListAsync() : query1.ToList();
                var results2 = async ? await query2.ToListAsync() : query2.ToList();

                var actual = (from p in results1
                              from mep in results2
                              where mep.TenMostExpensiveProducts == p.ProductName
                              select new { mep, p }).ToArray();

                Assert.Equal(10, actual.Length);
            }
        }

        private string NormalizeDelimetersInRawString(string sql)
            => Fixture.TestStore.NormalizeDelimetersInRawString(sql);

        protected virtual object[] GetTenMostExpensiveProductsParameters()
            => Array.Empty<object>();

        protected virtual object[] GetCustomerOrderHistorySprocParameters()
            => new[] { "ALFKI" };

        protected NorthwindContext CreateContext() => Fixture.CreateContext();

        protected abstract string TenMostExpensiveProductsSproc { get; }

        protected abstract string CustomerOrderHistorySproc { get; }
    }
}
