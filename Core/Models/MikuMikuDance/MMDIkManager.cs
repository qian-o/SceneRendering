﻿namespace Core.Models.MikuMikuDance;

public abstract class MMDIkManager
{
    public abstract int GetIkSolverCount();

    public abstract int FindIkSolverIndex(string name);

    public abstract MMDIkSolver GetIkSolver(int index);

    public MMDIkSolver? GetMMDIkSolver(string ikName)
    {
        int findIndex = FindIkSolverIndex(ikName);

        if (findIndex == -1)
        {
            return null;
        }

        return GetIkSolver(findIndex);
    }
}