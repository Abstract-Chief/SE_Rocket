IMyCameraBlock camera;
IMyProgrammableBlock prog;

public Program()
{
    camera = GridTerminalSystem.GetBlockWithName("scaner") as IMyCameraBlock;
    prog = GridTerminalSystem.GetBlockWithName("Rocket") as IMyProgrammableBlock;
    camera.EnableRaycast = true;
    Runtime.UpdateFrequency = UpdateFrequency.Update1;
}
void put_info(Vector3D v)
{
    prog.CustomData = $"{v.X}:{v.Y}:{v.Z}";
}
Vector3D scan(int size)
{
    if (camera.AvailableScanRange < size) return Vector3D.Zero;
    MyDetectedEntityInfo rayinfo = camera.Raycast(size, 0, 0);
    if (rayinfo.Type != MyDetectedEntityType.None)
    {
        last_scan_time = DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond;
        last_speed_scan = rayinfo.Velocity;
        return last_scan=(Vector3D)rayinfo.HitPosition;
    }
    return Vector3D.Zero;
}
double K = 1200;
Vector3D get_camera()
{
    double distance = (Me.GetPosition() - prog.GetPosition()).Length();
    Vector3D r = Me.GetPosition() + Vector3D.Normalize(camera.WorldMatrix.Forward) * (K + distance);
    put_info(r);
    return r;
}
Vector3D calculate()
{
    Vector3D vector = last_scan - prog.GetPosition();
    for(float time = 0; time < max_time; time += 0.05f)
    {
        Vector3D r = vector / time + last_speed_scan;
        gtime=time;
        gspeed=(float)r.Length();
        if( r.Length()<=100 && r.Length() > 90)
        {
            Vector3D coord = r * time+prog.GetPosition();
            return coord;
        }
    }
    return Vector3D.Zero;
}
bool type = false;
Vector3D last_scan = Vector3D.Zero;
Vector3D last_speed_scan = Vector3D.Zero;
float gtime = -1,gspeed=-1;
long last_scan_time=-1;
int flag = 0;
int flag2 = 0;
int distance = 8000;
int max_time = 360;
public void Main(string args)
{
    string info = "";
    if (args == "laser_on")  type = true; 
    else if (args == "calc_on")  type = false; 
    else if (args == "kill") { prog.CustomData = "kill"; flag = 100; }
    if (flag > 0) { info += "kill rocket\n"; flag--; }
    if (flag2 > 0) { info += "unreal target\n"; flag2--; }
    if (flag==0)
    {
        if (type)
        {
            info += "scan: Laser\n";
            get_camera();
        }
        else
        {
            info += "scan: Calculator\n";
            if(args=="scan"){
            if (scan(distance) != Vector3D.Zero)
            {
                Vector3D result = calculate();
                if (result != Vector3D.Zero) { put_info(result); flag2 = 0; }
                else
                    flag2 = 100;
                info+=$"result: {result}\n";
            }
            }
            if (last_scan_time > 0)
                info += $"last scan was {(DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond) - last_scan_time} mls before\n";
            info += $"time: {gtime*1000-((DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond) - last_scan_time)} speed: {gspeed}\n";
            info+=$"distance {(last_scan-prog.GetPosition()).Length()}\n";
           
        }
       

    }
    PutToPanel("panel", info);
}
bool PutToPanel(string name, string text)
{
    IMyTextPanel panel = GridTerminalSystem.GetBlockWithName(name) as IMyTextPanel;
    if (panel == null)
        return false;
    panel.WriteText(text);
    return true;
}