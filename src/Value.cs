using System.Diagnostics.CodeAnalysis;

enum ValueType : byte
{
    BOOLEAN,
    NIL,
    NUMBER,
}

struct Value(ValueType type, double value)
{
    public readonly ValueType type = type;
    public readonly double value = value;

    public static Value Nil() => new(ValueType.NIL, 0);
    public static Value Boolean(bool val) => new(ValueType.BOOLEAN, val ? 1 : 0);
    public static Value Number(double val) => new(ValueType.NUMBER, val);

    public bool AsBoolean => value != 0;
    public double AsNumber => value;

    public bool IsBoolean => type == ValueType.BOOLEAN;
    public bool IsNil => type == ValueType.NIL;
    public bool IsNumber => type == ValueType.NUMBER;

    public bool IsFalsy => IsNil || (IsBoolean && !AsBoolean);

    public override string ToString()
    {
        switch (type)
        {
            case ValueType.NIL: return "nil";
            case ValueType.BOOLEAN: return AsBoolean ? "true" : "false";
        }
        return value.ToString(System.Globalization.CultureInfo.InvariantCulture);
    }

    public override int GetHashCode()
    {
        return base.GetHashCode();
    }

    public override bool Equals(object? obj)
    {
        if (!(obj is Value)) return false;

        Value b = (Value)obj;
        if (type != b.type) return false;

        return type switch
        {
            ValueType.NIL => true,
            ValueType.BOOLEAN => AsBoolean == b.AsBoolean,
            ValueType.NUMBER => AsNumber == b.AsNumber,
            _ => false
        };
    }

    public static bool operator ==(Value lhs, Value rhs) => lhs.Equals(rhs);
    public static bool operator !=(Value lhs, Value rhs) => !lhs.Equals(rhs);
}
