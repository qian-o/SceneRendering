﻿using BulletSharp;

namespace Core.Models.MikuMikuDance;

public abstract class MMDMotionState : MotionState
{
    public abstract void Reset();

    public abstract void ReflectGlobalTransform();
}
