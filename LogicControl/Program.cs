using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LogicControl
{
    public static class Program
    {
        public static void Main(string[] args)
        {
            var code = @"
struct Input
{
    vector3 Position : IN_Position;
    vector3 Velocity : IN_Velocity;
}

struct Output
{
    vector3 Velocity : OUT_Velocity;
}

vector3 ApplyGravity(InputValues input) : OUT_Velocity
{
    a = 5;

    while (a < 5)
        a = a - 1;

    return input.Velocity + vector3(0, 0.5, 0);
}

float TestCount(float input : IN_Count) : OUT_Count
{
    return input + 5;
}

float TestWhile(float input : IN_Count) : OUT_Count
{
    a = 0;
    b = input;

    while (a < 5)
    {
        b += 5;
        a += 1;
    }

    return b;
}

struct FloatInputs
{
    float Test : IN_Count;
    string Name : IN_Name;
}

struct FloatOutputs
{
    float A : OUT_A;
    float B : OUT_Name;
}

FloatOutputs TestStructs(FloatInputs input)
{
    output = FloatOutputs();
    output.A = input.Test + 4;
    output.B = input.Name + ' world';
    return output;
}

float TestReturn() : OUT_Count
{
    return 4;
    return 5;
    return 6;
}

float TestCast(string str : IN_String) : OUT_Count
{
    return float(str);
}

";

            var script = LogicControl.LoadScript(code);
            var instance = script.CreateUnmappedInstance();

            var output = instance.Execute("TestWhile", new Dictionary<string, object> { { "IN_Count", 5f } });

            foreach (var kv in output)
            {
                Console.WriteLine(kv.Key + ": " + kv.Value);
            }

            output = instance.Execute("TestStructs", new Dictionary<string, object> { { "IN_Count", 5f }, { "IN_Name", "hello" } });

            foreach (var kv in output)
            {
                Console.WriteLine(kv.Key + ": " + kv.Value);
            }

            output = instance.Execute("TestReturn", new Dictionary<string, object>());

            foreach (var kv in output)
            {
                Console.WriteLine(kv.Key + ": " + kv.Value);
            }

            output = instance.Execute("TestCast", new Dictionary<string, object> { { "IN_String", "5.0" } });

            foreach (var kv in output)
            {
                Console.WriteLine(kv.Key + ": " + kv.Value);
            }
        }
    }
}
