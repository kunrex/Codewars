using System.Collections.Generic;
using System;
using System.Linq;

public class Finder
{
    private static int length = 0;
    private static int[,] maze;
  
    public static int PathFinder(string m)
    {
        length = m.IndexOf('\n');
        if(length == -1)//one line maze
            length = m.Length;
    
        maze = new int[length, length];
      
        for(int i = 0; i < length; i++)//converts string to ints
        for(int k = 0; k < length; k++)
        {
            switch(m[i * (length + 1) + k])
            {
                case '.':
                    maze[i,k] = 0;
                    break;
                case 'W':
                    maze[i, k] = -1;
                    break;
            }
        }

        maze[0, 0] = 1;
        CalculateState(0, 0);
    
        bool change;
        while(true)
        {
            change = false;
            for(int i = 0; i < length; i++)
            for(int j = 0; j < length; j++)
                change |= CalculateState(i, j); 
      
            if(!change)
                break;
        }
    
        if(maze[length - 1, length - 1] == 0)
            return -1;
        else
            return maze[length - 1, length - 1] - 1;
    }
  
    private static bool SetState(int i, int j, int val) {
        if(maze[i, j] == 0)
        {
            maze[i, j] = val;
            return true;
        }
        else if(val > 0 && maze[i, j] > val)
        {
            maze[i, j] = val;
            return true;
        }
    
        return false;
    }
  
    private static bool CalculateState(int i, int j) 
    {
        if(maze[i, j] == -1 || maze[i, j] == 0)//cant caluclate state for a position not reached or for a wall
            return false;
        if(i == j && i == length - 1)//end
            return false;
    
        bool change = false;
        if(i > 0)
            change |= SetState(i - 1, j, maze[i, j] + 1);
        if(j > 0)
            change |= SetState(i, j - 1, maze[i, j] + 1);
        if(i < length - 1)
            change |= SetState(i + 1, j, maze[i, j] + 1);
        if(j < length - 1)
            change |= SetState(i, j + 1, maze[i, j] + 1);
    
        return change;
    }  
}