using System.Collections.Generic;

namespace Solution {
  using System;
  public class BattleshipField {
    private static int[,] field;
    
    public static bool ValidateBattlefield(int[,] f) {
      var ships = new Dictionary<int, (int, int)>()
      {
        {4, (1, 0)},
        {3, (2, 0)},
        {2, (3, 0)},
        {1, (4, 0)}
      };
      
      field = new int[f.GetLength(0) + 1, f.GetLength(1) + 1];
      for(int i = 1; i < f.GetLength(0) + 1; i++)//less if conditions in following steps
        for(int k = 1; k < f.GetLength(1) + 1; k++)
          field[i, k] = f[i - 1, k - 1];
      
      for(int i = 1; i < field.GetLength(0) - 1; i++)
        for(int k = 1; k < field.GetLength(0) - 1; k++)
        {
          if(field[i, k] == 1)
          {
            var state = CalculateState(i, k);  
            Console.WriteLine(state);
            if(state == (-1, -1))
              return false;
          
            var ship = CalculateShip(i, k, state, 1);
            if(ship == -1)
              return false;
          
            if(ships.ContainsKey(ship))
              ships[ship] = (ships[ship].Item1, ships[ship].Item2 + 1);
            else
              return false;
          }
        }
      
      foreach(var key in ships.Keys)
      {
        var current = ships[key];
        if(current.Item1 != current.Item2)
          return false;
      }
      
      return true;
    }
    
    private static (int, int) CalculateState(int row, int col)
    {
      field[row, col] = -1;//for reference
      (int, int) state = (0, 0);
      
      bool breakOut = false;
      for(int i = row - 1; i < row + 2; i++)
      {
        for(int k = col - 1; k < col + 2; k++)
        {
          switch(field[i, k])
          {
            case 0:
            case -1:
              break;//position to calulcate state for or just an empty space
            case 1:
              if(state == (0, 0))//has to have only one state
                state = (i - row, k - col);
              else
              {
                breakOut = true;
                state = (-1, -1);
              }
              break;
            default:
              breakOut = true;
              state = (-1, -1);//2 ships close to each other
              break;
          }
        }
        
        if(breakOut)
          break;
      }
      
      field[row, col] = 1;//reset state
      return state;
    }
    
    private static int CalculateShip(int row, int col, (int, int) transformation, int count)
    {
      field[row, col] = 2;//confirmed ship, permenant change
      
      bool found = false;
      for(int i = row - 1; i < row + 2; i++)
        for(int k = col - 1; k < col + 2; k++)
        {
          switch(field[i, k])
          {
            case 0:
            case 2:
              break;//position to calulcate state for or just an empty space
            case 1:
              if((i - row, k - col) != transformation)//cant have 2 possible states for a single position
                return -1;
              
              found = true;
              break;
            default:
              return -1;
          }
        }
      
      if(found)
        return CalculateShip(row + transformation.Item1, col + transformation.Item2, transformation, count + 1);
      else
        return count;
    }
  }
}