IMyRemoteControl cocpit;
List<IMyThrust> Thrusters;
List<IMyGyro> gyroskops;
public Program()
{
    cocpit = GridTerminalSystem.GetBlockWithName("cocpit") as IMyRemoteControl;
    Thrusters = new List<IMyThrust>();
    gyroskops = new List<IMyGyro>();
    GridTerminalSystem.GetBlocksOfType<IMyGyro>(gyroskops);
    GridTerminalSystem.GetBlocksOfType<IMyThrust>(Thrusters);
}
Vector3D parse_info()
{
    string[] msg = Me.CustomData.Split(':');
    return new Vector3D(Convert.ToDouble(msg[0]), Convert.ToDouble(msg[1]), Convert.ToDouble(msg[2]));
}
public void Main(string argument, UpdateType uType)
{
    if (argument == "start")
        Runtime.UpdateFrequency = UpdateFrequency.Update1;
    else if (argument == "stop")
    {
        foreach (IMyGyro gyro in gyroskops)
            gyro.GyroOverride = false;
        Runtime.UpdateFrequency = UpdateFrequency.None;
    }
    else if (uType == UpdateType.Update1){
if(Me.CustomData=="kill") boom();
        if(Me.CustomData!="-")
            ToCoord(parse_info());
}
    
}
void boom()
{
    List<IMyWarhead> bombs=new List<IMyWarhead>();
    GridTerminalSystem.GetBlocksOfType<IMyWarhead>(bombs);
    foreach(IMyWarhead b in bombs)
    {
        b.Detonate();
    }
}
float K=1;
void ToCoord(Vector3D vector)
{
    if (Vector3D.Zero == vector) return;
    Vector3D v=Vector3D.Normalize(cocpit.GetPosition() - vector);
    ToVector(v+cocpit.GetShipVelocities().AngularVelocity*K);
}
void ToVector(Vector3D vector)
{
    
    Vector3D Norm = Vector3D.Normalize(vector);
    float PitchInput = (float)Norm.Dot(cocpit.WorldMatrix.Up);
    float RollInput = (float)Norm.Dot(cocpit.WorldMatrix.Left);
    float YawInput = 0;
    foreach (IMyGyro gyro in gyroskops)
    {
        gyro.GyroOverride = true;
        gyro.Yaw = YawInput;
        gyro.Roll = -RollInput;
        gyro.Pitch = -PitchInput;
    }
}