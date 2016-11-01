﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Baseline;
using Marten.Linq;
using Marten.Testing.Documents;
using Shouldly;
using Xunit;

namespace Marten.Testing.Linq
{
    public class query_with_select_many : IntegratedFixture
    {
        [Fact]
        public void can_do_simple_select_many_against_simple_array()
        {
            var product1 = new Product {Tags = new[] {"a", "b", "c"}};
            var product2 = new Product {Tags = new[] {"b", "c", "d"}};
            var product3 = new Product {Tags = new[] {"d", "e", "f"}};

            using (var session = theStore.OpenSession())
            {
                session.Store(product1, product2, product3);
                session.SaveChanges();
            }

            using (var query = theStore.QuerySession())
            {
                var distinct = query.Query<Product>().SelectMany(x => x.Tags).Distinct().ToList();

                distinct.OrderBy(x => x).ShouldHaveTheSameElementsAs("a", "b", "c", "d", "e", "f");

                var names = query.Query<Product>().SelectMany(x => x.Tags).ToList();
                names
                    .Count().ShouldBe(9);
            }
        }



        [Fact]
        public void select_many_against_complex_type_with_count()
        {
            var product1 = new Product {Tags = new[] {"a", "b", "c"}};
            var product2 = new Product {Tags = new[] {"b", "c", "d"}};
            var product3 = new Product {Tags = new[] {"d", "e", "f"}};

            using (var session = theStore.OpenSession())
            {
                session.Store(product1, product2, product3);
                session.SaveChanges();
            }

            using (var query = theStore.QuerySession())
            {
                query.Query<Product>().SelectMany(x => x.Tags)
                    .Count().ShouldBe(9);
            }
        }

        [Fact]
        public async Task select_many_against_complex_type_with_count_async()
        {
            var product1 = new Product {Tags = new[] {"a", "b", "c"}};
            var product2 = new Product {Tags = new[] {"b", "c", "d"}};
            var product3 = new Product {Tags = new[] {"d", "e", "f"}};

            using (var session = theStore.OpenSession())
            {
                session.Store(product1, product2, product3);
                await session.SaveChangesAsync();
            }

            using (var query = theStore.QuerySession())
            {
                (await query.Query<Product>().SelectMany(x => x.Tags)
                    .CountAsync()).ShouldBe(9);
            }
        }

        [Fact]
        public void select_many_against_complex_type_without_transformation()
        {
            var targets = Target.GenerateRandomData(10).ToArray();
            var expectedCount = targets.SelectMany(x => x.Children).Count();

            expectedCount.ShouldBeGreaterThan(0);


            using (var session = theStore.OpenSession())
            {
                session.Store(targets);
                session.SaveChanges();
            }

            using (var query = theStore.QuerySession())
            {
                var list = query.Query<Target>().SelectMany(x => x.Children).ToList();
                list.Count.ShouldBe(expectedCount);
            }
        }

        [Fact]
        public void select_many_against_integer_array()
        {
            var product1 = new ProductWithNumbers {Tags = new[] {1, 2, 3}};
            var product2 = new ProductWithNumbers {Tags = new[] {2, 3, 4}};
            var product3 = new ProductWithNumbers {Tags = new[] {3, 4, 5}};

            using (var session = theStore.OpenSession())
            {
                session.Store(product1, product2, product3);
                session.SaveChanges();
            }


            using (var query = theStore.QuerySession())
            {
                var distinct = query.Query<ProductWithNumbers>().SelectMany(x => x.Tags).Distinct().ToList();

                distinct.OrderBy(x => x).ShouldHaveTheSameElementsAs(1, 2, 3, 4, 5);

                var names = query.Query<ProductWithNumbers>().SelectMany(x => x.Tags).ToList();
                names
                    .Count().ShouldBe(9);
            }
        }

        [Fact]
        public async Task select_many_against_integer_array_async()
        {
            var product1 = new ProductWithNumbers {Tags = new[] {1, 2, 3}};
            var product2 = new ProductWithNumbers {Tags = new[] {2, 3, 4}};
            var product3 = new ProductWithNumbers {Tags = new[] {3, 4, 5}};

            using (var session = theStore.OpenSession())
            {
                session.Store(product1, product2, product3);
                await session.SaveChangesAsync();
            }


            using (var query = theStore.QuerySession())
            {
                var distinct = await query.Query<ProductWithNumbers>().SelectMany(x => x.Tags).Distinct().ToListAsync();

                distinct.OrderBy(x => x).ShouldHaveTheSameElementsAs(1, 2, 3, 4, 5);

                var names = query.Query<ProductWithNumbers>().SelectMany(x => x.Tags).ToList();
                names
                    .Count().ShouldBe(9);
            }
        }

        [Fact]
        public void select_many_with_any()
        {
            var product1 = new Product {Tags = new[] {"a", "b", "c"}};
            var product2 = new Product {Tags = new[] {"b", "c", "d"}};
            var product3 = new Product {Tags = new[] {"d", "e", "f"}};

            using (var session = theStore.OpenSession())
            {
                session.Store(product1, product2, product3);

                // Some Target docs w/ no children
                session.Store(Target.Random(), Target.Random(), Target.Random());

                session.SaveChanges();
            }

            using (var query = theStore.QuerySession())
            {
                query.Query<Product>().SelectMany(x => x.Tags)
                    .Any().ShouldBeTrue();

                query.Query<Target>().SelectMany(x => x.Children)
                    .Any().ShouldBeFalse();
            }
        }

        [Fact]
        public async Task select_many_with_any_async()
        {
            var product1 = new Product {Tags = new[] {"a", "b", "c"}};
            var product2 = new Product {Tags = new[] {"b", "c", "d"}};
            var product3 = new Product {Tags = new[] {"d", "e", "f"}};

            using (var session = theStore.OpenSession())
            {
                session.Store(product1, product2, product3);

                // Some Target docs w/ no children
                session.Store(Target.Random(), Target.Random(), Target.Random());

                await session.SaveChangesAsync().ConfigureAwait(false);
            }

            using (var query = theStore.QuerySession())
            {
                (await query.Query<Product>().SelectMany(x => x.Tags)
                    .AnyAsync()).ShouldBeTrue();

                (await query.Query<Target>().SelectMany(x => x.Children)
                    .AnyAsync()).ShouldBeFalse();
            }
        }


        [Fact]
        public void select_many_with_chained_where()
        {
            var targets = Target.GenerateRandomData(1000).ToArray();
            theStore.BulkInsert(targets);

            using (var query = theStore.QuerySession())
            {
                var expected = targets.SelectMany(x => x.Children).Where(x => x.Flag).Select(x => x.Id).OrderBy(x => x).ToList();
                expected.Any().ShouldBeTrue();

                var results = query.Query<Target>().SelectMany(x => x.Children).Where(x => x.Flag).ToList();

                results.Select(x => x.Id).OrderBy(x => x).ShouldHaveTheSameElementsAs(expected);
            }
        }

        [Fact]
        public void select_many_with_chained_where_and_order()
        {
            var targets = Target.GenerateRandomData(1000).ToArray();
            theStore.BulkInsert(targets);

            using (var query = theStore.QuerySession())
            {
                var expected = targets.SelectMany(x => x.Children).Where(x => x.Flag).Select(x => x.Id).OrderBy(x => x).ToList();
                expected.Any().ShouldBeTrue();

                var results = query.Query<Target>().SelectMany(x => x.Children).Where(x => x.Flag).OrderBy(x => x.Id).ToList();

                results.Select(x => x.Id).ShouldHaveTheSameElementsAs(expected);
            }
        }

        [Fact]
        public void select_many_with_chained_where_and_order_and_skip_and_take()
        {
            var targets = Target.GenerateRandomData(1000).ToArray();
            theStore.BulkInsert(targets);

            using (var query = theStore.QuerySession())
            {
                var expected = targets
                    .SelectMany(x => x.Children)
                    .Where(x => x.Flag)
                    .OrderBy(x => x.Id)
                    .Skip(20)
                    .Take(15)
                    .Select(x => x.Id)
                    .ToList();


                expected.Any().ShouldBeTrue();

                var results = query.Query<Target>()
                    .SelectMany(x => x.Children)
                    .Where(x => x.Flag)
                    .OrderBy(x => x.Id)
                    .Skip(20)
                    .Take(15)
                    .ToList();

                results.Select(x => x.Id).ShouldHaveTheSameElementsAs(expected);
            }
        }

        [Fact]
        public void select_many_with_stats()
        {
            var targets = Target.GenerateRandomData(1000).ToArray();
            theStore.BulkInsert(targets);

            using (var query = theStore.QuerySession())
            {
                QueryStatistics stats = null;

                var actual = query.Query<Target>()
                    .Stats(out stats)
                    .SelectMany(x => x.Children)
                    .Where(x => x.Flag)
                    .OrderBy(x => x.Id)
                    .Take(10).ToList();

                var expectedCount = targets
                    .SelectMany(x => x.Children)
                    .Where(x => x.Flag)
                    .OrderBy(x => x.Id).LongCount();

                stats.TotalResults.ShouldBe(expectedCount);
            }
        }

        [Fact]
        public void select_many_with_includes()
        {
            var user1 = new User();
            var user2 = new User();
            var user3 = new User();

            theStore.BulkInsert(new [] {user1, user2, user3});

            var targets = Target.GenerateRandomData(1000).ToArray();

            foreach (var target in targets)
            {
                if (target.Children.Any())
                {
                    target.Children[0].UserId = user1.Id;
                }

                if (target.Children.Length >= 2)
                {
                    target.Children[1].UserId = user2.Id;
                }
            }

            theStore.BulkInsert(targets);



            using (var query = theStore.QuerySession())
            {
                var dict = new Dictionary<Guid, User>();

                var results = query.Query<Target>()
                    .SelectMany(x => x.Children)
                    .Include(x => x.UserId, dict)
                    .ToList();

                dict.Count.ShouldBe(2);

                dict.ContainsKey(user1.Id).ShouldBeTrue();
                dict.ContainsKey(user2.Id).ShouldBeTrue();

            }
        }

        [Fact]
        public void select_many_with_select_transformation()
        {
            var targets = Target.GenerateRandomData(100).ToArray();
            theStore.BulkInsert(targets);

            using (var query = theStore.QuerySession())
            {
                var actual = query.Query<Target>()
                    .SelectMany(x => x.Children)
                    .Where(x => x.Color == Colors.Green)
                    .Select(x => new {Id = x.Id, Shade = x.Color})
                    .ToList();

                var expected = targets
                    .SelectMany(x => x.Children).Count(x => x.Color == Colors.Green);

                actual.Count.ShouldBe(expected);

                actual.Each(x => x.Shade.ShouldBe(Colors.Green));
            }
        }


        [Fact]
        public void select_many_with_select_and_as_json()
        {
            var targets = Target.GenerateRandomData(100).ToArray();
            theStore.BulkInsert(targets);

            using (var query = theStore.QuerySession())
            {
                var actual = query.Query<Target>()
                    .SelectMany(x => x.Children)
                    .Where(x => x.Color == Colors.Green)
                    .Select(x => new { Id = x.Id, Shade = x.Color })
                    .AsJson()
                    .ToList();

                var expected = targets
                    .SelectMany(x => x.Children).Count(x => x.Color == Colors.Green);

                actual.Count.ShouldBe(expected);
            }
        }
    }

    public class Product
    {
        public Guid Id;
        public string[] Tags { get; set; }
    }

    public class ProductWithNumbers
    {
        public Guid Id;
        public int[] Tags { get; set; }
    }
}