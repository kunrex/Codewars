//https://www.codewars.com/kata/52ffcfa4aff455b3c2000750
using System;
using System.Linq;
using System.Text;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace InterpreterKata
{
  using InterpreterKata.Lexing;
  
  using InterpreterKata.Nodes;
  using InterpreterKata.Parsing;
  
  using InterpreterKata.Caching;
  
  public class Interpreter 
  {
    private readonly GlobalCache cache;
    
    public Interpreter()
    {
      cache = new GlobalCache();  
    }
    
    public double? input(string input)
    {  
      if(string.IsNullOrEmpty(input))
        return null;
      
      var result = new Parser(Lexer.Lex(input), cache).Parse();

      switch(result.Type)
      {
        case NodeType.Null:
          break;
        case NodeType.Constant:
        case NodeType.Operator:
        case NodeType.Identifier:
        case NodeType.Assignment:
        case NodeType.FunctionCall:
          return result.Value(cache);
        case NodeType.FunctionDefine:
          cache.SetFunction((Function)result);
          break;
      }
      return null;
    }
  }
}

namespace InterpreterKata.Tokens
{
  enum TokenType : byte
  {
    Keyword,
    Constant,
    Identifier,
    
    Operator,
    Seperator
  }
  
  internal abstract class Token
  {
    protected readonly string value;
    public string Value { get => value; }
    
    public abstract TokenType Type { get; }
    
    protected Token(string _value)
    {
      value = _value;  
    }
  }

  internal sealed class Keyword : Token
  {
    public override TokenType Type { get => TokenType.Keyword; }
    
    public static Keyword Function = new Keyword("fn");
    
    private Keyword(string _value) : base(_value)
    {}
    
    public Token Clone() => new Keyword(value);
  }
  
  internal sealed class Identifer : Token
  {
    public override TokenType Type { get => TokenType.Identifier; }
    
    public Identifer(string _value) : base(_value)
    {}
  }
  
  internal sealed class Constant : Token
  {
    public override TokenType Type { get => TokenType.Constant; }
  
    public Constant(string _value) : base(_value)
    {}
  }
  
  internal sealed class Seperator : Token
  {
    public override TokenType Type { get => TokenType.Seperator; }
    
    public static Seperator Lamda = new Seperator("=>");
    
    public static Seperator OpenBracket = new Seperator("(");
    public static Seperator CloseBracket = new Seperator(")");
  
    private Seperator(string _value) : base(_value)
    {}
    
    public Token Clone() => new Seperator(value);
  }
  
  internal sealed class Operator : Token
  {
    public override TokenType Type { get => TokenType.Operator; }
  
    private readonly int precedence;
    public int Precedence { get => precedence; }
    
    private readonly bool leftAssociative;
    public bool LeftAssociative { get => leftAssociative; }
    
    public static Operator Addition = new Operator("+", 2, true);
    public static Operator Subtraction = new Operator("-", 2, true);
    
    public static Operator Multiplication = new Operator("*", 1, true);
    public static Operator Division = new Operator("/", 1, true);
    public static Operator Remainder = new Operator("%", 1, true);
    
    public static Operator Assignment = new Operator("=", 3, false);
    
    private Operator(string _value, int _precedence, bool _leftAssociative) : base(_value)
    {
      precedence = _precedence;
      leftAssociative = _leftAssociative;
    }
    
    public Token Clone() => new Operator(value, precedence, leftAssociative);
  }
}

namespace InterpreterKata.Lexing 
{
  using InterpreterKata.Tokens;

  internal static class Lexer
  {
    public static List<Token> Lex(string input)
    {
      var tokens = new List<Token>();
      var s = new StringBuilder();
      
      for(int i = 0; i < input.Length; i++)
      {
        switch(input[i])
        {
          case ' ':
            break;
          case '+':
            tokens.Add(Operator.Addition.Clone());
            break;
          case '-':
            tokens.Add(Operator.Subtraction.Clone());
            break;
          case '*':
            tokens.Add(Operator.Multiplication.Clone());
            break;
          case '/':
            tokens.Add(Operator.Division.Clone());
            break;
          case '%':
            tokens.Add(Operator.Remainder.Clone());
            break;
          case '=':
            if(input[i + 1] == '>')
            {
              i++;
              tokens.Add(Seperator.Lamda.Clone());
            }
            else
              tokens.Add(Operator.Assignment.Clone());
            break;
          case '(':
            tokens.Add(Seperator.OpenBracket.Clone());
            break;
          case ')':
            tokens.Add(Seperator.CloseBracket.Clone());
            break;
          default:
            s.Clear();
            for(; i < input.Length; i++)
            {
              if(char.IsLetter(input[i]) || char.IsNumber(input[i]))
              {
                s.Append(input[i]);
                continue;
              }
              
              break;
            }
            
            var result = s.ToString();
            if(result == "fn")
              tokens.Add(Keyword.Function.Clone());
            else if(double.TryParse(result, out double number))
              tokens.Add(new Constant(number.ToString()));
            else
              tokens.Add(new Identifer(result));
            
            i--;
            break;
        }
      }

      return tokens;
    }
  }
}

namespace InterpreterKata.Nodes 
{
  using InterpreterKata.Caching;

  enum NodeType : byte
  {
    Constant,
    Identifier,
    
    Operator,
    
    FunctionCall,
    FunctionDefine,
    
    Null,
    Assignment
  }
  
  internal abstract class Node
  {
    public abstract NodeType Type { get; }
   
    public abstract double? Value(Cache cache);
  }
  
  internal sealed class IdentifierNode : Node 
  {
    public override NodeType Type { get => NodeType.Identifier; }
    
    private readonly string name;
    public string Name { get => name; }
    
    public IdentifierNode(string _name)
    {
      name = _name;
    }
    
    public override double? Value(Cache cache)
    {
      return cache.GetVariable(name);
    }
  }
  
  internal sealed class ConstantNode : Node 
  {
    public override NodeType Type { get => NodeType.Constant; }
    
    private readonly double value;
    
    public ConstantNode(double _value)
    {
      value = _value;
    }
    
    public override double? Value(Cache cache) => value;
  }
  
  internal abstract class OperatorNode : Node 
  {
    public override NodeType Type { get => NodeType.Operator; }

    protected readonly Node operhand1;
    protected readonly Node operhand2;
    
    protected OperatorNode(Node _op1, Node _op2)
    {
      operhand1 = _op1;
      operhand2 = _op2;
    }
  }
  
  internal sealed class Addition : OperatorNode
  {
    public Addition(Node _op1, Node _op2) : base(_op1, _op2)
    { }
    
    public override double? Value(Cache cache) => operhand1.Value(cache) + operhand2.Value(cache);
  }
  
  internal sealed class Subtraction : OperatorNode
  {
    public Subtraction(Node _op1, Node _op2) : base(_op1, _op2)
    { }
    
    public override double? Value(Cache cache) => operhand1.Value(cache) - operhand2.Value(cache);
  }
  
  internal sealed class Multiplication : OperatorNode
  {
    public Multiplication(Node _op1, Node _op2) : base(_op1, _op2)
    { }
    
    public override double? Value(Cache cache) => operhand1.Value(cache) * operhand2.Value(cache);
  }
  
  internal sealed class Division : OperatorNode
  {
    public Division(Node _op1, Node _op2) : base(_op1, _op2)
    { }
    
    public override double? Value(Cache cache) => operhand1.Value(cache) / operhand2.Value(cache);
  }
  
  internal sealed class Remainder : OperatorNode
  {
    public Remainder(Node _op1, Node _op2) : base(_op1, _op2)
    { }
    
    public override double? Value(Cache cache) => operhand1.Value(cache) % operhand2.Value(cache);
  }
  
  internal sealed class Assignment : OperatorNode
  {
    public override NodeType Type { get => NodeType.Assignment; }
    
    private readonly IdentifierNode identifier;
  
    public Assignment(IdentifierNode _op1, Node _op2) : base(_op1, _op2)
    { 
      identifier = _op1;
    }
    
    public override double? Value(Cache cache) 
    {
      cache.SetVariable(identifier.Name, operhand2.Value(cache));
      
      return cache.GetVariable(identifier.Name);
    } 
  }
  
    internal sealed class Null : Node
    {
      public override NodeType Type { get => NodeType.Null; }
      
      public Null() 
      { }
      
      public override double? Value(Cache cache) => null;
    }
  
  internal sealed class Function : Node
  {
    public override NodeType Type { get => NodeType.FunctionDefine; }
    
    private readonly string name;
    public string Name { get => name; }
    
    private readonly Node expression;
    
    private readonly string[] arguments;
    public int ArgLength { get => arguments.Length; }
    
    public string this[int index]
    {
      get => arguments[index];
    }
    
    public Function(string _name, Node _expression, string[] _arguments)
    {
      name = _name;
      expression = _expression;
      arguments = _arguments;
    }
    
    public override double? Value(Cache cache) => expression.Value(cache);
  }
  
  internal sealed class FunctionCall : Node
  {
    public override NodeType Type { get => NodeType.FunctionCall; }
    
    private readonly Function function;
    public int FunctionArgLength { get => function.ArgLength; }
    
    private readonly List<Node> arguments;
    public Node this[int index]
    {
      get => arguments[index];
    }
    
    public FunctionCall(Function _function)
    {
      function = _function;
      arguments = new List<Node>();
    }
    
    public void WithArgument(Node arg)
    {
      arguments.Add(arg);
    }
    
    public override double? Value(Cache cache)
    {
      if(arguments.Count != function.ArgLength)
        return null;
        
      var localCache = new LocalCache(cache.GetGlobal());
      for(int i = 0; i < arguments.Count; i++)
        localCache.SetVariable(function[i], arguments[i].Value(cache));

      return function.Value(localCache);
    }
  }
}

namespace InterpreterKata.Parsing 
{
  using InterpreterKata.Nodes;
  using InterpreterKata.Tokens;
  using InterpreterKata.Caching;

  internal class Parser
  {
    private int index;
    
    private readonly GlobalCache cache;
    private readonly List<Token> tokens;
    
    public Parser(List<Token> _tokens, GlobalCache _cache)
    {
      index = 0;
      
      cache = _cache;
      tokens = _tokens;
    }

    public Node Parse()
    {
      if(tokens[0].Type == TokenType.Keyword)
        return ParseFunction();
  
      return ParseExpression();
    }
    
    private Node ParseExpression()
    {
      Stack<Node> output = new Stack<Node>();
      Stack<Token> stack = new Stack<Token>();
      
      for(; index < tokens.Count; index++)
      {
        var current = tokens[index];
        switch(current.Type)
        {
          case TokenType.Seperator:
            if(current.Value == "(")
              stack.Push(current);
            else 
            {
              while(stack.Count > 0)
              {
                var top = stack.Pop();
                if(top.Type == TokenType.Seperator)
                  break;
                
                output.Push(ParseExpression(top, output));
              }
            }
            break;
          case TokenType.Identifier:
            var function = cache.GetFunction(current.Value);
            
            if(function != null)
            {
              stack.Push(current); 
              output.Push(function);
            }
            else
              output.Push(new IdentifierNode(current.Value));
            break;
          case TokenType.Constant:
            output.Push(new ConstantNode(double.Parse(current.Value)));
            break;
          case TokenType.Operator:
            if(stack.Count > 0 && stack.Peek().Type == TokenType.Operator)
            {
              var cur = (Operator)current;
              var top = (Operator)stack.Pop();

              if (cur.Precedence < top.Precedence || (top.Precedence == cur.Precedence && !cur.LeftAssociative))
                stack.Push(top);
              else
                output.Push(ParseExpression(top, output));
            }
              
            stack.Push(current);
            break;
        }
      }
      
             
      while(stack.Count > 0)
        output.Push(ParseExpression(stack.Pop(), output));
      
      if(output.Count == 1)
        return output.Pop();
      
      return null;
    }

    private Node ParseExpression(Token top, Stack<Node> output)
    {
      if(top.Type == TokenType.Identifier)
        return ParseFunctionCall(output);
      
      var rhs = output.Pop();
      var lhs = output.Pop();
      switch(top.Value)
      {
        case "+":
          return new Addition(lhs, rhs);
        case "-":
          return new Subtraction(lhs, rhs);
        case "*":
          return new Multiplication(lhs, rhs);
        case "/":
          return new Division(lhs, rhs);
        case "%":
          return new Remainder(lhs, rhs);
        case "=":
          if(lhs.Type == NodeType.Identifier)
            return new Assignment((IdentifierNode)lhs, rhs);
          break;    
      }
      
      return new Null();  
    }
    
    private FunctionCall ParseFunctionCall(Stack<Node> output)
    {
      var arguments = new Stack<Node>();
      
      FunctionCall call = null;
      while(output.Count > 0)
      {
        var top = output.Pop();
        if(top.Type == NodeType.FunctionDefine)
        {
          var function = (Function)top;
          call = new FunctionCall(function);
          break;
        }
        
        arguments.Push(top);
      }
      
      for(int i = 0; i < call.FunctionArgLength; i++)
        call.WithArgument(arguments.Pop());
      
      while(arguments.Count > 0)
        output.Push(arguments.Pop());

      return call;
    }
    
    private Function ParseFunction()
    {
      index++;  
      
      var name = tokens[index].Value;
      
      List<string> arguments = new List<string>();
      for(index++; index < tokens.Count; index++)
      {
        if(tokens[index].Type == TokenType.Seperator)
          break;
        
        arguments.Add(tokens[index].Value);
      }
      
      index++;
      var expression = ParseExpression();
      return new Function(name, expression, arguments.ToArray());
    }
  }
}

namespace InterpreterKata.Caching
{
  using InterpreterKata.Nodes;

  internal abstract class Cache
  {
    public abstract void SetVariable(string name, double? value);
    public abstract double? GetVariable(string name);
    
    public abstract void SetFunction(Function function);
    public abstract Function GetFunction(string name);
    
    public abstract GlobalCache GetGlobal();
  }
  
  internal sealed class GlobalCache : Cache
  {
    private readonly Dictionary<string, double?> variableCache;    
    private readonly Dictionary<string, Function> functionCache;
    
    public GlobalCache()
    {
      variableCache = new Dictionary<string, double?>();
      functionCache = new Dictionary<string, Function>();
    }
    
    public override double? GetVariable(string name) 
    {
      if(variableCache.ContainsKey(name))
        return variableCache[name];
      else
        return null;
    }
    
    public override void SetVariable(string name, double? value)
    {
      if(variableCache.ContainsKey(name))
        variableCache[name] = value;
      else if(!functionCache.ContainsKey(name))
        variableCache.Add(name, value);        
    }

    public override Function GetFunction(string name) 
    {
      if(functionCache.ContainsKey(name))
        return functionCache[name];
      else
        return null;
    }

    public override void SetFunction(Function function)
    {
      if(functionCache.ContainsKey(function.Name))
        functionCache[function.Name] = function;
      else if(!variableCache.ContainsKey(function.Name))
        functionCache.Add(function.Name, function);
    }
    
    public override GlobalCache GetGlobal() => this;
  }
  
  internal sealed class LocalCache : Cache
  {
    private readonly GlobalCache globalCache;
    private readonly Dictionary<string, double?> variableCache;    
    
    public LocalCache(GlobalCache _global)
    {
      globalCache = _global;
      variableCache = new Dictionary<string, double?>();
    }
    
    public override double? GetVariable(string name) 
    {
      if(variableCache.ContainsKey(name))
        return variableCache[name];
      else
        return globalCache.GetVariable(name);
    }
    
    public override void SetVariable(string name, double? value)
    {
      if(variableCache.ContainsKey(name))
        variableCache[name] = value;
      else
        variableCache.Add(name, value);  
    }
    
    public override Function GetFunction(string name)  => globalCache.GetFunction(name);
    
    public override void SetFunction(Function function) => globalCache.SetFunction(function);
    
    public override GlobalCache GetGlobal() => globalCache;
  }
}
