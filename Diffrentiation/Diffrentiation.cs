using System;
using System.Text;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

public class PrefixDiff // Class name required
{
    private int index;
    private string baseString;

    public string Diff(string expr)
    {
        index = 0;
        baseString = expr;

        var expression = ParseBaseExpression();
        return expression.Diffrentiate().ToString();
    }

    private BaseEntity ParseBaseExpression()
    {
        var stack = new List<string>();
        var output = new List<BaseEntity>();

        bool breakOut = false;
        for (; index < baseString.Length; index++)
        {
            var current = baseString[index];
            switch (current)
            {
                case ' ':
                    break;
                case 'x':
                    output.Add(VariableNode.Variable);
                    break;
                case '(':
                    index++;
                    output.Add(ParseBaseExpression());
                    break;
                case ')':
                    breakOut = true;
                    break;
                default:
                    if (char.IsNumber(current))
                        output.Add(ParseConstant());
                    else if (char.IsLetter(current))
                    {
                        string sub;
                        if (current == 'l')
                            sub = baseString.Substring(index, 2);
                        else
                            sub = baseString.Substring(index, 3);

                        switch (sub)
                        {
                            case "sin":
                            case "cos":
                            case "tan":
                            case "exp":
                            case "ln":
                                stack.Add(sub);
                                break;
                            default:
                                throw new ParseException($"INVALID character detected: {current} at {index}");
                        }

                        index += sub.Length;
                    }
                    else
                    {
                        switch (current)
                        {
                            case '+':
                            case '-':
                            case '*':
                            case '/':
                            case '^':
                                stack.Add($"{current}");
                                break;
                            default:
                                throw new ParseException($"INVALID character detected: {current} at {index}");
                        }
                    }
                    break;

            }

            if (breakOut)
                break;
        }

        for (int i = stack.Count - 1; i >= 0; i--)
        {
            var current = stack[i];

            switch (current)
            {
                case "+":
                    output.Add(new AdditionExpr(output.Pop(), output.Pop()));
                    break;
                case "-":
                    output.Add(new SubtractionExpr(output.Pop(), output.Pop()));
                    break;
                case "*":

                    output.Add(new MultiplicationExpr(output.Pop(), output.Pop()));
                    break;
                case "/":
                    output.Add(new DivisionExpr(output.Pop(), output.Pop()));
                    break;
                case "^":
                    output.Add(new PowerExpression(output.Pop(), output.Pop()));
                    break;

                case "sin":
                    output.Add(new SinExpression(output.Pop()));
                    break;
                case "cos":
                    output.Add(new CosExpression(output.Pop()));
                    break;
                case "tan":
                    output.Add(new TanExpression(output.Pop()));
                    break;
                case "exp":
                    output.Add(new ExponentExpression(output.Pop()));
                    break;
                case "ln":
                    output.Add(new LogExpression(output.Pop()));
                    break;
            }
        }

        return output[0];
    }

    private ConstantNode ParseConstant()
    {
        var builder = new StringBuilder();

        for (; index < baseString.Length; index++)
        {
            var current = baseString[index];
            if (char.IsNumber(current))
                builder.Append(current);
            else if (current == '.')
                builder.Append('.');
            else
            {
                index--;
                break;
            }
        }

        return new ConstantNode(float.Parse(builder.ToString()));
    }
}

static class Extensions
{
    public static T Pop<T>(this List<T> l)
    {
        T el = l[l.Count - 1];
        l.RemoveAt(l.Count - 1);

        return el;
    }
}

//basics
interface IDiffrentiable
{
    public BaseEntity Diffrentiate();
}

enum EntityType : byte
{
    Constant = 1,
    Variable = 2,

    ArithmeticExpression = 4,
    FunctionalExpression = 8
}

internal abstract class BaseEntity : IDiffrentiable, IEquatable<BaseEntity>
{
    public abstract EntityType Type { get; }
    public abstract BaseEntity Diffrentiate();

    public void Debug(string indent, bool last)
    {
        Console.Write(indent);
        if (last)
        {
            Console.Write("\\-");
            indent += "  ";
        }
        else
        {
            Console.Write("|-");
            indent += "| ";
        }

        Console.WriteLine(DebugSelf());
        DebugChildren(indent, last);
    }

    public static bool operator ==(BaseEntity obj1, BaseEntity obj2)
    {
        if (ReferenceEquals(obj1, obj2))
            return true;
        if (ReferenceEquals(obj1, null))
            return false;
        if (ReferenceEquals(obj2, null))
            return false;

        return obj1.Equals(obj2);
    }

    public static bool operator !=(BaseEntity obj1, BaseEntity obj2) => !(obj1 == obj2);

    public override bool Equals(object other) => Equals(other as BaseEntity);

    public bool Equals([AllowNull] BaseEntity other)
    {
        if (ReferenceEquals(other, null))
            return false;
        if (ReferenceEquals(this, other))
            return true;

        if (other.Type != Type)
            return false;
        else if (other.Type == EntityType.Constant)
            if (((ConstantNode)this).Value == ((ConstantNode)other).Value)
                return true;

        return false;
    }

    public override int GetHashCode() => base.GetHashCode();

    protected abstract string DebugSelf();
    protected abstract void DebugChildren(string indent, bool last);
}

interface ISimplifiable
{
    public BaseEntity Simplify();
}

sealed class ParseException : Exception
{
    public ParseException(string s) : base($"EXCEPION ENCOUNTERED DURING PARSING: {s}") { }
}

//nodes
abstract class Node<ValueType> : BaseEntity
{
    protected readonly ValueType value;
    public ValueType Value
    {
        get => value;
    }

    public Node(ValueType _value)
    {
        value = _value;
    }

    public override string ToString() => value.ToString();
}

sealed class ConstantNode : Node<float>
{
    public static ConstantNode Zero = new ConstantNode(0);

    public static ConstantNode One = new ConstantNode(1);
    public static ConstantNode MinusOne = new ConstantNode(-1);

    public static ConstantNode Two = new ConstantNode(2);
    public static ConstantNode MinusTwo = new ConstantNode(-2);

    public override EntityType Type
    {
        get => EntityType.Constant;
    }

    public ConstantNode(float _value) : base(_value) { }

    public override BaseEntity Diffrentiate() => Zero;

    protected override string DebugSelf() => $"Constant Node: [{value}]";
    protected override void DebugChildren(string indent, bool last) { }
}

sealed class VariableNode : Node<char>
{
    public static VariableNode Variable = new VariableNode();

    public override EntityType Type
    {
        get => EntityType.Variable;
    }

    private VariableNode() : base('x') { }

    public override BaseEntity Diffrentiate() => ConstantNode.One;

    protected override string DebugSelf() => $"Variable Node: [{value}]";
    protected override void DebugChildren(string indent, bool last) { }
}

//expressions
abstract class Expression : BaseEntity { }

abstract class ArithmeticExpression : Expression, ISimplifiable
{
    protected BaseEntity lhs;
    protected BaseEntity rhs;

    public override EntityType Type
    {
        get => EntityType.ArithmeticExpression;
    }

    public ArithmeticExpression(BaseEntity _rhs, BaseEntity _lhs)
    {
        lhs = _lhs;
        rhs = _rhs;
    }

    public abstract BaseEntity Simplify();

    protected override void DebugChildren(string indent, bool last)
    {
        lhs.Debug(indent + ' ', false);
        rhs.Debug(indent + ' ', true);
    }
}

sealed class AdditionExpr : ArithmeticExpression
{
    public AdditionExpr(BaseEntity _rhs, BaseEntity _lhs) : base(_rhs, _lhs) { }

    public override BaseEntity Diffrentiate() =>
        new AdditionExpr(
            rhs.Diffrentiate(),
            lhs.Diffrentiate())
        .Simplify();

    public override BaseEntity Simplify()
    {
        if (lhs.Type == EntityType.Constant)
        {
            var constant = (ConstantNode)lhs;

            if (lhs.Type == rhs.Type)
                return new ConstantNode(constant.Value + ((ConstantNode)rhs).Value);
            else if (constant.Value == 0)
                return rhs;
        }
        else if (rhs == ConstantNode.Zero)
            return lhs;

        return this;
    }

    protected override string DebugSelf() => $"Addition";
    public override string ToString() => $"(+ {lhs} {rhs})";
}

sealed class SubtractionExpr : ArithmeticExpression
{
    public SubtractionExpr(BaseEntity _rhs, BaseEntity _lhs) : base(_rhs, _lhs) { }

    public override BaseEntity Diffrentiate() =>
        new SubtractionExpr(
            rhs.Diffrentiate(),
            lhs.Diffrentiate())
        .Simplify();

    public override BaseEntity Simplify()
    {
        if (lhs.Type == EntityType.Constant)
        {
            var constant = (ConstantNode)lhs;

            if (lhs.Type == rhs.Type)
                return new ConstantNode(constant.Value - ((ConstantNode)rhs).Value);
            else if (constant.Value == 0)
                return new MultiplicationExpr(ConstantNode.MinusOne, rhs);
        }
        else if (rhs == ConstantNode.Zero)
            return lhs;

        return this;
    }

    protected override string DebugSelf() => $"Subtraction";
    public override string ToString() => $"(- {lhs} {rhs})";
}

sealed class MultiplicationExpr : ArithmeticExpression
{
    public MultiplicationExpr(BaseEntity _rhs, BaseEntity _lhs) : base(_rhs, _lhs) { }

    public override BaseEntity Diffrentiate() =>
        new AdditionExpr(
            new MultiplicationExpr(rhs, lhs.Diffrentiate()).Simplify(),
            new MultiplicationExpr(rhs.Diffrentiate(), lhs).Simplify())
        .Simplify();

    public override BaseEntity Simplify()
    {
        if (lhs == ConstantNode.Zero)
            return ConstantNode.Zero;
        else if (lhs.Type == EntityType.Constant)
        {
            var constant = (ConstantNode)lhs;

            if (rhs.Type == EntityType.Constant)
                return new ConstantNode(constant.Value * ((ConstantNode)rhs).Value);
            else if (constant.Value == 1)
                return rhs;
        }
        else if (rhs == ConstantNode.One)
            return lhs;
        else if (rhs == ConstantNode.Zero)
            return ConstantNode.Zero;

        return this;
    }

    protected override string DebugSelf() => $"Multiplication";
    public override string ToString() => $"(* {lhs} {rhs})";
}

sealed class DivisionExpr : ArithmeticExpression
{
    public DivisionExpr(BaseEntity _rhs, BaseEntity _lhs) : base(_rhs, _lhs) { }

    public override BaseEntity Diffrentiate()
    {
        var lhsDifferential = new MultiplicationExpr(rhs, lhs.Diffrentiate()).Simplify();
        var rhsDifferential = new MultiplicationExpr(rhs.Diffrentiate(), lhs).Simplify();

        var numerator = new SubtractionExpr(rhsDifferential, lhsDifferential).Simplify();
        var denominator = new PowerExpression(ConstantNode.Two, rhs).Simplify();

        return new DivisionExpr(denominator, numerator).Simplify();
    }

    public override BaseEntity Simplify()
    {
        if (lhs == ConstantNode.Zero)
            return ConstantNode.Zero;
        else if (rhs == ConstantNode.Zero)
            throw new ParseException("DIVISION BY ZERO");
        if (lhs.Type == EntityType.Constant && rhs.Type == EntityType.Constant)
            return new ConstantNode(((ConstantNode)lhs).Value / ((ConstantNode)rhs).Value);

        return this;
    }

    protected override string DebugSelf() => $"Division";
    public override string ToString() => $"(/ {lhs} {rhs})";
}

sealed class PowerExpression : ArithmeticExpression
{
    public BaseEntity LHS { get => lhs; }
    public BaseEntity RHS { get => rhs; }

    public PowerExpression(BaseEntity _rhs, BaseEntity _lhs) : base(_rhs, _lhs) { }

    public override BaseEntity Diffrentiate()
    {
        var result = Simplify();
        if (result.Type != EntityType.ArithmeticExpression)
            return result.Diffrentiate();

        var constant = (ConstantNode)rhs;
        var coeff = new ConstantNode(constant.Value - 1);

        return new MultiplicationExpr(new MultiplicationExpr(new PowerExpression(coeff, lhs).Simplify(), lhs.Diffrentiate()).Simplify(), constant).Simplify();
    }

    public override BaseEntity Simplify()
    {
        if (lhs == ConstantNode.Zero)
            return ConstantNode.Zero;
        else if (lhs.Type == EntityType.Constant && rhs.Type == EntityType.Constant)
            return new ConstantNode((float)Math.Pow(((ConstantNode)lhs).Value, ((ConstantNode)rhs).Value));
        else if (rhs == ConstantNode.One)
            return lhs;
        else if (rhs == ConstantNode.Zero)
            return ConstantNode.One;

        return this;
    }

    protected override string DebugSelf() => $"Power";
    public override string ToString() => $"(^ {lhs} {rhs})";
}

//if diffrential is constant, multipliy it first, if not multiply it second :)
abstract class FunctionalExpression : Expression
{
    protected readonly BaseEntity expression;

    public override EntityType Type
    {
        get => EntityType.FunctionalExpression;
    }

    public FunctionalExpression(BaseEntity _expr)
    {
        expression = _expr;
    }

    protected BaseEntity ChainRule(BaseEntity baseDifferential, BaseEntity chainRuleProduct)
    {
        MultiplicationExpr result;
        if (chainRuleProduct.Type == EntityType.Constant)
            result = new MultiplicationExpr(baseDifferential, chainRuleProduct);
        else
            result = new MultiplicationExpr(chainRuleProduct, baseDifferential);

        return result.Simplify();
    }

    protected override void DebugChildren(string indent, bool last) => expression.Debug(indent + ' ', true);
}

sealed class SinExpression : FunctionalExpression
{
    public SinExpression(BaseEntity _expr) : base(_expr) { }

    public override BaseEntity Diffrentiate() =>
        ChainRule(new CosExpression(expression), expression.Diffrentiate());

    protected override string DebugSelf() => $"Sin";
    public override string ToString() => $"(sin {expression})";
}

sealed class CosExpression : FunctionalExpression
{
    public CosExpression(BaseEntity _expr) : base(_expr) { }

    public override BaseEntity Diffrentiate() =>
        ChainRule(new MultiplicationExpr(new SinExpression(expression), ConstantNode.MinusOne), expression.Diffrentiate());

    protected override string DebugSelf() => $"Cos";
    public override string ToString() => $"(cos {expression})";
}

sealed class TanExpression : FunctionalExpression
{
    public TanExpression(BaseEntity _expr) : base(_expr) { }

    public override BaseEntity Diffrentiate() =>
        ChainRule(new PowerExpression(ConstantNode.MinusTwo, new CosExpression(expression)), expression.Diffrentiate());

    protected override string DebugSelf() => $"Tan";
    public override string ToString() => $"(tan {expression})";
}

sealed class ExponentExpression : FunctionalExpression
{
    public ExponentExpression(BaseEntity _expr) : base(_expr) { }

    public override BaseEntity Diffrentiate() =>
        ChainRule(new ExponentExpression(expression), expression.Diffrentiate());

    protected override string DebugSelf() => $"Exp";
    public override string ToString() => $"(exp {expression})";
}

sealed class LogExpression : FunctionalExpression
{
    public LogExpression(BaseEntity _expr) : base(_expr) { }

    public override BaseEntity Diffrentiate() =>
        ChainRule(new DivisionExpr(expression, ConstantNode.One), expression.Diffrentiate());

    protected override string DebugSelf() => $"Log";
    public override string ToString() => $"log( {expression})";
}