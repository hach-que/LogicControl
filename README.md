LogicControl
============

LogicControl is a light-weight scripting language for specifying logic in external files.  It is inspired by HLSL syntax, since shaders often express arithmetic and simple procedural logic flow.

Here is an example of a LogicControl script:

```
struct Input
{
    vector3 Position : IN_Position;
    vector3 Velocity : IN_Velocity;
}

struct Output
{
    vector3 Velocity : OUT_Velocity;
}

vector3 ApplyLandGravity(InputValues input) : OUT_Velocity
{
    return input.Velocity + vector3(0, 0.5, 0);
}

Output ApplyWaterGravity(vector3 velocity : IN_Velocity)
{
    output = Output()
    output.Velocity = velocity + vector3(0, 0.1, 0);
    return output;
}
```

Semantics such as `IN_Position` and `OUT_Velocity` are marked in the calling application.  Ideally, in the calling application you define a C# class like so:

```
public interface IGravityApplier
{
    [Semantic("IN_Position")]
    Vector3 Position { get; set; }
    
    [Semantic("IN_Velocity")]
    Vector3 Velocity { get; set; }
    
    [Semantic("OUT_Position")]
    Vector3? ResultPosition { get; set; }
    
    [Semantic("OUT_Velocity")]
    Vector3? ResultVelocity { get; set; }
    
    void ApplyLandGravity();
    
    void ApplyWaterGravity();
}
```

and you would operate it like so:

```
var script = LogicControl.LoadScript("script.lc");

var gravityApplier1 = script.CreateInstance<IGravityApplier>();

gravityApplier1.Position = new Vector3(10, 10, 10);
gravityApplier1.Velocity = new Vector3(0, 0.5f, 0);
gravityApplier1.ApplyLandGravity();

Console.WriteLine("Result position: " + gravityApplier1.ResultPosition);
Console.WriteLine("Velocity position: " + gravityApplier1.ResultVelocity);

var gravityApplier2 = script.CreateInstance<IGravityApplier>();

gravityApplier2.Position = new Vector3(10, 10, 10);
gravityApplier2.Velocity = new Vector3(0, 0.5f, 0);
gravityApplier2.ApplyWaterGravity();

Console.WriteLine("Result position: " + gravityApplier2.ResultPosition);
Console.WriteLine("Velocity position: " + gravityApplier2.ResultVelocity);
```
