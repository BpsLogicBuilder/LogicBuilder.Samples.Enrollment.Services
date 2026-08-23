using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query.SqlExpressions;
using System;
using System.Collections.Generic;

namespace Enrollment.Contexts
{
    [System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
    public static class BaseDbContextSqlFunctions
    {
#pragma warning disable IDE0060//method signature is required for database translation
        public static string FormatDateTime(DateTime value, string format, string culture) => value.ToString(format);
        public static string FormatDecimal(decimal value, string format, string culture) => value.ToString(format);
#pragma warning restore IDE0060

        public static void Register(ModelBuilder modelBuilder)
        {
            modelBuilder.HasDbFunction
            (
                typeof(BaseDbContextSqlFunctions).GetMethod(nameof(FormatDateTime), [typeof(DateTime), typeof(string), typeof(string)])!//NOSOSNAR FormatDateTime exists
            )
            .HasTranslation(Transalate);

            modelBuilder.HasDbFunction
            (
                typeof(BaseDbContextSqlFunctions).GetMethod(nameof(FormatDecimal), [typeof(decimal), typeof(string), typeof(string)])!//NOSOSNAR FormatDecimal exists
            )
            .HasTranslation(Transalate);
        }

        private static SqlExpression Transalate(IReadOnlyCollection<SqlExpression> args)
            => new SqlFunctionExpression
            (
                "FORMAT",
                args,
                false,
                [true, true, true],
                typeof(string),
                null
            );
    }
}
