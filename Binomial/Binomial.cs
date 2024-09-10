using System;
using System.Text;
using System.Text.RegularExpressions;

public class KataSolution
{
  //im bad at regex
  private static Regex regex = new Regex(@"^\((-?)([0-9]+)?([a-z])(\+|-)([0-9]+)\)\^([0-9]+)$");

  public static string Expand(string expr)  
  {
    var match = regex.Match(expr);
    StringBuilder s = new StringBuilder();

    Console.WriteLine(expr);
    if(match.Success)
    {
      char variable = match.Groups[3].Value[0];
      int exponent = int.Parse(match.Groups[6].Value);
      
      int a = ReadNumber($"{match.Groups[1].Value}{match.Groups[2].Value}");
      int b = ReadNumber($"{match.Groups[4].Value}{match.Groups[5].Value}");
      
      for(int i = exponent; i >= 0; i--)
        PushTerm(variable, a, b, i, exponent, in s);
    }
    
    if(s[0] == '+')
       return (s.Length == 1) ? "1" : s.ToString(1, s.Length - 1);
    else
      return s.ToString();
  }
  
  private static int ReadNumber(string s)
  {
    if(s == String.Empty)
      return 1;
    else if(s.Trim() == "-")
      return -1;
    
    return int.Parse(s);
  }
  
  private static void PushTerm(char variable, int a, int b, int r, int n, in StringBuilder s)
  {
    long coeff = CalculateCombinations(n, r) * Pow(a, r) * Pow(b, n - r);
    switch(coeff)
    {
      case 0:
        return;
      case 1:
        s.Append(r == 0 ? "+1" : "+");
        break;
      case -1:
        s.Append(r == 0 ? "-1" : "-");
        break;
      default:
        if(coeff > 0)
          s.Append('+');
        
        s.Append(coeff.ToString());
        break;
    }
    
    switch(r)//power of x
    {
      case 0:
        break;
      case 1:
        s.Append(variable);
        break;
      default:
        s.Append($"{variable}^{r}");
        break;
    }
  }
  
  private static int CalculateCombinations(int n, int r)
  {
    int max = Math.Max(r, n - r);
    int min = Math.Min(r, n - r);

    int c = 1;
    for (int i = max + 1; i <= n; i++)
      c *= i;

    for (int i = 2; i <= min; i++)
      c /= i;

    return c;
  }
  
  private static long Pow(int n, int p) 
  {
    long r = 1;
    for(int i = 0; i < p; i++)
      r *= n;
    
    return r;
  }
}