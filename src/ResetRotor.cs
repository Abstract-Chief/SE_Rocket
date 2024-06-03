IMyMotorAdvancedStator h, v;

public Program()
{
    h = GridTerminalSystem.GetBlockWithName("rocket_rotor_h") as IMyMotorAdvancedStator;
    v = GridTerminalSystem.GetBlockWithName("rocket_rotor_v") as IMyMotorAdvancedStator;
}
void set_rotor(IMyMotorAdvancedStator r,float a,float b,float v)
{
    r.SetValueFloat("UpperLimit", a);
    r.SetValueFloat("LowerLimit", b);
    r.SetValueFloat("Velocity", v);
}
public void Main(string args)
{
    if (args == "r_rotor_h")
        set_rotor(h, 360, 0, -30);
    else if (args == "r_rotor_v")
        set_rotor(v, 90, 0, -30);
    else if(args=="update_rotor")
    {
        set_rotor(h, float.MaxValue, float.MinValue, 0);
        set_rotor(v, float.MaxValue, float.MinValue, 0);
    }    
}