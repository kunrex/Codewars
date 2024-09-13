//https://www.codewars.com/kata/53e57dada0cb0400ba000688
using System;
using System.Linq;
using System.Collections.Generic;

//gotta love factorials
public class Kata
{
    public static ulong ListPosition(string s)
    {
        var characters = new Dictionary<char, int>();
    
        foreach(char c in s)
            if(characters.ContainsKey(c))
                characters[c] = characters[c] + 1;
            else
                characters.Add(c, 1);
    
        var keys = characters.Keys.OrderBy(x => x).ToList();
    
        ulong pos = 0;
        foreach(char c in s)
        {
            var index = keys.IndexOf(c);
      
            if(index != 0)
            {
                for(int i = 0; i < index; i++)
                {
                    characters[keys[i]] = characters[keys[i]] - 1;//temporary
          
                    int sum = characters.Values.Sum(x => x);
                    ulong fact = Factorial(sum);
          
                    for(int k = 0; k < keys.Count; k++)
                        fact /= Factorial(characters[keys[k]]);
            
                    pos += fact;
                    characters[keys[i]] = characters[keys[i]] + 1;//reset state
                }
            }
      
            characters[c] = characters[c] - 1;
            if(characters[c] == 0)
            {
                keys.Remove(c);
                characters.Remove(c);
            }
        }
    
        return pos + 1;
    }
  
    private static ulong Factorial(int n)
    {
        ulong s = 1;
        for(int i = 2; i <= n; i++)
            s *= (ulong)i;
    
        return s;
    }
}