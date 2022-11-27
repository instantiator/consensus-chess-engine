using System;
using ConsensusChessShared.DTO;
using Microsoft.EntityFrameworkCore;
using Moq;

namespace ConsensusChessSharedTests.Data
{
	public class MockDbHelper
	{
        public static DbSet<T> GetQueryableMockDbSet<T>(string name) where T : class, IDTO
        {
            var list = new List<T>();
            return GetQueryableMockDbSet<T>(list);
        }

        public static DbSet<T> GetQueryableMockDbSet<T>(List<T> sourceList) where T : class, IDTO
        {
            var queryable = sourceList.AsQueryable();

            var dbSet = new Mock<DbSet<T>>();
            dbSet.As<IQueryable<T>>().Setup(m => m.Provider).Returns(queryable.Provider);
            dbSet.As<IQueryable<T>>().Setup(m => m.Expression).Returns(queryable.Expression);
            dbSet.As<IQueryable<T>>().Setup(m => m.ElementType).Returns(queryable.ElementType);
            dbSet.As<IQueryable<T>>().Setup(m => m.GetEnumerator()).Returns(() => queryable.GetEnumerator());
            dbSet.Setup(d => d.Add(It.IsAny<T>())).Callback<T>((s) => sourceList.Add(s));

            return dbSet.Object;
        }
    }
}

