#region License

/*
 * Copyright � 2002-2006 the original author or authors.
 * 
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 * 
 *      http://www.apache.org/licenses/LICENSE-2.0
 * 
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */

#endregion

using System;
using System.Globalization;
using System.Threading;
using NUnit.Framework;
using Spring.Context;
using Spring.Threading;
using Spring.Util;

namespace Spring.Data.Common
{
    /// <summary>
    /// Test for loading of DbProviders
    /// </summary>
    /// <author>Mark Pollack (.NET)</author>
    [TestFixture]
    public class DbProviderFactoryTests
    {
        #region Helper classes for threading tests

        public class AsyncTestDbProviderFactory : AsyncTestTask
        {
            private string providerName;

            public AsyncTestDbProviderFactory(int iterations, string providerName)
                : base(iterations)
            {
                this.providerName = providerName;
            }

            public override void DoExecute()
            {
                object result = DbProviderFactory.GetDbProvider(providerName);
                Assert.IsNotNull(result);
            }
        }

        #endregion

        private static string altConfig = "assembly://Spring.Data.Tests/Spring.Data.Common/AdditionalProviders.xml";


        [SetUp]
        public void Setup()
        {
            DbProviderFactory.DBPROVIDER_ADDITIONAL_RESOURCE_NAME = altConfig; 
        }

#if NET_2_0   
     
        [Test]
        public void ThreadSafety()
        {
            AsyncTestTask t1 = new AsyncTestDbProviderFactory(1000, "SqlServer-2.0").Start();
            AsyncTestTask t2 = new AsyncTestDbProviderFactory(1000, "SqlServer-2.0").Start();
            AsyncTestTask t3 = new AsyncTestDbProviderFactory(1000, "SqlServer-2.0").Start();
            AsyncTestTask t4 = new AsyncTestDbProviderFactory(1000, "SqlServer-2.0").Start();
            
            t1.AssertNoException();
            t2.AssertNoException();
            t3.AssertNoException();
            t4.AssertNoException();

        }

        [Test]
        [Ignore("Can't guarantee test order")]
        public void AdditionalResourceName()
        {
           
            IDbProvider provider = DbProviderFactory.GetDbProvider("Test-SqlServer-2.0");
            Assert.IsNotNull(provider);
        }

        [Test]
        [Ignore("Can't guarantee test order")]
        public void BadErrorExpression()
        {

            IDbProvider provider = DbProviderFactory.GetDbProvider("Test-SqlServer-2.0-BadErrorCodeExpression");
            Assert.IsNotNull(provider);
            string errorCode = provider.ExtractError(new Exception("foo"));
            Assert.AreEqual("156",errorCode);
        }

        [Test]
        public void DefaultInstanceWithSqlServer2005()
        {
            Thread.CurrentThread.CurrentCulture = new CultureInfo("en-US", false);
            IApplicationContext ctx = DbProviderFactory.ApplicationContext;            
            Assert.IsNotNull(ctx);
            Thread.CurrentThread.CurrentCulture = new CultureInfo("tr-TR", false);
            IDbProvider provider = DbProviderFactory.GetDbProvider("SqlServer-2.0");
            AssertIsSqlServer2005(provider);
            provider = DbProviderFactory.GetDbProvider("System.Data.SqlClient");
            AssertIsSqlServer2005(provider);
            Assert.IsNull(provider.ConnectionString);
            Assert.IsNotNull(provider.CreateCommand());
            Assert.IsNotNull(provider.CreateCommandBuilder());
            Assert.IsNotNull(provider.CreateConnection());
            Assert.IsNotNull(provider.CreateDataAdapter());
            Assert.IsNotNull(provider.CreateParameter());
            Assert.AreEqual("@Foo", provider.CreateParameterName("Foo"));
        }
        
 
        [Test]
        public void DefaultInstanceWithOleDb20()
        {
            IDbProvider provider = DbProviderFactory.GetDbProvider("OleDb-2.0");
            Assert.AreEqual("OleDb, provider V2.0.0.0 in framework .NET V2", provider.DbMetadata.ProductName);
            Assert.IsNotNull(provider.CreateCommand());
            Assert.IsNotNull(provider.CreateCommandBuilder());
            Assert.IsNotNull(provider.CreateConnection());
            Assert.IsNotNull(provider.CreateDataAdapter());
            Assert.IsNotNull(provider.CreateParameter());
            Assert.AreEqual("?", provider.CreateParameterName("Foo"));
        }
        
        [Test]
        public void DefaultInstanceWithMicrsoftOracleClient20()
        {
            IDbProvider provider = DbProviderFactory.GetDbProvider("OracleClient-2.0");
            Assert.AreEqual("Oracle, Microsoft provider V2.0.0.0", provider.DbMetadata.ProductName);
            Assert.IsNotNull(provider.CreateCommand());
            Assert.IsNotNull(provider.CreateCommandBuilder());
            Assert.IsNotNull(provider.CreateConnection());
            Assert.IsNotNull(provider.CreateDataAdapter());
            Assert.IsNotNull(provider.CreateParameter());
            Assert.AreEqual(":Foo", provider.CreateParameterName("Foo"));
        }
#endif
       
        [Test]   
        [Ignore("until find out if can add oracle.dll to cvs repository")]
        public void DefaultInstanceWithOracleClient20()
        {
            IDbProvider provider = DbProviderFactory.GetDbProvider("OracleODP-2.0");
            Assert.AreEqual("Oracle, Oracle provider V2.102.2.20", provider.DbMetadata.ProductName);
            Assert.IsNotNull(provider.CreateCommand());
            Assert.IsNotNull(provider.CreateCommandBuilder());
            Assert.IsNotNull(provider.CreateConnection());
            Assert.IsNotNull(provider.CreateDataAdapter());
            Assert.IsNotNull(provider.CreateParameter());
            Assert.AreEqual(":Foo", provider.CreateParameterName("Foo")); 

        }

        /*
        [Test]
        public void DefaultInstanceWithMySql()
        {
            DbProviderFactory.DBPROVIDER_ADDITIONAL_RESOURCE_NAME =
                "assembly://Spring.Data.Tests/Spring.Data.Common/AdditonalProviders.xml";
            IDbProvider provider = DbProviderFactory.GetDbProvider("MySqlPersonal");
            Assert.AreEqual("MySQL, MySQL provider 1.0.7.30072", provider.DbMetadata.ProductName);

        }
         
        */

        [Test]
        public void TestDb2()
        {
            IApplicationContext ctx = DbProviderFactory.ApplicationContext;
            string[] dbProviderNames = ctx.GetObjectNamesForType(typeof(IDbProvider));
            Console.WriteLine(
                String.Format("{0} DbProviders Available. [{1}]", dbProviderNames.Length,
                              StringUtils.ArrayToCommaDelimitedString(dbProviderNames)));

        }


#if NET_1_1

        [Test]
        public void DefaultInstanceWithOleDb11()
        {
            IDbProvider provider = DbProviderFactory.GetDbProvider("OleDb-1.1");
            Assert.AreEqual("OleDb, provider V1.0.5000.0 in framework .NET V1.1", provider.DbMetadata.ProductName);
            Assert.IsNotNull(provider.CreateCommand());
            Assert.IsNotNull(provider.CreateCommandBuilder());
            Assert.IsNotNull(provider.CreateConnection());
            Assert.IsNotNull(provider.CreateDataAdapter());
            Assert.IsNotNull(provider.CreateParameter());
            Assert.AreEqual("?", provider.CreateParameterName("Foo"));
        }

        [Test]
        public void DefaultInstanceWithSqlServer2000()
        {
            IDbProvider provider = DbProviderFactory.GetDbProvider("SqlServer-1.1");
            AssertIsSqlServer2000(provider);
            Assert.IsNull(provider.ConnectionString);
            Assert.IsNotNull(provider.CreateCommand());
            Assert.IsNotNull(provider.CreateCommandBuilder());
            Assert.IsNotNull(provider.CreateConnection());
            Assert.IsNotNull(provider.CreateDataAdapter());
            Assert.IsNotNull(provider.CreateParameter());
            Assert.AreEqual("@Foo", provider.CreateParameterName("Foo"));
        }

        private void AssertIsSqlServer2000(IDbProvider provider)
        {
            Assert.AreEqual("Microsoft SQL Server, provider V1.0.5000.0 in framework .NET V1.1",
                            provider.DbMetadata.ProductName);
            AssertCommonSqlServerErrorCodes(provider);
        }
#endif
        private void AssertIsSqlServer2005(IDbProvider provider)
        {
            Assert.AreEqual("Microsoft SQL Server, provider V2.0.0.0 in framework .NET V2.0",
                            provider.DbMetadata.ProductName);
            AssertCommonSqlServerErrorCodes(provider);
        }

        private static void AssertCommonSqlServerErrorCodes(IDbProvider provider)
        {
            ErrorCodes codes = provider.DbMetadata.ErrorCodes;
            Assert.IsTrue(codes.BadSqlGrammarCodes.Length > 0);
            Assert.IsTrue(codes.DataIntegrityViolationCodes.Length > 0);
            // This had better be a Bad SQL Grammar code
            Assert.IsTrue(Array.IndexOf(codes.BadSqlGrammarCodes, "156") >= 0);
            // This had better NOT be
            Assert.IsFalse(Array.IndexOf(codes.BadSqlGrammarCodes, "1xx56") >= 0);
        }


    }
}