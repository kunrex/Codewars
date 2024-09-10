using System;
using System.Collections.Generic;

public class RegExpParser {
  
  static int index;
  static List<Token> tokens;
  
  public static Reg.Exp parse (string input) {
    Lex(input);
    
    index = 0;
    return Parse(false);
  }
  
  static void Lex(string input)
  {
    tokens = new List<Token>();
    for(index = 0; index < input.Length; index++)
    {
      switch(input[index])
      {
        case '.':
          tokens.Add(Seperator.Any);
          break;
        case '*':
          tokens.Add(Operator.ZeroOrMore);
          if(index > 0 && tokens[index - 1].Type == TokenType.ZeroOrMore)
          {
            tokens.Clear();
            return;
          }
          break;
        case '|':
          tokens.Add(Operator.Or);
          break;
        case '(':
          tokens.Add(Seperator.OpenBracket);
          break;
        case ')':
          tokens.Add(Seperator.CloseBracket);
          break;
        default:
          tokens.Add(new Character(input[index]));
          break;
      }
    }
  }
  
  static Reg.Exp Parse(bool matchClose)
  {
    int stack = 0;
    List<Reg.Exp> output = new List<Reg.Exp>();
    
    bool breakOut = false;
    TokenType expected = TokenType.OpenBracket | TokenType.Any | TokenType.Character;
    for(; index < tokens.Count; index++)
    {
      var current = tokens[index];
      if((current.Type & expected) != current.Type)
      {
        stack = 0;
        output.Clear();
        breakOut = true;
      }
      else
      {
        switch(current.Type)
        {
          case TokenType.Character:
            output.Add(Reg.normal(tokens[index].Value));
            expected = TokenType.Character | TokenType.Any | TokenType.CloseBracket | TokenType.ZeroOrMore | TokenType.Or;
            break;
          case TokenType.Any:
            output.Add(Reg.any());
            expected = TokenType.Character | TokenType.Any | TokenType.CloseBracket | TokenType.ZeroOrMore | TokenType.Or;
            break;
          case TokenType.OpenBracket:
            index++;
            var toAdd = Parse(true);
  
            if(toAdd == null)
            {
              stack = 0;
              output.Clear();
              breakOut = true;
            }
            else
            {
              output.Add(toAdd);
              expected = TokenType.Or | TokenType.ZeroOrMore | TokenType.Any | TokenType.CloseBracket;
            } 
            break;
          case TokenType.CloseBracket:
            breakOut = true;
            break;
          case TokenType.Or:         
            SequenceOutput(output);
            index++;
            var rhs = Parse(matchClose);

            output.Add(Reg.or(output[0], rhs));
            output.RemoveAt(0);
            if(index < tokens.Count && tokens[index].Type == TokenType.CloseBracket && matchClose)
              breakOut = true;
            else
              expected = TokenType.Character | TokenType.CloseBracket | TokenType.ZeroOrMore | TokenType.Or | TokenType.Any;
            break;
          case TokenType.ZeroOrMore:
            output.Add(Reg.zeroOrMore(output[output.Count - 1]));
            output.RemoveAt(output.Count - 2);
            
            expected = TokenType.Or | TokenType.CloseBracket;
            break;  
        }
      }
        
      if(breakOut)
        break;
    }
    
    
    if(matchClose && !breakOut)
    {
      Console.WriteLine("hi");
      return null;
    }
    
    SequenceOutput(output);
    return output[0];
  }

  static void SequenceOutput(List<Reg.Exp> output)
  {
    switch(output.Count)
    {
      case 0:
        output.Clear();
        output.Add(null);
        break;
      case 1:
        break;
      default:
        var first = Reg.add(Reg.str(output[0]), output[1]);
        output.RemoveAt(0);
        output.RemoveAt(0);
        output.Insert(0, first);
        while(output.Count > 1)
        {
          var toAdd = Reg.add((Reg.Str)output[0], output[1]);
          output.RemoveAt(0);
          output.RemoveAt(0);
          output.Insert(0, toAdd);
        }    
        break;
    }
  }
  
  static void ClearStack(List<Reg.Exp> output, int stack)
  {
    for(int i = 0; i < stack; i++)
    {
      if(output.Count < 2)
      {
        output.Clear();
        return;
      }
      
      var toAdd = Reg.or(output[output.Count - 2], output[output.Count - 1]);
      output.RemoveAt(output.Count - 1);
      output.RemoveAt(output.Count - 1);
      output.Add(toAdd);
    }
  }

  static Reg.Exp ParseSequence()
  {
    List<Reg.Exp> output = new List<Reg.Exp>();
    
    bool breakOut = false;
    for(; index < tokens.Count; index++)
    {
      switch(tokens[index].Type)
      {
        case TokenType.Character:
          output.Add(Reg.normal(tokens[index].Value));
          break;
        case TokenType.Any:
          output.Add(Reg.any());
          break;
        case TokenType.ZeroOrMore:
          output.Add(Reg.zeroOrMore(output[output.Count - 1]));
          output.RemoveAt(output.Count - 2);
          break;
        default:
          index--;
          breakOut = true;
          break;
      }
      
      if(breakOut)
        break;
    }
    
    SequenceOutput(output);
    return output[0];
  }
}

[Flags]
enum TokenType : byte
{
  Any,
  Character,
  
  Or,
  ZeroOrMore,

  OpenBracket,
  CloseBracket
}

abstract class Token
{
  protected readonly char value;
  public char Value { get => value; }
  
  public abstract TokenType Type { get; }
    
  protected Token(char _value)
  {
    value = _value;
  }
}

sealed class Character : Token
{
  public override TokenType Type { get => TokenType.Character; }

  public Character(char _value) : base(_value)
  { }
}

sealed class Seperator : Token
{
  public static Seperator Any = new Seperator('.', TokenType.Any); 
  public static Seperator OpenBracket = new Seperator('(', TokenType.OpenBracket); 
  public static Seperator CloseBracket = new Seperator(')', TokenType.CloseBracket); 

  private readonly TokenType type;
  public override TokenType Type { get => type; }
  
  public Seperator(char _value, TokenType _type) : base(_value)
  {
    type = _type;
  }
}

sealed class Operator : Token
{
  public static Operator Or = new Operator('|', TokenType.Or); 
  public static Operator ZeroOrMore = new Operator('*', TokenType.ZeroOrMore); 

  private readonly TokenType type;
  public override TokenType Type { get => type; }
  
  public Operator(char _value, TokenType _type) : base(_value)
  {
    type = _type;
  }
}
