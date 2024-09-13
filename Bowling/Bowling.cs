//https://www.codewars.com/kata/5531abe4855bcc8d1f00004c
using System;
using System.Collections.Generic;

public static class Kata
{
    static int[][] scores = new int[10][];
  
    public static int BowlingScore(string f)
    {
        Console.WriteLine("-------" + f);
    
        int frame = 0;
        foreach(var s in f.Split(' '))
        {
            var scoreSet = new int[s.Length];
      
            for(int i = 0; i < s.Length; i++)
            {
                switch(s[i])
                {
                    case 'X':
                        scoreSet[i] = 10;
                        break;
                    case '/':
                        scoreSet[i] = 10 - scoreSet[i - 1];
                        break;
                    default:
                        scoreSet[i] = s[i] - '0';
                        break;
                }
            }
      
            scores[frame] = scoreSet;
            frame++;
        }
    
        int final = 0;
        for(frame = 0; frame < 9; frame++)
        {
            var current = scores[frame];
      
            if(current[0] == 10)//strike
            {
                final += 10;
                final += SumNextMoves(frame);
            }
            else if(Sum(current) == 10)//spare
            {
                final += 10;
                final += scores[frame + 1][0];
            }
            else//none
                final += Sum(current);
        }

        return final + Sum(scores[scores.Length - 1]);
    }

    private static int Sum(int[] arr)
    {
        int sum = 0;
        for(int i = 0; i < arr.Length; i++)
            sum += arr[i];
    
        return sum;
    }
  
    private static int SumNextMoves(int f) {
        var sum = 0;
    
        int i = 0;
        for(int frame = f + 1, pos = 0; i < 2; i++) {
            if(frame >= scores.Length)
                break;
      
            var current = scores[frame][pos];
            sum += current;
      
            pos++;
            if(pos == scores[frame].Length)
            {
                pos = 0;
                frame++;
            }
        }
    
        if(i == 2)
            return sum;
    
        return 0;
    }
}