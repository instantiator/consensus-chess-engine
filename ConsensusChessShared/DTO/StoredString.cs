using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace ConsensusChessShared.DTO
{
	public class StoredString
	{
        public StoredString() { }
        public StoredString(string? value) { Value = value; }

        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid Id { get; set; }
        public string? Value { get; set; }

        public static explicit operator string?(StoredString ss) => ss.Value;
        public static explicit operator StoredString(string? s) => new StoredString(s);

        public override bool Equals(object? obj)
        {
            if (obj is string) { return Value == obj as string; }
            if (obj is StoredString) { return Value == (obj as StoredString)?.Value; }
            return false;
        }

        public override int GetHashCode()
        {
            return Value?.GetHashCode() ?? base.GetHashCode();
        }
    }
}

