using System;
public class Finder
{
    private static int length;
    private static string mazeString;
  
    private static int[,] maze;
  
    public static int PathFinder(string m)
    {
        mazeString = m;
        length = mazeString.IndexOf('\n');
        if(length == -1)
            length = mazeString.Length;
    
        maze = new int[length, length];
        for(int i = 0; i < length; i++)
        for(int k = 0; k < length; k++)
            maze[i, k] = -1;
    
        maze[0, 0] = Map(0, 0);

        bool change;
        while(true)
        {
            change = false;
            for(int i = 0; i < maze.GetLength(0); i++)
            for(int k = 0; k < maze.GetLength(1); k++)  
                if(maze[i, k] != -1)
                    change |= CalculateState(i, k);
      
            if(!change)
                break;
        }
    
        return maze[length - 1, length - 1] - maze[0, 0];
    }
  
    private static int Map(int i, int k) => mazeString[i * (length + 1) + k] - '0';
  
    private static bool SetState(int i, int j, int state, int stateRaw) 
    {
        var val = state + Math.Abs(stateRaw - Map(i, j));
        if(maze[i, j] == -1 || maze[i, j] > val)
        {
            maze[i, j] = val;
            return true;
        }
    
        return false;
    }
  
    private static bool CalculateState(int i, int j) 
    {
        bool change = false;
        if(i > 0)
            change |= SetState(i - 1, j, maze[i, j], Map(i, j));
        if(j > 0)
            change |= SetState(i, j - 1, maze[i, j], Map(i, j));
        if(i < length - 1)
            change |= SetState(i + 1, j, maze[i, j], Map(i, j));
        if(j < length - 1)
            change |= SetState(i, j + 1, maze[i, j], Map(i, j));
    
        return change;
    }  
}