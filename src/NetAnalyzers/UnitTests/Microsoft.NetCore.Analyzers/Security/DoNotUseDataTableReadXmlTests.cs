﻿// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.Threading.Tasks;
using Microsoft.CodeAnalysis.Testing;
using Xunit;
using VerifyCS = Test.Utilities.CSharpSecurityCodeFixVerifier<
    Microsoft.NetCore.Analyzers.Security.DoNotUseDataTableReadXml,
    Microsoft.CodeAnalysis.Testing.EmptyCodeFixProvider>;

namespace Microsoft.NetCore.Analyzers.Security.UnitTests
{
    public class DoNotUseDataTableReadXmlTests
    {
        [Fact]
        public async Task ReadXml_Diagnostic()
        {
            await VerifyCS.VerifyAnalyzerAsync(@"
using System.IO;
using System.Data;

namespace Blah
{
    public class Program
    {
        public void Unsafe(Stream s)
        {
            DataTable dataTable = new DataTable();
            dataTable.ReadXml(s);
        }
    }
}",
                GetCSharpResultAt(12, 13, "XmlReadMode DataTable.ReadXml(Stream stream)"));
        }

        [Fact]
        public async Task DerivedReadXml_Diagnostic()
        {
            await VerifyCS.VerifyAnalyzerAsync(@"
using System.IO;
using System.Data;

namespace Blah
{
    public class Program
    {
        public void Unsafe(string s)
        {
            MyDataTable dataTable = new MyDataTable();
            dataTable.ReadXml(s);
        }
    }

    public class MyDataTable : DataTable
    {
    }
}",
                GetCSharpResultAt(12, 13, "XmlReadMode DataTable.ReadXml(string fileName)"));
        }

        [Fact]
        public async Task RejectChanges_NoDiagnostic()
        {
            await VerifyCS.VerifyAnalyzerAsync(@"
using System.IO;
using System.Data;

namespace Blah
{
    public class Program
    {
        public void Safe(Stream s)
        {
            DataTable dataTable = new DataTable();
            dataTable.RejectChanges();
        }
    }
}");
        }

        private static DiagnosticResult GetCSharpResultAt(int line, int column, params string[] arguments)
            => VerifyCS.Diagnostic(DoNotUseDataTableReadXml.RealMethodUsedDescriptor)
                .WithLocation(line, column)
                .WithArguments(arguments);
    }
}