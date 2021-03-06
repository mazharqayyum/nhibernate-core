﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by AsyncGenerator.
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------


using System.Collections;
using NUnit.Framework;

namespace NHibernate.Test.UserCollection.Parameterized
{
	using System.Threading.Tasks;
	[TestFixture]
	public class ParameterizedUserCollectionTypeFixtureAsync : TestCase
	{
		protected override string MappingsAssembly
		{
			get { return "NHibernate.Test"; }
		}

		protected override string[] Mappings
		{
			get { return new string[] { "UserCollection.Parameterized.Mapping.hbm.xml" }; }
		}

		[Test]
		public async Task BasicOperationAsync()
		{
			using (ISession s = OpenSession())
			using (ITransaction t = s.BeginTransaction())
			{
				var entity = new Entity("tester");
				entity.Values.Add("value-1");
				await (s.PersistAsync(entity));
				await (t.CommitAsync());
			}

			using (var s = OpenSession())
			using (var t = s.BeginTransaction())
			{
				var entity = await (s.GetAsync<Entity>("tester"));
				Assert.IsTrue(NHibernateUtil.IsInitialized(entity.Values));
				Assert.AreEqual(1, entity.Values.Count);
				Assert.AreEqual("Hello", ((IDefaultableList) entity.Values).DefaultValue);

				await (s.DeleteAsync(entity));
				await (t.CommitAsync());
			}
		}
	}
}